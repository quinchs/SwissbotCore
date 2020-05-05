using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace SwissbotCore
{
    public class AutoSlowmode
    {
        class fuck : DiscordSocketClient { }

        Dictionary<ulong, int> sList = new Dictionary<ulong, int>();

        internal System.Timers.Timer autoSlowmode = new System.Timers.Timer() { Enabled = false, AutoReset = true, Interval = 1000 };

        public DiscordSocketClient client { get; set; }

        public AutoSlowmode(DiscordSocketClient client)
        {
            this.client = client;
            client.MessageReceived += Client_MessageReceived;
        }

        private async System.Threading.Tasks.Task Client_MessageReceived(SocketMessage arg)
        {
            if (sList.ContainsKey(arg.Channel.Id))
            {

            }
        }
    }
}
