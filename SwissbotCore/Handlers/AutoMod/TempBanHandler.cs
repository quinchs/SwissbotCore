using Discord;
using Discord.WebSocket;
using SwissbotCore.Handlers.EventVC;
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
        private static Dictionary<ulong, bool> RoleRevokes = new Dictionary<ulong, bool>();
        private SocketRole bannedRole
            => client.GetGuild(Global.SwissGuildId).GetRole(783462878976016385);
        private SocketRole unverified 
            => client.GetGuild(Global.SwissGuildId).GetRole(627683033151176744);

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

            client.GuildMemberUpdated += Client_GuildMemberUpdated;

            Timer t = new Timer();

            t.Interval = 3000;

            t.Elapsed += T_Elapsed;

            t.Start();
        }

        private async Task Client_GuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
        {
            if(RoleRevokes.Any(x => x.Key == arg2.Id && !x.Value))
            {
                if(arg2.Roles.Count == 3 && arg2.Roles.Contains(bannedRole) && arg2.Roles.Contains(unverified))
                {
                    RoleRevokes[arg2.Id] = true;
                }
            }
        }

        private async void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool modified = false;

            foreach (var item in TempBans.ToArray())
            {
                if ((DateTime.UtcNow - item.Time).TotalMilliseconds > 0)
                {
                    SocketRole bannedRole = client.GetGuild(Global.SwissGuildId).GetRole(783462878976016385);
                    SocketRole memberRole = client.GetGuild(Global.SwissGuildId).GetRole(Global.MemberRoleID);
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
                    embed.AddField("Duration", (item.Time - DateTime.Parse(item.log.Date), true).ToString());
                    embed.AddField("Moderator", $"<@{item.log.ModeratorID}>", true);
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

        public static async Task<ulong[]> ClearAndAddTempbanRoles(ulong id)
        {
            var user = await Global.GetSwissbotUser(id);
            if (user == null)
                return null;

            ulong[] roles = null;

            RoleRevokes.Add(id, false);

            while (RoleRevokes.Any(x => x.Key == id && !x.Value))
            {
                user = await Global.GetSwissbotUser(id);
                roles = user.RoleIds.Where(x => x != Global.SwissGuildId && x != 627683033151176744 && x != 783462878976016385).ToArray();

                await SwissbotWorkerHandler.AssignTasks(WorkerTask.RemoveRoles, "remove", roles.Select(x => x).ToArray(), user.Id);

                if (!user.RoleIds.Any(x => x == 783462878976016385))
                    WorkerTaskCreator.CreateTask(WorkerTask.AddRoles, user.Id, "add", 783462878976016385);
                if (!user.RoleIds.Any(x => x == 627683033151176744))
                    WorkerTaskCreator.CreateTask(WorkerTask.AddRoles, user.Id, "add", 627683033151176744);

                await Task.Delay(1000);
            }

            RoleRevokes.Remove(id);

            return roles;
        }
        public static async Task RestoreTempbanRoles(ulong id)
        {
            var user = await Global.GetSwissbotUser(id);
            if (user == null)
                return;

            var tmban = TempBans.FirstOrDefault(x => x.UserId == id);

            if (tmban == null)
                return;

            await SwissbotWorkerHandler.AssignTasks(WorkerTask.AddRoles, "add", tmban.PreviousRoles, user.Id);

            WorkerTaskCreator.CreateTask(WorkerTask.RemoveRoles, user.Id, "remove", 783462878976016385);
            WorkerTaskCreator.CreateTask(WorkerTask.RemoveRoles, user.Id, "remove", 627683033151176744);
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
