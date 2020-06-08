using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using Discord;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using System.Timers;
using SwissbotCore;
using static SwissbotCore.CustomCommandService;
using System.Runtime.InteropServices.ComTypes;
using RedditNet.Extensions;
using SwissbotCore.Handlers;

namespace SwissbotCore.Modules
{
    [DiscordCommandClass()]
    public class ModDatabase : CommandModuleBase
    {
        static string ModLogsPath = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}Modlogs.json";
        static internal System.Timers.Timer autoSlowmode = new System.Timers.Timer() { Enabled = false, AutoReset = true, Interval = 1000 };
        public static ModlogsJson currentLogs { get; set; }
        static Dictionary<ulong, int> sList = new Dictionary<ulong, int>();
        static DiscordSocketClient _client;
        static Dictionary<ulong, int> currentSlowmodeList = new Dictionary<ulong, int>();
        public static async Task Start( DiscordSocketClient client)
        {
            currentLogs = new ModlogsJson() { Users = new List<User>() };
            _client = client;
            if (!File.Exists(ModLogsPath)) { File.Create(ModLogsPath).Close(); }
            //load logs
            currentLogs = LoadModLogs();

            //create muted role if it doesnt exist
            //change text channel perms for muted role if not set
            //client.MessageReceived += AutoSlowmode;
            //autoSlowmode.Enabled = true;
            //autoSlowmode.Elapsed += AutoSlowmode_Elapsed;
        }

        private static async void AutoSlowmode_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Global.AutoSlowmodeToggle)
            {
                foreach (var item in sList.ToList())
                {
                    if (item.Value >= Global.AutoSlowmodeTrigger)
                    {
                        var chan = _client.GetGuild(Global.SwissGuildId).GetTextChannel(item.Key);
                        var aChan = _client.GetGuild(Global.SwissGuildId).GetTextChannel(664606058592993281);
                        var mLink = await chan.GetMessagesAsync(1).FlattenAsync();

                        if (chan.SlowModeInterval > 0) //the channel has slowmode already
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
                        if (chan.SlowModeInterval >= 5)
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
                            if (currentSlowmodeList.Keys.Contains(chan.Id))
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

        private static async Task AutoSlowmode(SocketMessage arg)
        {
            if (sList.ContainsKey(arg.Channel.Id))
            {
                sList[arg.Channel.Id]++;
            }
            else
            {
                sList.Add(arg.Channel.Id, 1);
            }
        }

        static ModlogsJson LoadModLogs()
        {
            try
            {
                var d = JsonConvert.DeserializeObject<ModlogsJson>(File.ReadAllText(ModLogsPath));
                if(d == null) { throw new Exception(); }
                return d;
            }
            catch(Exception ex)
            {
                return new ModlogsJson() { Users = new List<User>() };
            }
             
        }
        static public void SaveModLogs()
        {
            string json = JsonConvert.SerializeObject(currentLogs);
            File.WriteAllText(ModLogsPath, json);
        }
        public class ModlogsJson
        {
            public List<User> Users { get; set; }
        }
        public class User
        {
            public List<UserModLogs> Logs { get; set; }
            public ulong userId { get; set; }
            public string username { get; set; }
        }
        public class UserModLogs
        {
            public string Reason { get; set; }
            public Action Action { get; set; }
            public ulong ModeratorID { get; set; }
            public string Date { get; set; }
        }

        public enum Action
        {
            Warned,
            Kicked,
            Banned,
            Muted,
            voiceban
        }

        static async Task AddModlogs(ulong userID, Action action, ulong ModeratorID, string reason, string username)
        {
            if(currentLogs.Users.Any(x => x.userId == userID))
            {
                currentLogs.Users[currentLogs.Users.FindIndex(x => x.userId == userID)].Logs.Add(new UserModLogs()
                {
                    Action = action,
                    ModeratorID = ModeratorID,
                    Reason = reason,
                    Date = DateTime.UtcNow.ToString("r")
                });
            }
            else
            {
                currentLogs.Users.Add(new User()
                {
                    Logs = new List<UserModLogs>()
                    {
                        { new UserModLogs(){
                            Action = action,
                            ModeratorID = ModeratorID,
                            Reason = reason,
                            Date = DateTime.UtcNow.ToString("r")
                        } }
                    },
                    userId = userID,
                    username = username
                });
            }
            SaveModLogs();
        }
        public async Task<bool> HasPerms(SocketGuildUser user)
        {
            if (user.Guild.GetRole(Global.ModeratorRoleID).Position <= user.Hierarchy)
                return true;
            else if (user.Guild.GetUser(user.Id).Roles.Contains(user.Guild.GetRole(Global.DeveloperRoleId)))
                return true;
            else
                return false;
        }

        public async Task CreateAction(string[] args, Action type, SocketCommandContext curContext)
        {
            if (!HasPerms(curContext.Guild.GetUser(curContext.Message.Author.Id)).Result)
            {
                await curContext.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "You do not have permission to execute this command",
                    Description = "You do not have the valid permission to execute this command",
                    Color = Color.Red
                }.Build());
                return;
            }

            string typeName = Enum.GetName(typeof(Action), type);

            string user, reason;

            if (args.Length == 1)
            {
                await curContext.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "Give me a reason!",
                    Description = "You need to provide a reason",
                    Color = Color.Red
                }.Build());
            }
            if (args.Length == 0)
            {
                await curContext.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = $"Who do you want to {typeName}?",
                    Description = "Mention someone or provide an id!",
                    Color = Color.Red
                }.Build());
            }
            if (args.Length > 1)
            {
                user = args[0];
                reason = string.Join(' ', args).Replace(user + " ", "");
                Regex r = new Regex("(\\d{18}|\\d{17})");
                if (!r.IsMatch(user))
                {
                    await curContext.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                    {
                        Title = "Invalid ID",
                        Description = "The ID you provided is invalid!",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                ulong id;
                try
                {
                    id = Convert.ToUInt64(r.Match(user).Groups[1].Value);
                }
                catch(Exception ex)
                {
                    await curContext.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                    {
                        Title = "Invalid ID",
                        Description = "The ID you provided is invalid!",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                var usr = curContext.Guild.GetUser(id);
                if(usr == null)
                {
                    await curContext.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                    {
                        Title = "Thats an invalid user, they might not be in the discord server",
                        Description = "we cant use commands on people who are not here :/",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                if (usr.Hierarchy >= _client.GetGuild(Global.SwissGuildId).GetUser(Context.Message.Author.Id).Hierarchy)
                {
                    await curContext.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                    {
                        Title = "You do not have permission to execute this command",
                        Description = "You do not have the valid permission to execute this command",
                        Color = Color.Red
                    }.Build());
                    return;
                }

                if (usr == null)
                {
                    await curContext.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                    {
                        Title = "Invalid ID",
                        Description = "The ID you provided is invalid!",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                await AddModlogs(id, type, curContext.Message.Author.Id, reason, usr.ToString());



                EmbedBuilder b = new EmbedBuilder()
                {
                    Title = $"You have been **{typeName}** on **{curContext.Guild.Name}**",
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        { new EmbedFieldBuilder(){
                            Name = "Moderator",
                            Value = curContext.Message.Author.ToString(),
                            IsInline = true
                        } },
                        {new EmbedFieldBuilder()
                        {
                            Name = "Reason",
                            Value = reason,
                            IsInline = true
                        } }
                    }
                };
                if (type is Action.Banned)
                {
                    b.Footer = new EmbedFooterBuilder()
                    {
                        Text = "To appeal your ban, visit https://bans.swissdev.team/"
                    };
                }

                Embed b2 = new EmbedBuilder()
                {
                    Title = $"Successfully **{typeName}** {usr.ToString()} ({usr.Id})",
                    Description = $"The user {usr.Mention} has been successfully **{typeName}**",
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        { new EmbedFieldBuilder()
                        {
                            Name = "Moderator",
                            Value = curContext.Message.Author.ToString(),
                            IsInline = true
                        } },
                        {new EmbedFieldBuilder()
                        {
                            Name = "Reason",
                            Value = reason,
                            IsInline = true
                        } }
                    }
                }.Build();
                bool notif = true;
                try
                {
                    await usr.SendMessageAsync("", false, b.Build());
                }
                catch(Exception ex)
                {
                    notif = false;
                    await Context.Channel.SendMessageAsync("Couldn't notify " + usr.ToString() + " that they were " + typeName);
                }
                await curContext.Channel.SendMessageAsync("", false, b2);
                if (type is Action.Kicked)
                    await usr.KickAsync(reason);
                if (type is Action.Banned)
                {
                    await usr.BanAsync(7, reason);
                    await Context.Guild.GetTextChannel(657691746171355151).SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Color = Color.Red,
                        Author = new EmbedAuthorBuilder()
                        {
                            IconUrl = usr.GetAvatarUrl(),
                            Name = usr.ToString(),
                        } ,
                        Fields = new List<EmbedFieldBuilder>()
                        {
                            {new EmbedFieldBuilder()
                            {
                                Name = "Banned by",
                                Value = Context.Message.Author.ToString(),
                            } },
                            {new EmbedFieldBuilder()
                            {
                                Name = "Reason",
                                Value = reason,
                            } },
                            {new EmbedFieldBuilder()
                            {
                                Name = "Notified in Dm's",
                                Value = notif.ToString(),
                            } }
                        }
                    }.Build());
                }
            }
        }
        [DiscordCommand("clearlogs", new char[] { '?', '*' }, RequiredPermission = true, description ="Clears the log of a user", commandHelp = "Use `(PREFIX)clearlogs <@user>` to view there modlogs\nUse `(PREFIX)clearlogs <@user> <lognumber>` to lear a log")]
        public async Task clearwarn(string user)
        {
            //check if user exists
            //check logs -> if has logs show
            //prompt for log number
            if (!HasExecutePermission)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "You do not have permission to execute this command",
                    Description = "You do not have the valid permission to execute this command",
                    Color = Color.Red
                }.Build());
                return;

            }
            Regex r = new Regex("(\\d{18}|\\d{17})");
            if (!r.IsMatch(user))
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "Invalid ID",
                    Description = "The ID you provided is invalid!",
                    Color = Color.Red
                }.Build());
                return;
            }
            ulong id;
            try
            {
                id = Convert.ToUInt64(r.Match(user).Groups[1].Value);
            }
            catch 
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "Invalid ID",
                    Description = "The ID you provided is invalid!",
                    Color = Color.Red
                }.Build());
                return;
            }

            if(currentLogs.Users.Any(x => x.userId == id))
            {
                var usrlogs = currentLogs.Users[currentLogs.Users.FindIndex(x => x.userId == id)];
                string usrnm = Context.Guild.GetUser(usrlogs.userId) == null ? usrlogs.username : Context.Guild.GetUser(usrlogs.userId).ToString();
                EmbedBuilder b = new EmbedBuilder()
                {
                    Title = $"{usrlogs.username} logs",
                    Color = Color.DarkMagenta,
                    Description = $"Modlogs for {usrnm},\nTo remove a log type `{Global.Preflix}clearlogs <user> <log number>`\n",
                    Fields = new List<EmbedFieldBuilder>()
                };
                for(int i = 0; i != usrlogs.Logs.Count; i++)
                {
                    var log = usrlogs.Logs[i];
                    b.Fields.Add(new EmbedFieldBuilder()
                    {
                        IsInline = false,
                        Name = (i + 1).ToString(),
                        Value = 
                        $"**{log.Action}**\n" +
                        $"Reason: {log.Reason}\n" +
                        $"Moderator: <@{log.ModeratorID}> ({log.ModeratorID.ToString()}\n" +
                        $"Date: {log.Date}"
                    });
                }
                await Context.Channel.SendMessageAsync("", false, b.Build());
            }
            else
            {
                EmbedBuilder b = new EmbedBuilder()
                {
                    Title = "User has no logs!",
                    Description = $"The user <@{id}> has no logs!",
                    Color = Color.Red,
                };
                await Context.Channel.SendMessageAsync("", false, b.Build());
            }
        }
        [DiscordCommand("clearlogs", new char[] { '?', '*' }, RequiredPermission = true)]
        public async Task clearwarn(string user, int number)
        {
            if (!HasExecutePermission)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "You do not have permission to execute this command",
                    Description = "You do not have the valid permission to execute this command",
                    Color = Color.Red
                }.Build());
                return;
            }
            Regex r = new Regex("(\\d{18}|\\d{17})");
            if (!r.IsMatch(user))
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "Invalid ID",
                    Description = "The ID you provided is invalid!",
                    Color = Color.Red
                }.Build());
                return;
            }
            ulong id;
            try
            {
                id = Convert.ToUInt64(r.Match(user).Groups[1].Value);
            }
            catch
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "Invalid ID",
                    Description = "The ID you provided is invalid!",
                    Color = Color.Red
                }.Build());
                return;
            }

            if (currentLogs.Users.Any(x => x.userId == id))
            {
                var usrlogs = currentLogs.Users[currentLogs.Users.FindIndex(x => x.userId == id)];
                usrlogs.Logs.RemoveAt(number - 1);
                string usrnm = Context.Guild.GetUser(usrlogs.userId) == null ? usrlogs.username : Context.Guild.GetUser(usrlogs.userId).ToString();
                EmbedBuilder b = new EmbedBuilder()
                {
                    Title = $"Successfully cleared a log for **{usrnm}**",
                    Color = Color.DarkMagenta,
                    Description = $"Modlogs for {usrlogs.username},\nTo remove a log type `{Global.Preflix}clearlog <user> <log number>`\n",
                    Fields = new List<EmbedFieldBuilder>()
                };
                for (int i = 0; i != usrlogs.Logs.Count; i++)
                {
                    var log = usrlogs.Logs[i];
                    b.Fields.Add(new EmbedFieldBuilder()
                    {
                        IsInline = false,
                        Name = (i + 1).ToString(),
                        Value =
                        $"**{log.Action}**\n" +
                        $"Reason: {log.Reason}\n" +
                        $"Moderator: <@{log.ModeratorID}> ({log.ModeratorID.ToString()}\n" +
                        $"Date: {log.Date}"
                    });
                }
                await Context.Channel.SendMessageAsync("", false, b.Build());
            }
            else
            {
                EmbedBuilder b = new EmbedBuilder()
                {
                    Title = "User has no logs!",
                    Description = $"The user <@{id}> has no logs!",
                    Color = Color.Red,
                };
                await Context.Channel.SendMessageAsync("", false, b.Build());
            }

        }

        [DiscordCommand("warn", new char[] { '?', '*'}, RequiredPermission = true, description = "Warns a user", commandHelp = "Use `(PREFIX)warn <@user> <reason>` to warn a user")]
        public async Task warn(params string[] args)
        {
            await CreateAction(args, Action.Warned, Context);
        }
        [DiscordCommand("kick", new char[] { '?', '*' }, RequiredPermission = true, description = "Kicks a user", commandHelp = "Use `(PREFIX)kick <@user> <reason>` to kick a user")]
        public async Task kick(params string[] args)
        {
            await CreateAction(args, Action.Kicked, Context);
        }
        [DiscordCommand("ban", new char[] { '?', '*' }, RequiredPermission = true, description = "Bans a user", commandHelp = "Use `(PREFIX)ban <@user> <reason>` to ban a user")]
        public async Task ban(params string[] args)
        {
            await CreateAction(args, Action.Banned, Context);
        }

        [DiscordCommand("unmute",
            prefixes =  new char[] { '?', '*'},
            RequiredPermission = true,
            description =  "Unmutes a member", 
            BotCanExecute = false,
            commandHelp = "Use `(PREFIX)unmute <@user>`")]
        public async Task Unmute(params string[] args)
        {
            if(args.Length == 0)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "Who..?",
                    Description = "Who do you want me to unmute",
                    Color = Color.Red
                }.Build());
                return;
            }
            if (args.Length >= 2)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "W-w-wh-what?",
                    Description = "thats way to many arguments buddy....",
                    Color = Color.Red
                }.Build());
                return;
            }
            if (!HasExecutePermission)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "You do not have permission to execute this command",
                    Description = "You do not have the valid permission to execute this command",
                    Color = Color.Red
                }.Build());
                return;
            }    
            Regex r = new Regex("(\\d{18}|\\d{17})");
            if (!r.IsMatch(args.First()))
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "Invalid ID",
                    Description = "The ID you provided is invalid!",
                    Color = Color.Red
                }.Build());
                return;
            }
            ulong id;
            try
            {
                id = Convert.ToUInt64(r.Match(args.First()).Groups[1].Value);
            }
            catch
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "Invalid ID",
                    Description = "The ID you provided is invalid!",
                    Color = Color.Red
                }.Build());
                return;
            }
            var usr = Context.Guild.GetUser(id);
            if(usr == null)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "That user isnt in the server :/",
                    Description = "who? i cant get there details lol...",
                    Color = Color.Red
                }.Build());
                return;
            }
            if (usr.Roles.Any(x => x.Id == Global.MutedRoleID))
            {
                var role = Context.Guild.GetRole(Global.MutedRoleID);
                await usr.RemoveRoleAsync(role);
                MutedHandler.CurrentMuted.Remove(usr.Id);
                Global.SaveMutedUsers();
                Embed b2 = new EmbedBuilder()
                {
                    Title = $"Successfully **Unmuted** user **{usr.ToString()}**",
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        { new EmbedFieldBuilder(){
                            Name = "Moderator",
                            Value = Context.Message.Author.ToString(),
                            IsInline = true
                        } }
                    }
                }.Build();
                await Context.Channel.SendMessageAsync("", false, b2);
            }
            else
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "That user isnt muted",
                    Description = "There not muted lol. dont know what else you want me to say",
                    Color = Color.Red
                }.Build());
                return;
            }
        }
        [DiscordCommand("mute", new char[] { '?', '*' }, RequiredPermission = true, commandHelp = "Use `(PREFIX)mute <@user> <time> <reason>`\nTime formats are structured like this: `<x><H/D/M/S>`", description = "Mutes a user for x time")]
        public async Task mute(params string[] args)
        {
            if (!HasPerms(Context.Guild.GetUser(Context.Message.Author.Id)).Result)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "You do not have permission to execute this command",
                    Description = "You do not have the valid permission to execute this command",
                    Color = Color.Red
                }.Build());
                return;
            }
            if(args.Length == 1)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "Give me a time!",
                    Description = $"if you wanted to mute for 10 minutes use `{Global.Preflix}mute <user> 10m`",
                    Color = Color.Red
                }.Build());
                return;
            }
            if(args.Length == 2)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "Give me a Reason!",
                    Description = $"You need to provide a reason",
                    Color = Color.Red
                }.Build());
                return;
            }
            if(args.Length > 2)
            {
                string[] formats = { @"h\h", @"s\s", @"m\m\ s\s", @"h\h\ m\m\ s\s", @"m\m", @"h\h\ m\m" };
                string user, time, reason;
                user = args[0];
                time = args[1];
                Regex r = new Regex("(\\d{18})");
                if (!r.IsMatch(user))
                {
                    await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                    {
                        Title = "Invalid ID",
                        Description = "The ID you provided is invalid!",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                ulong id;
                try
                {
                    id = Convert.ToUInt64(r.Match(user).Groups[1].Value);
                }
                catch (Exception ex)
                {
                    await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                    {
                        Title = "Invalid ID",
                        Description = "The ID you provided is invalid!",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                var usr = Context.Guild.GetUser(id);
                if(usr.Roles.Any(x => x.Id == Global.MutedRoleID))
                {
                    await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                    {
                        Title = "That user is muted!",
                        Description = "There already muted lol <3",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                reason = string.Join(' ', args).Replace($"{user} {time} ", "");
                TimeSpan t = TimeSpan.ParseExact(time, formats, null);
                var dt = DateTime.UtcNow.Add(t);
                //Timer tmr = new Timer()
                //{
                //    AutoReset = false,
                //    Interval = t.TotalMilliseconds
                //};
                string guildName = Context.Guild.Name;
                await usr.AddRoleAsync(Context.Guild.GetRole(Global.MutedRoleID));

                //tmr.Elapsed += async (object send, ElapsedEventArgs arg) =>
                //{
                //    try
                //    {
                //        await usr.RemoveRoleAsync(Context.Guild.GetRole(Global.MutedRoleID));
                //    }
                //    catch(Exception ex)
                //    {
                //        Console.WriteLine(ex);
                //    }
                //    try
                //    {
                //        await usr.SendMessageAsync($"**You have been unmuted on {guildName}**");
                //    }
                //    catch { }
                //};
                Embed b = new EmbedBuilder()
                {
                    Title = $"You have been **Muted** on **{guildName}** for **{t.ToString()}",
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        { new EmbedFieldBuilder(){
                            Name = "Moderator",
                            Value = Context.Message.Author.ToString(),
                            IsInline = true
                        } },
                        {new EmbedFieldBuilder()
                        {
                            Name = "Reason",
                            Value = reason,
                            IsInline = true
                        } }
                    }
                }.Build();
                Embed b2 = new EmbedBuilder()
                {
                    Title = $"Successfully **Muted** user **{usr.ToString()}**",
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        { new EmbedFieldBuilder(){
                            Name = "Moderator",
                            Value = Context.Message.Author.ToString(),
                            IsInline = true
                        } },
                        {new EmbedFieldBuilder()
                        {
                            Name = "Reason",
                            Value = reason,
                            IsInline = true
                        } }
                    }
                }.Build();
                await Context.Channel.SendMessageAsync("", false, b2);
                try
                {
                    await usr.SendMessageAsync("", false, b);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync($"Couldn't notify **{usr.ToString()}** of their mute");
                }
                await AddModlogs(id, Action.Muted, Context.Message.Author.Id, reason, usr.ToString());
                MutedHandler.AddNewMuted(usr.Id, dt);
                //tmr.Enabled = true;
            }
        }
        [DiscordCommand("modlogs", new char[] { '?', '*' },RequiredPermission =true , commandHelp = "Use `(PREFIX)modlogs <@user>`", description = "view the logs of a user")]
        public async Task Modlogs(string mention)
        {
            if (!HasPerms(Context.Guild.GetUser(Context.Message.Author.Id)).Result)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "You do not have permission to execute this command",
                    Description = "You do not have the valid permission to execute this command",
                    Color = Color.Red
                }.Build());
                return;
            }
            Regex r = new Regex("(\\d{18})");
            ulong id;
            try
            {
                id = Convert.ToUInt64(r.Match(mention).Groups[1].Value);
            }
            catch(Exception ex)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "Invalid ID",
                    Description = "The ID you provided is invalid!",
                    Color = Color.Red
                }.Build());
                return;
            }
            //var user = Context.Guild.GetUser(id);
            //if (user == null)
            //{
            //    await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
            //    {
            //        Title = "Invalad ID",
            //        Description = "The ID you provided is invalad!",
            //        Color = Color.Red
            //    }.Build());
            //    return;
            //}
            if (currentLogs.Users.Any(x => x.userId == id))
            {
                var user = currentLogs.Users[currentLogs.Users.FindIndex(x => x.userId == id)];
                var logs = user.Logs;
                string usrnm = Context.Guild.GetUser(user.userId) == null ? user.username : Context.Guild.GetUser(user.userId).ToString();
                EmbedBuilder b = new EmbedBuilder()
                {
                    Title = $"Modlogs for **{usrnm}** ({id})",
                    Description = $"To remove a log type `*clearlog <user> <log number>` or `?clearlog <user> <log number>`",
                    Color = Color.Green,
                    Fields = new List<EmbedFieldBuilder>()
                };
                foreach(var log in logs)
                {
                    b.Fields.Add(new EmbedFieldBuilder()
                    {
                        IsInline = false,
                        Name = Enum.GetName(typeof(Action), log.Action),
                        Value = $"Reason: {log.Reason}\nModerator: <@{log.ModeratorID}>\nDate: {log.Date}"
                    });
                }
                if(logs.Count == 0)
                {
                    b.Description = "This user has not logs!";
                }
                await Context.Channel.SendMessageAsync("", false, b.Build());
            }
            else
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = $"Modlogs for ({id})",
                    Description = "This user has no logs! :D",
                    Color = Color.Green
                }.Build());
                return;
            }
        }
        [DiscordCommand("purge", RequiredPermission = true, commandHelp = "Parameters - `(PREFIX)purge <ammount>`", description = "Deletes `x` ammount of messages")]
        public async Task purge(uint amount)
        {
            var r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
            var adminrolepos = Context.Guild.Roles.FirstOrDefault(x => x.Id == Global.ModeratorRoleID).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null || r.Contains(Context.Guild.Roles.FirstOrDefault(x => x.Id == 622156934778454016)))
            {
                var messages = await Context.Channel.GetMessagesAsync((int)amount + 1).FlattenAsync();
                await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
                const int delay = 2000;
                var m = await Context.Channel.SendMessageAsync($"Purge completed!");
                await Task.Delay(delay);
                await m.DeleteAsync();
            }
            else
            {
                await Context.Channel.SendMessageAsync("You do not have permission to use this command!");
            }
        }
        [DiscordCommand("purge")]
        public async Task purge(string usr, uint ammount)
        {
            if(!HasExecutePermission)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "What are you tryna do? hack earth?",
                    Description = "you do **NOT** have permission!",
                    Color = Color.Red
                }.Build());
                return;
            }
            Regex r = new Regex("(\\d{18})");
            ulong id;
            try
            {
                id = Convert.ToUInt64(r.Match(usr).Groups[1].Value);
            }
            catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "Invalid ID",
                    Description = "The ID you provided is invalid!",
                    Color = Color.Red
                }.Build());
                return;
            }
            var user = Context.Guild.GetUser(id);
            if(user == null)
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "Invalid ID",
                    Description = "The user is not in the server or the ID is invalid!",
                    Color = Color.Red
                }.Build());
                return;
            }
            var tmp = await Context.Channel.GetMessagesAsync(100).FlattenAsync();
            if(!tmp.Any(x => x.Author.Id == id))
            {
                await Context.Channel.SendMessageAsync("", false, new Discord.EmbedBuilder()
                {
                    Title = "Unable to find messages",
                    Description = $"we cant find messages from <@{id}>!",
                    Color = Color.Red
                }.Build());
                return;
            }
            var messages = tmp.Where(x => x.Author.Id == id).Take((int)ammount);
            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
            const int delay = 2000;
            var m = await Context.Channel.SendMessageAsync($"Purge completed!");
            await Task.Delay(delay);
            await m.DeleteAsync();

        }
    }
}
