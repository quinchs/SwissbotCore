using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace SwissbotCore.Handlers
{
    [DiscordHandler]
    class MemberCountHandler
    {
        public DiscordSocketClient client;
        public MemberCountHandler(DiscordSocketClient c)
        {
            client = c;

            Timer t = new Timer() { AutoReset = true, Interval = new TimeSpan(0, 0, 7, 30).TotalMilliseconds, /* 7.5 minutes */ Enabled = true};
            t.Elapsed += HandleUserCount;
            t.Start();
            Global.ConsoleLog("Started Member count timer!", ConsoleColor.Blue);
            HandleUserCount(null, null); 
        }

        private async void HandleUserCount(object sender, ElapsedEventArgs e)
        {
            //get stat channel 
            var g = client.GetGuild(Global.SwissGuildId);
            if (g == null)
                return;
            var chn = g.GetVoiceChannel(Global.StatsTotChanID);
            if (chn == null)
                return;
            string msg = $"Total Users: {g.Users.Count}";
            Global.ConsoleLog($"{msg}!", ConsoleColor.Blue);

            if (chn.Name != msg)
            {
                await chn.ModifyAsync(x => x.Name = msg);
                Global.ConsoleLog($"Updated user count: {g.Users.Count}");
            }
        }
    }
}
