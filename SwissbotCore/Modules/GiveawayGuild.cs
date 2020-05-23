using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SwissbotCore.Global;

namespace SwissbotCore.Modules
{
    class GiveawayGuild
    {
        internal DiscordSocketClient _client = Global.Client;
        internal RestVoiceChannel chantimer;
        internal Global.GiveAway currgiveaway;

        internal GiveawayTimer gt;
        internal string inviteURL;

        internal async Task createguild(GiveAway currGiveaway)
        {
            //try
            //{
            //    var newguild = await _client.CreateGuildAsync($"{currGiveaway.GiveAwayItem} Giveaway", _client.VoiceRegions.FirstOrDefault(n => n.Name == "US East"));
            //    GiveawayGuildObj g = new GiveawayGuildObj();
            //    g.create(newguild);
            //    currgiveaway = currGiveaway;
            //    currgiveaway.giveawayguild = g;
            //    GuildPermissions adminguildperms = new GuildPermissions(true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true);
            //    GuildPermissions Contestantperms = new GuildPermissions(false, false, false, false, false, false, true, false, true, true, false, false, true, true, true, false, true, true, true, false, false, false, true, false, true, false, false, false, false);

            //    await newguild.CreateRoleAsync("Admins", adminguildperms, Color.Red, true);
            //    await newguild.CreateRoleAsync("Contestants", Contestantperms, Color.Blue, false);

            //    var chanContestants = await newguild.CreateTextChannelAsync("Contestants", x => x.Topic = "talk in here till bans are unleashed >:)");
            //    var chanInfo = await newguild.CreateTextChannelAsync("Info", x => x.Topic = "Rules and info");
            //    var chanCount = await newguild.CreateVoiceChannelAsync("Time: xxx");
            //    chantimer = chanCount;

            //    EmbedBuilder eb = new EmbedBuilder();
            //    eb.Title = "***INFO***";
            //    eb.Color = Color.Gold;
            //    eb.Description = $"Welcome to the giveaway guild! the prize for this giveaway is {currGiveaway.GiveAwayItem}!\n\n **How to play** once the timer reaches 0 everyone with the `Contesters` role will be givin access to the \"ban command, its a FFA to the death! the last player(s) remaining will get the prize! this is a fun interactive competative giveaway where users can decide who wins!";
            //    eb.Footer = new EmbedFooterBuilder();

            //    var username = CommandHandler._client.GetGuild(SwissGuildId).Users.FirstOrDefault(x => x.Id == currgiveaway.GiveAwayUser);
            //    eb.Footer.Text = $"Giveaway by {username.ToString()}";
            //    eb.Footer.IconUrl = _client.GetGuild(Global.SwissGuildId).GetUser(currGiveaway.GiveAwayUser).GetAvatarUrl();
            //    await chanInfo.SendMessageAsync("", false, eb.Build());

            //    OverwritePermissions adminperms = new OverwritePermissions(PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Allow);
            //    await chanInfo.AddPermissionOverwriteAsync(newguild.Roles.FirstOrDefault(r => r.Name == "Admins"), adminperms);
            //    OverwritePermissions contesterperms = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny);
            //    await chanInfo.AddPermissionOverwriteAsync(newguild.Roles.FirstOrDefault(r => r.Name == "Contestants"), contesterperms);
            //    var url = chanInfo.CreateInviteAsync(null, null, false, false);
            //    _client.UserJoined += userjoinGiveaway;
            //    inviteURL = url.Result.Url;
            //}
            //catch (Exception ex)
            //{

            //}
        }

        private async Task userjoinGiveaway(SocketGuildUser arg)
        {
            if (arg.Guild.Id == currgiveaway.giveawayguild.guildID)
            {
                var role = _client.GetGuild(currgiveaway.giveawayguild.guildID).Roles.FirstOrDefault(r1 => r1.Name == "Admins");
                var role2 = _client.GetGuild(currgiveaway.giveawayguild.guildID).Roles.FirstOrDefault(r2 => r2.Name == "Contestants");
                var r = _client.GetGuild(SwissGuildId).GetUser(arg.Id).Roles;
                var adminrolepos = _client.GetGuild(SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.DeveloperRoleId).Position;
                var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
                if (rolepos != null)
                {
                    await arg.AddRoleAsync(role);
                }
                else
                {
                    await arg.AddRoleAsync(role2);
                    GiveawayUser u = new GiveawayUser()
                    {
                        id = arg.Id,
                        user = arg,
                        bannedUsers = new List<GiveawayUser>(),
                        bans = 0,
                        DiscordName = arg.ToString()
                    };
                    currgiveaway.giveawayguild.giveawayEntryMembers.Add(u);
                }
            }
        }

        internal async Task UpdateTime(int seconds)
        {
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            string timefromsec = "";
            if (ts.Days != 0)
                timefromsec += $"{ts.Days} Days, ";
            if (ts.Hours != 0)
                timefromsec += $"{ts.Hours} Hours, ";
            if (ts.Minutes != 0)
                timefromsec += $"{ts.Minutes} Minutes";
            if (ts.Seconds != 0)
                if (ts.Minutes != 0)
                    timefromsec += $", and {ts.Seconds} Seconds";
                else
                    timefromsec += $"{ts.Seconds} Seconds";

            await chantimer.ModifyAsync(x => x.Name = $"Time: {timefromsec}");
            EmbedBuilder eb = new EmbedBuilder();
            eb.Title = "GIVEAWAY";
            eb.Color = Color.Blue;
            eb.Description = $"{_client.GetGuild(SwissGuildId).GetUser(currgiveaway.GiveAwayUser).Mention} Has started a giveaway for **{currgiveaway.GiveAwayItem}** with {currgiveaway.numWinners} winners, to enter the giveaway join {currgiveaway.discordInvite}\n\n **How does it work?** \n after the timer reaches 0 everyone will get access to the \"ban command, its like a FFA. the last person(s) remaining will get the giveaway item \n \n ***GIVEAWAY STARTS IN {timefromsec} ({seconds} seconds)***";
            await currgiveaway.giveawaymsg.ModifyAsync(x => x.Embed = eb.Build());
        }
        internal async Task AllowBans()
        {
            currgiveaway.giveawayguild.bansActive = true;
            var guild = _client.GetGuild(currgiveaway.giveawayguild.guildID);
            ulong id = guild.TextChannels.FirstOrDefault(x => x.Name == "contestants").Id;
            await guild.GetTextChannel(id).SendMessageAsync($"@everyone BANS ARE NOW ACTIVE!! Use `{Global.Preflix}ban <@user>` to ban people! you cannot ban admins so dont try");
        }
    }
}
