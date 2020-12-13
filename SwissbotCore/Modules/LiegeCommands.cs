//using Discord;
//using Discord.Commands;
//using Discord.Rest;
//using Discord.WebSocket;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Discord.Net;
//using System.Runtime.InteropServices;
//using SwissbotCore;
//using System.Globalization;
//using System.ComponentModel;

//namespace Official_Bot.Modules
//{
//    public class Commands : CommandModuleBase
//    {
//        //Lock & Unlock Commands
//        [DiscordCommand("lock", RequiredPermission = true, description = "Locks the current channel or another channel", commandHelp = "`*lock` or `*lock <channel>`")]
//        public async Task LockChannel(params string[] args)
//        {
//            var memberRole = Context.Guild.GetRole(Global.MemberRoleID);
//            var user = Context.User as SocketGuildUser;

//            SocketTextChannel targetChannel = args.Length == 0 ? null : (SocketTextChannel)GetChannel(args[0]);

//            //if no args, locks the context channel
//            if (targetChannel == null)
//            {
//                targetChannel = Context.Channel as SocketTextChannel;

//                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
//                {
//                    Title = "Success",
//                    Description = $"{targetChannel.Mention} has been locked!",
//                    Color = Color.Green
//                }.WithCurrentTimestamp().Build());

//                await targetChannel.AddPermissionOverwriteAsync(memberRole,
//                    new OverwritePermissions(
//                        sendMessages: PermValue.Deny,
//                        viewChannel: PermValue.Allow,
//                        attachFiles: PermValue.Deny,
//                        embedLinks: PermValue.Deny
//                    ));

//                return;
//            }

//            await targetChannel.AddPermissionOverwriteAsync(memberRole, new OverwritePermissions(sendMessages: PermValue.Deny, viewChannel: PermValue.Allow, attachFiles: PermValue.Deny, embedLinks: PermValue.Deny));

//            //if tagetChannel is the context channel
//            if (targetChannel.Id == Context.Channel.Id)
//            {
//                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
//                {
//                    Title = $"{targetChannel.Name} has been Locked",
//                    Fields = new List<EmbedFieldBuilder>()
//                    {
//                        new EmbedFieldBuilder()
//                        {
//                            Name = "Moderator:",
//                            Value = Context.User.Mention
//                        }
//                    }
//                }.WithCurrentTimestamp().Build());
//            }

//            //if target channel is another channel
//            else
//            {
//                await targetChannel.SendMessageAsync("", false, new EmbedBuilder()
//                {
//                    Title = "This channel is now Locked",
//                    Description = $"This channel has been locked by {Context.User.Mention}"
//                }.WithCurrentTimestamp().Build());

//                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
//                {
//                    Title = $"{targetChannel.Name} has been Locked",
//                    Fields = new List<EmbedFieldBuilder>()
//                    {
//                        new EmbedFieldBuilder()
//                        {
//                            Name = "Moderator:",
//                            Value = Context.User.Mention
//                        }
//                    }
//                }.WithCurrentTimestamp().Build());
//            }
//        }
//        [DiscordCommand("unlock", RequiredPermission = true, description = "Unlocks the current channel or another channel", commandHelp = "`*unlock` or `*unlock <channel>`")]
//        public async Task UnLockChannel(params string[] args)
//        {
//            var memberRole = Context.Guild.GetRole(Global.MemberRoleID);
//            var user = Context.User as SocketGuildUser;
//            SocketTextChannel targetChannel = args.Length == 0 ? null : (SocketTextChannel)GetChannel(args[0]);

//            //if no args, unlocks the context channel
//            if (targetChannel == null)
//            {
//                targetChannel = Context.Channel as SocketTextChannel;

//                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
//                {
//                    Title = "Success",
//                    Description = $"{targetChannel.Mention} has been unlocked!",
//                    Color = Color.Green
//                }.WithCurrentTimestamp().Build());

//                await targetChannel.AddPermissionOverwriteAsync(memberRole,
//                    new OverwritePermissions(
//                        sendMessages: PermValue.Allow,
//                        viewChannel: PermValue.Allow,
//                        attachFiles: PermValue.Deny,
//                        embedLinks: PermValue.Deny
//                    ));

//                return;
//            }

//            await targetChannel.AddPermissionOverwriteAsync(memberRole, new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow, attachFiles: PermValue.Deny, embedLinks: PermValue.Deny));

//            //if tagetChannel is the context channel
//            if (targetChannel.Id == Context.Channel.Id)
//            {
//                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
//                {
//                    Title = $"{targetChannel.Name} has been Unocked",
//                    Fields = new List<EmbedFieldBuilder>()
//                    {
//                        new EmbedFieldBuilder()
//                        {
//                            Name = "Moderator:",
//                            Value = Context.User.Mention
//                        }
//                    }
//                }.WithCurrentTimestamp().Build());
//            }

//            //if target channel is another channel
//            else
//            {
//                await targetChannel.SendMessageAsync("", false, new EmbedBuilder()
//                {
//                    Title = "This channel is now Unocked",
//                    Description = $"This channel has been unlocked by {Context.User.Mention}"
//                }.WithCurrentTimestamp().Build());

//                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
//                {
//                    Title = $"{targetChannel.Name} has been unocked",
//                    Fields = new List<EmbedFieldBuilder>()
//                    {
//                        new EmbedFieldBuilder()
//                        {
//                            Name = "Moderator:",
//                            Value = Context.User.Mention
//                        }
//                    }
//                }.WithCurrentTimestamp().Build());
//            }


//        }
//        //--------------------------------------------------------------

//        //Temporarily Ban & Unban Commands
//        [DiscordCommand("tempban", RequiredPermission = true, commandHelp = "`*tempban <user> <time> <reason>`", description = "Temporarily ban a user from seeing channels")]
//        public async Task Tempban(params string[] args)
//        {
//            var unverified = Context.Guild.GetRole(627683033151176744);
//            var user = Context.User as SocketGuildUser;
//            var member = Context.Guild.GetRole(Global.MemberRoleID);
//            var banned = Context.Guild.GetRole(783462878976016385);


//            if (args.Length == 0) // *tempban
//            {
//                var error = new EmbedBuilder();
//                error.WithTitle("Error");
//                error.WithDescription("Who would you like to temporarily ban?");
//                error.WithColor(Color.Red);
//                await Context.Channel.SendMessageAsync("", false, error.Build());
//                return;
//            }

//            if (args.Length == 1) // *tempban Liege
//            {
//                var error = new EmbedBuilder();
//                error.WithTitle("Error");
//                error.WithDescription("How long do you want to ban this user for?");
//                error.WithColor(Color.Red);
//                await Context.Channel.SendMessageAsync("", false, error.Build());
//                return;
//            }

//            if (args.Length == 2) // *tempban Liege 2
//            {
//                var error = new EmbedBuilder();
//                error.WithTitle("Error");
//                error.WithDescription("Please provide a reason!");
//                error.WithColor(Color.Red);
//                await Context.Channel.SendMessageAsync("", false, error.Build());
//                return;
//            }

//            TimeSpan t;

//            // 7d
//            string[] formats = new string[] { @"h\h", @"s\s", @"m\m", @"d\d" };

//            if (!TimeSpan.TryParseExact(args[1], formats, CultureInfo.CurrentCulture, out t))
//            {
//                var error = new EmbedBuilder();
//                error.WithTitle("Error");
//                error.WithDescription("Please provide a valid duration!");
//                error.WithColor(Color.Red);
//                await Context.Channel.SendMessageAsync("", false, error.Build());
//                return;
//            }



//            string reason = string.Join(' ', args.Skip(2));
//            SocketGuildUser userAccount = GetUser(args[0]);
//            DateTime unbanTime = DateTime.UtcNow.Add(t);

//            if (userAccount.Roles.Contains(banned))
//            {
//                var error = new EmbedBuilder();
//                error.WithTitle("Error");
//                error.WithDescription("This user is already banned!");
//                error.WithColor(Color.Red);
//                await Context.Channel.SendMessageAsync("", false, error.Build());
//                return;
//            }

//            await userAccount.AddRoleAsync(banned);
//            await userAccount.AddRoleAsync(unverified);
//            await userAccount.RemoveRoleAsync(member);

//            var embed = new EmbedBuilder();
//            embed.WithTitle($"Successfully temporaily banned {userAccount} ({userAccount.Id})");
//            embed.WithDescription($"The user {userAccount.Mention} has been successfully **Temporaily Banned**");
//            embed.AddField("Moderator", $"{user}", true);
//            embed.AddField("Reason", reason, true);
//            await Context.Channel.SendMessageAsync("", false, embed.Build());

//            var dm = new EmbedBuilder();
//            dm.WithTitle("You have been Temporaily Banned from the Swiss001 Official Discord Server");
//            dm.AddField("Moderator", user.Username, true);
//            dm.AddField("Reason", reason, true);
//            await userAccount.SendMessageAsync("", false, dm.Build());



//        }
//        [DiscordCommand("tempunban", RequiredPermission = true, commandHelp = "`*untempban <user>`", description = "Removes a user from their temporary ban")]
//        public async Task Tempunban(params string[] args)
//        {
//            var unverified = Context.Guild.GetRole(627683033151176744);
//            var user = Context.User as SocketGuildUser;
//            var member = Context.Guild.GetRole(Global.MemberRoleID);
//            var banned = Context.Guild.GetRole(783462878976016385);
//            SocketGuildUser userAccount = GetUser(args[0]);

//            if (args.Length == 0) // *tempban
//            {
//                var error = new EmbedBuilder();
//                error.WithTitle("Error");
//                error.WithDescription("Who would you like to unban?");
//                error.WithColor(Color.Red);
//                await Context.Channel.SendMessageAsync("", false, error.Build());
//                return;
//            }

//            if (!userAccount.Roles.Contains(banned))
//            {
//                var error = new EmbedBuilder();
//                error.WithTitle("Error");
//                error.WithDescription("This user is already unbanned!");
//                error.WithColor(Color.Red);
//                await Context.Channel.SendMessageAsync("", false, error.Build());
//                return;
//            }

//            await userAccount.RemoveRoleAsync(banned);
//            await userAccount.RemoveRoleAsync(unverified);
//            await userAccount.AddRoleAsync(member);

//            var embed = new EmbedBuilder();
//            embed.WithTitle($"Successfully unbanned {userAccount} ({userAccount.Id})");
//            embed.WithDescription($"The user {userAccount.Mention} has been successfully **Unbanned**");
//            embed.AddField("Moderator", $"{user}", true);
//            await Context.Channel.SendMessageAsync("", false, embed.Build());

//            var dm = new EmbedBuilder();
//            dm.WithTitle("You have been Unbanned from the Swiss001 Official Discord Server");
//            dm.AddField("Moderator", user.Username, true);
//            dm.AddField("Reason", reason, true);
//            await userAccount.SendMessageAsync("", false, dm.Build());
//        }
//        //--------------------------------------------------------------

//        [DiscordCommand("unverify", RequiredPermission = true, description = "This command allows you to easily unverify a user. This is a pointless command :/", commandHelp = "`*unverify <user>`")]
//        public async Task Unverify(params string[] args)
//        {
//            var unverified = Context.Guild.GetRole(627683033151176744);
//            var user = Context.User as SocketGuildUser;
//            var memberRole = Context.Guild.GetRole(Global.MemberRoleID);

//            SocketGuildUser userAccount = args.Length == 0 ? null : GetUser(args[0]);

//            if (userAccount == null)
//            {
//                // Send an error message saying invalid user you dumb shit i swear to god i will eat your ass like taco bell, also return in here because you want to stop code execution
//                var embed = new EmbedBuilder();
//                embed.WithTitle("What? Who??");
//                embed.WithDescription("Please provide a valid user!");
//                embed.WithColor(Color.Red);
//                await Context.Channel.SendMessageAsync("", false, embed.Build());
//                return;
//            }

//            if (userAccount.Roles.Contains(unverified))
//            {
//                await Context.Channel.SendMessageAsync("uhm... the user is already unverified. *sigh*");
//                return;
//            }
//            await userAccount.AddRoleAsync(unverified);
//            await userAccount.RemoveRoleAsync(memberRole);

//            var success = new EmbedBuilder();
//            success.WithTitle($"Successfully unverified {userAccount} ({userAccount.Id})");
//            success.WithDescription($"The user {userAccount.Mention} has been successfully **Unverified**");
//            success.AddField("Moderator", $"{user}");
//            await Context.Channel.SendMessageAsync("", false, success.Build());
//            await userAccount.SendMessageAsync("You have been unverified on the Swiss001 server.");

//        }

//        [DiscordCommand("bean", RequiredPermission = false)]
//        public async Task FakeBan(params string[] args)
//        {
//            SocketGuildUser userAccount = args.Length == 0 ? null : GetUser(args[0]);

//            if (userAccount == null)
//            {
//                // Send an error message saying invalid user you dumb shit i swear to god i will eat your ass like taco bell, also return in here because you want to stop code execution
//                var embed = new EmbedBuilder();
//                embed.WithTitle("What? Who??");
//                embed.WithDescription("Who do you wanna bean??");
//                embed.WithColor(Color.Red);
//                await Context.Channel.SendMessageAsync("", false, embed.Build());
//                return;
//            }

//            if (args.Length == 1)
//            {
//                var embed = new EmbedBuilder();
//                embed.WithTitle($"Successfully beaned {userAccount} ({userAccount.Id})");
//                embed.WithDescription($"The user {userAccount.Mention} has been successfully **beaned!**");
//                embed.AddField("Moderator", $"{Context.User}");
//                embed.WithFooter("This is a fake ban and does not acctually do anything... unless :/");
//                await Context.Channel.SendMessageAsync("", false, embed.Build());
//            }

//            var success = new EmbedBuilder();
//            success.WithTitle($"Successfully beaned {userAccount} ({userAccount.Id})");
//            success.WithDescription($"The user {userAccount.Mention} has been successfully **beaned!**");
//            success.AddField("Moderator", $"{Context.User}", true);
//            success.AddField("Reason", reason, true);
//            success.WithFooter("This is a fake ban and does not acctually do anything... unless :/");
//            await Context.Channel.SendMessageAsync("", false, success.Build());

//        }

//        [DiscordCommand("helpme", RequiredPermission = false)]
//        public async Task HelpMe()
//        {
//            await Context.Channel.SendMessageAsync("Need help? Try dming <@622148936425275392>");
//        }

//        [DiscordCommand("roles", RequiredPermission = false)]
//        public async Task Roles()
//        {
//            var embed = new EmbedBuilder();
//            embed.WithTitle("Heard you wanted some custom roles!");
//            embed.AddField("Event/Youtube Notifications", "If you are interested in being notified about server events and Swiss001's youtube videos, you can click [here](https://discord.com/channels/592458779006730264/592466974488002570/717595157393834028).");
//            embed.AddField("Favorite Aircraft Roles", "If you have a favorite aircraft company, you can pick up their roles [here](https://discord.com/channels/592458779006730264/592466974488002570/610020838808485908).");
//            embed.AddField("More Roles", "There are also some roles that you can collect if you support Swiss001, are a content creator or even a developer, you can learn more [here](https://discord.com/channels/592458779006730264/592466974488002570/693558378710171650).");
//            embed.AddField("Region Roles", "If you want to collect the role that assosiates with your region, click [here](https://discord.com/channels/592458779006730264/592466974488002570/607266891572052000).");
//            embed.WithColor(Color.Green);
//            await Context.Channel.SendMessageAsync("", false, embed.Build());
//        }

//        [DiscordCommand("info", RequiredPermission = false)]
//        public async Task Info()
//        {
//            var embed = new EmbedBuilder();
//            embed.WithTitle("Heres some helpful tips and commands!");
//            embed.AddField("Submitting Suggestions", "`*suggest <suggestion>`: Submit a suggestion for people to vote and discuss on!");
//            embed.AddField("Roles", "`*roles`: Collect some roles!");
//            embed.AddField("Ask the staff", "`+ask-staff`: Ask staff some questions about themselves or about the server!");
//            embed.AddField("Getting help", "If you need some help, DM <@622148936425275392>!");
//            embed.AddField("Reddit", "`*reddit`: Get a random meme off the Swiss001 subreddit!");
//            embed.WithColor(Color.Green);
//            await Context.Channel.SendMessageAsync("", false, embed.Build());
//        }
//    }
//}