﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord;
using System.Data;
using System.Linq;
using Bobii.src.Bobii;
using Discord.Interactions;
using Bobii.src.InteractionModules.Slashcommands;
using Bobii.src.InteractionModules.ModalInteractions;

namespace Bobii.src.Handler
{
    public class HandlingService
    {
        #region Declarations 
        public static DiscordSocketClient _client;
        public static InteractionService _interactionService;
        public static IServiceProvider _serviceProvider;

        public static SocketGuildChannel _serverCountChannelBobStyDE;
        public static SocketGuildChannel _serverCountChannelBobii;
        public static ISocketMessageChannel _dmChannel;
        private SocketTextChannel _joinLeaveLogChannel;
        public static Helper BobiiHelper;
        public static Cache Cache;
        public static SocketTextChannel _consoleChannel;
        public static SocketGuild _bobStyDEGuild;
        public static SocketGuild _developerGuild;
        public static TempChannel.DelayOnDelete _delayOnDelete;
        public TempChannel.VoiceUpdateHandler VoiceUpdatedHandler;
        #endregion

        #region Constructor  
        public HandlingService(IServiceProvider services, InteractionService interactionService)
        {
            _serviceProvider = services;
            _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
            _interactionService = interactionService;

            BobiiHelper = new Bobii.Helper();
            Cache = new Bobii.Cache();

            _client.InteractionCreated += HandleInteractionCreated;
            _client.Ready += ClientReadyAsync;
            _client.MessageReceived += HandleMessageReceived;
            _client.LeftGuild += HandleLeftGuild;
            _client.JoinedGuild += HandleJoinGuild;
            _client.UserVoiceStateUpdated += HandleUserVoiceStateUpdatedAsync;
            _client.ChannelDestroyed += HandleChannelDestroyed;
            _client.UserLeft += HandleUserLeftGuild;
            _client.ModalSubmitted += HandleModalSubmitted;

            BobiiHelper.WriteConsoleEventHandler += HandleWriteToConsole;
        }
        #endregion

        #region Tasks
        public async Task HandleModalSubmitted(SocketModal modal)
        {
            await ModalHandler.HandleModal(modal, _client);
        }

        public async Task HandleWriteToConsole(object src, Bobii.EventArg.WriteConsoleEventArg eventArg)
        {
            await _consoleChannel.SendMessageAsync(embed: Bobii.Helper.CreateEmbed(_bobStyDEGuild, eventArg.Message.Remove(0, 9), error: eventArg.Error).Result);
        }

        private async Task HandleUserLeftGuild(SocketGuild guild, SocketUser user)
        {
            if (FilterLink.EntityFramework.FilterLinkUserGuildHelper.IsUserOnWhitelistInGuild(guild.Id, user.Id).Result)
            {
                await FilterLink.EntityFramework.FilterLinkUserGuildHelper.RemoveWhiteListUserFromGuild(guild.Id, user.Id);
            }
        }

        private async Task HandleMessageReceived(SocketMessage message)
        {
            _ = Task.Run(async () => MessageReceivedHandler.FilterMessageHandler(message, _client, _dmChannel));
        }

        private async Task HandleInteractionCreated(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //switch (interaction.Type)
            //{
            //    case InteractionType.ApplicationCommand:
            //        await SlashCommandHandlingService.SlashCommandHandler(interaction, _client);
            //        break;
            //    case InteractionType.ApplicationCommandAutocomplete:
            //        await AutocompletionHandlingService.HandleAutocompletion((SocketAutocompleteInteraction)interaction);
            //        break;
            //    case InteractionType.MessageComponent:
            //        await MessageComponentHandlingService.MessageComponentHandler(interaction, _client);
            //        break;
            //    default: // We dont support it
            //        Console.WriteLine("Unsupported interaction type: " + interaction.Type);
            //        break;
            //}
        }

        private async Task HandleChannelDestroyed(SocketChannel channel)
        {
            //Temp Channels
            var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(channel.Id).Result;

            if (tempChannel != null)
            {
                if (tempChannel.textchannelid != 0)
                {
                    var textChannel = (SocketTextChannel)_client.GetChannel(tempChannel.textchannelid.Value);
                    if (textChannel != null)
                    {
                        _ = textChannel.DeleteAsync();
                    }
                }
                _ = TempChannel.EntityFramework.TempChannelsHelper.RemoveTC(0, channel.Id);
            }

            //Create Temp Channels
            var createTempChannel = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelList()
                .Result.Where(ch => ch.createchannelid == channel.Id)
                .FirstOrDefault();

            if (createTempChannel != null)
            {
                _ = TempChannel.EntityFramework.CreateTempChannelsHelper.RemoveCC("No Guild supplyed", channel.Id);
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Channel: '{channel.Id}' was successfully deleted");
            }

            //FilterLinkLogs
            var filterLinkLog = FilterLink.EntityFramework.FilterLinkLogsHelper.GetFilterLinkLogChannels()
                .Result
                .Where(ch => ch.channelid == channel.Id)
                .FirstOrDefault();

            if (filterLinkLog != null)
            {
                var guildChannel = (SocketGuildChannel)channel;
                _ = FilterLink.EntityFramework.FilterLinkLogsHelper.RemoveFilterLinkLogChannel(guildChannel.Guild.Id);
                Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Channel: '{channel.Id}' was successfully deleted");
            }

            await Task.CompletedTask;
        }

        private async Task HandleUserVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice)
        {
            await TempChannel.VoiceUpdateHandler.HandleVoiceUpdated(oldVoice, newVoice, user, _client, _delayOnDelete);
        }


        private async Task HandleLeftGuild(SocketGuild guild)
        {
            _ = Task.Run(async () => RefreshServerCountChannels());
            _ = _joinLeaveLogChannel.SendMessageAsync(null, false, Bobii.Helper.CreateEmbed(_joinLeaveLogChannel.Guild, $"**Membercount:** {guild.MemberCount}", $"I left: {guild.Name}").Result);
            _ = Bobii.EntityFramework.BobiiHelper.DeleteEverythingFromGuild(guild);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Bot left the guild: {guild.Name} | ID: {guild.Id}");
        }

        private async Task HandleJoinGuild(SocketGuild guild)
        {
            _ = Task.Run(async () => RefreshServerCountChannels());
            var owner = _client.Rest.GetUserAsync(guild.OwnerId).Result;
            await _joinLeaveLogChannel.SendMessageAsync(null, false, Bobii.Helper.CreateEmbed(_joinLeaveLogChannel.Guild, $"**Owner ID:** {guild.OwnerId}\n**Owner Name:** {owner}\n**Membercount:** {guild.MemberCount}", $"I joined: {guild.Name}").Result);
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Bot joined the guild: {guild.Name} | ID: {guild.Id}");
            var test = guild.GetAuditLogsAsync(limit: 100, actionType: ActionType.BotAdded).FlattenAsync().Result;
        }

        private async Task InitializeInteractionModules()
        {
            // Tempchannel
            await _interactionService.AddModuleAsync<CreateTempChannelSlashCommands>(_serviceProvider);
            await _interactionService.AddModuleAsync<TempChannelModalInteractions>(_serviceProvider);
            await _interactionService.AddModuleAsync<TempChannelSlashCommands>(_serviceProvider);
        }

        public async Task AddGuildCommandsToMainGuild()
        {
            try
            {
                // TODO
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public async Task AddGobalCommandsAsync()
        {
            try
            {
                if (!System.Diagnostics.Debugger.IsAttached)
                {
                    // TODO checken warum das hier nicht funktioniert
                    await _interactionService.AddModulesToGuildAsync(_developerGuild, false, _interactionService.GetModuleInfo<CreateTempChannelSlashCommands>());
                    await _interactionService.AddModulesToGuildAsync(_developerGuild, false, _interactionService.GetModuleInfo<TempChannelSlashCommands>());
                    await _interactionService.AddModulesToGuildAsync(_developerGuild, false, _interactionService.GetModuleInfo<HelpShlashCommands>());
                }
                else
                {
                    await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<CreateTempChannelSlashCommands>());
                    await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<TempChannelSlashCommands>());
                    await _interactionService.AddModulesGloballyAsync(false, _interactionService.GetModuleInfo<HelpShlashCommands>());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task ClientReadyAsync()
        {
            _bobStyDEGuild = _client.GetGuild(Helper.ReadBobiiConfig(ConfigKeys.MainGuildID).ToUlong());
            _developerGuild = _client.GetGuild(Helper.ReadBobiiConfig(ConfigKeys.DeveloperGuildID).ToUlong());

            await InitializeInteractionModules();

            await AddGobalCommandsAsync();
            //await AddGuildCommandsToMainGuild();
            //await AddGobalCommandsAsync();

            _client.Ready -= ClientReadyAsync;
            VoiceUpdatedHandler = new TempChannel.VoiceUpdateHandler();
            var bobiiSupportServerGuild = _client.GetGuild(Helper.ReadBobiiConfig(ConfigKeys.SupportGuildID).ToUlong());

            _serverCountChannelBobii = bobiiSupportServerGuild.GetChannel(Helper.ReadBobiiConfig(ConfigKeys.SupportGuildCountChannelID).ToUlong());
            _serverCountChannelBobStyDE = _bobStyDEGuild.GetChannel(Helper.ReadBobiiConfig(ConfigKeys.MainGuildCountChannelID).ToUlong());
            _joinLeaveLogChannel = _bobStyDEGuild.GetTextChannel(Helper.ReadBobiiConfig(ConfigKeys.JoinLeaveLogChannelID).ToUlong());
            _dmChannel = _bobStyDEGuild.GetTextChannel(Helper.ReadBobiiConfig(ConfigKeys.DMChannelID).ToUlong());
            _consoleChannel = _bobStyDEGuild.GetTextChannel(Helper.ReadBobiiConfig(ConfigKeys.ConsoleChannelID).ToUlong());

            Cache.Captions = Bobii.EntityFramework.BobiiHelper.GetCaptions().Result;
            Cache.Contents = Bobii.EntityFramework.BobiiHelper.GetContents().Result;
            Cache.Commands = Bobii.EntityFramework.BobiiHelper.GetCommands().Result;

            _delayOnDelete = new TempChannel.DelayOnDelete();

            await _delayOnDelete.InitializeDelayDelete(_client);
            await TempChannel.Helper.CheckAndDeleteEmptyVoiceChannels(_client);


            _ = Task.Run(async () => RefreshServerCountChannels());
            await Program.SetBotStatusAsync(_client);

            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Handler     Client Ready");
        }

        /// <summary>
        /// Resets the Cache
        /// </summary>
        /// <returns></returns>
        public static async Task ResetCache()
        {
            Cache.Captions = Bobii.EntityFramework.BobiiHelper.GetCaptions().Result;
            Cache.Contents = Bobii.EntityFramework.BobiiHelper.GetContents().Result;
            Cache.Commands = Bobii.EntityFramework.BobiiHelper.GetCommands().Result;
            await Task.CompletedTask;
        }

        public static async Task RefreshServerCountChannels()
        {
            try
            {
                if (!System.Diagnostics.Debugger.IsAttached)
                {
                    await _serverCountChannelBobStyDE.ModifyAsync(channel => channel.Name = $"Server count: {_client.Guilds.Count}");
                    await _serverCountChannelBobii.ModifyAsync(channel => channel.Name = $"Server count: {_client.Guilds.Count}");
                }
            }
            catch (Exception)
            {
                //Do nothing because sometimes it cant do it ... This is not an important Task anyways
            }
        }
        #endregion
    }
}
