using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static SwissbotCore.Modules.ModDatabase;

namespace SwissbotCore.Handlers.AutoMod
{
    //[DiscordHandler]
    public class TempBanHandler
    {
        public record TempBan(ulong UserId, DateTime Time, UserModLogs log);
        public static List<TempBan> TempBans = new List<TempBan>();

        private DiscordSocketClient client;
        
        public TempBanHandler(DiscordSocketClient c)
        {
            client = c;

            // Load the records
            try
            {
                TempBans = SwissbotStateHandler.LoadObject<List<TempBan>>("TempBans.json").Result;
            }
            catch { }


            Timer t = new Timer();

            t.Interval = 3000;

            t.Elapsed += T_Elapsed;

            t.Start();
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var item in TempBans)
            {
                if (item.Time.Ticks <= DateTime.UtcNow.Ticks)
                {

                }        
            }
        }

        private static void SaveTempBans()
        {

        }

        public void AddTempBan(SocketGuildUser target, DateTime unbanTime, UserModLogs log)
        {
            TempBan tmban = new TempBan(target.Id, unbanTime, log);


        }
    }
}
