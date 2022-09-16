﻿using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Bobii.src.Handler
{
    class MessageReceivedHandler
    {
        public static async Task FilterMessageHandler(SocketMessage message, DiscordSocketClient client, ISocketMessageChannel dmChannel)
        {
            if (message.Author.IsBot)
            {
                return;
            }

            if (await DMSupport.Helper.IsPrivateMessage(message))
            {
                await DMSupport.Helper.HandleDMs(message, (SocketTextChannel)dmChannel, client);
                return;
            }

            var guild = ((IGuildChannel)message.Channel).Guild.Id;

            if (guild == 712373862179930144)
            {
                foreach (SocketThreadChannel thread in ((SocketTextChannel)dmChannel).Threads)
                {
                    if (ulong.TryParse(thread.Name, out _) && thread.Name.Length == 18 && message.Channel.Id == thread.Id)
                    {
                        await DMSupport.Helper.HandleSendDMs(message, thread.Name, client);
                        return;
                    }
                }
            }
        }
    }
}
