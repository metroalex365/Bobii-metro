﻿using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class TempChannelHandler
    {
        #region Handler

        public static async Task VoiceChannelActions(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice, DiscordSocketClient client)
        {
            SocketGuild guild;
            if (newVoice.VoiceChannel != null)
            {
                guild = newVoice.VoiceChannel.Guild;
            }
            else
            {
                guild = oldVoice.VoiceChannel.Guild;
                if (TempChannel.EntityFramework.TempChannelsHelper.DoesOwnerExist(user.Id).Result && TempChannel.EntityFramework.TempChannelsHelper.DoesTempChannelExist(oldVoice.VoiceChannel.Id).Result)
                {
                    var tempChannel = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannel(oldVoice.VoiceChannel.Id).Result;
                    if (tempChannel.textchannelid != 0)
                    {
                        var textChannel = client.Guilds
                            .SelectMany(g => g.Channels)
                            .SingleOrDefault(c => c.Id == tempChannel.textchannelid);

                        if (textChannel != null)
                        {
                            await TempChannel.Helper.RemoveManageChannelRightsToUserTc(user, textChannel as SocketTextChannel);
                        }
                    }
                    await TempChannel.Helper.RemoveManageChannelRightsToUserVc(user, oldVoice.VoiceChannel);
                    await TempChannel.Helper.TansferOwnerShip(oldVoice.VoiceChannel, client);
                }
            }
            var createTempChannels = TempChannel.EntityFramework.CreateTempChannelsHelper.GetCreateTempChannelListOfGuild(guild);
            var tempchannelIDs = TempChannel.EntityFramework.TempChannelsHelper.GetTempChannelList(guild.Id).Result;

            if (oldVoice.VoiceChannel != null)
            {
                if (tempchannelIDs.Count > 0)
                {
                    await TempChannel.Helper.CheckAndDeleteEmptyVoiceChannels(client, guild, tempchannelIDs, user);
                    if (newVoice.VoiceChannel == null)
                    {
                        return;
                    }
                }
            }

            if (newVoice.VoiceChannel != null)
            {

                var createTempChannel = createTempChannels.Result.Where(ch => ch.createchannelid == newVoice.VoiceChannel.Id).FirstOrDefault();
                if (createTempChannel != null)
                {
                    await TempChannel.Helper.CreateAndConnectToVoiceChannel(user, newVoice, createTempChannel.tempchannelname, createTempChannel.channelsize, createTempChannel.textchannel.Value, client);
                }
            }
            else
            {
                return;
            }
        }
        #endregion
    }
}
