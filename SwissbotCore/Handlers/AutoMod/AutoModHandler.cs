using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SwissbotCore.Handlers
{
    //[DiscordHandler]
    class AutoModHandler
    {
        public DiscordSocketClient client { get; set; }
        public static List<SlowmodeChannel> CurrentSlowmodes = Global.LoadAutoSlowmode();
        public class SlowmodeChannel
        {
            public ulong ChanId { get; set; }
            public int messages { get; set; }
            public int currentslowmode { get; set; }
        }
        public AutoModHandler(DiscordSocketClient _client)
        {
            client = _client;

            client.MessageReceived += HandleAutoSlowmode;

            //
            new Timer() { AutoReset = true, Interval = 60000, Enabled = true }.Elapsed += (object sender, ElapsedEventArgs e) 
                => Global.SaveAutoSlowmode();
            new Timer() { AutoReset = true, Interval = 1000, Enabled = true }.Elapsed += async (object sender, ElapsedEventArgs e)
                  => await HandleClearSlowmodes();
        }
        public async Task HandleClearSlowmodes()
        {

        }
        private async Task HandleAutoSlowmode(SocketMessage arg)
        {
            if(CurrentSlowmodes.Any(x => x.ChanId == arg.Channel.Id))
            {
                var chan = client.GetGuild(Global.SwissGuildId).GetTextChannel(arg.Channel.Id);
                var chnl = CurrentSlowmodes.Find(x => x.ChanId == arg.Channel.Id);
                if (chan.SlowModeInterval != chnl.currentslowmode)
                    chnl.currentslowmode = chnl.currentslowmode;
                chnl.messages++;
                if(chnl.messages >= 7)
                {
                    HandleSlowmodeChange(chnl);
                }
            }
            else
            {
                if(arg.Channel.GetType() == typeof(SocketTextChannel))
                {
                    if(client.GetGuild(Global.SwissGuildId).TextChannels.Any(x => x.Id == arg.Channel.Id))
                    {
                        var chan = client.GetGuild(Global.SwissGuildId).GetTextChannel(arg.Channel.Id);
                        CurrentSlowmodes.Add(new SlowmodeChannel()
                        {
                            ChanId = arg.Channel.Id,
                            currentslowmode = chan.SlowModeInterval,
                            messages = 1
                        });
                    }
                }
            }
        }
        public async Task HandleSlowmodeChange(SlowmodeChannel SChan)
        {
            var chan = client.GetGuild(Global.SwissGuildId).GetTextChannel(SChan.ChanId);
            int newslowmode = 0;
            bool expl = false;
            if (SChan.currentslowmode > 0)
            {
                newslowmode = SChan.currentslowmode + 3;
                expl = true;
            }
            else
                newslowmode = 5;
            CurrentSlowmodes.Find(x => x.ChanId == SChan.ChanId).currentslowmode = newslowmode;
            await chan.ModifyAsync(x => x.SlowModeInterval = newslowmode);
            var t = new Timer() { AutoReset = true, Interval = 60000, Enabled = true };

            t.Elapsed += async (object sender, ElapsedEventArgs e) =>
            {
                if(expl)
                { 

                }
                else
                    await chan.ModifyAsync(x => x.SlowModeInterval = 0);

            };
        }
    }
}
