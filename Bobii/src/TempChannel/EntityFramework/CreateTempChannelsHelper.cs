﻿using Bobii.src.EntityFramework;
using Bobii.src.EntityFramework.Entities;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobii.src.TempChannel.EntityFramework
{
    class CreateTempChannelsHelper
    {
        #region Tasks
        public static async Task WriteToConsol(string message)
        {
            Console.WriteLine($"{DateTime.Now.TimeOfDay:hh\\:mm\\:ss} CCTChannel   {message}");
            await Task.CompletedTask;
        }

        public static async Task AddCC(ulong guildid, string createChannelName, ulong creatChannelId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var createTempChannel = new createtempchannels();
                    createTempChannel.guildid = guildid;
                    createTempChannel.createchannelid = creatChannelId;
                    createTempChannel.tempchannelname = createChannelName;
                    context.CreateTempChannels.Add(createTempChannel);

                    context.SaveChanges();
                    await Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                await WriteToConsol($"Error: | Method: AddCC | Guild: {guildid} | {ex.Message}");
            }
        }

        public static async Task RemoveCC(string guildid, ulong createChannelId)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var createTempChannel = context.CreateTempChannels.AsQueryable().Where(channel => channel.createchannelid == createChannelId).First();
                    context.CreateTempChannels.Remove(createTempChannel);
                    context.SaveChanges();
                    await Task.CompletedTask;
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await WriteToConsol($"Error: | Method: RemoveCC | Guild: {guildid} | {ex.Message}");
            }
        }

        public static async Task ChangeTempChannelName(string newName, ulong createChannelID)
        {
            try
            {
                // §TODO JG/16.11.2021 schauen ob das hier wirklich klappt
                using (var context = new BobiiEntities())
                {
                    var createTempChannel = context.CreateTempChannels.AsQueryable().Where(channel => channel.createchannelid == createChannelID).First();
                    createTempChannel.tempchannelname = newName;
                    context.CreateTempChannels.Update(createTempChannel);
                    context.SaveChanges();
                    await Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                await WriteToConsol($"Error: | Method: ChangeTempChannelName | CreateChannelId: {createChannelID} | {ex.Message}");
            }
        }

        public static async Task<bool> CheckIfCreateVoiceChannelExist(SocketGuild guild, ulong createChannelID)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    var createTempChannel = context.CreateTempChannels.AsQueryable().Where(channel => channel.createchannelid == createChannelID).FirstOrDefault();
                    if (createTempChannel == null)
                    {
                        await Task.CompletedTask;
                        return false;
                    }
                    else
                    {
                        await Task.CompletedTask;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                await WriteToConsol($"Error: {guild.Name} | Function: CheckIfCreateVoiceChannelExist | Guild: {guild.Id} | {ex.Message}");
                return false;
            }
        }

        public static async Task<List<createtempchannels>> GetCreateTempChannelList()
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.CreateTempChannels.ToList();
                }
            }
            catch (Exception ex)
            {
                await WriteToConsol($"Error: | Function: CraeteTempChannelListWithAll | {ex.Message}");
                return null;
            }
        }

        public static async Task<List<createtempchannels>> GetCreateTempChannelListOfGuild(SocketGuild guild)
        {
            try
            {
                using (var context = new BobiiEntities())
                {
                    return context.CreateTempChannels.AsQueryable().Where(channel => channel.guildid == guild.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                await WriteToConsol($"Error: {guild.Name} | Function: GetCreateTempChannelListFromGuild | Guild: {guild.Id} | {ex.Message}");
                return null;
            }
        }
        #endregion
    }
}