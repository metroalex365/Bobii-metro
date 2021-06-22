﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Commands
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;

        public Commands(CommandService service)
        {
            _commandService = service;
        }

        [Command("help")]
        [Summary("Summary of all my commands")]
        public async Task Help()
        {
            await Context.Message.ReplyAsync("", false, CommandHelper.CreateHelpInfo(_commandService));
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    'help was used by {Context.User}");
        }

        [Command("vcname")]
        [Summary("Command to edit VoiceChat Name")]
        public async Task ChangeVoiceChatName(string voiceNameNew)
        {
        }

        [Command("cvcinfo")]
        [Summary("Gives info about the currently set create temp voicechannels")]
        public async Task TempVoiceChannelInof()
        {
            await Context.Message.ReplyAsync("", false, CommandHelper.CreateVoiceChatInfo());
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    'vcinfo was used by \"{Context.User}\"");
        }

        [Command("cvcadd")]
        [Summary("Adds a new create temp voice channel with: cvcadd <VoiceChannelID>")]
        public async Task AddCreateVoiceChannel(string id)
        {
            //The length is hardcoded! Check  if the Id-Length can change
            if (!ulong.TryParse(id, out _) && id.Length != 18)
            {
                CommandHelper.ReplyAndDeleteMessage(Context, null, CommandHelper.CreateOneLineEmbed($"The given ID: \"{id}\" is not valid! Make sure to copy the ID from the voice channel directly!"));
                return;
            }

            if (CommandHelper.CheckIfConfigKeyExistsAlready("CreateTempChannels", id.ToString()))
            {
                CommandHelper.ReplyAndDeleteMessage(Context, null, CommandHelper.CreateOneLineEmbed($"The create temp voice channel with the ID: \"{id}\" exists already!"));
                return;
            }

            CommandHelper.EditConfig("CreateTempChannels", id, Context.Guild.GetChannel(ulong.Parse(id)).Name);
            CommandHelper.ReplyAndDeleteMessage(Context, null, CommandHelper.CreateOneLineEmbed("\"" + Context.Guild.GetChannel(ulong.Parse(id)).Name + $"\" was sucessfully added by \"{Context.User}\" to the create temp voicechannel list!"));
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} Commands    Voicechat: \"{Context.Guild.GetChannel(ulong.Parse(id)).Name}\" with the ID: \"{id}\" was successfully added by {Context.User}");

            //TODO JG 18.06.2021 Check if cvc already exists and reply with message! 
            //Also check if I need ReplyAndDeleteMessage
        }

        [Command("vcvremove")]
        [Summary("Removes a already existing create temp voice channel with: cvcremove <VoiceChannelID>")]
        public async Task RemoveCreateVoiceChannel(string id)
        {
            if (!ulong.TryParse(id, out _) && id.Length != 18)
            {
                CommandHelper.ReplyAndDeleteMessage(Context, null, CommandHelper.CreateOneLineEmbed($"The given ID: \"{id}\" is not valid! Make sure to copy the ID from the voicechannel directly!"));
                return;
            }
        }
    }
}
