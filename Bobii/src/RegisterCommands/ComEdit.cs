﻿using Discord;
using Discord.Net;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Bobii.src.RegisterCommands
{
    class ComEdit
    {
        #region Declarations
        private static ulong _myGuildID = 712373862179930144;
        #endregion

        #region Utility

        #region Global
        public static async Task Register(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
            .WithName("comregister")
            .WithDescription("Registers a slashcommand")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("commandname")
                .WithDescription("The name oft he command which should be registered")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).Build();

            try
            {
                await client.Rest.CreateGuildCommand(command, _myGuildID);
            }
            catch (ApplicationCommandException ex)
            {
                Bobii.WriteToConsol($"Error | {ex.Message}");
            }
        }

        public static async Task Delete(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
            .WithName("comdelete")
            .WithDescription("Removes a slashcommand")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("commandname")
                .WithDescription("The name oft he command which should be removed")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).Build();

            try
            {
                await client.Rest.CreateGuildCommand(command, _myGuildID);
            }
            catch (ApplicationCommandException ex)
            {
                Bobii.WriteToConsol($"Error | {ex.Message}");
            }
        }
        #endregion

        #region Guild
        public static async Task GuildDelete(DiscordSocketClient client)
        {
            var command = new SlashCommandBuilder()
            .WithName("comdeleteguild")
            .WithDescription("Removes a slashcommand from a guild")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("commandname")
                .WithDescription("The name oft he command which should be removed")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("guildid")
                .WithDescription("The guild in wich the command to delete is")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).Build();

            try
            {
                await client.Rest.CreateGuildCommand(command, _myGuildID);
            }
            catch (ApplicationCommandException ex)
            {
                Bobii.WriteToConsol($"Error | {ex.Message}");
            }
        }
        #endregion
        #endregion
    }
}