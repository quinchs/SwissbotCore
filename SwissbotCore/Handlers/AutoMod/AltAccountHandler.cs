using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static SwissbotCore.Handlers.VerificationHandler;

namespace SwissbotCore.Handlers
{
    [DiscordHandler]
    class AltAccountHandler
    {
        public DiscordSocketClient _client;
        public AltAccountHandler(DiscordSocketClient client)
        {
            _client = client;

            //_client.UserJoined += CheckAlt;
        }
       

        public static bool IsAlt(SocketGuildUser arg)
        {
            if ((DateTime.UtcNow - arg.CreatedAt.UtcDateTime).TotalHours < Global.AltVerificationHours)
                return true;
            return false;
        }
    }
}
