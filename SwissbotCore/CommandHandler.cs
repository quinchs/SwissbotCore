using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SwissBot.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Audio;
using System.Timers;
using System.Threading;
using Discord.Rest;

namespace SwissBot
{
    class CommandHandler
    {
        public static DiscordSocketClient _client;
        private CommandService _service;
        internal System.Timers.Timer t = new System.Timers.Timer();
        internal System.Timers.Timer autoSlowmode = new System.Timers.Timer() { Enabled = false, AutoReset = true, Interval = 1000 };
        Dictionary<ulong, int> sList = new Dictionary<ulong, int>();
        Dictionary<ulong, int> currentSlowmodeList = new Dictionary<ulong, int>();
        public CommandHandler(DiscordSocketClient client)
        {
            _client = client;

            _client.SetGameAsync(Global.Status, "https://github.com/quinchs/Swiss001Bot", ActivityType.Playing);

            _client.SetStatusAsync(UserStatus.DoNotDisturb);

            _service = new CommandService();

            _service.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            // _service.AddSingleton(new AudioService());

            _client.MessageReceived += LogMessage;

            _client.MessageReceived += HandleCommandAsync;

            _client.UserJoined += UpdateUserCount;

            _client.UserJoined += WelcomeMessage;

            _client.UserJoined += checkAlt;

            _client.MessageReceived += responce;

            _client.UserLeft += UpdateUserCount;

            _client.JoinedGuild += _client_JoinedGuild;

            _client.ReactionAdded += ReactionHandler;

            _client.LatencyUpdated += _client_LatencyUpdated;

            _client.Ready += Init;

            autoSlowmode.Enabled = true;
            autoSlowmode.Elapsed += AutoSlowmode_Elapsed;

            Console.WriteLine("[" + DateTime.Now.TimeOfDay + "] - " + "Services loaded, update init");
        }
        SocketMessage r;
        private async void aiTrd()
        {
            while (true)
            {
                
                await Task.Delay(5000);
                if (r != null)
                    await r.Channel.SendMessageAsync(GenerateAIResponse(r, new Random()).Result);
            }
        }

        private async Task checkAlt(SocketGuildUser arg)
        {
            var dt = DateTime.UtcNow;
            if ((dt - arg.CreatedAt.UtcDateTime).TotalHours < 12.0)
            {
                EmbedBuilder b = new EmbedBuilder()
                {
                    Title = "Alt Alert",
                    Description = $"The account **{arg.ToString()}** has been flagged down becuase it is was created less than 12 hours ago",
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        {new EmbedFieldBuilder()
                        {
                            IsInline = true,
                            Name = "Username and ID",
                            Value = $"{arg.ToString()} ({arg.Id})"
                        } },
                        {new EmbedFieldBuilder()
                        {
                            IsInline = true,
                            Name = "Created at (UTC)",
                            Value = arg.CreatedAt.UtcDateTime.ToString()
                        } },
                        {new EmbedFieldBuilder()
                        {
                            IsInline = true,
                            Name = "Joined at (UTC)",
                            Value = arg.JoinedAt.Value.UtcDateTime.ToString()
                        } }
                    },
                    Color = Color.Orange,
                };
                await _client.GetGuild(Global.SwissGuildId).GetTextChannel(665647956816429096).SendMessageAsync("", false, b.Build());
            }
        }

        private async void AutoSlowmode_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Global.AutoSlowmodeToggle)
            {

                foreach (var item in sList.ToList())
                {
                    if (item.Value >= Global.AutoSlowmodeTrigger)
                    {
                        var chan = _client.GetGuild(Global.SwissGuildId).GetTextChannel(item.Key);
                        var aChan = _client.GetGuild(Global.SwissGuildId).GetTextChannel(665647956816429096);
                        var mLink = await chan.GetMessagesAsync(1).FlattenAsync();

                        if(chan.SlowModeInterval > 0) //the channel has slowmode already
                        {
                            if (currentSlowmodeList.Keys.Contains(chan.Id))
                                currentSlowmodeList[chan.Id] = currentSlowmodeList[chan.Id] + 5;
                            else
                                currentSlowmodeList.Add(chan.Id, chan.SlowModeInterval);
                        }
                        EmbedBuilder b = new EmbedBuilder()
                        {
                            Color = Color.Orange,
                            Title = "Auto Alert",
                            Fields = new List<EmbedFieldBuilder>() { { new EmbedFieldBuilder() { Name = "Reason", Value = $"Message limit of {Global.AutoSlowmodeTrigger}/sec reached" } }, { new EmbedFieldBuilder() { Name = "Channel", Value = $"<#{chan.Id}>" } }, { new EmbedFieldBuilder() { Name = "Message Link", Value = mLink.First().GetJumpUrl() } } }
                        };
                        if(chan.SlowModeInterval >= 5)
                            await chan.ModifyAsync(x => x.SlowModeInterval = 5 + chan.SlowModeInterval);
                        else
                            await chan.ModifyAsync(x => x.SlowModeInterval = 5);
                        await aChan.SendMessageAsync("", false, b.Build());
                        System.Timers.Timer lt = new System.Timers.Timer()
                        {
                            Interval = 60000,
                        };
                        sList.Remove(item.Key);
                        lt.Enabled = true;
                        lt.Elapsed += (object s, ElapsedEventArgs arg) =>
                        {
                            if(currentSlowmodeList.Keys.Contains(chan.Id))
                                chan.ModifyAsync(x => x.SlowModeInterval = currentSlowmodeList[chan.Id]);
                            else
                                chan.ModifyAsync(x => x.SlowModeInterval = 0);
                        };
                    }
                    else
                    {
                        sList[item.Key] = 0;
                        sList.Remove(item.Key);
                    }
                }
            }
        }

        private async Task _client_JoinedGuild(SocketGuild arg)
        {
            if (arg.Id != Global.SwissBotDevGuildID && arg.Id != Global.SwissGuildId && Global.GiveAwayGuilds.FirstOrDefault(x => x.giveawayguild.guildID == arg.Id).giveawayguild.guildOBJ != null)
            {
                await arg.LeaveAsync();

            }
        }

        internal async Task<string> GenerateAIResponse(SocketMessage arg, Random r)
        {
            bool dbug = false;
            string dbugmsg = "";


            Regex r1 = new Regex("what time is it in .*");
            if (arg == null)
            {
                dbugmsg += "arg was null... \n";
                string[] filecontUn = File.ReadAllLines(Global.aiResponsePath);
                //var list = filecontUn.ToList();
                //var d = list.FirstOrDefault(x => x.ToLower() == arg.Content.ToLower());
                Regex rg2 = new Regex(".*(\\d{18})>.*");
                string msg = filecontUn[r.Next(0, filecontUn.Length)];
                //if (d != "") { msg = d; }
                if (rg2.IsMatch(msg))
                {
                    dbugmsg += "Found a ping in there, sanitizing..\n";
                    var rm = rg2.Match(msg);
                    var user = _client.GetGuild(Global.SwissGuildId).GetUser(Convert.ToUInt64(rm.Groups[1].Value));
                    msg = msg.Replace(rm.Groups[0].Value, $"**(non-ping: {user.Username}#{user.Discriminator})**");
                }
                if (msg == "") { return filecontUn[r.Next(0, filecontUn.Length)]; }
                else { return msg; }
            }
            else
            {
                string oMsg = arg.Content.ToLower();
                if (arg.Content.StartsWith("*debug "))
                {
                    dbug = true;
                    oMsg = oMsg.Replace("*debug ", "");
                }
                dbugmsg += "Arg was not null. starting AI responces..\n";
                try
                {
                    if (r1.IsMatch(oMsg.ToLower()))
                    {
                        dbugmsg += "User looking for the time. starting up Time API..\n";
                        HttpClient c = new HttpClient();
                        string link = $"https://www.google.com/search?q={oMsg.ToLower().Replace(' ', '+')}";
                        var req = await c.GetAsync(link);
                        var resp = await req.Content.ReadAsStringAsync();
                        Regex x = new Regex(@"<div class=""BNeawe iBp4i AP7Wnd""><div><div class=""BNeawe iBp4i AP7Wnd"">(.*?)<\/div><\/div>");
                        if (x.IsMatch(resp))
                        {
                            string time = x.Match(resp).Groups[1].Value;
                            c.Dispose();
                            dbugmsg += "Found the time to be " + time + "\n";
                            return $"The current time in {oMsg.ToLower().Replace("what time is it in ", "")} is {time}";
                        }
                        else { c.Dispose(); return $"Sorry buddy but could not get the time for {arg.Content.ToLower().Replace("what time is it in ", "")}"; }
                    }
                    //if (oMsg.ToLower() == "are you gay") { return "no ur gay lol"; }
                    //if (oMsg.ToLower() == "how is your day going") { return "kinda bad. my creator beats me and hurts me help"; }
                    //if (oMsg.ToLower() == "are you smart") { return "smarter than your mom lol goteme"; }
                    //if (oMsg.ToLower() == "hi") { return "hello mortal"; }
                    string[] filecontUn = File.ReadAllLines(Global.aiResponsePath);
                    for (int i = 0; i != filecontUn.Length; i++)
                        filecontUn[i] = filecontUn[i].ToLower();
                    Regex rg2 = new Regex(".*(\\d{18})>.*");
                    string msg = "";
                    var ar = filecontUn.Select((b, i) => b == oMsg ? i : -1).Where(i => i != -1).ToArray();
                    Random ran = new Random();
                    dbugmsg += $"Found {ar.Length} indexed responces for the question\n";
                    if (ar.Length != 0)
                    {
                        var ind = (ar[ran.Next(0, ar.Length)]);
                        if (ind != 0 && (ind + 1) < filecontUn.Length)
                            msg = filecontUn[ind + 1];
                        dbugmsg += $"Picked the best answer: {msg}\n";
                    }
                    else
                    {
                        var words = oMsg.Split(' ');
                        var query = from state in filecontUn.AsParallel()
                                    let StateWords = state.Split(' ')
                                    select (Word: state, Count: words.Intersect(StateWords).Count());

                        var sortedDict = from entry in query orderby entry.Count descending select entry;
                        string rMsg = sortedDict.First().Word;
                        var reslt = filecontUn.Select((b, i) => b == rMsg ? i : -1).Where(i => i != -1).ToArray();
                        if (reslt.Length != 0)
                        {
                            var ind = (reslt[ran.Next(0, reslt.Length)]);
                            if (ind != 0 && (ind + 1) < filecontUn.Length)
                                msg = filecontUn[ind + 1];
                            dbugmsg += $"Picked the best answer: {msg}\n";
                        }
                        else { msg = rMsg; }
                        //string[] words = oMsg.Split(' ');
                        //Dictionary<string, int> final = new Dictionary<string, int>();
                        //foreach (var state in filecontUn)
                        //{
                        //    int count = 0;
                        //    foreach (var word in state.Split(' '))
                        //    {
                        //        if (words.Contains(word))
                        //            count++;
                        //    }
                        //    if (!final.Keys.Contains(state) && count != 0)
                        //        final.Add(state, count);
                        //}
                        //string res = sortedDict.First().Key;

                    }
                    if (msg == "") { msg = filecontUn[r.Next(0, filecontUn.Length)]; }

                    if (rg2.IsMatch(msg))
                    {
                        var rm = rg2.Match(msg);
                        var user = _client.GetGuild(Global.SwissGuildId).GetUser(Convert.ToUInt64(rm.Groups[1].Value));
                        if (user != null)
                        {
                            msg = msg.Replace(rm.Groups[0].Value, $"**(non-ping: {user.Username}#{user.Discriminator})**");
                            dbugmsg += "Sanitized ping.. \n";
                        }
                        else
                        {
                            try
                            {
                                var em = await _client.GetGuild(Global.SwissGuildId).GetEmoteAsync(Convert.ToUInt64(rm.Groups[1].Value));
                                if (em == null)
                                {
                                    dbugmsg += $"Could not find a user for {rm.Value}, assuming emoji or user is not in server..\n";
                                    msg = msg.Replace(rm.Groups[0].Value, $"**(non-ping: {rm.Value})**");
                                }
                            }
                            catch (Exception ex) { dbugmsg += $"{ex.Message}.. \n"; }
                        }
                    }
                    if (msg.Contains("@everyone")) { msg = msg.Replace("@everyone", "***(Non-ping @every0ne)***"); }
                    if (msg.Contains("@here")) { msg = msg.Replace("@here", "***(Non-ping @h3re)***"); }
                    dbugmsg += "Sanitized for @h3re and @every0ne\n";
                    EmbedBuilder eb = new EmbedBuilder()
                    {
                        Color = Color.Orange,
                        Title = "Ai Debug",
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = _client.CurrentUser.ToString(),
                            IconUrl = _client.CurrentUser.GetAvatarUrl()
                        },
                        Description = "```Ai Debug Log```\n`" + dbugmsg + "`",


                    };
                    if (dbug)
                        await arg.Channel.SendMessageAsync("", false, eb.Build());
                    return msg;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return "uh oh ai broke stinkie";
                }
            }
        }
        private async Task responce(SocketMessage arg)
        {
            if (arg.Channel.Id == Global.BotAiChanID && !arg.Author.IsBot)
            {
                var d = arg.Channel.EnterTypingState();
                try
                {
                    Random r = new Random();
                    var mu = arg.MentionedUsers.FirstOrDefault(x => x.Username == _client.CurrentUser.Username);
                    if (r.Next(1, 2) == 1 || mu != null)
                    {
                        string msg = await GenerateAIResponse(arg, r);
                        if (msg != "")
                        {
                            if (msg != ("*terminate"))
                                await arg.Channel.SendMessageAsync(msg);
                        }
                    }
                    d.Dispose();
                }
                catch (Exception ex)
                {
                    Global.SendExeption(ex);
                    d.Dispose();
                }
            }
        }

        private async Task ReactionHandler(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            try
            {
                await CheckVerification(arg1, arg2, arg3);
                await checkSub(arg1, arg2, arg3);
            }
            catch (Exception ex)
            {
                Global.ConsoleLog($"Reaction handler error: {ex.Message} \n {ex.StackTrace}", ConsoleColor.Red);
                Global.SendExeption(ex);
            }
        }
        Dictionary<ulong, ulong> FList = new Dictionary<ulong, ulong>();
        private async Task CheckVerification(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg2.Id == Global.VerificationChanID)
            {
                var emote = new Emoji("✅");
                if (arg3.Emote.Name == emote.Name)
                {
                    var user = _client.GetGuild(Global.SwissGuildId).GetUser(arg3.UserId);
                    if (user == null) { user = _client.GetGuild(Global.SwissGuildId).GetUser(arg1.Value.Author.Id); }
                    string nick = "";
                    if (user.Nickname != null)
                        nick = user.Nickname;
                    if (user.ToString().ToLower().Contains("ace") || user.ToString().ToLower().Contains("wing") || nick.ToLower().Contains("ace") || nick.ToLower().Contains("wing"))
                    {
                        //man

                        EmbedBuilder b = new EmbedBuilder()
                        {

                            Title = "Manual Verification",
                            Description = "This user was flagged down for the expression `username OR nickname contains \"ace &| wing\"`",
                            Color = Color.Red
                        };
                        b.Fields.Add(new EmbedFieldBuilder()
                        {
                            Name = "Join at",
                            Value = user.JoinedAt
                        });
                        b.Fields.Add(new EmbedFieldBuilder()
                        {
                            Name = "Created at",
                            Value = user.CreatedAt
                        });
                        b.Fields.Add(new EmbedFieldBuilder()
                        {
                            Name = "Username",
                            Value = user.ToString()
                        });
                        string anick = "None";
                        if (user.Nickname != null)
                            anick = user.Nickname;


                        b.Fields.Add(new EmbedFieldBuilder()
                        {
                            Name = "Nickname",
                            Value = anick
                        });
                        b.Fields.Add(new EmbedFieldBuilder()
                        {
                            Name = "ID",
                            Value = user.Id
                        });


                        //{
                        //         { 
                        //         { new EmbedFieldBuilder() {
                        //            Name = "Created at",
                        //            Value = user.CreatedAt
                        //         } },
                        //         { new EmbedFieldBuilder() {
                        //            Name = "Username",
                        //            Value = user.ToString()
                        //         } },
                        //         { new EmbedFieldBuilder() {
                        //            Name = "Nickname",
                        //            Value = user.Nickname
                        //         } },
                        //        {new EmbedFieldBuilder()
                        //        {
                        //            Name = "ID",
                        //            Value = user.Id
                        //        } }
                        //};

                        Discord.Rest.RestUserMessage msg = await _client.GetGuild(Global.SwissGuildId).GetTextChannel(662142405377654804).SendMessageAsync("", false, b.Build());

                        await msg.AddReactionAsync(new Emoji("✅"));
                        await msg.AddReactionAsync(new Emoji("❌"));
                        FList.Add(msg.Id, user.Id);
                        return;
                    }
                    else
                    {
                        var unVertRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.UnverifiedRoleID);
                        if (user.Roles.Contains(unVertRole))
                        {
                            var userRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.MemberRoleID);
                            await user.AddRoleAsync(userRole);
                            Console.WriteLine($"Verified user {user.Username}#{user.Discriminator}");
                            EmbedBuilder eb2 = new EmbedBuilder()
                            {
                                Title = $"Verified {user.Mention}",
                                Color = Color.Green,
                                Footer = new EmbedFooterBuilder()
                                {
                                    IconUrl = user.GetAvatarUrl(),
                                    Text = $"{user.Username}#{user.Discriminator}"
                                },
                                Fields = new List<EmbedFieldBuilder>()
                            {
                                {new EmbedFieldBuilder()
                                {
                                    IsInline = true,
                                    Name = "AutoVerified?",
                                    Value = "false"
                                } }
                            }
                            };
                            var chan = _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.VerificationLogChanID);
                            await chan.SendMessageAsync("", false, eb2.Build());
                            await user.RemoveRoleAsync(unVertRole);
                            string welcomeMessage = WelcomeMessageBuilder(Global.WelcomeMessage, user);
                            EmbedBuilder eb = new EmbedBuilder()
                            {
                                Title = $"***Welcome to Swiss001's Discord server!***",
                                Footer = new EmbedFooterBuilder()
                                {
                                    IconUrl = user.GetAvatarUrl(),
                                    Text = $"{user.Username}#{user.Discriminator}"
                                },
                                Description = welcomeMessage,
                                ThumbnailUrl = Global.WelcomeMessageURL,
                                Color = Color.Green
                            };
                            await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.WelcomeMessageChanID).SendMessageAsync("", false, eb.Build());
                            Global.ConsoleLog($"WelcomeMessage for {user.Username}#{user.Discriminator}", ConsoleColor.Blue);
                        }
                    }
                }
            }
            else if (arg2.Id == 662142405377654804)
            {
                if (arg3.User.Value.IsBot) { return; }
                if (FList.Keys.Contains(arg3.MessageId))
                {
                    var user = _client.GetGuild(Global.SwissGuildId).GetUser(FList[arg3.MessageId]);

                    var msg = _client.GetGuild(Global.SwissGuildId).GetTextChannel(662142405377654804).GetMessageAsync(arg3.MessageId).Result;
                    var ch = new Emoji("✅");
                    var ex = new Emoji("❌");
                    if (arg3.Emote.Name == ch.Name)
                    {
                        var unVertRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.UnverifiedRoleID);
                        if (user.Roles.Contains(unVertRole))
                        {
                            var userRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.MemberRoleID);
                            await user.AddRoleAsync(userRole);
                            Console.WriteLine($"Verified user {user.Username}#{user.Discriminator}");
                            EmbedBuilder eb2 = new EmbedBuilder()
                            {
                                Title = $"Verified {user.Mention}",
                                Color = Color.Green,
                                Footer = new EmbedFooterBuilder()
                                {
                                    IconUrl = user.GetAvatarUrl(),
                                    Text = $"{user.Username}#{user.Discriminator}"
                                },
                            };
                            var chan = _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.VerificationLogChanID);
                            await chan.SendMessageAsync("", false, eb2.Build());
                            await user.RemoveRoleAsync(unVertRole);
                            string welcomeMessage = WelcomeMessageBuilder(Global.WelcomeMessage, user);
                            EmbedBuilder eb = new EmbedBuilder()
                            {
                                Title = $"***Welcome to Swiss001's Discord server!***",
                                Footer = new EmbedFooterBuilder()
                                {
                                    IconUrl = user.GetAvatarUrl(),
                                    Text = $"{user.Username}#{user.Discriminator}"
                                },
                                Description = welcomeMessage,
                                ThumbnailUrl = Global.WelcomeMessageURL,
                                Color = Color.Green
                            };
                            await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.WelcomeMessageChanID).SendMessageAsync("", false, eb.Build());
                            Global.ConsoleLog($"WelcomeMessage for {user.Username}#{user.Discriminator}", ConsoleColor.Blue);
                            await msg.DeleteAsync();
                            FList.Remove(arg3.MessageId);
                            await _client.GetGuild(Global.SwissGuildId).GetTextChannel(662142405377654804).SendMessageAsync($"Allowed {user.ToString()}. Moderator: {arg3.User.ToString()}");
                        }
                    }
                    if (arg3.Emote.Name == ex.Name)
                    {
                        if (user.Roles.Count == 2)
                        {
                            await user.BanAsync(7, "Denied by Staff");
                        }
                        await msg.DeleteAsync();
                        FList.Remove(arg3.MessageId);
                        await _client.GetGuild(Global.SwissGuildId).GetTextChannel(662142405377654804).SendMessageAsync($"Banned {user.ToString()}. Moderator: {arg3.User.ToString()}");
                    }
                }
            }
        }

        private async Task checkSub(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg2.Id == Global.SubmissionChanID)
            {
                foreach (var item in Global.SubsList)
                {
                    Global.ConsoleLog("Error, Reaction Message doesnt exist, Using ID to get message", ConsoleColor.Red);

                    var linkmsg = _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.SubmissionChanID).GetMessageAsync(arg3.MessageId).Result;
                    if (item.linkMsg.Content == linkmsg.Content)
                    {
                        if (!arg3.User.Value.IsBot) //not a bot
                        {
                            string rs = "";
                            if (arg3.Emote.Name == item.checkmark.Name)
                            {
                                //good img
                                string curr = File.ReadAllText(Global.ButterFile);
                                File.WriteAllText(Global.ButterFile, curr + "\n" + item.url);
                                Global.ConsoleLog($"the image {item.url} has been approved by {arg3.User.Value.Username}#{arg3.User.Value.Discriminator}");
                                try { await _client.GetUser(item.SubmitterID).SendMessageAsync($"Your butter submission was approved by {arg3.User.Value.Username}#{arg3.User.Value.Discriminator} ({item.url})"); }
                                catch (Exception ex) { Global.ConsoleLog($"Error, {ex.Message}", ConsoleColor.Red); }
                                await item.botMSG.DeleteAsync();
                                await item.linkMsg.DeleteAsync();
                                Global.SubsList.Remove(item);
                                rs = "Accepted";
                            }
                            if (arg3.Emote.Name == item.Xmark.Name)
                            {
                                //bad img
                                Global.ConsoleLog($"the image {item.url} has been Denied by {arg3.User.Value.Username}#{arg3.User.Value.Discriminator}", ConsoleColor.Red);
                                await item.botMSG.DeleteAsync();
                                await item.linkMsg.DeleteAsync();
                                Global.SubsList.Remove(item);
                                try { await _client.GetUser(item.SubmitterID).SendMessageAsync($"Your butter submission was approved by {arg3.User.Value.Username}#{arg3.User.Value.Discriminator} ({item.url})"); }
                                catch (Exception ex) { Global.ConsoleLog($"Error, {ex.Message}", ConsoleColor.Red); }
                                rs = "Denied";
                            }

                            EmbedBuilder eb = new EmbedBuilder()
                            {
                                Title = "Submission Result",
                                Color = Color.Blue,
                                Description = $"The image {item.url} Submitted by {_client.GetUser(item.SubmitterID).Mention} has been **{rs}** by {arg3.User.Value.Mention} ({arg3.User.Value.Username}#{arg3.User.Value.Discriminator})",
                                Footer = new EmbedFooterBuilder()
                                {
                                    Text = "Result Autogen",
                                    IconUrl = _client.CurrentUser.GetAvatarUrl()
                                }
                            };
                            await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.SubmissionsLogChanID).SendMessageAsync("", false, eb.Build());
                            return;
                        }
                    }
                }
            }
        }

        private async Task _client_LatencyUpdated(int arg1, int arg2)
        {
            await UpdateUserCount(null);
            if (_client.GetGuild(Global.SwissGuildId).GetUser(_client.CurrentUser.Id).Nickname != "SwissBot ( * )")
            {
                await _client.GetGuild(Global.SwissGuildId).GetUser(_client.CurrentUser.Id).ModifyAsync(x => x.Nickname = "SwissBot ( * )");
            }
        }

        private async Task WelcomeMessage(SocketGuildUser arg)
        {
            var unVertRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.UnverifiedRoleID);
            await arg.AddRoleAsync(unVertRole);
            Console.WriteLine($"The member {arg.Username}#{arg.Discriminator} joined the guild");
            await checkMembers();
        }
        internal static async Task checkMembers()
        {
            //switch (_client.GetGuild(Global.SwissGuildId).MemberCount)
            //{
            //    case 5000:
            //        await SendMilestone(5000);
            //        break;
            //    case 6000:
            //        await SendMilestone(6000);
            //        break;
            //    case 7000:
            //        await SendMilestone(7000);
            //        break;
            //    case 7500:
            //        await SendMilestone(7500);
            //        break;
            //}
        }
        public static async Task SendMilestone(int count, ulong chanid = 0)
        {
            SocketTextChannel MilestoneChan;
            if (chanid != 0) { MilestoneChan = _client.GetGuild(Global.SwissGuildId).GetTextChannel(chanid); }
            else { MilestoneChan = _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.MilestonechanID); }
            var memberList = _client.GetGuild(Global.SwissGuildId).Users.ToList();
            Random r = new Random();
            var mem1 = memberList[r.Next(0, memberList.Count)];
            var mem2 = memberList[r.Next(0, memberList.Count)];
            var mem3 = memberList[r.Next(0, memberList.Count)];
            var msg = await MilestoneChan.SendMessageAsync("@everyone", false, new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = $":tada: We did it! Congratulations on {count} Members!! :tada:",
                Footer = new EmbedFooterBuilder()
                {
                    Text = "Swiss001's Discord",
                    IconUrl = _client.GetUser(365958535768702988).GetAvatarUrl(),

                },
                ThumbnailUrl = Global.WelcomeMessageURL,
                Description = $"Thank you everyone we made it to {count} Members, Congrats everyone :tada: :tada:!\n\nWow what an accomplishment we have achieved! Being a part of this server has been a trill for me, with everyone talking to me in <#665639044721541130> and i cant wait to talk with all 6000 of you, and big congrats to <@365958535768702988> for making vids for us all to watch. Thanks everyone, \n*Swiss001 Staff Team*",
                Url = "https://www.youtube.com/channel/UCYiaHzwtsww6phfxwUtZv8w"
            }.Build());
            Global.ConsoleLog("\n\n Milestone Reached! \n\n", ConsoleColor.Blue);
            await msg.ModifyAsync(x => x.Content = " ");
        }
        internal static string WelcomeMessageBuilder(string orig, SocketGuildUser user)
        {
            if (orig.Contains("(USER)"))
                orig = orig.Replace("(USER)", $"<@{user.Id}>");

            if (orig.Contains("(USERCOUNT)"))
                orig = orig.Replace("(USERCOUNT)", Global.UserCount.ToString());
            return orig;
        }

        private async Task Init()
        {
            try
            {

                Global.ConsoleLog("Starting Init... \n\n Updating UserCounts...", ConsoleColor.DarkCyan);
                Global.UserCount = _client.GetGuild(Global.SwissGuildId).Users.Count;
                try { await UpdateUserCount(null); } catch (Exception ex) { Global.ConsoleLog($"Ex,{ex} ", ConsoleColor.Red); }
                Global.ConsoleLog("Finnished UserCount", ConsoleColor.Cyan);
                try { await AddUnVert(); } catch (Exception ex) { Global.ConsoleLog($"Ex,{ex} ", ConsoleColor.Red); }
                try { await CheckVerts(); } catch (Exception ex) { Global.ConsoleLog($"Ex,{ex} ", ConsoleColor.Red); }
                foreach (var arg in _client.Guilds)
                {
                    if (arg.Id != Global.SwissBotDevGuildID && arg.Id != Global.SwissGuildId)
                    {
                        try { await arg.DeleteAsync(); }
                        catch { await arg.LeaveAsync(); }
                    }
                }

                t.Elapsed += DCRS;
                t.Enabled = true;
                t.Interval = 300000;

                try { UserSubCashing(); } catch (Exception ex) { Console.WriteLine(ex); }
                // await MassWelcome();
                //LoadLLogs();
                Thread t2 = new Thread(aiTrd);
                t2.Start();
                Global.ConsoleLog("Finnished Init!", ConsoleColor.Black, ConsoleColor.DarkGreen);
            }
            catch (Exception ex)
            {

            }
        }
        private async Task LoadLLogs()
        {
            Global.linkLogs = new Dictionary<string, List<Global.LogItem>>();
            foreach (var file in Directory.GetFiles(Global.LinksDirpath))
            {
                string[] cont = File.ReadAllLines(file);

                string pat = @"\[(.*?) : (.*?)\] USER: (.*?) CHANNEL: (.*?) LINKS: (.*?,)*";
                string date = "";
                List<Global.LogItem> l = new List<Global.LogItem>();
                foreach (var line in cont)
                {
                    var match = Regex.Match(line, pat);
                    if (date == "") { date = file.Split(Global.systemSlash).Last(); }
                    var user = _client.GetUser(match.Groups[3].Value.Split('#').First(), match.Groups[3].Value.Split('#').Last());
                    ulong id = 0;
                    if (user != null)
                        id = user.Id;
                    l.Add(new Global.LogItem()
                    {
                        date = $"{match.Groups[1].Value} : {match.Groups[2].Value}",
                        username = $"{match.Groups[3].Value}",
                        id = id.ToString(),
                        channel = $"{match.Groups[4].Value}",
                        message = $"{match.Groups[5].Value}"
                    });
                }
                Global.linkLogs.Add(date, l);
            }
            Global.messageLogs = new Dictionary<string, List<Global.LogItem>>();
            int c = 0;
            int max = Directory.GetFiles(Global.MessageLogsDir).Length;
            List<Thread> tl = new List<Thread>();
            m = max;
            foreach (var file in Directory.GetFiles(Global.MessageLogsDir))
            {
                Thread t = new Thread(() => log(file));
                t.Start();
                tl.Add(t);
                Console.WriteLine("t added");
            }

        }
        public int m = 0;
        public void log(string file)
        {
            string[] cont = File.ReadAllLines(file);

            string pat = @"\[(.*?) : (.*?)\] USER: (.*?) CHANNEL: (.*?) MESSAGE: (.*?,)*";
            string date = "";
            List<Global.LogItem> l = new List<Global.LogItem>();
            foreach (var line in cont)
            {
                var match = Regex.Match(line, pat);
                if (date == "") { date = file.Split(Global.systemSlash).Last(); }
                var user = _client.GetUser(match.Groups[3].Value.Split('#').First(), match.Groups[3].Value.Split('#').Last());
                ulong id = 0;
                if (user != null)
                    id = user.Id;
                l.Add(new Global.LogItem()
                {
                    date = $"{match.Groups[1].Value} : {match.Groups[2].Value}",
                    username = $"{match.Groups[3].Value}",
                    id = id.ToString(),
                    channel = $"{match.Groups[4].Value}",
                    message = $"{match.Groups[5].Value}"
                });
            }
            Global.messageLogs.Add(date, l);
            m--;
            Console.WriteLine($"{m} left");
        }
        private async Task MassWelcome()
        {
            var users = _client.GetGuild(Global.SwissGuildId).Users.Where(x => x.Roles.Contains(_client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x2 => x2.Id == 627683033151176744)) && x.Roles.Count == 2);
            foreach (var arg3 in users)
            {
                var user = _client.GetGuild(Global.SwissGuildId).GetUser(arg3.Id);
                var unVertRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.UnverifiedRoleID);
                if (user.Roles.Contains(unVertRole))
                {
                    var userRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.MemberRoleID);
                    await user.AddRoleAsync(userRole);
                    Console.WriteLine($"Verified user {user.Username}#{user.Discriminator}");
                    EmbedBuilder eb2 = new EmbedBuilder()
                    {
                        Title = $"Verified {user.Mention}",
                        Color = Color.Green,
                        Footer = new EmbedFooterBuilder()
                        {
                            IconUrl = user.GetAvatarUrl(),
                            Text = $"{user.Username}#{user.Discriminator}"
                        },
                    };
                    var chan = _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.VerificationLogChanID);
                    await chan.SendMessageAsync("", false, eb2.Build());
                    await user.RemoveRoleAsync(unVertRole);
                    string welcomeMessage = WelcomeMessageBuilder(Global.WelcomeMessage, user);
                    EmbedBuilder eb = new EmbedBuilder()
                    {
                        Title = $"***Welcome to Swiss001's Discord server!***",
                        Footer = new EmbedFooterBuilder()
                        {
                            IconUrl = user.GetAvatarUrl(),
                            Text = $"{user.Username}#{user.Discriminator}"
                        },
                        Description = welcomeMessage,
                        ThumbnailUrl = Global.WelcomeMessageURL,
                        Color = Color.Green
                    };
                    await _client.GetGuild(Global.SwissGuildId).GetTextChannel(648608707826941963).SendMessageAsync("", false, eb.Build());
                    Global.ConsoleLog($"WelcomeMessage for {user.Username}#{user.Discriminator}", ConsoleColor.Blue);
                }
            }
        }
        private async Task CheckVerts()
        {
            var unVertRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.UnverifiedRoleID);
            var userRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.MemberRoleID);

            //foreach (var user in _client.GetGuild(Global.SwissGuildId).Users.Where(x => x.Roles.Contains(unVertRole) && x.Roles.Count == 2))
            //{
            //    await user.AddRoleAsync(userRole);
            //    Global.ConsoleLog($"Found the user {user.Username}#{user.Discriminator} who hasnt recieved verification yet. Gave them Member role", ConsoleColor.White, ConsoleColor.DarkBlue);
            //    EmbedBuilder eb2 = new EmbedBuilder()
            //    {
            //        Title = $"Verified {user.Mention}",
            //        Color = Color.Green,
            //        Footer = new EmbedFooterBuilder()
            //        {
            //            IconUrl = user.GetAvatarUrl(),
            //            Text = $"{user.Username}#{user.Discriminator}"
            //        },
            //    };
            //    var chan = _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.VerificationLogChanID);
            //    await chan.SendMessageAsync("", false, eb2.Build());
            //    await user.RemoveRoleAsync(unVertRole);
            //    string welcomeMessage = WelcomeMessageBuilder(Global.WelcomeMessage, user);
            //    EmbedBuilder eb = new EmbedBuilder()
            //    {
            //        Title = $"***Welcome to Swiss001's Discord server!***",
            //        Footer = new EmbedFooterBuilder()
            //        {
            //            IconUrl = user.GetAvatarUrl(),
            //            Text = $"{user.Username}#{user.Discriminator}"
            //        },
            //        Description = welcomeMessage,
            //        ThumbnailUrl = Global.WelcomeMessageURL,
            //        Color = Color.Green
            //    };
            //    await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.WelcomeMessageChanID).SendMessageAsync("", false, eb.Build());

            //}
            IUserMessage sMessage = (IUserMessage)await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.VerificationChanID).GetMessageAsync(627680940155469844);
            var emote = new Emoji("✅");
            var reActs = await sMessage.GetReactionUsersAsync(emote, 5000).FlattenAsync();
            foreach (var rUsers in reActs.ToList())
            {
                var user = _client.GetGuild(Global.SwissGuildId).GetUser(rUsers.Id);
                if (user != null)
                {
                    if (user.Roles.Contains(unVertRole))
                    {
                        await user.AddRoleAsync(userRole);
                        Global.ConsoleLog($"Found the user {user.Username}#{user.Discriminator} who hasnt recieved verification yet. Gave them Member role", ConsoleColor.White, ConsoleColor.DarkBlue);
                        EmbedBuilder eb2 = new EmbedBuilder()
                        {
                            Title = $"Verified {user.Mention}",
                            Color = Color.Green,
                            Footer = new EmbedFooterBuilder()
                            {
                                IconUrl = user.GetAvatarUrl(),
                                Text = $"{user.Username}#{user.Discriminator}"
                            },
                            Fields = new List<EmbedFieldBuilder>()
                            {
                                {new EmbedFieldBuilder()
                                {
                                    IsInline = true,
                                    Name = "AutoVerified?",
                                    Value = "true"
                                } }
                            }
                        };
                        var chan = _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.VerificationLogChanID);
                        await chan.SendMessageAsync("", false, eb2.Build());
                        await user.RemoveRoleAsync(unVertRole);
                        string welcomeMessage = WelcomeMessageBuilder(Global.WelcomeMessage, user);
                        EmbedBuilder eb = new EmbedBuilder()
                        {
                            Title = $"***Welcome to Swiss001's Discord server!***",
                            Footer = new EmbedFooterBuilder()
                            {
                                IconUrl = user.GetAvatarUrl(),
                                Text = $"{user.Username}#{user.Discriminator}"
                            },
                            Description = welcomeMessage,
                            ThumbnailUrl = Global.WelcomeMessageURL,
                            Color = Color.Green
                        };
                        await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.WelcomeMessageChanID).SendMessageAsync("", false, eb.Build());

                    }
                }
                else { }
            }
        }
        private async Task AddUnVert()
        {
            var noRoleUsers = _client.GetGuild(Global.SwissGuildId).Users.Where(x => x.Roles.Count == 1).ToList();
            Global.ConsoleLog($"Found {noRoleUsers.Count} Users without verification, Adding the Unverivied role...", ConsoleColor.Cyan);
            var unVertRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.UnverifiedRoleID);
            int i = 0;
            foreach (var user in noRoleUsers) { await user.AddRoleAsync(unVertRole); i++; Global.ConsoleLog($"Gave Unvert role to {user.Username}, {noRoleUsers.Count - i} users left", ConsoleColor.White, ConsoleColor.DarkBlue); }
        }
        private async Task UserSubCashing()
        {
            var messages = await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.SubmissionChanID).GetMessagesAsync().FlattenAsync();

            foreach (var message in messages)
            {
                if (message.Embeds.Count >= 1 && message.Embeds.First().Description != null)
                {
                    if (message.Embeds.First().Description.Contains("This image was submitted by"))
                    {
                        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex("<@(.*?)>");
                        System.Text.RegularExpressions.Regex r2 = new System.Text.RegularExpressions.Regex("LINK: (.*?);");
                        string disc = message.Embeds.First().Description;
                        var link = r2.Match(disc).Groups[1].Value;
                        var userid = r.Match(disc).Value.Trim('<', '>', '@', '!');

                        try
                        {
                            Global.UnnaprovedSubs ua = new Global.UnnaprovedSubs()
                            {
                                linkMsg = messages.FirstOrDefault(x => x.Content.Contains(link)),
                                botMSG = message,
                                checkmark = new Emoji("✅"),
                                Xmark = new Emoji("❌"),
                                SubmitterID = Convert.ToUInt64(userid),
                                url = link
                            };
                            Global.SubsList.Add(ua);
                        }
                        catch (Exception ex) { }

                    }
                }
            }
        }
        private async Task UpdateUserCount(SocketGuildUser arg)
        {
            var users = _client.GetGuild(Global.SwissGuildId).Users;
            Console.WriteLine($"Ucount {users.Count - 20}, usersSCount{users.Count}");
            await _client.GetGuild(Global.SwissGuildId).GetVoiceChannel(Global.StatsTotChanID).ModifyAsync(x =>
            {
                x.Name = $"Total Users: {users.Count + 2}";
            });
        }

        private async Task LogMessage(SocketMessage arg)
        {
            try
            {
                var msg = arg as SocketUserMessage;
                if (msg.Channel.Id == 669305903710863440)
                {
                    r = arg;
                }

                if (sList.ContainsKey(arg.Channel.Id))
                {
                    sList[arg.Channel.Id]++;
                }
                else
                {
                    sList.Add(arg.Channel.Id, 1);
                }

                if (arg.Attachments.Count >= 1 || Regex.IsMatch(arg.Content, "(.*?://.*?)"))
                {
                    string links = "";
                    if (arg.Attachments.Count == 0)
                        links = arg.Content;
                    else
                        foreach (var attach in arg.Attachments)
                            links += attach.ProxyUrl + ", ";
                    string loglMsg = $"[{DateTime.UtcNow.ToLongDateString() + " : " + DateTime.UtcNow.ToLongTimeString()}] ";
                    loglMsg += $"USER: {arg.Author.Username}#{arg.Author.Discriminator} CHANNEL: {arg.Channel.Name} LINKS: {links}";
                    var fname = DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year;
                    if (File.Exists(Global.LinksDirpath + $"{Global.systemSlash}{fname}.txt"))
                    {
                        string curr = File.ReadAllText(Global.LinksDirpath + $"{Global.systemSlash}{fname}.txt");
                        File.WriteAllText(Global.LinksDirpath + $"{Global.systemSlash}{fname}.txt", $"{curr}\n{loglMsg}");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"Logged link (from {arg.Author.Username})");
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    }
                    else
                    {
                        File.Create(Global.LinksDirpath + $"{Global.systemSlash}{fname}.txt").Close();
                        File.WriteAllText(Global.LinksDirpath + $"{Global.systemSlash}{fname}.txt", $"{loglMsg}");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"Logged message (from {arg.Author.Username}) and created new logfile");
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    }
                    var item = new Global.LogItem()
                    {
                        date = $"{DateTime.UtcNow.ToLongDateString() + " : " + DateTime.UtcNow.ToLongTimeString()}",
                        id = arg.Author.Id.ToString(),
                        username = arg.Author.ToString(),
                        channel = arg.Channel.Name,
                        message = arg.Content
                    };
                    //if (Global.linkLogs.ContainsKey($"{DateTime.UtcNow.ToLongDateString()}"))
                    //    Global.linkLogs[$"{DateTime.UtcNow.ToLongDateString()}"].Add(item);
                    //else
                    //    Global.linkLogs.Add($"{DateTime.UtcNow.ToLongDateString()}", new List<Global.LogItem>() { item });
                }
                //Log messages to txt file
                string logMsg = "";
                logMsg += $"[{DateTime.UtcNow.ToLongDateString() + " : " + DateTime.UtcNow.ToLongTimeString()}] ";
                logMsg += $"USER: {arg.Author.Username}#{arg.Author.Discriminator} CHANNEL: {arg.Channel.Name} MESSAGE: {arg.Content}";
                var name = DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year;
                if (File.Exists(Global.MessageLogsDir + $"{Global.systemSlash}{name}.txt"))
                {
                    string curr = File.ReadAllText(Global.MessageLogsDir + $"{Global.systemSlash}{name}.txt");
                    File.WriteAllText(Global.MessageLogsDir + $"{Global.systemSlash}{name}.txt", $"{curr}\n{logMsg}");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"Logged message (from {arg.Author.Username})");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }
                else
                {
                    File.Create(Global.MessageLogsDir + $"{Global.systemSlash}{name}.txt").Close();
                    File.WriteAllText(Global.MessageLogsDir + $"{Global.systemSlash}{name}.txt", $"{logMsg}");
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine($"Logged message (from {arg.Author.Username}) and created new logfile");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }
                if (arg.Channel.Id == 592463507124125706)
                {
                    string cont = File.ReadAllText(Global.aiResponsePath);
                    if (cont == "") { cont = arg.Content; }
                    else { cont += $"\n{arg.Content}"; }
                    File.WriteAllText(Global.aiResponsePath, cont);
                }
                if (CheckCensor(arg).Result)
                {
                    EmbedBuilder b = new EmbedBuilder()
                    {
                        Title = "Censor Alert",
                        Color = Color.Orange,
                        Fields = new List<EmbedFieldBuilder>()
                        {
                            {new EmbedFieldBuilder()
                            {
                                Name = "Message",
                                Value = arg.Content
                            } },
                            {new EmbedFieldBuilder()
                            {
                                Name = "Author",
                                Value = arg.Author.ToString() + $" ({arg.Author.Id})"
                            } },
                            {new EmbedFieldBuilder()
                            {
                                Name = "Action",
                                Value = "Message Deleted",
                            } }
                        }
                    };
                    foreach (var item in Global.CensoredWords)
                        if (arg.Content.Contains(item))
                            b.Fields.Add(new EmbedFieldBuilder() { Name = "Feild", Value = item });

                            await _client.GetGuild(Global.SwissGuildId).GetTextChannel(665647956816429096).SendMessageAsync("", false, b.Build());
                    await arg.DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public async Task<bool> CheckCensor(SocketMessage arg)
        {
            if (arg.Author.IsBot) { return false; }
            var r = _client.GetGuild(Global.SwissGuildId).GetUser(arg.Author.Id).Roles;
            var adminrolepos =_client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.ModeratorRoleID).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null || r.Contains(_client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == 622156934778454016)))
            { return false; }
                string cont = arg.Content;
            foreach (var item in Global.CensoredWords)
                if (cont.Contains(item))
                    return true;
            return false;
        }
        public async Task EchoMessage(SocketCommandContext Context)
        {
            try
            {
                if (Context.User.Id == 259053800755691520)
                {
                    var echomsg = Context.Message.Content.Replace($"{Global.Preflix}echo", "");
                    await _client.GetGuild(Global.SwissGuildId).GetTextChannel(592463507124125706).SendMessageAsync(echomsg);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task HandleCommandAsync(SocketMessage s)
        {
            try
            {
                if (s.Channel.Id == 592463507124125706)
                {
                    t.Stop();
                    t.AutoReset = true;
                    t.Enabled = true;
                    t.Interval = 300000;
                    t.Start();
                }

                var msg = s as SocketUserMessage;
                if (msg == null) return;

                var context = new SocketCommandContext(_client, msg);
                if (Commands.giveawayinProg) { Commands.checkGiveaway(s); }

                int argPos = 0;
                if (msg.Channel.GetType() == typeof(SocketDMChannel)) { await checkKey(context); }
                if (msg.HasCharPrefix(Global.Preflix, ref argPos))
                {
                    if (msg.Content.StartsWith($"{Global.Preflix}echo")) { await EchoMessage(context); return; }
                    var result = await _service.ExecuteAsync(context, argPos, null, MultiMatchHandling.Best);

                    if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    {
                        EmbedBuilder b = new EmbedBuilder();
                        b.Color = Color.Red;
                        b.Description = $"The following info is the Command error info, `{msg.Author.Username}#{msg.Author.Discriminator}` tried to use the `{msg}` Command in {msg.Channel}: \n \n **COMMAND ERROR**: ```{result.Error.Value}``` \n \n **COMMAND ERROR REASON**: ```{result.ErrorReason}```";
                        b.Author = new EmbedAuthorBuilder();
                        b.Author.Name = msg.Author.Username + "#" + msg.Author.Discriminator;
                        b.Author.IconUrl = msg.Author.GetAvatarUrl();
                        b.Footer = new EmbedFooterBuilder();
                        b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                        b.Title = "Bot Command Error!";
                        await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.DebugChanID).SendMessageAsync("", false, b.Build());
                        await _client.GetGuild(Global.SwissBotDevGuildID).GetTextChannel(622164033902084145).SendMessageAsync("", false, b.Build());
                    }
                    await HandleCommandresult(result, msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private async Task checkKey(SocketCommandContext context)
        {
            if(context.Message.Author.Id == 259053800755691520)
            {
                if (context.Message.Content.StartsWith("*update"))
                {
                    string update = context.Message.Content.Replace("*update ", "");
                    EmbedBuilder b = new EmbedBuilder()
                    {
                        Title = "Swissbot update",
                        Description = update,
                        Color = Color.Green
                    };
                    await _client.GetGuild(Global.SwissGuildId).GetTextChannel(665520384044695563).SendMessageAsync("", false, b.Build());
                }
                
            }
        }
        private async void DCRS(object sender, ElapsedEventArgs e)
        {
            try
            {
                var r = new Random();
                string msg = "";
                try
                {
                    msg = await GenerateAIResponse(null, r);
                }
                catch { DCRS(null, null); }
                if (msg != "")
                    await _client.GetGuild(Global.SwissGuildId).GetTextChannel(592463507124125706).SendMessageAsync(msg);
            }
            catch (Exception ex)
            {
                Global.SendExeption(ex);
                Console.WriteLine(ex);
            }
        }

        internal async Task HandleCommandresult(IResult result, SocketUserMessage msg)
        {
            string logMsg = "";
            logMsg += $"[UTC TIME - {DateTime.UtcNow.ToLongDateString() + " : " + DateTime.UtcNow.ToLongTimeString()}] ";
            string completed = resultformat(result.IsSuccess);
            if (!result.IsSuccess)
                logMsg += $"COMMAND: {msg.Content} USER: {msg.Author.Username + "#" + msg.Author.Discriminator} COMMAND RESULT: {completed} ERROR TYPE: {result.Error.Value} EXCEPTION: {result.ErrorReason}";
            else
                logMsg += $"COMMAND: {msg.Content} USER: {msg.Author.Username + "#" + msg.Author.Discriminator} COMMAND RESULT: {completed}";
            var name = DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year;
            if (File.Exists(Global.CommandLogsDir + $"{Global.systemSlash}{name}.txt"))
            {
                string curr = File.ReadAllText(Global.CommandLogsDir + $"{Global.systemSlash}{name}.txt");
                File.WriteAllText(Global.CommandLogsDir + $"{Global.systemSlash}{name}.txt", $"{curr}\n{logMsg}");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Logged Command (from {msg.Author.Username})");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
            else
            {
                File.Create(Global.MessageLogsDir + $"{Global.systemSlash}{name}.txt").Close();
                File.WriteAllText(Global.CommandLogsDir + $"{Global.systemSlash}{name}.txt", $"{logMsg}");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Logged Command (from {msg.Author.Username}) and created new logfile");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
            if (result.IsSuccess)
            {
                EmbedBuilder eb = new EmbedBuilder();
                eb.Color = Color.Green;
                eb.Title = "**Command Log**";
                eb.Description = $"The Command {msg.Content.Split(' ').First()} was used in {msg.Channel.Name} by {msg.Author.Username + "#" + msg.Author.Discriminator} \n\n **Full Message** \n `{msg.Content}`\n\n **Result** \n {completed}";
                eb.Footer = new EmbedFooterBuilder();
                eb.Footer.Text = "Command Autogen";
                eb.Footer.IconUrl = _client.CurrentUser.GetAvatarUrl();
                await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.DebugChanID).SendMessageAsync("", false, eb.Build());
            }

        }
        internal static string resultformat(bool isSuccess)
        {
            if (isSuccess)
                return "Sucess";
            if (!isSuccess)
                return "Failed";
            return "Unknown";
        }

    }
}
