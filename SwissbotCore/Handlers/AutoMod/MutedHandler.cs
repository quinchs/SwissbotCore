using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SwissbotCore.Handlers
{
    [DiscordHandler]
    public class MutedHandler
    {
        public static Dictionary<ulong, DateTime> CurrentMuted = Global.LoadMuted();
        private Dictionary<ulong, Timer> MuteTimers = new Dictionary<ulong, Timer>();
        public static DiscordSocketClient client { get; set; }
        public MutedHandler(DiscordSocketClient c)
        {
            client = c;

            //load init
            Global.ConsoleLog($"Starting Muted init with {CurrentMuted.Count}", ConsoleColor.Blue);
            LoadMuted();
        }
        public void LoadMuted()
        {
            foreach (var mt in CurrentMuted)
            {
                //get dt
                var desttime = mt.Value - DateTime.UtcNow;
                if(desttime.TotalMilliseconds <= 0)
                {
                    //unmute
                    Unmute(mt.Key).GetAwaiter().GetResult();
                }
                else
                {
                    Timer t = new Timer()
                    {
                        AutoReset = false,
                        Interval = desttime.TotalMilliseconds
                    };
                    var usr = mt.Key;

                    t.Elapsed += (object sender, ElapsedEventArgs ag) =>
                    {
                        Unmute(usr).GetAwaiter().GetResult();
                        t.Dispose();
                    };
                    t.Start();

                }
            }
        }
        public static async Task Unmute(ulong usrID)
        { 
            CurrentMuted.Remove(usrID);
            Global.SaveMutedUsers();
            Global.ConsoleLog("Unmuted usr", ConsoleColor.Cyan);
            var usr = client.GetGuild(Global.SwissGuildId).GetUser(usrID);
            try
            {
                if (usr.Roles.Any(x => x.Id == Global.MutedRoleID))
                    await usr.RemoveRoleAsync(client.GetGuild(Global.SwissGuildId).GetRole(Global.MutedRoleID));
                else
                    return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }
            try
            {
                await usr.SendMessageAsync($"**You have been unmuted on {client.GetGuild(Global.SwissGuildId).Name}**");
            }
            catch { }
        }
        public static void AddNewMuted(ulong id, DateTime unmutetime)
        {
            var desttime = unmutetime - DateTime.UtcNow;
            CurrentMuted.Add(id, unmutetime);
            Global.ConsoleLog("Muted usr", ConsoleColor.Cyan);
            Global.SaveMutedUsers();
            if (desttime.TotalMilliseconds <= 0)
            {
                //unmute
                Unmute(id).GetAwaiter().GetResult();
            }
            else
            {
                Timer t = new Timer()
                {
                    AutoReset = false,
                    Interval = desttime.TotalMilliseconds
                };
                var usr = id;

                t.Elapsed += (object sender, ElapsedEventArgs ag) =>
                {
                    Unmute(usr).GetAwaiter().GetResult();
                    t.Dispose();
                };
                t.Start();
            }
        }
    }
}
