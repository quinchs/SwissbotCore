using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SwissbotCore.Global;
using SwissbotCore.Modules;
namespace SwissbotCore
{
    class GiveawayTimer
    {
        internal GiveAway currGiveaway { get; set; }
        internal GiveawayGuild gguild { get; set; }
        internal int Time = 0;
        internal System.Timers.Timer loopingTimer;

        internal Task StartTimer()
        {
            gguild.gt = this;
            loopingTimer = new System.Timers.Timer()
            {
                Interval = 15000,
                AutoReset = true,
                Enabled = true
            };
            loopingTimer.Elapsed += LoopingTimer_Elapsed;
            return Task.CompletedTask;
        }

        private async void LoopingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Time = Time - 15;
            if (Time <= 0)// giveawaytime
            {
                this.loopingTimer.Enabled = false;
                await gguild.AllowBans();
            }
            else
            {
                await gguild.UpdateTime(Time);
            }
        }
    }
}
