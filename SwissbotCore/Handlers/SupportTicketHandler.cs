using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using SwissbotCore.Modules;
using System.Net;
using System.Timers;
using Discord.Net;
using System.IO;
using Newtonsoft.Json;
using SwissbotCore.HTTP.Websocket;
using Discord.Rest;

namespace SwissbotCore.Handlers
{
    [DiscordHandler]
    public class SupportTicketHandler
    {
        public DiscordSocketClient client { get; set; }
        static Dictionary<KeyValuePair<ulong, ulong>, Timer> closingState { get; set; }
        public static List<SupportTicket> CurrentTickets { get; set; }
        public static List<ulong> WelcomeMessages { get; set; } = new List<ulong>();

        public static List<ulong> BlockedUsers = Global.LoadBlockedUsers();
        public static Dictionary<string, string> Snippets = Global.LoadSnippets();
        public class SupportTicket
        {
            public ulong UserID { get; set; }
            public ulong DMChannelID { get; set; }
            public ulong TicketChannel { get; set; }
            public TypingState DMTyping { get; set; }
            public bool Welcomed { get; set; } = false;

            public TicketTranscript Transcript;
        }
        public class TypingState
        {
            public bool Typing { get; set; }
            public IDisposable TypingObject { get; set; }
        }
        public SupportTicketHandler(DiscordSocketClient client)
        {
            this.client = client;

            CurrentTickets = Global.ReadSupportTickets();

            closingState = new Dictionary<KeyValuePair<ulong, ulong>, Timer>();

            client.MessageReceived += CheckNewThread;

            client.ReactionAdded += CheckInitThread;

            client.UserIsTyping += CheckTicketTyping;

            client.ChannelDestroyed += CheckSupportChannel;
        }

        private async Task CheckSupportChannel(SocketChannel arg)
        {
            if(CurrentTickets.Any(x => x.TicketChannel == arg.Id))
            {
                var ticket = CurrentTickets.Find(x => x.TicketChannel == arg.Id);
                var usr = client.GetGuild(Global.SwissGuildId).GetUser(ticket.UserID);
                CurrentTickets.Remove(ticket);
                if(usr != null)
                {
                    try
                    {
                        await usr.SendMessageAsync("Your ticket with the staff team is now closed. If you wish to open another ticket, please send a message.");
                    }
                    catch { };
                }
            }
        }

        private async Task CheckTicketTyping(SocketUser arg1, ISocketMessageChannel arg2)
        {
            if(CurrentTickets.Any(x => x.DMChannelID == arg2.Id))
            {
                var ticket = CurrentTickets.Find(x => x.DMChannelID == arg2.Id);

                var tchan = client.GetGuild(Global.SwissGuildId).GetTextChannel(ticket.TicketChannel);
                if (ticket.DMTyping.Typing)
                {
                    ticket.DMTyping.TypingObject.Dispose();
                    ticket.DMTyping.Typing = false;
                }
                ticket.DMTyping.Typing = true;
                ticket.DMTyping.TypingObject = tchan.EnterTypingState();
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

            if (arg2.GetType() != typeof(SocketDMChannel))
            {
                if (closingState.Keys.Any(x => x.Key == arg2.Id))
                {
                    var o = closingState.First(x => x.Key.Key == arg2.Id);
                    var m = await arg2.GetMessageAsync(o.Key.Value);

                    if (arg3.Emote.Equals(new Emoji("❌")))
                    {
                        o.Value.Stop();
                        o.Value.Dispose();
                        closingState.Remove(o.Key);
                        await m.DeleteAsync();
                    }
                }
                else if (WelcomeMessages.Contains(arg3.MessageId))
                {
                    if (arg3.Emote.Equals(new Emoji("❌")))
                    {
                        var m = await arg2.GetMessageAsync(arg3.MessageId);
                        await m.DeleteAsync();
                    }
                    if (arg3.Emote.Equals(new Emoji("✅")))
                    {
                        var ticket = CurrentTickets.Find(x => x.TicketChannel == arg2.Id);
                        var m = await arg2.GetMessageAsync(arg3.MessageId);
                        var dmchan = await client.GetUser(ticket.UserID).GetOrCreateDMChannelAsync();
                        var gusr = usr as SocketGuildUser;
                        await m.DeleteAsync();
                        string tmsg = $"**[Staff] {(gusr.Nickname == null ? usr.ToString() : gusr.Nickname)}** - Hello! Swiss001 Support! How May I help you?";
                        await arg2.SendMessageAsync(tmsg);
                        await dmchan.SendMessageAsync(tmsg);
                    }
                }
                return;
            }

            var msg = await client.Rest.GetDMChannelAsync(arg2.Id).Result.GetMessageAsync(arg1.Id);

            if (isValidSetup(msg))
            {
                //checkmark
                if (arg3.Emote.Equals(new Emoji("✅")))
                {
                    if (client.GetGuild(Global.SwissGuildId).Users.Any(x => x.Id == usr.Id))
                    {
                        if (BlockedUsers.Any(x => x == usr.Id))
                        {
                            await arg2.SendMessageAsync("", false, new EmbedBuilder()
                            {
                                Title = "You are blocked!",
                                Description = "Looks like your blocked from creating support tickets :/",
                                Color = Color.Red
                            }.Build());
                            await msg.DeleteAsync();
                            return;
                        }
                        else
                        {
                            await msg.DeleteAsync();
                            var tmpmsg = await arg2.SendMessageAsync("**Creating support ticket with the staff team...**");
                            RestDMChannel rChan = (RestDMChannel)await client.Rest.GetDMChannelAsync(arg2.Id);
                            var msgs = await rChan.GetMessagesAsync(arg1.Id, Direction.Before, 1).FlattenAsync();
                            await CreateNewTicket(arg2, usr, msgs.First().Content);
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
                else if (arg3.Emote.Equals(new Emoji("❌"))) // xmark
                {
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
                DMTyping = new TypingState()
                {
                    Typing = false,
                    TypingObject = null
                }
            };
            var guilduser = client.GetGuild(Global.SwissGuildId).GetUser(user.Id);
            //create new ticket channel
            var ticketchan = await client.GetGuild(Global.SwissGuildId).CreateTextChannelAsync($"{user.Username}-{user.Discriminator}", x => x.CategoryId = Global.TicketCategoryID);
            var logs = ModDatabase.currentLogs.Users.Any(x => x.userId == user.Id) ? ModDatabase.currentLogs.Users.First(x => x.userId == user.Id) : null;

            ticket.Transcript = new TicketTranscript(ticket, omsg);

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

            var msg = await ticketchan.SendMessageAsync("@here", false, embed.Build());
            await ticketchan.SendMessageAsync($"**\nV----------------START-OF-TICKET----------------V**\n\n**[Ticketer] {user}** - " + omsg);
            await msg.PinAsync();
            ticket.TicketChannel = ticketchan.Id;
            CurrentTickets.Add(ticket);
            Global.SaveSupportTickets();
        }   

        private async Task CheckNewThread(SocketMessage arg)
        {
            if (arg.Author.IsBot)
                return;

            if (arg.Author.Id == client.CurrentUser.Id)
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
                    ticket.DMTyping.Typing = false;
                    if (ticket.DMTyping.TypingObject != null)
                        ticket.DMTyping.TypingObject.Dispose();
                    string msg = $"**[Ticketer] {arg.Author}** - {arg.Content.Replace("@everyone", "~~@ everyone~~").Replace("@here", "~~@ here~~")}";
                    var tkchan = client.GetGuild(Global.SwissGuildId).GetTextChannel(ticket.TicketChannel);
                    await tkchan.SendMessageAsync(msg);
                    ticket.Transcript.AddMessage(arg);
                    if (arg.Attachments.Count > 0)
                    {
                        foreach (var attc in arg.Attachments)
                        {
                            var bt = new WebClient().DownloadData(new Uri(attc.ProxyUrl));
                            File.WriteAllBytes(Environment.CurrentDirectory + $"{Path.DirectorySeparatorChar}{attc.Filename}", bt);
                            await tkchan.SendFileAsync(Environment.CurrentDirectory + $"{Path.DirectorySeparatorChar}{attc.Filename}", $"**[Ticketer]**");
                        }
                    }
                    
            
                }
                else 
                {
                    var msg = await arg.Channel.SendMessageAsync("Hello " + arg.Author.Mention + ", if you want to open a support ticket please click the checkmark, otherwise to delete this message click the X");
                    await msg.AddReactionsAsync(new IEmote[] { new Emoji("✅"), new Emoji("❌") });
                }
                
            }
            else if(CurrentTickets.Any(x => x.TicketChannel == arg.Channel.Id))
            {
                //check snippets
                await SnippetHandler(arg);
            }
        }

        public bool isValidSetup(IMessage m)
        {
            if (m == null)
                return false;
            return m.Author.Id == client.CurrentUser.Id && m.Content.Contains("if you want to open a support ticket please click the checkmark, otherwise to delete this message click the X");
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
                ticket.Transcript.AddMessage(snipval, arg.Author, arg.Id, arg.Timestamp);
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
            [DiscordCommand("w",
            BotCanExecute = false,
            description = "Welcomes a user in a thread",
            commandHelp = "`(PREFIX)w`",
            prefixes = new char[] { '!', '*', },
            RequiredPermission = true
            )]
            public async Task Welcome()
            {
                if (!HasExecutePermission)
                    return;
                if (CurrentTickets.Any(x => x.TicketChannel == Context.Channel.Id))
                {
                    var ticket = CurrentTickets.Find(x => x.TicketChannel == Context.Channel.Id);
                    if(!ticket.Welcomed)
                    {
                        var dmchan = await Context.Client.GetUser(ticket.UserID).GetOrCreateDMChannelAsync();
                        var usr = Context.Message.Author as SocketGuildUser;
                        await Context.Message.DeleteAsync();
                        ticket.Transcript.AddMessage("Hello! Swiss001 Support! How May I help you?", usr, Context.Message.Id, Context.Message.Timestamp);
                        string msg = $"**[Staff] {(usr.Nickname == null ? usr.ToString() : usr.Nickname)}** - Hello! Swiss001 Support! How May I help you?";
                        await Context.Guild.GetTextChannel(ticket.TicketChannel).SendMessageAsync(msg);
                        await dmchan.SendMessageAsync(msg);
                        ticket.Welcomed = true;
                    
                    }
                    else
                    {
                        var msg = await Context.Channel.SendMessageAsync("The user has been welcomed, are you sure you want to still want to send them another welcome message?");
                        await msg.AddReactionsAsync(new IEmote[] { new Emoji("✅"), new Emoji("❌") });
                        WelcomeMessages.Add(msg.Id);
                    }
                }
            }
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
                    if(dmchan == null)
                    {
                        await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                        {
                            Title = $"Looks like we cant slide into <@{ticket.UserID}> dm's",
                            Description = "They either blocked us or have left the server :("
                        }.Build());
                        return;
                    }
                    var usr = Context.Guild.GetUser(Context.Message.Author.Id);
                    string msg = $"**[Staff] {(usr.Nickname == null ? usr.ToString() : usr.Nickname)}** - {string.Join(" ", args)}";

                    
                    if (Context.Message.Attachments.Count > 0)
                    {
                        foreach (var attc in Context.Message.Attachments)
                        {
                            var bt = new WebClient().DownloadData(new Uri(attc.ProxyUrl));
                            File.WriteAllBytes(Environment.CurrentDirectory + $"{Path.DirectorySeparatorChar}{attc.Filename}", bt);
                            await dmchan.SendFileAsync(Environment.CurrentDirectory + $"{Path.DirectorySeparatorChar}{attc.Filename}", msg);
                            await Context.Channel.SendFileAsync(Environment.CurrentDirectory + $"{Path.DirectorySeparatorChar}{attc.Filename}", msg);
                        }
                    }
                    else
                    {
                        try
                        {
                            await dmchan.SendMessageAsync(msg);
                            ticket.Transcript.AddMessage(string.Join(" ", args), usr, Context.Message.Id, Context.Message.Timestamp);
                        }
                        catch (Exception e)
                        {
                            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                            {
                                Title = $"Looks like we cant slide into <@{ticket.UserID}> dm's",
                                Description = "They either blocked us or have left the server :("
                            }.Build());
                            return;
                        }
                        await Context.Channel.SendMessageAsync(msg);
                    }
                    await Context.Message.DeleteAsync();
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
                if(CurrentTickets.Any(x => x.TicketChannel == Context.Channel.Id))
                {
                    var ticket = CurrentTickets.Find(x => x.TicketChannel == Context.Channel.Id);
                    var dmchan = await Context.Client.GetUser(ticket.UserID).GetOrCreateDMChannelAsync();
                    if (dmchan == null)
                    {
                        await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                        {
                            Title = $"Looks like we cant slide into <@{ticket.UserID}> dm's",
                            Description = "They either blocked us or have left the server :("
                        }.Build());
                        return;
                    }
                    string msg = $"**Staff** - {string.Join(" ", args)}";
                    if (Context.Message.Attachments.Count > 0)
                    {
                        foreach (var attc in Context.Message.Attachments)
                        {
                            var bt = new WebClient().DownloadData(new Uri(attc.ProxyUrl));
                            File.WriteAllBytes(Environment.CurrentDirectory + $"{Path.DirectorySeparatorChar}{attc.Filename}", bt);
                            await dmchan.SendFileAsync(Environment.CurrentDirectory + $"{Path.DirectorySeparatorChar}{attc.Filename}", $"**[Staff]**");
                            await Context.Channel.SendFileAsync(Environment.CurrentDirectory + $"{Path.DirectorySeparatorChar}{attc.Filename}", $"**[Staff]**");
                        }
                    }
                    else
                    {
                        try
                        {
                            await dmchan.SendMessageAsync(msg);
                        }
                        catch (Exception e)
                        {
                            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                            {
                                Title = $"Looks like we cant slide into <@{ticket.UserID}> dm's",
                                Description = "They either blocked us or have left the server :("
                            }.Build());
                            return;
                        }
                        await Context.Channel.SendMessageAsync(msg);
                    }
                    await Context.Message.DeleteAsync();
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
                Global.ConsoleLog("Has Permission: " + HasExecutePermission);
                if (!HasExecutePermission)
                    return;
                if (CurrentTickets.Any(x => x.TicketChannel == Context.Channel.Id))
                {
                    var ticket = CurrentTickets.Find(x => x.TicketChannel == Context.Channel.Id);

                    if (closingState.Keys.Any(x => x.Key == Context.Channel.Id))
                    {
                        Global.ConsoleLog("No Closing state");

                        return;
                    }
                    var cmsg = await Context.Channel.SendMessageAsync("This thread will be closed in 5 seconds. If you wish to cancel this, please click the X mark.");

                    await cmsg.AddReactionAsync(new Emoji("❌"));
                    var t = new Timer() { AutoReset = false, Interval = 5000, Enabled = true };
                    t.Elapsed += async (object s, ElapsedEventArgs a) =>
                    {
                        var chan = Context.Channel as SocketTextChannel;
                        closingState.Remove(closingState.Keys.First(x => x.Key == chan.Id));
                        CurrentTickets.Remove(ticket);

                        var ts = ticket.Transcript.CompileAndSave();

                        await Global.Client.GetGuild(Global.SwissGuildId).GetTextChannel(770875781823463424).SendMessageAsync("", false, new EmbedBuilder()
                        {
                            Title = "New ticket transcript",
                            Description = $"New [transcript](https://api.swissdev.team/apprentice/v1/tickets/{ts.id}/{ts.timestamp}) from <@{ticket.UserID}>",
                            Fields = new List<EmbedFieldBuilder>()
                            {
                                new EmbedFieldBuilder()
                                {
                                    Name = "Notice",
                                    Value = "To view tickets you have to sign in with discord, The only information i recieve is Username, Profile, and User ID. I use this information to check if you are staff. Links will not allow non staff members to view tickets.\nYou can view all tickets [here](https://api.swissdev.team/apprentice/v1/tickets)"
                                }
                            },
                            Color = Color.Green
                        }.WithCurrentTimestamp().Build());

                        await chan.DeleteAsync(new RequestOptions() { AuditLogReason = "Ticket Closed" });
                        var dmUser = Context.Client.GetUser(ticket.UserID);
                        if(dmUser != null)
                        {
                            var dmchan = await dmUser.GetOrCreateDMChannelAsync();
                            if (dmchan != null)
                                await dmchan.SendMessageAsync("Your ticket with the staff team is now closed. If you wish to open another ticket, please send a message.");
                        }
                       
                        Global.SaveSupportTickets();
                    };
                    closingState.Add(new KeyValuePair<ulong, ulong>(Context.Channel.Id, cmsg.Id), t);
                    
                }
                else
                    Global.ConsoleLog("cant find chan for close");
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
