using Discord;
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
    [DiscordHandler]
    public class TempBanHandler
    {
        public record TempBan(ulong UserId, DateTime Time, UserModLogs log, ulong[] PreviousRoles);
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

        private async void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool modified = false;

            foreach (var item in TempBans.ToArray())
            {
                if ((DateTime.UtcNow - item.Time).TotalMilliseconds > 0)
                {
                    var bannedRole = client.GetGuild(Global.SwissGuildId).GetRole(783462878976016385);
                    var memberRole = client.GetGuild(Global.SwissGuildId).GetRole(Global.MemberRoleID);
                    var unverifiedRole = client.GetGuild(Global.SwissGuildId).GetRole(Global.UnverifiedRoleID);

                    var userAccount = await Global.GetSwissbotUser(item.UserId);

                    if (userAccount == null)
                    {
                        TempBans.Remove(item);
                        modified = true;
                        continue;
                    }

                    await userAccount.RemoveRolesAsync(new IRole[] { bannedRole, unverifiedRole });

                    List<IRole> roles = new List<IRole>();

                    foreach (var role in item.PreviousRoles)
                        roles.Add(Global.SwissGuild.GetRole(role));

                    await userAccount.AddRolesAsync(roles.Where(x => x != null));

                    TempBans.Remove(item);
                    modified = true;

                    var embed = new EmbedBuilder();
                    embed.WithTitle("Your temporary ban has expired on the Swiss001 Official Discord Server!");
                    embed.WithDescription("You may now access the server normally.");
                    embed.AddField("Reason", item.log.Reason, true);
                    embed.AddField("Duration", (item.Time - DateTime.Parse(item.log.Date)).ToString());
                    embed.AddField("Moderator", $"<@{item.log.ModeratorID}>");
                    embed.WithCurrentTimestamp();

                    try { await userAccount.SendMessageAsync("", false, embed.Build()); } catch { }
                }
            }

            if(modified)
                SaveTempBans();

        }

        private static void SaveTempBans()
        {
            SwissbotStateHandler.SaveObject("TempBans.json", TempBans);
        }

        public async void RemoveTempBan(ulong id)
        {
            var tmban = TempBans.FirstOrDefault(x => x.UserId == id);

            if (tmban == null)
                return;

            var u = await Global.GetSwissbotUser(id);

            if (u != null)
            {
                List<IRole> roles = new List<IRole>();

                foreach (var role in tmban.PreviousRoles)
                    roles.Add(Global.SwissGuild.GetRole(role));

                await u.AddRolesAsync(roles);
            }    

            TempBans.Remove(tmban);
            SaveTempBans();
        }

        public void RemoveTempBan(SocketGuildUser target)
            => RemoveTempBan(target.Id);

        public void AddTempBan(SocketGuildUser target, DateTime unbanTime, UserModLogs log, ulong[] roles)
        {
            TempBan tmban = new TempBan(target.Id, unbanTime, log, roles);
            TempBans.Add(tmban);
            SaveTempBans();
        }
    }
}
