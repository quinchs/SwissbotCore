﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
using SwissbotCore;
using SwissbotCore.Modules;
using static SwissbotCore.CustomCommandService;
using SwissbotCore.Handlers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace SwissbotCore
{
    class CommandHandler
    {
        public static DiscordSocketClient _client;
        private CustomCommandService _service;
        private HandlerService handlerService;
        //public static AltAccountHandler althandler;
        //public static VerificationHandler verificationHandler;
        //public static HelpMessageHandler helpMessageHandler;
        //public static RoleAssignerHandler roleAssignerHandler;
        //public static SupportTicketHandler supportTicketHandler;
        internal System.Timers.Timer t = new System.Timers.Timer();
        
        Dictionary<ulong, int> ChannelPostitions = new Dictionary<ulong, int>();
        public CommandHandler(DiscordSocketClient client, CustomCommandService service, HandlerService s)
        {
            _client = client;

            _client.SetGameAsync(Global.Status, null, ActivityType.Playing);

            _client.SetStatusAsync(UserStatus.DoNotDisturb);

            _service = service;

            handlerService = s;

            _client.MessageReceived += LogMessage;

            _client.MessageReceived += HandleCommandAsync;

            //_client.UserJoined += _client_UserLeft;

            // deprecated
            //_client.UserJoined += WelcomeMessage;

            //_client.MessageReceived += responce;

            //_client.UserLeft += _client_UserLeft; 

            _client.JoinedGuild += _client_JoinedGuild;

            _client.ReactionAdded += ReactionHandler;

            _client.MessageReceived += CheckYoutube;

            _client.Ready += Init;

            SwissbotCore.Modules.ModDatabase.Start(_client);
            Console.WriteLine("[" + DateTime.Now.TimeOfDay + "] - " + "Services loaded, update init");

        }

        private async Task CheckYoutube(SocketMessage arg)
        {
            if(arg.Channel.Id == 593107179700224050)
            {
                if(arg.Author.Id == 204255221017214977)
                {
                    await arg.Channel.SendMessageAsync("<@&724842603492933633>");
                }
            }
        }

        //SocketMessage r;
        //private async void aiTrd()
        //{
        //    while (true)
        //    {

        //        await Task.Delay(5000);
        //        if (r != null)
        //        {
        //            var es = GenerateAIResponse(r, new Random()).Result;
        //            if(es == "") { es = "e"; }
        //            foreach (var word in Global.CensoredWords)
        //            {
        //                string newword = "";
        //                if (es.Contains(word))
        //                {
        //                    newword += word.ToCharArray().First();
        //                    for (int i = 1; i != word.ToCharArray().Length; i++)
        //                        newword += "\\*";
        //                    es.Replace(word, newword);
        //                }
        //            }
        //            try
        //            {
        //                await r.Channel.SendMessageAsync(es);
        //            }
        //            catch(Exception ex) { Console.WriteLine(ex); }
        //        }
        //    }
        //}

        private async Task _client_JoinedGuild(SocketGuild arg)
        {
            if (arg.Id != Global.SwissBotDevGuildID && arg.Id != Global.SwissGuildId && Global.GiveAwayGuilds.FirstOrDefault(x => x.giveawayguild.guildID == arg.Id).giveawayguild.guildOBJ != null)
            {
                await arg.LeaveAsync();

            }
        }

        //internal async Task<string> GenerateAIResponse(SocketMessage arg, Random r)
        //{
        //    bool dbug = false;
        //    string dbugmsg = "";


        //    Regex r1 = new Regex("what time is it in .*");
        //    if (arg == null)
        //    {
        //        dbugmsg += "arg was null... \n";
        //        string[] filecontUn = File.ReadAllLines(Global.aiResponsePath);
        //        //var list = filecontUn.ToList();
        //        //var d = list.FirstOrDefault(x => x.ToLower() == arg.Content.ToLower());
        //        Regex rg2 = new Regex(".*(\\d{18})>.*");
        //        string msg = filecontUn[r.Next(0, filecontUn.Length)];
        //        //if (d != "") { msg = d; }
        //        if (rg2.IsMatch(msg))
        //        {
        //            dbugmsg += "Found a ping in there, sanitizing..\n";
        //            var rm = rg2.Match(msg);
        //            var user = _client.GetGuild(Global.SwissGuildId).GetUser(Convert.ToUInt64(rm.Groups[1].Value));
        //            msg = msg.Replace(rm.Groups[0].Value, $"**(non-ping: {user.Username}#{user.Discriminator})**");
        //        }
        //        if (msg == "") { return filecontUn[r.Next(0, filecontUn.Length)]; }
        //        else { return msg; }
        //    }
        //    else
        //    {
        //        string oMsg = arg.Content.ToLower();
        //        if (arg.Content.StartsWith("*debug "))
        //        {
        //            dbug = true;
        //            oMsg = oMsg.Replace("*debug ", "");
        //        }
        //        dbugmsg += "Arg was not null. starting AI responces..\n";
        //        try
        //        {
        //            if (r1.IsMatch(oMsg.ToLower()))
        //            {
        //                dbugmsg += "User looking for the time. starting up Time API..\n";
        //                HttpClient c = new HttpClient();
        //                string link = $"https://www.google.com/search?q={oMsg.ToLower().Replace(' ', '+')}";
        //                var req = await c.GetAsync(link);
        //                var resp = await req.Content.ReadAsStringAsync();
        //                Regex x = new Regex(@"<div class=""BNeawe iBp4i AP7Wnd""><div><div class=""BNeawe iBp4i AP7Wnd"">(.*?)<\/div><\/div>");
        //                if (x.IsMatch(resp))
        //                {
        //                    string time = x.Match(resp).Groups[1].Value;
        //                    c.Dispose();
        //                    dbugmsg += "Found the time to be " + time + "\n";
        //                    return $"The current time in {oMsg.ToLower().Replace("what time is it in ", "")} is {time}";
        //                }
        //                else { c.Dispose(); return $"Sorry buddy but could not get the time for {arg.Content.ToLower().Replace("what time is it in ", "")}"; }
        //            }
        //            //if (oMsg.ToLower() == "are you gay") { return "no ur gay lol"; }
        //            //if (oMsg.ToLower() == "how is your day going") { return "kinda bad. my creator beats me and hurts me help"; }
        //            //if (oMsg.ToLower() == "are you smart") { return "smarter than your mom lol goteme"; }
        //            //if (oMsg.ToLower() == "hi") { return "hello mortal"; }
        //            string[] filecontUn = File.ReadAllLines(Global.aiResponsePath);
        //            for (int i = 0; i != filecontUn.Length; i++)
        //                filecontUn[i] = filecontUn[i].ToLower();
        //            Regex rg2 = new Regex(".*?[@!&](\\d{18}|\\d{17})>.*?");
        //            string msg = "";
        //            var ar = filecontUn.Select((b, i) => b == oMsg ? i : -1).Where(i => i != -1).ToArray();
        //            Random ran = new Random();
        //            dbugmsg += $"Found {ar.Length} indexed responces for the question\n";
        //            if (ar.Length != 0)
        //            {
        //                var ind = (ar[ran.Next(0, ar.Length)]);
        //                if (ind != 0 && (ind + 1) < filecontUn.Length)
        //                    msg = filecontUn[ind + 1];
        //                dbugmsg += $"Picked the best answer: {msg}\n";
        //            }
        //            else
        //            {
        //                dbugmsg += $"Question has 0 indexed responces, starting word analisys...\n";
        //                var words = oMsg.Split(' ');
        //                var query = from state in filecontUn.AsParallel()
        //                            let StateWords = state.Split(' ')
        //                            select (Word: state, Count: words.Intersect(StateWords).Count());

        //                var sortedDict = from entry in query orderby entry.Count descending select entry;
        //                string rMsg = sortedDict.First().Word;
        //                var s = sortedDict.Where(x => x.Count >= 1);
        //                dbugmsg += $"Found common phrase based off of {s.Count()} results: {rMsg}\n";
        //                var reslt = filecontUn.Select((b, i) => b == rMsg ? i : -1).Where(i => i != -1).ToArray();
        //                if (reslt.Length != 0)
        //                {
        //                    var ind = (reslt[ran.Next(0, reslt.Length)]);
        //                    if (ind != 0 && (ind + 1) < filecontUn.Length)
        //                        msg = filecontUn[ind + 1];
        //                    dbugmsg += $"Picked the best answer: {msg}\n";
        //                }
        //                else { msg = rMsg; }
        //                //string[] words = oMsg.Split(' ');
        //                //Dictionary<string, int> final = new Dictionary<string, int>();
        //                //foreach (var state in filecontUn)
        //                //{
        //                //    int count = 0;
        //                //    foreach (var word in state.Split(' '))
        //                //    {
        //                //        if (words.Contains(word))
        //                //            count++;
        //                //    }
        //                //    if (!final.Keys.Contains(state) && count != 0)
        //                //        final.Add(state, count);
        //                //}
        //                //string res = sortedDict.First().Key;

        //            }
        //            if (msg == "") { msg = filecontUn[r.Next(0, filecontUn.Length)]; }

        //            if (rg2.IsMatch(msg))
        //            {
        //                var rem = rg2.Matches(msg);
        //                foreach(Match rm in rem)
        //                {
        //                    var user = _client.GetGuild(Global.SwissGuildId).GetUser(Convert.ToUInt64(rm.Groups[1].Value));
        //                    var role = _client.GetGuild(Global.SwissGuildId).GetRole(Convert.ToUInt64(rm.Groups[1].Value));
        //                    if (user != null)
        //                    {
        //                        msg = msg.Replace("<@" + rm.Groups[1].Value + ">", $"**(non-ping: {user.Username}#{user.Discriminator})**");
        //                        msg = msg.Replace("<@!" + rm.Groups[1].Value + ">", $"**(non-ping: {user.Username}#{user.Discriminator})**");
        //                        dbugmsg += "Sanitized ping.. \n";
        //                    }
        //                    else if (role != null)
        //                    {
        //                        msg = msg.Replace("<@&" + rm.Groups[1].Value + ">", $"**(non-ping: {role.Name})**");
        //                    }
        //                    else
        //                    {
        //                        try
        //                        {
        //                            var em = await _client.GetGuild(Global.SwissGuildId).GetEmoteAsync(Convert.ToUInt64(rm.Groups[1].Value));
        //                            if (em == null)
        //                            {
        //                                dbugmsg += $"Could not find a user for {rm.Value}, assuming emoji or user is not in server..\n";
        //                                msg = msg.Replace(rm.Groups[0].Value, $"**(non-ping: {rm.Value})**");
        //                            }
        //                        }
        //                        catch (Exception ex) { dbugmsg += $"{ex.Message}.. \n"; }
        //                    }
        //                }
        //            }
        //            if (msg.Contains("@everyone")) { msg = msg.Replace("@everyone", "***(Non-ping @every0ne)***"); }
        //            if (msg.Contains("@here")) { msg = msg.Replace("@here", "***(Non-ping @h3re)***"); }
        //            dbugmsg += "Sanitized for @h3re and @every0ne\n";
                    
        //            if (dbug)
        //            {
        //                EmbedBuilder eb = new EmbedBuilder()
        //                {
        //                    Color = Color.Orange,
        //                    Title = "Ai Debug",
        //                    Author = new EmbedAuthorBuilder()
        //                    {
        //                        Name = _client.CurrentUser.ToString(),
        //                        IconUrl = _client.CurrentUser.GetAvatarUrl()
        //                    },
        //                    Description = "```Ai Debug Log```\n`" + dbugmsg + "`",
        //                };
        //                await arg.Channel.SendMessageAsync("", false, eb.Build());
        //            }
                        
                    
        //            return msg;
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex);
        //            return "uh oh ai broke stinkie";
        //        }
        //    }
        //}
        //private async Task responce(SocketMessage arg)
        //{
        //    if (arg.Channel.Id == Global.BotAiChanID && !arg.Author.IsBot)
        //    {
        //        var d = arg.Channel.EnterTypingState();
        //        try
        //        {
        //            Random r = new Random();
        //            var mu = arg.MentionedUsers.FirstOrDefault(x => x.Username == _client.CurrentUser.Username);
        //            if (r.Next(1, 2) == 1 || mu != null)
        //            {
        //                string msg = await GenerateAIResponse(arg, r);
        //                foreach (var word in Global.CensoredWords)
        //                {
        //                    string newword = "";
        //                    if (msg.Contains(word))
        //                    {
        //                        newword += word.ToCharArray().First();
        //                        for (int i = 1; i != word.ToCharArray().Length; i++)
        //                            newword += "\\*";
        //                        msg.Replace(word, newword);
        //                    }
        //                }
        //                if (msg != "")
        //                {
        //                    if (msg != ("*terminate"))
        //                        await arg.Channel.SendMessageAsync(msg);
        //                }
        //            }
        //            d.Dispose();
        //        }
        //        catch (Exception ex)
        //        {
        //            Global.SendExeption(ex);
        //            d.Dispose();
        //        }
        //    }
        //}

        private async Task ReactionHandler(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
               
            try
            {
                await checkSub(arg1, arg2, arg3);
            }
            catch (Exception ex)
            {
                Global.ConsoleLog($"Reaction handler error: {ex.Message} \n {ex.StackTrace}", ConsoleColor.Red);
                Global.SendExeption(ex);
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
        private async Task Init()
        {
            Global.ConsoleLog("Starting Init... \n\n Updating UserCounts...", ConsoleColor.DarkCyan);

            try
            {
                Console.WriteLine("Starting handler loading...");
                await StartHandlers();
                Global.UserCount = _client.GetGuild(Global.SwissGuildId).Users.Count;
                try { await AddUnVert(); } catch (Exception ex) { Global.ConsoleLog($"Ex,{ex} ", ConsoleColor.Red); }
                foreach (var arg in _client.Guilds)
                {
                    if (arg.Id != Global.SwissBotDevGuildID && arg.Id != Global.SwissGuildId && arg.Id != 726857672942420070)
                    {
                        try { await arg.DeleteAsync(); }
                        catch { await arg.LeaveAsync(); }
                    }
                }

               

                try { UserSubCashing(); } catch (Exception ex) { Console.WriteLine(ex); }
                //await MassWelcome();
                //LoadLLogs();
                Global.ConsoleLog("Finnished Init!", ConsoleColor.Black, ConsoleColor.DarkGreen);
                foreach(var chan in _client.GetGuild(Global.SwissGuildId).Channels)
                {
                    ChannelPostitions.Add(chan.Id, chan.Position);
                }
                Console.WriteLine("[" + DateTime.Now.TimeOfDay + "] - " + "Command Handler ready");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public bool FirstPass = false;
        public async Task StartHandlers()
        {
            handlerService.CreateHandlers();
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
       
        private async Task AddUnVert()
        {
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
        private async Task LogMessage(SocketMessage arg)
        {
            Global.ConsoleLog("Message from: " + arg.Author, ConsoleColor.Magenta);
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
                if (context.Guild == null)
                    return;
                if (context.Guild.Id == 726857672942420070)
                    return;
                if (Modules.Commands.giveawayinProg) { Modules.Commands.checkGiveaway(s); }

                int argPos = 0;
                if (msg.Channel.GetType() == typeof(SocketDMChannel)) { await checkKey(context); }
                var ca = msg.Content.ToCharArray();
                if (ca.Length == 0)
                    return;
                if (_service.ContainsUsedPrefix(msg.Content))
                {
                    new Thread(async ()  => 
                    {
                        if (msg.Content.StartsWith($"{Global.Preflix}echo")) { await EchoMessage(context); return; }
                        var result = await _service.ExecuteAsync(context);
                        Global.ConsoleLog($"Executed {context.Message.Content} with a result of {result.Result}!", ConsoleColor.Cyan);
                        if (result.MultipleResults)
                        {
                            foreach(var r in result.Results)
                            {
                                if (r.Result == CommandStatus.Unknown || r.Result == CommandStatus.Error)
                                {
                                    EmbedBuilder ce = new EmbedBuilder()
                                    {
                                        Title = "Uh oh... :(",
                                        Description = "Looks like the command didnt work :/ ive dm'ed quin the errors and he should be fixing it soon.",
                                        Color = Color.Red
                                    };
                                    await msg.Channel.SendMessageAsync("", false, ce.Build());

                                    await _client.GetGuild(Global.SwissGuildId).GetUser(259053800755691520).SendMessageAsync("Command: " + msg.Content);
                                    File.WriteAllText(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "error.txt", r.Exception.ToString());
                                    await _client.GetUser(259053800755691520).SendFileAsync(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "error.txt");

                                    EmbedBuilder b = new EmbedBuilder();
                                    b.Color = Color.Red;
                                    b.Description = $"The following info is the Command error info, `{msg.Author.Username}#{msg.Author.Discriminator}` tried to use the `{msg}` Command in {msg.Channel}: \n \n **COMMAND ERROR REASON**: ```{r.Exception.Message}```";
                                    b.Author = new EmbedAuthorBuilder();
                                    b.Author.Name = msg.Author.Username + "#" + msg.Author.Discriminator;
                                    b.Author.IconUrl = msg.Author.GetAvatarUrl();
                                    b.Footer = new EmbedFooterBuilder();
                                    b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                                    b.Title = "Bot Command Error!";
                                    await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.DebugChanID).SendMessageAsync("", false, b.Build());
                                    await _client.GetGuild(Global.SwissBotDevGuildID).GetTextChannel(622164033902084145).SendMessageAsync("", false, b.Build());
                                }
                            }
                        }
                        else if (result.Result == CommandStatus.Unknown || result.Result == CommandStatus.Error)
                        {
                            EmbedBuilder ce = new EmbedBuilder()
                            {
                                Title = "Uh oh... :(",
                                Description = "Looks like the command didnt work :/ ive dm'ed quin the errors and he should be fixing it soon.",
                                Color = Color.Red
                            };
                            await msg.Channel.SendMessageAsync("", false, ce.Build());
                            
                            await _client.GetGuild(Global.SwissGuildId).GetUser(259053800755691520).SendMessageAsync("Command: " + msg.Content);
                            File.WriteAllText(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "error.txt", result.Exception.ToString());
                            await _client.GetUser(259053800755691520).SendFileAsync(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "error.txt");

                            EmbedBuilder b = new EmbedBuilder();
                            b.Color = Color.Red;
                            b.Description = $"The following info is the Command error info, `{msg.Author.Username}#{msg.Author.Discriminator}` tried to use the `{msg}` Command in {msg.Channel}: \n \n **COMMAND ERROR REASON**: ```{result.Exception.Message}```";
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
                    }).Start();
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
        //private async void DCRS(object sender, ElapsedEventArgs e)
        //{
        //    try
        //    {
        //        var r = new Random();
        //        string msg = "";
        //        try
        //        {
        //            msg = await GenerateAIResponse(null, r);
        //        }
        //        catch { DCRS(null, null); }
        //        if (msg != "")
        //            await _client.GetGuild(Global.SwissGuildId).GetTextChannel(592463507124125706).SendMessageAsync(msg);
        //    }
        //    catch (Exception ex)
        //    {
        //        Global.SendExeption(ex);
        //        Console.WriteLine(ex);
        //    }
        //}

        internal async Task HandleCommandresult(ICommandResult result, SocketUserMessage msg)
        {
            //string logMsg = "";
            //logMsg += $"[UTC TIME - {DateTime.UtcNow.ToLongDateString() + " : " + DateTime.UtcNow.ToLongTimeString()}] ";
            string completed = resultformat(result.IsSuccess);
            //if (!result.IsSuccess)
            //    logMsg += $"COMMAND: {msg.Content} USER: {msg.Author.Username + "#" + msg.Author.Discriminator} COMMAND RESULT: {completed} ERROR TYPE: EXCEPTION: {result.Exception}";
            //else
            //    logMsg += $"COMMAND: {msg.Content} USER: {msg.Author.Username + "#" + msg.Author.Discriminator} COMMAND RESULT: {completed}";
            //var name = DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year;
            //if (File.Exists(Global.CommandLogsDir + $"{Global.systemSlash}{name}.txt"))
            //{
            //    string curr = File.ReadAllText(Global.CommandLogsDir + $"{Global.systemSlash}{name}.txt");
            //    File.WriteAllText(Global.CommandLogsDir + $"{Global.systemSlash}{name}.txt", $"{curr}\n{logMsg}");
            //    Console.ForegroundColor = ConsoleColor.Magenta;
            //    Console.WriteLine($"Logged Command (from {msg.Author.Username})");
            //    Console.ForegroundColor = ConsoleColor.DarkGreen;
            //}
            //else
            //{
            //    File.Create(Global.MessageLogsDir + $"{Global.systemSlash}{name}.txt").Close();
            //    File.WriteAllText(Global.CommandLogsDir + $"{Global.systemSlash}{name}.txt", $"{logMsg}");
            //    Console.ForegroundColor = ConsoleColor.Cyan;
            //    Console.WriteLine($"Logged Command (from {msg.Author.Username}) and created new logfile");
            //    Console.ForegroundColor = ConsoleColor.DarkGreen;
            //}
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
