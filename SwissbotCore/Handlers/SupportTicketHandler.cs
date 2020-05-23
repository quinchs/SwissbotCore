using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using SwissbotCore.Modules;
using System.Net;
using System.Timers;

namespace SwissbotCore.Handlers
{
    public class SupportTicketHandler
    {
        public DiscordSocketClient client { get; set; }
        Dictionary<ulong, KeyValuePair<ulong, string>> Setups { get; set; }
        static Dictionary<KeyValuePair<ulong, ulong>, Timer> closingState { get; set; }
        public static List<SupportTicket> CurrentTickets { get; set; }
        public static List<ulong> BlockedUsers = Global.LoadBlockedUsers();
        public static Dictionary<string, string> Snippets = Global.LoadSnippets();

        public class SupportTicket
        {
            public ulong UserID { get; set; }
            public ulong DMChannelID { get; set; }
            public ulong TicketChannel { get; set; }
            public TypingState DmTyping { get; set; }
            public TypingState TicketTyping { get; set; }
            public class TypingState
            {
                public bool Typing { get; set; }
                public IDisposable TypingObject { get; set; }
            }
        }

        public SupportTicketHandler(DiscordSocketClient client)
        {
            this.client = client;

            Setups = new Dictionary<ulong, KeyValuePair<ulong, string>>();

            CurrentTickets = Global.ReadSupportTickets();

            closingState = new Dictionary<KeyValuePair<ulong, ulong>, Timer>();

            client.MessageReceived += CheckNewThread;

            client.ReactionAdded += CheckInitThread;

            client.UserIsTyping += CheckTicketTyping;
        }

        private async Task CheckTicketTyping(SocketUser arg1, ISocketMessageChannel arg2)
        {
            if(CurrentTickets.Any(x => x.DMChannelID == arg2.Id))
            {
                var ticket = CurrentTickets.Find(x => x.DMChannelID == arg2.Id);

                var tchan = client.GetGuild(Global.SwissGuildId).GetTextChannel(ticket.TicketChannel);
                if (ticket.DmTyping.Typing)
                {
                    ticket.DmTyping.TypingObject.Dispose();
                    ticket.DmTyping.Typing = false;
                }
                ticket.DmTyping.Typing = true;
                ticket.DmTyping.TypingObject = tchan.EnterTypingState();
            }
            //else if(CurrentTickets.Any(x => x.TicketChannel == arg2.Id))
            //{
            //    var ticket = CurrentTickets.Find(x => x.TicketChannel == arg2.Id);
            //    var tchan = await client.GetDMChannelAsync(ticket.DMChannelID);
            //    if (ticket.TicketTyping.Typing)
            //    {
            //        ticket.TicketTyping.TypingObject.Dispose();
            //        ticket.TicketTyping.Typing = false;
            //    }
            //    var tp = tchan.EnterTypingState();
            //    ticket.TicketTyping.Typing = true;
            //    ticket.TicketTyping.TypingObject = tp;
            
        }

        private async Task CheckInitThread(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var usr = await arg2.GetUserAsync(arg3.UserId);
            if (usr.IsBot)
                return;

            if (Setups.ContainsKey(arg2.Id))
            {
                var msg = await arg2.GetMessageAsync(Setups[arg2.Id].Key);

                //checkmark
                if (arg3.Emote.Equals(new Emoji("✅")))
                {
                    if(client.GetGuild(Global.SwissGuildId).Users.Any(x => x.Id == usr.Id))
                    {
                        if(BlockedUsers.Any(x => x == usr.Id))
                        {
                            await arg2.SendMessageAsync("", false, new EmbedBuilder()
                            {
                                Title = "You are blocked!",
                                Description = "Looks like your blocked from creating support tickets :/",
                                Color = Color.Red
                            }.Build());
                            Setups.Remove(arg2.Id);
                            await msg.DeleteAsync();
                            return;
                        }
                        else
                        {
                            await msg.DeleteAsync();
                            var tmpmsg = await arg2.SendMessageAsync("**Creating support ticket with the staff team...**");
                            await CreateNewTicket(arg2, usr, Setups[arg2.Id].Value);
                            await tmpmsg.ModifyAsync(x => x.Embed = new EmbedBuilder()
                            {
                                Title = "Congrats! You are now talking with staff!",
                                Description = "Please note staff **Can** take moderation action for anything you say in a ticket. The same rules apply as the server.\n_This ticket may also be archived and used for training purposes._",
                                Color = Color.Green
                            }.Build());
                        }
                    }
                    else
                    {
                        await usr.SendMessageAsync("", false, new EmbedBuilder() 
                        {
                            Title = "Sorry... :(",
                            Description = "The staff team does not accept tickets from users who are not in the server.",
                            Color = Color.Red
                        }.Build());
                    }

                }
                else if(arg3.Emote.Equals(new Emoji("❌"))) // xmark
                {
                    Setups.Remove(arg2.Id);
                    await msg.DeleteAsync();
                } 
            }
            else if (closingState.Keys.Any(x => x.Key == arg2.Id))
            {
                var o = closingState.First(x => x.Key.Key == arg2.Id);
                var msg = await arg2.GetMessageAsync(o.Key.Value);

                if (arg3.Emote.Equals(new Emoji("❌")))
                {
                    o.Value.Stop();
                    o.Value.Dispose();
                    closingState.Remove(o.Key);
                    await msg.DeleteAsync();
                }
            }
        }

        public async Task CreateNewTicket(ISocketMessageChannel chan, IUser user, string omsg)
        {
            var ticket = new SupportTicket()
            {
                DMChannelID = chan.Id,
                UserID = user.Id,
                DmTyping = new SupportTicket.TypingState() { Typing = false },
                TicketTyping = new SupportTicket.TypingState() { Typing = false }
            };
            var guilduser = client.GetGuild(Global.SwissGuildId).GetUser(user.Id);
            //create new ticket channel
            var ticketchan = await client.GetGuild(Global.SwissGuildId).CreateTextChannelAsync($"{user.Username}-{user.Discriminator}", x => x.CategoryId = Global.TicketCategoryID);
            var logs = ModDatabase.currentLogs.Users.Any(x => x.userId == user.Id) ? ModDatabase.currentLogs.Users.First(x => x.userId == user.Id) : null;

            var embed = new EmbedBuilder()
            {
                Title = $"**New support ticket from {user}**",
                Description = $"To view ticket snippets, do `!snippets` or `*snippets`",
                Author = new EmbedAuthorBuilder() { Name = user.ToString(), IconUrl = user.GetAvatarUrl() },
                Fields = new List<EmbedFieldBuilder>()
                {
                    { new EmbedFieldBuilder()
                    {
                        Name = "User",
                        Value = $"Name: {user}\nNickname: {guilduser.Nickname}\nID: {guilduser.Id}\nMention: {guilduser.Mention}\nCreated at: {guilduser.CreatedAt.UtcDateTime.ToString("f")} UTC\nJoined at: {guilduser.JoinedAt.Value.ToString("f")} UTC",
                    } },
                    { new EmbedFieldBuilder()
                    {
                        Name = "Roles",
                        Value = 
                        string.Join("\n", guilduser.Roles.OrderBy(x => x.Position).Select(x => x.Mention)).Length > 1024 
                        ? $"Unable to display all roles, listing top ten:\n{string.Join("\n", guilduser.Roles.OrderBy(x => x.Position).Select(x => x.Mention).Take(10))}" 
                        : string.Join("\n", guilduser.Roles.Select(x => x.Mention))
                    } },

                },
                Color = Color.DarkPurple
            };
            if(logs == null)
            {
                embed.AddField("Modlogs", "None");
            }
            else
            {
                string mlogs = "None <3";
                foreach(var mlog in logs.Logs.OrderBy(x => x.Date).Reverse())
                {
                    if (mlogs == "None <3")
                        mlogs = "";
                    mlogs += $"**{mlog.Action.ToString()}** on **{mlog.Date}**\n**Reason:** {mlog.Reason}\n**Moderator:** <@{mlog.ModeratorID}>\n\n";
                }
                embed.AddField("Modlogs", mlogs.Length > 1024 ? $"Modlogs too long, listing top 5\n\n{string.Join("\n\n", mlogs.Split("\n\n").Take(5))}" : string.Join("\n\n", mlogs.Split("\n\n")));
            }

            await ticketchan.SendMessageAsync("@here", false, embed.Build());
            await ticketchan.SendMessageAsync($"**\nV----------------START-OF-TICKET----------------V**\n\n**[Ticketer] {user}** - " + omsg);

            ticket.TicketChannel = ticketchan.Id;
            CurrentTickets.Add(ticket);
            Global.SaveSupportTickets();
        }   

        private async Task CheckNewThread(SocketMessage arg)
        {
            if (arg.Author.IsBot)
                return;
            if(arg.Channel.GetType() == typeof(SocketDMChannel))
            {
                if (BlockedUsers.Any(x => x == arg.Author.Id))
                {
                    return;
                }
                if (CurrentTickets.Any(x => x.DMChannelID == arg.Channel.Id))
                {
                    var ticket = CurrentTickets.Find(x => x.DMChannelID == arg.Channel.Id);
                    ticket.DmTyping.Typing = false;
                    if (ticket.DmTyping.TypingObject != null)
                        ticket.DmTyping.TypingObject.Dispose();
                    string msg = $"**[Ticketer] {arg.Author}** - {arg.Content}";
                    if (arg.Attachments.Count > 0)
                    {
                        foreach (var attc in arg.Attachments)
                        {
                            msg += $"\n**Attachment** - {attc.Url}";
                        }
                    }
                    await client.GetGuild(Global.SwissGuildId).GetTextChannel(ticket.TicketChannel).SendMessageAsync(msg);
            
                }
                else if (!Setups.ContainsKey(arg.Author.Id))
                {
                    var msg = await arg.Channel.SendMessageAsync("Hello " + arg.Author.Mention + ", if you want to open a support ticket please click the checkmark, otherwise to delete this message click the X");
                    await msg.AddReactionsAsync(new IEmote[] { new Emoji("✅"), new Emoji("❌") });
                    Setups.Add(arg.Channel.Id, new KeyValuePair<ulong, string>(msg.Id, arg.Content));
                }
                
            }
            else if(CurrentTickets.Any(x => x.TicketChannel == arg.Channel.Id))
            {
                //check snippets
                await SnippetHandler(arg);
            }
        }

        public async Task SnippetHandler(SocketMessage arg)
        {
            if (!CurrentTickets.Any(x => x.TicketChannel == arg.Channel.Id))
                return;
            var ticket = CurrentTickets.Find(x => x.TicketChannel == arg.Channel.Id);
            if (!arg.Content.StartsWith('!') && !arg.Content.Contains(' '))
                return;
            if (arg.Content == "!snippets")
                return;
            
            string snipname = arg.Content;
           
            if (Snippets.ContainsKey(snipname))
            {
                var dmchan = await client.GetUser(ticket.UserID).GetOrCreateDMChannelAsync();
                string snipval = Snippets[snipname];
                await arg.DeleteAsync();
                await dmchan.SendMessageAsync($"**[Staff] {arg.Author.ToString()}** - {snipval}");
                await arg.Channel.SendMessageAsync($"**[Staff] {arg.Author.ToString()}** - {snipval}");
            }
            else
            {
                //await arg.Channel.SendMessageAsync("", false, new EmbedBuilder()
                //{
                //    Title = "Invalid snippet!",
                //    Description = $"Looks like the snippet \"{arg.Content}\" Doesn't exist! For a list of snippets, do `!snippets`",
                //    Color = Color.Red
                //}.Build());
            }
        }
        [DiscordCommandClass()]
        public class SupportCommandModule : CommandModuleBase
        {
            [DiscordCommand("r",
            description = "Respond to a thread; *only works in thread channels*",
            BotCanExecute = false,
            commandHelp = "Parameters - `(PREFIX)r <msg>`",
            prefixes = new char[] {'!', '*'},
            RequiredPermission = true
            )]
            public async Task Respond(params string[] args)
            {
                if (!HasExecutePermission)
                    return;
                if(CurrentTickets.Any(x => x.TicketChannel == Context.Channel.Id))
                {
                    var ticket = CurrentTickets.Find(x => x.TicketChannel == Context.Channel.Id);
                    var dmchan = await Context.Client.GetUser(ticket.UserID).GetOrCreateDMChannelAsync();
                    await Context.Message.DeleteAsync();
                    var usr = Context.Guild.GetUser(Context.Message.Author.Id);
                    string msg = $"**[Staff] {(usr.Nickname == null ? usr.ToString() : usr.Nickname)}** - {string.Join(" ", args)}";
                    if (Context.Message.Attachments.Count > 0)
                    {
                        foreach (var attc in Context.Message.Attachments)
                        {
                            msg += $"\n**Attachment** - {attc.Url}";
                        }
                    }
                    await dmchan.SendMessageAsync(msg);
                    await Context.Channel.SendMessageAsync(msg);
                }
            }
            [DiscordCommand("ar",
            description = "Respond to a thread *anonymously*; *only works in thread channels*",
            BotCanExecute = false,
            commandHelp = "Parameters - `(PREFIX)ar <msg>`",
            prefixes = new char[] { '!', '*' },
            RequiredPermission = true
            )]
            public async Task anonRespond(params string[] args)
            {
                if (!HasExecutePermission)
                    return;
                if (CurrentTickets.Any(x => x.TicketChannel == Context.Channel.Id))
                {
                    var ticket = CurrentTickets.Find(x => x.TicketChannel == Context.Channel.Id);
                    var dmchan = await Context.Client.GetUser(ticket.UserID).GetOrCreateDMChannelAsync();
                    await Context.Message.DeleteAsync();
                    string msg = $"**Staff** - {string.Join(" ", args)}";
                    if (Context.Message.Attachments.Count > 0)
                    {
                        foreach (var attc in Context.Message.Attachments)
                        {
                            msg += $"\n**Attachment** - {attc.Url}";
                        }
                    }
                    await dmchan.SendMessageAsync(msg);
                    await Context.Channel.SendMessageAsync(msg);
                }
            }
            [DiscordCommand("close",
            description = "Closes a thread; *only works in thread channels*",
            BotCanExecute = false,
            commandHelp = "Parameters - `(PREFIX)close`",
            prefixes = new char[] { '!', '*' },
            RequiredPermission = true
            )]
            public async Task Close()
            {
                if (!HasExecutePermission)
                    return;
                if(CurrentTickets.Any(x => x.TicketChannel == Context.Channel.Id))
                {
                    var ticket = CurrentTickets.Find(x => x.TicketChannel == Context.Channel.Id);

                    if (closingState.Keys.Any(x => x.Key == Context.Channel.Id))
                        return;
                    
                    var cmsg = await Context.Channel.SendMessageAsync("This thread will be closed in 5 seconds. If you wish to cancel this, please click the X mark.");

                    await cmsg.AddReactionAsync(new Emoji("❌"));
                    var t = new Timer() { AutoReset = false, Interval = 5000, Enabled = true };
                    t.Elapsed += async (object s, ElapsedEventArgs a) =>
                    {
                        var chan = Context.Channel as SocketTextChannel;
                        await chan.DeleteAsync(new RequestOptions() { AuditLogReason = "Ticket Closed" });
                        var dmchan = await Context.Client.GetUser(ticket.UserID).GetOrCreateDMChannelAsync();
                        await dmchan.SendMessageAsync("Your ticket with the staff team is now closed. If you wish to open another ticket, please send a message.");
                        CurrentTickets.Remove(ticket);
                        Global.SaveSupportTickets();
                    };
                    closingState.Add(new KeyValuePair<ulong, ulong>(Context.Channel.Id, cmsg.Id), t);
                    
                }
            }
            [DiscordCommand("snippets",
            description = "View the ticket snippets *only works in thread channels*",
            BotCanExecute = false,
            commandHelp = "Parameters:\n`(PREFIX)snippets`,\n`(PREFIX)snippets add <SnippetName> <SnippetValue>`\n`(PREFIX)snippets remove <SnippetName>`",
            prefixes = new char[] { '!', '*' },
            RequiredPermission = true
            )]
            public async Task snippets(params string[] args)
            {
                if (!HasExecutePermission)
                    return;
                if (args.Length == 0)
                {
                    EmbedBuilder b = new EmbedBuilder()
                    {
                        Title = "Ticket Snippets",
                        Color = Color.DarkPurple,
                        Description = "To add a snippet:\n`!snippets add <SnippetName> <SnippetValue>`\nTo Remove a snippet:\n`!snippets remove <SnippetName>`"
                    };
                    foreach(var snip in Snippets)
                        b.AddField(snip.Key, snip.Value);
                    await Context.Channel.SendMessageAsync("", false, b.Build());
                }
                else
                {
                    if(args.Length >= 2)
                    {
                        switch (args[0].ToLower())
                        {
                            case "add":
                                {
                                    string snip = args[1];
                                    string snipval = string.Join(' ', args.Skip(2));
                                    if (!Snippets.ContainsKey(snip))
                                    {
                                        if(snipval.Length > 1024)
                                        {
                                            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                            {
                                                Title = $"wOaH bUddY",
                                                Description = $"Your snippet is tooooooo long! try adding one that less than 1000 characters"
                                            }.Build());
                                            return;
                                        }
                                        Snippets.Add(snip, snipval);
                                        Global.SaveSnippets();
                                        await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                        {
                                            Title = $"Added **{snip}**",
                                            Description = $"Succesfully added **{snip}** to the snippets"
                                        }.Build());
                                    }
                                    else
                                    {
                                        await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                        {
                                            Title = $"That snippet exists!",
                                            Description = $"someone already added that snippet!"
                                        }.Build());
                                    }
                                }
                                break;
                            case "remove":
                                {
                                    string snip = args[1];
                                    if (Snippets.ContainsKey(snip))
                                    {
                                        //remove snippet
                                        Snippets.Remove(snip);
                                        Global.SaveSnippets();
                                        await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                        {
                                            Title = $"Removed **{snip}**",
                                            Description = $"Successfully removed the snippet {snip}",
                                            Color = Color.Green
                                        }.Build());
                                    }
                                    else
                                    {
                                        await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                        {
                                            Title = "uhm.. wat?",
                                            Description = "That snippet doesnt exist, please do `!snippets` to view the current snippets!",
                                            Color = Color.Red
                                        }.Build());
                                        return;
                                    }
                                }
                                break;
                            default:
                                {
                                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                    {
                                        Title = "uhm.. wat?",
                                        Description = "that command is unreconized :/",
                                        Color = Color.Red
                                    }.Build());
                                }
                                break;
                        }
                    }
                }
            }
            [DiscordCommand("block",
            description = "Blocks a user from creating threads",
            BotCanExecute = false,
            commandHelp = "Parameters:\n`(PREFIX)block <user>`",
            prefixes = new char[] { '!', '*' },
            RequiredPermission = true
            )]
            public async Task blockuser(string user)
            {
                if (!HasExecutePermission)
                    return;
                if (Context.Message.MentionedUsers.Count == 0)
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "who tf is that xD",
                        Description = "you didnt mention a valid user :/",
                        Color = Color.Red
                    }.Build());
                    return;
                }

                if (!BlockedUsers.Contains(Context.Message.MentionedUsers.First().Id))
                {
                    BlockedUsers.Add(Context.Message.MentionedUsers.First().Id);
                    Global.SaveBlockedUsers();
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = $"Succes",
                        Description = $"{Context.Message.MentionedUsers.First()} cant make threads no more lmao <3",
                        Color = Color.Green
                    }.Build());
                    return;
                }
                else
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "***wait'nt***",
                        Description = "looks like there blocked already :/",
                        Color = Color.Green
                    }.Build());
                }
            }
            [DiscordCommand("unblock",
            description = "unblocks a user from creating threads",
            BotCanExecute = false,
            commandHelp = "Parameters:\n`(PREFIX)unblock <user>`",
            prefixes = new char[] { '!', '*' },
            RequiredPermission = true
            )]
            public async Task unblock(string user)
            {
                if (!HasExecutePermission)
                    return;
                if (Context.Message.MentionedUsers.Count == 0)
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "who tf is that xD",
                        Description = "you didnt mention a valid user :/",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                if (BlockedUsers.Contains(Context.Message.MentionedUsers.First().Id))
                {
                    BlockedUsers.Remove(Context.Message.MentionedUsers.First().Id);
                    Global.SaveBlockedUsers();
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = $"Succes!",
                        Description = $"{Context.Message.MentionedUsers.First()} can make threads again",
                        Color = Color.Green
                    }.Build());
                    return;
                }
                else
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "***wait'nt***",
                        Description = "looks like that user isnt blocked :/",
                        Color = Color.Green
                    }.Build());
                }

            }
        }
    }
}
