using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static SwissBot.Global;
using Discord.Audio;
using System.Diagnostics;
using static SwissbotCore.RedditHandler;
using SwissbotCore;

namespace SwissBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task help()
        {
            var r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
            var adminrolepos = Context.Guild.Roles.FirstOrDefault(x => x.Id == 593106382111113232).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);

            if (Context.Guild.Id == SwissBotDevGuildID || Context.Guild.GetCategoryChannel(Global.TestingCat).Channels.Contains(Context.Guild.GetTextChannel(Context.Channel.Id)) || rolepos != null)
            {
                EmbedBuilder eb = new EmbedBuilder()
                {
                    Title = "***SwissBot Help***",
                    Color = Color.Green,
                    Description = "These are the following commands for SwissBot!\n\n" +
                $"```{Global.Preflix}modify```\n**Parameters** - `{Global.Preflix}modify (ITEMNAME) (NEWVALUE)`\n use `{Global.Preflix}modify list` to view the `.config` file\n\n" +
                $"```{Global.Preflix}welcome```\n Use this command to test the welcome message\n\n" +
                $"```{Global.Preflix}butter```\n**Parameters** - `{Global.Preflix}butter (LINK)`\n use this command to get or submit butter landings!\n\n" +
                $"```{Global.Preflix}commandlogs```\n**Parameters** - `{Global.Preflix}commandlogs (LOG_NAME)`\n use `{Global.Preflix}commandlogs list` to view all command logs\n\n" +
                $"```{Global.Preflix}messagelogs```\n**Parameters** - `{Global.Preflix}messagelogs (LOG_NAME)`\n use `{Global.Preflix}messagelogs list` to view all message logs\n\n" +
                $"```{Global.Preflix}help```\n View this help message :D\n" +
                $"```{Global.Preflix}muteusers```\n use this command in a voice channel to server mute all members in a voice channel\n **Does not mute staff**\n" +
                $"```{Global.Preflix}unmuteusers```\n use this command in a voice channel to unserver mute all members in a voice channel\n **Does not mute/unmute staff**" +
                $"```{Global.Preflix}autoslowmode```\n**Parameters**\n - `{Global.Preflix}autoslowmode set <NEWVALUE>`\n   Sets the current message limit, ex *5* would be 5 messages per second\n\n `{Global.Preflix}autoslowmode <on/off>`\n   Sets autoslowmode on or off\n\n",
                    Footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                        Text = "Help Autogen"
                    },
                };
                await Context.Channel.SendMessageAsync("", false, eb.Build());
            }
            else
            {
                EmbedBuilder eb = new EmbedBuilder()
                {
                    Title = "***SwissBot Help***",
                    Color = Color.Green,
                    Description = "These are the following commands for SwissBot!\n\n" +
                // $"**{Global.Preflix}**modify**\n**Parameters** - ```{Global.Preflix}modify (ITEMNAME) (NEWVALUE)```\n use `{Global.Preflix}modify list` to view the `.config` file\n\n" +
                //$"**{Global.Preflix}welcome**\n Use this command to test the welcome message\n\n" +
                $"```{Global.Preflix}butter```\n**Parameters** - `{Global.Preflix}butter <link>`\n use this command to get or submit butter landings!\n\n" +
                //$"**{Global.Preflix}commandlogs**\n**Parameters** - ```{Global.Preflix}commandlogs (LOG_NAME)```\n use `{Global.Preflix}commandlogs list` to view all command logs\n\n" +
                //$"**{Global.Preflix}messagelogs**\n**Parameters** - ```{Global.Preflix}messagelogs (LOG_NAME)```\n use `{Global.Preflix}messagelogs list` to view all message logs\n\n" +
                $"```{Global.Preflix}help```\n View this help message :D" +
                $"```{Global.Preflix}ping```\nGet the bots ping" +
                $"```{Global.Preflix}welcome```\nTest the welcome message",
                    Footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                        Text = "Help Autogen"
                    },
                    ThumbnailUrl = Global.WelcomeMessageURL
                };
                await Context.Channel.SendMessageAsync("", false, eb.Build());
            }
        }
        [Command("newvideo", RunMode = RunMode.Async)]
        public async Task nv()
        {
            var vc = Context.Guild.GetUser(Context.Message.Author.Id).VoiceChannel;
            if (vc != null)
            {
                try
                {
                    var audioClient = await vc.ConnectAsync();
                    await SendAsync(audioClient, @"C:\Users\plynch\Downloads\Ilyushin IL-96 VS Airbus a340.wav");
                    await audioClient.StopAsync();
                }
                catch (Exception ex)
                {
                    Global.SendExeption(ex);
                }

            }
            else
            {
                await Context.Channel.SendMessageAsync("You need to be in a voice channel");
            }
        }
        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        private async Task SendAsync(IAudioClient client, string path)
        {
            // Create FFmpeg using the previous example
            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
        }
        [Command("mlogs ")]
        public async Task lLogs(string log, params string[] inp)
        {
            //if(File.Exists($"{Global.MessageLogsDir}\\{log}.txt"))
            //string[] recur = File.ReadAllLines($"{Global.MessageLogsDir}\\{log}.txt");
        }
        [Command("modwars")]
        public async Task mw()
        {
            await Context.Channel.SendMessageAsync("MODWARS, coming 2024, directed by Doggo and S.Legend, Executive producers: DJ, Quin, Starring: Ian https://cdn.discordapp.com/attachments/592463507124125706/658138630203637771/modwars.png");

        }
        public enum LogType
        {
            LinkLog,
            MessageLog

        }
        List<LogItem> HandleExpression(string[] expression, LogType type, string logname)
        {
            //string exp = string.Join(" ", expression);
            //if (expression.Length != 0)
            //{

            //    Dictionary<string, List<LogItem>> inputList = new Dictionary<string, List<LogItem>>();

            //    switch (type)
            //    {
            //        case LogType.LinkLog:
            //            inputList = Global.linkLogs;
            //            break;
            //        case LogType.MessageLog:
            //            inputList = Global.messageLogs;
            //            break;
            //    }
            //    var name = logname + ".txt";
            //    if (!inputList.ContainsKey(name))
            //        throw new Exception($"No log found with the name {logname}");

            //    List<LogItem> result;
            //    try
            //    {
            //        //result = inputList[name].Where(exp).ToList();
            //        //List<string> msgs = new List<string>();
            //        //foreach (var r in result)
            //        //    msgs.Add(r.message);
            //        return result;
            //    }
            //    catch (Exception ex)
            //    {
            //        result = null;
            //        throw ex;
            //    }
            //}
            //else
            //    throw new Exception("Expression is empty");
            return null;

        }
        [Command("backup")]
        public async Task backup(ulong id)
        {
            var r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
            var adminrolepos = Context.Guild.Roles.FirstOrDefault(x => x.Id == Global.ModeratorRoleID).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null || r.Contains(Context.Guild.Roles.FirstOrDefault(x => x.Id == 622156934778454016)))
            {
                SocketGuild guild = Context.Client.GetGuild(id);
                JsonGuildObj obj = new JsonGuildObj()
                {
                    AFKChannel = new VoiceChannels()
                    {
                        Name = guild.AFKChannel.Name,
                        Bitrate = guild.AFKChannel.Bitrate,
                        CategoryName = guild.AFKChannel.Category.Name,
                        PermissionOverwrites = guild.AFKChannel.PermissionOverwrites,
                        Position = guild.AFKChannel.Position,
                        UserLimit = guild.AFKChannel.UserLimit
                    },
                    SocketCategory = await GetCategoryChannel(guild.CategoryChannels),
                    AFKTimeout = guild.AFKTimeout,
                    textChannels = await GetTextChannels(guild.TextChannels),
                    VoiceChannels = await GetVoiceChannels(guild.VoiceChannels),
                    DefaultChannel = new TextChannels()
                    {
                        Name = guild.DefaultChannel.Name,
                        Position = guild.DefaultChannel.Position,
                        PermissionOverwrites = guild.DefaultChannel.PermissionOverwrites,
                        CategoryName = guild.DefaultChannel.Category.Name,
                        IsNsfw = guild.DefaultChannel.IsNsfw,
                        SlowModeInterval = guild.DefaultChannel.SlowModeInterval,
                        Topic = guild.DefaultChannel.Topic
                    },
                    IconURL = guild.IconUrl,
                    name = guild.Name,
                    roles = await GetRoles(guild.Roles)

                };
                Thread t = new Thread(() => GenNerGuild(obj, guild));
                t.Start();
            }
        }
        public async void GenNerGuild(JsonGuildObj obj, SocketGuild guild)
        {
        //    try
        //    {
        //        var newguild = await Context.Client.CreateGuildAsync(obj.name, Context.Client.VoiceRegions.FirstOrDefault(n => n.Name == "US East"));
        //        //var newguild = Client.Guilds.ToArray()[3];
        //        await Context.Channel.SendMessageAsync($"Created backup guild: {await newguild.GetTextChannelsAsync().Result.First().CreateInviteAsync()}");

        //        Roles[] rz = new Roles[obj.roles.Count];
        //        foreach (var item in obj.roles) rz[item.Position] = item;
        //        foreach (var role in rz.Reverse())
        //        {
        //            var rl = await newguild.CreateRoleAsync(role.Name, role.Permissions, role.Color, role.IsHoisted);
        //            await rl.ModifyAsync(x =>
        //            {
        //                x.Mentionable = role.IsMentionable;
        //                x.Position = obj.roles.FirstOrDefault(x2 => x2.Name == role.Name).Position;
        //            });
        //            Thread.Sleep(1000);
        //        }
        //        //foreach (var role in newguild.Roles)
        //        //{
        //        //    await role.ModifyAsync(x =>
        //        //    {
        //        //        x.Position = 
        //        //    });
        //        //}
        //        foreach (var cat in obj.SocketCategory)
        //        {
        //            var d = await newguild.CreateCategoryChannelAsync(cat.Name);

        //            foreach (var perms in cat.PermissionOverwrites)
        //            {
        //                var type = perms.TargetType;
        //                if (perms.TargetId != guild.Id)
        //                {
        //                    if (type == PermissionTarget.Role)
        //                    {
        //                        var role = guild.GetRole(perms.TargetId);
        //                        if (role != null)
        //                            await d.AddPermissionOverwriteAsync(newguild.Roles.FirstOrDefault(x => x.Name == role.Name), perms.Permissions);
        //                    }
        //                    if (type == PermissionTarget.User)
        //                    {
        //                        await d.AddPermissionOverwriteAsync(newguild.Roles.FirstOrDefault(x => x.Name == guild.GetRole(perms.TargetId).Name), perms.Permissions);
        //                    }
        //                }
        //            }
        //        }
        //        foreach (SocketGuildChannel cats in Client.GetGuild(newguild.Id).CategoryChannels)
        //        {
        //            await cats.ModifyAsync(x =>
        //            {
        //                x.Position = obj.SocketCategory.FirstOrDefault(z => z.Name == cats.Name).Position;
        //            });
        //        }
        //        foreach (var chan in obj.textChannels)
        //        {
        //            var c = await newguild.CreateTextChannelAsync(chan.Name, x =>
        //            {
        //                x.IsNsfw = chan.IsNsfw;
        //                x.SlowModeInterval = chan.SlowModeInterval;
        //                x.Topic = chan.Topic;
        //                x.CategoryId = newguild.GetCategoryChannelsAsync().Result.FirstOrDefault(x2 => x2.Name == chan.CategoryName).Id;
        //            });
        //        }
        //        foreach (var chan in obj.VoiceChannels)
        //        {
        //            var c = await newguild.CreateVoiceChannelAsync(chan.Name, x =>
        //            {
        //                x.Bitrate = chan.Bitrate;
        //                x.UserLimit = chan.UserLimit;
        //                x.CategoryId = newguild.GetCategoryChannelsAsync().Result.FirstOrDefault(x2 => x2.Name == chan.CategoryName).Id;
        //            });
        //        }
        //        foreach (var chan in newguild.GetTextChannelsAsync().Result)
        //        {
        //            await chan.ModifyAsync(x => x.Position = obj.textChannels.FirstOrDefault(y => y.Name == chan.Name).Position);
        //        }
        //        foreach (var chan in newguild.GetVoiceChannelsAsync().Result)
        //        {
        //            await chan.ModifyAsync(x => x.Position = obj.VoiceChannels.FirstOrDefault(y => y.Name == chan.Name).Position);
        //        }
        //        using (var client = new WebClient())
        //        {
        //            client.DownloadFile(obj.IconURL, "Guildicon.jpeg");
        //        }
        //        await newguild.ModifyAsync(x =>
        //        {
        //            x.AfkChannel = Client.GetGuild(newguild.Id).VoiceChannels.FirstOrDefault(x2 => x2.Name == obj.AFKChannel.Name);
        //            x.AfkTimeout = obj.AFKTimeout;
        //            x.Icon = new Optional<Image?>(new Image($"{Environment.CurrentDirectory}{Global.systemSlash}Guildicon.jpeg"));
        //            x.VerificationLevel = VerificationLevel.Low;
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Global.SendExeption(ex);
        //        Console.WriteLine(ex);
        //    }
        }
        [Command("sendjson")]
        public async Task sendjson()
        {
            await Context.Channel.SendMessageAsync($"Sent to neoney lolz, Response was this gamer\n```{await Global.SendJsontoNeoney()}```");
        }
        [Command("listbackups")]
        public async Task getbacks()
        {
            await Context.Channel.SendMessageAsync($"did a get thingie lolz, Response was this gamer\n```{await Global.getNeoneyStuff()}```");

        }
        [Command("censor")]
        public async Task Censer(params string[] inp)
        {
            var r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
            var adminrolepos = Context.Guild.Roles.FirstOrDefault(x => x.Id == 592464345322094593).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null || r.Contains(Context.Guild.Roles.FirstOrDefault(x => x.Id == 622156934778454016)))
            {
                string full = string.Join(' ', inp);
                if (full.ToLower() == "list")
                {
                    EmbedBuilder eb = new EmbedBuilder()
                    {
                        Title = "Censored word List",
                        Description = $"These are the censored words, to add one do `{Global.Preflix}censor add <word>`\n```{string.Join('\n', Global.CensoredWords)}```",
                        Color = Color.Red
                    };
                    await Context.Channel.SendMessageAsync("", false, eb.Build());
                    return;
                }
                if (inp[0].ToLower() == "add")
                {
                    string word = full.Replace("add ", "").ToLower();
                    if (CensoredWords.Contains(word)) { await Context.Channel.SendMessageAsync("Word already added!"); return; }
                    Global.CensoredWords.Add(word);
                    SaveCensor();
                    EmbedBuilder b = new EmbedBuilder()
                    {
                        Color = Color.Green,
                        Title = "Word Added",
                        Description = $"Added `{word}` to the list"
                    };
                    await Context.Channel.SendMessageAsync("", false, b.Build());
                }
                if (inp[0].ToLower() == "remove")
                {
                    string word = full.Replace("remove ", "");

                    if (CensoredWords.Contains(word.ToLower()))
                    {
                        CensoredWords.Remove(word.ToLower());
                        SaveCensor();
                        EmbedBuilder b = new EmbedBuilder()
                        {
                            Color = Color.Green,
                            Title = "Word Removed",
                            Description = $"Removed `{word}` from the list"
                        };
                        await Context.Channel.SendMessageAsync("", false, b.Build());
                    }
                    else
                    {
                        EmbedBuilder b = new EmbedBuilder()
                        {
                            Color = Color.Green,
                            Title = "Word doesnt exist",
                            Description = $"the word {word} doesnt exist in the list, do `censor list` to view the censored words"
                        };
                        await Context.Channel.SendMessageAsync("", false, b.Build());
                    }
                    
                }
            }
           
        }
        [Command("butter")]
        public async Task butter(string url)
        {
            //add butter link to butter file
            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult);
            if (result)
            {
                if (Context.Channel.Id == Global.SubmissionChanID)
                {
                    string curr = File.ReadAllText(Global.ButterFile);
                    File.WriteAllText(ButterFile, curr + url + "\n");
                    ConsoleLog($"User {Context.Message.Author.Username}#{Context.Message.Author.Discriminator} has submitted the image {url}");
                    var msg = await Context.Channel.SendMessageAsync($"Added {url} to the butter database!");
                    await Context.Message.DeleteAsync();
                    await Task.Delay(5000);
                    await msg.DeleteAsync();
                }
                else
                {
                    UnnaprovedSubs us = new UnnaprovedSubs();

                    us.url = url;
                    us.SubmitterID = Context.Message.Author.Id;
                    await Context.Channel.SendMessageAsync($"Thank you, {Context.Message.Author.Mention} for the submission, we will get back to you!");
                    EmbedBuilder eb = new EmbedBuilder();
                    //eb.ImageUrl = us.url;
                    eb.Title = "**Butter Submission**";
                    eb.Description = $"This image was submitted by {Context.Guild.GetUser(us.SubmitterID).Mention}. LINK: {us.url};";
                    eb.Color = Color.Orange;
                    var msg = await Context.Guild.GetTextChannel(Global.SubmissionChanID).SendMessageAsync("", false, eb.Build());
                    var msg2 = await Context.Guild.GetTextChannel(Global.SubmissionChanID).SendMessageAsync(us.url);

                    await msg2.AddReactionAsync(new Emoji("✅"));
                    await msg2.AddReactionAsync(new Emoji("❌"));
                    us.checkmark = new Emoji("✅");
                    us.Xmark = new Emoji("❌");
                    us.botMSG = msg;
                    us.linkMsg = msg2;
                    SubsList.Add(us);
                    var curr = getUnvertCash();
                    curr.Add(msg.Id.ToString());
                    saveUnvertCash(curr);
                }
            }
            else { await Context.Channel.SendMessageAsync("That is not a valad URL!"); }
        }
        [Command("testmilestone")]
        public async Task testMilestone(string count)
        {
            if (Context.Guild.GetCategoryChannel(Global.TestingCat).Channels.Contains(Context.Guild.GetTextChannel(Context.Channel.Id)))
            {
                await CommandHandler.SendMilestone(Convert.ToInt32(count), Context.Channel.Id);
            }
        }

        [Command("purge")]
        public async Task purge(uint amount)
        {
            var r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
            var adminrolepos = Context.Guild.Roles.FirstOrDefault(x => x.Id == Global.ModeratorRoleID).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null || r.Contains(Context.Guild.Roles.FirstOrDefault(x => x.Id == 622156934778454016)))
            {
                var messages = await this.Context.Channel.GetMessagesAsync((int)amount + 1).FlattenAsync();
                await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
                const int delay = 2000;
                var m = await this.ReplyAsync($"Purge completed!");
                await Task.Delay(delay);
                await m.DeleteAsync();
            }
            else
            {
                await Context.Channel.SendMessageAsync("You do not have permission to use this command!");
            }
        }
        [Command("terminate")]
        public async Task term()
        {
            if (Context.Guild.Id == Global.SwissBotDevGuildID)
            {
                await Context.Channel.SendMessageAsync("Shutting down!");
                Environment.Exit(1);
            }
            else
            {
                if (Context.Guild.GetUser(Context.User.Id).Roles.Contains(Context.Guild.Roles.FirstOrDefault(x => x.Id == Global.DeveloperRoleId)) || Context.User.Id == Context.Client.CurrentUser.Id)
                {
                    await Context.Channel.SendMessageAsync("Shutting down Overlord!");
                    Environment.Exit(1);
                }
            }
        }
        [Command("ping")]
        public async Task ping()
        {
            await Context.Channel.SendMessageAsync($"Pong: {Context.Client.Latency}ms!");
        }
        [Command("QL")]
        public async Task ql(string rep)
        {
            if (rep == "good" || rep == "bad")
            {
                await Context.Channel.SendMessageAsync($"ive been a {rep} boi, input noted down for the future");
            }
        }
        public static async Task<JsonGuildObj> GetGuildObj()
        {
            var guild = Client.GetGuild(Global.SwissGuildId);

            JsonGuildObj obj = new JsonGuildObj()
            {
                AFKChannel = new VoiceChannels()
                {
                    Name = guild.AFKChannel.Name,
                    Bitrate = guild.AFKChannel.Bitrate,
                    CategoryName = guild.AFKChannel.Category.Name,
                    PermissionOverwrites = guild.AFKChannel.PermissionOverwrites,
                    Position = guild.AFKChannel.Position,
                    UserLimit = guild.AFKChannel.UserLimit
                },
                SocketCategory = await GetCategoryChannel(guild.CategoryChannels),
                AFKTimeout = guild.AFKTimeout,
                textChannels = await GetTextChannels(guild.TextChannels),
                VoiceChannels = await GetVoiceChannels(guild.VoiceChannels),
                DefaultChannel = new TextChannels()
                {
                    Name = guild.DefaultChannel.Name,
                    Position = guild.DefaultChannel.Position,
                    PermissionOverwrites = guild.DefaultChannel.PermissionOverwrites,
                    CategoryName = guild.DefaultChannel.Category.Name,
                    IsNsfw = guild.DefaultChannel.IsNsfw,
                    SlowModeInterval = guild.DefaultChannel.SlowModeInterval,
                    Topic = guild.DefaultChannel.Topic
                },
                IconURL = guild.IconUrl,
                name = guild.Name,
                roles = await GetRoles(guild.Roles)
            };
            return obj;
        }
        public static JsonGuildObj guildobj { get; set; }
        [Command("jsonfy")]
        public async Task jsonfy(ulong id)
        {
            IReadOnlyCollection<SocketRole> r;
            int adminrolepos;
            SocketRole rolepos;
            if (Context.Guild.Id != Global.SwissBotDevGuildID)
            {
                r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
                adminrolepos = Context.Guild.Roles.First(x => x.Id == Global.ModeratorRoleID).Position;
                rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
                if (rolepos == null || !r.Contains(Context.Guild.Roles.FirstOrDefault(x => x.Id == 622156934778454016)))
                {
                    return;
                }
            }
            var guild = Context.Client.GetGuild(id);

            JsonGuildObj obj = new JsonGuildObj()
            {
                AFKChannel = new VoiceChannels()
                {
                    Name = guild.AFKChannel.Name,
                    Bitrate = guild.AFKChannel.Bitrate,
                    CategoryName = guild.AFKChannel.Category.Name,
                    PermissionOverwrites = guild.AFKChannel.PermissionOverwrites,
                    Position = guild.AFKChannel.Position,
                    UserLimit = guild.AFKChannel.UserLimit
                },
                SocketCategory = await GetCategoryChannel(guild.CategoryChannels),
                AFKTimeout = guild.AFKTimeout,
                textChannels = await GetTextChannels(guild.TextChannels),
                VoiceChannels = await GetVoiceChannels(guild.VoiceChannels),
                DefaultChannel = new TextChannels()
                {
                    Name = guild.DefaultChannel.Name,
                    Position = guild.DefaultChannel.Position,
                    PermissionOverwrites = guild.DefaultChannel.PermissionOverwrites,
                    CategoryName = guild.DefaultChannel.Category.Name,
                    IsNsfw = guild.DefaultChannel.IsNsfw,
                    SlowModeInterval = guild.DefaultChannel.SlowModeInterval,
                    Topic = guild.DefaultChannel.Topic
                },
                IconURL = guild.IconUrl,
                name = guild.Name,
                roles = await GetRoles(guild.Roles)
            };
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.Create($"{Environment.CurrentDirectory}//Data//{id}.json").Close();
            File.WriteAllText($"{Environment.CurrentDirectory}//Data//{id}.json", json);
            await Context.Channel.SendFileAsync($"{Environment.CurrentDirectory}//Data//{id}.json", $"Here is the Json object of {guild.Name}!");
            guildobj = obj;
        }
        public struct JsonGuildObj
        {
            public string name { get; set; }
            public string IconURL { get; set; }
            public VoiceChannels AFKChannel { get; set; }
            public int AFKTimeout { get; set; }
            public TextChannels DefaultChannel { get; set; }
            public List<CategoryChannel> SocketCategory { get; set; }
            public List<Roles> roles { get; set; }
            public List<TextChannels> textChannels { get; set; }
            public List<VoiceChannels> VoiceChannels { get; set; }
        }

        public static async Task<List<Roles>> GetRoles(IReadOnlyCollection<SocketRole> roles)
        {
            List<Roles> l = new List<Roles>();
            foreach (var role in roles)
            {
                Roles r = new Roles()
                {
                    Color = role.Color,
                    IsEveryone = role.IsEveryone,
                    IsHoisted = role.IsHoisted,
                    IsManaged = role.IsManaged,
                    IsMentionable = role.IsMentionable,
                    Name = role.Name,
                    Permissions = role.Permissions,
                    Position = role.Position
                };
                l.Add(r);
            }
            return l;
        }
        public static async Task<List<VoiceChannels>> GetVoiceChannels(IReadOnlyCollection<SocketVoiceChannel> VoiceChannels)
        {
            List<VoiceChannels> l = new List<VoiceChannels>();
            foreach (var chan in VoiceChannels)
            {
                VoiceChannels c = new VoiceChannels()
                {
                    Bitrate = chan.Bitrate,
                    CategoryName = chan.Category.Name,
                    Name = chan.Name,
                    PermissionOverwrites = chan.PermissionOverwrites,
                    Position = chan.Position,
                    UserLimit = chan.UserLimit
                };

                l.Add(c);
            }
            return l;
        }
        public static async Task<List<CategoryChannel>> GetCategoryChannel(IReadOnlyCollection<SocketCategoryChannel> CategoryChannel)
        {
            List<CategoryChannel> l = new List<CategoryChannel>();
            foreach (var chan in CategoryChannel)
            {
                CategoryChannel c = new CategoryChannel()
                {
                    Name = chan.Name,
                    PermissionOverwrites = chan.PermissionOverwrites,
                    Position = chan.Position,
                };
                List<string> chanNames = new List<string>();
                foreach (var chanl in chan.Channels)
                    chanNames.Add(chanl.Name);
                c.ChannelsName = chanNames;
                l.Add(c);
            }
            return l;
        }
        public static async Task<List<TextChannels>> GetTextChannels(IReadOnlyCollection<SocketTextChannel> TextChannels)
        {
            List<TextChannels> l = new List<TextChannels>();
            foreach (var chan in TextChannels)
            {
                try
                {
                    var d = chan.PermissionOverwrites;
                    TextChannels c = new TextChannels()
                    {
                        CategoryName = chan.Category.Name,
                        Topic = chan.Topic,
                        IsNsfw = chan.IsNsfw,
                        Name = chan.Name,
                        PermissionOverwrites = chan.PermissionOverwrites,
                        Position = chan.Position,
                        SlowModeInterval = chan.SlowModeInterval
                    };
                    l.Add(c);
                }
                catch (Exception ex)
                {
                    TextChannels c = new TextChannels()
                    {
                        CategoryName = chan.Category.Name,
                        Topic = chan.Topic,
                        IsNsfw = chan.IsNsfw,
                        Name = chan.Name,
                        PermissionOverwrites = null,
                        Position = chan.Position,
                        SlowModeInterval = 0
                    };
                    l.Add(c);
                }



            }
            return l;
        }
        public struct CategoryChannel
        {
            public List<string> ChannelsName { get; set; }
            public string Name { get; set; }
            public IReadOnlyCollection<Overwrite> PermissionOverwrites { get; set; }
            public int Position { get; set; }

        }
        public struct TextChannels
        {
            public string CategoryName { get; set; }
            public bool IsNsfw { get; set; }
            public string Name { get; set; }
            public IReadOnlyCollection<Overwrite> PermissionOverwrites { get; set; }
            public int Position { get; set; }
            public int SlowModeInterval { get; set; }
            public string Topic { get; set; }
        }
        public struct VoiceChannels
        {
            public string CategoryName { get; set; }
            public string Name { get; set; }
            public IReadOnlyCollection<Overwrite> PermissionOverwrites { get; set; }
            public int Position { get; set; }
            public int? UserLimit { get; set; }
            public int Bitrate { get; set; }
        }
        public struct Roles
        {
            public Color Color { get; set; }
            public string Name { get; set; }
            public int Position { get; set; }
            public bool IsEveryone { get; set; }
            public bool IsHoisted { get; set; }
            public bool IsManaged { get; set; }
            public bool IsMentionable { get; set; }
            public GuildPermissions Permissions { get; set; }
        }
        [Command("muteusers")]
        public async Task muteusers()
        {
            var r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
            var adminrolepos = Context.Guild.Roles.FirstOrDefault(x => x.Id == Global.DeveloperRoleId).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null)
            {
                if (Context.Guild.GetUser(Context.Message.Author.Id).VoiceChannel != null)
                {
                    await Context.Channel.SendMessageAsync($"Starting to mute members...");
                    int u = 0;
                    foreach (var user in Context.Guild.GetUser(Context.Message.Author.Id).VoiceChannel.Users)
                    {
                        var r2 = Context.Guild.GetUser(user.Id).Roles;
                        var adminrolepos2 = Context.Guild.Roles.FirstOrDefault(x => x.Id == Global.DeveloperRoleId).Position;
                        var rolepos2 = r2.FirstOrDefault(x => x.Position >= adminrolepos);
                        if (rolepos2 == null)
                        {
                            await user.ModifyAsync(x => x.Mute = true);
                            u++;
                        }
                    }
                    await Context.Channel.SendMessageAsync($"Muted {u} members");
                }
            }
        }
        [Command("unmuteusers")]
        public async Task unmuteusers()
        {
            var r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
            var adminrolepos = Context.Guild.Roles.FirstOrDefault(x => x.Id == Global.DeveloperRoleId).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null)
            {
                if (Context.Guild.GetUser(Context.Message.Author.Id).VoiceChannel != null)
                {
                    await Context.Channel.SendMessageAsync($"Starting to Unmute members...");
                    int u = 0;

                    foreach (var user in Context.Guild.GetUser(Context.Message.Author.Id).VoiceChannel.Users)
                    {
                        if (user.IsMuted)
                        {
                            await user.ModifyAsync(x => x.Mute = false);
                            u++;
                        }
                    }
                    await Context.Channel.SendMessageAsync($"UnMuted {u} members");
                }
            }
        }
        static int rot = 0;
        [Command("r")]
        public async Task r()
        {
            HttpClient c = new HttpClient();
            var req = await c.GetAsync("https://www.reddit.com/r/swissbot.json");
            string resp = await req.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<RedditHandler>(resp);
            Regex r = new Regex(@"https:\/\/i.redd.it\/(.*?)\.");
            var childs = data.Data.Children.Where(x => r.IsMatch(x.Data.Url.ToString()));
            //var childs = data.Data.Children;
            Random rnd = new Random();
            int count = childs.Count();
            if (rot >= count - 1)
                rot = 0;
            var post = childs.ToArray()[rot];
            rot++;
            EmbedBuilder b = new EmbedBuilder()
            {
                Color = new Color(0xFF4301),
                Title = "r/Swissbot",
                Description = post.Data.Title,
                ImageUrl = post.Data.Url.ToString(),
                Footer = new EmbedFooterBuilder()
                {
                    Text = "u/" + post.Data.Author
                }
            };
            await Context.Channel.SendMessageAsync("", false, b.Build());

        }
        [Command("guess")]
        public async Task guess(params string[] arg)
        {
            if (arg.Length == 1)
            {
                Uri i;
                if (Uri.TryCreate(arg.First(), UriKind.RelativeOrAbsolute, out i))
                {
                    var tp = Context.Channel.EnterTypingState();
                    HttpClient c = new HttpClient();
                    string url = "https://www.google.com/searchbyimage?image_url=" + arg.First();
                    c.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36");
                    var g = await c.GetAsync(url);
                    string resp = await g.Content.ReadAsStringAsync();
                    Regex r = new Regex("title=\"Search\" value=\"(.*?)\" aria-label=\"Search\"");
                    if (r.IsMatch(resp))
                    {
                        var mtch = r.Match(resp);
                        var val = mtch.Groups[1].Value;
                        var m = await Context.Channel.SendMessageAsync($"Is that {val}?");
                        List<IEmote> em = new List<IEmote>()
                        {
                            new Emoji("✅"),
                            new Emoji("❌")
                        };
                        await m.AddReactionsAsync(em.ToArray());

                    }
                    else
                        await Context.Channel.SendMessageAsync(@"¯\_(ツ)_/¯ idk m8");
                    tp.Dispose();
                }
                else
                {
                    await Context.Channel.SendMessageAsync("not a valad url reee");
                }
            }
            else if (Context.Message.Attachments.Count == 1)
            {
                var tp = Context.Channel.EnterTypingState();

                HttpClient c = new HttpClient();
                string url = "https://www.google.com/searchbyimage?image_url=" + Context.Message.Attachments.First().ProxyUrl;
                c.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36");
                var g = await c.GetAsync(url);
                string resp = await g.Content.ReadAsStringAsync();
                Regex r = new Regex("title=\"Search\" value=\"(.*?)\" aria-label=\"Search\"");
                if (r.IsMatch(resp))
                {
                    var mtch = r.Match(resp);
                    var val = mtch.Groups[1].Value;
                    var m = await Context.Channel.SendMessageAsync($"Is that {val}?");
                    List<IEmote> em = new List<IEmote>()
                        {
                            new Emoji("✅"),
                            new Emoji("❌")
                        };
                    await m.AddReactionsAsync(em.ToArray());
                    tp.Dispose();
                }
                else
                    await Context.Channel.SendMessageAsync(@"¯\_(ツ)_/¯ idk m8");
                tp.Dispose();

            }
            else { await Context.Channel.SendMessageAsync("theres nothing to guess or there is too much :/"); }
        }
        [Command("banreport")]
        public async Task br(ulong id, params string[] reasonnAppeal)
        {
            string all = string.Join(" ", reasonnAppeal);
            //var user = Context.Guild.GetUser(id);
            string reason = all.Replace(reasonnAppeal.Last(), "");
            string appeal = reasonnAppeal.Last();
            EmbedBuilder b = new EmbedBuilder()
            {
                Title = "**Ban Report**",
                Color = Color.DarkRed,
                Timestamp = new DateTimeOffset(DateTime.Now),
                Description = $"The user **<@{id}>** was banned by **{Context.Message.Author.ToString()}**" +
                    $"```Reason```" +
                    $"**{reason}**\n\n" +
                    $"```Appealable?```" +
                    $"*{appeal}*",
                ThumbnailUrl = Global.WelcomeMessageURL
            };
            await Context.Channel.SendMessageAsync("", false, b.Build());
            await Context.Message.DeleteAsync();
        }
        [Command("butter")]
        public async Task butter()
        {
            //add butter link to butter file
            if (Context.Message.Attachments.Count >= 1)
            {
                foreach (var attachment in Context.Message.Attachments)
                {
                    UnnaprovedSubs us = new UnnaprovedSubs();

                    us.url = attachment.Url;
                    us.SubmitterID = Context.Message.Author.Id;
                    await Context.Channel.SendMessageAsync($"Thank you, {Context.Message.Author.Mention} for the submission, we will get back to you!");
                    EmbedBuilder eb = new EmbedBuilder();
                    //eb.ImageUrl = us.url;
                    eb.Title = "**Butter Submission**";
                    eb.Description = $"This image was submitted by {Context.Guild.GetUser(us.SubmitterID).Mention}. LINK: {us.url};";
                    eb.Color = Color.Orange;
                    var msg = await Context.Guild.GetTextChannel(Global.SubmissionChanID).SendMessageAsync("", false, eb.Build());
                    var msg2 = await Context.Guild.GetTextChannel(Global.SubmissionChanID).SendMessageAsync(us.url);

                    await msg2.AddReactionAsync(new Emoji("✅"));
                    await msg2.AddReactionAsync(new Emoji("❌"));
                    us.checkmark = new Emoji("✅");
                    us.Xmark = new Emoji("❌");
                    us.botMSG = msg;
                    us.linkMsg = msg2;
                    SubsList.Add(us);
                    var curr = getUnvertCash();
                    curr.Add(msg.Id.ToString());
                    saveUnvertCash(curr);
                }
            }
            else //get a random butter
            {
                Random r = new Random();
                int max = File.ReadAllLines(ButterFile).Count();
                int num = r.Next(0, max);
                string link = File.ReadAllLines(ButterFile)[num];
                await Context.Channel.SendMessageAsync($"50, 40, 30, 20, 10, **Butter** \n {link}");
            }
        }
        [Command("ban")]
        public async Task ban(string userstring)
        {
            var t = Global.GiveAwayGuilds.Where(x => x.giveawayguild.guildID.Equals(Context.Guild.Id));
            if (t != null)
            {
                if (t.First().giveawayguild.bansActive)
                {
                    var reciv = Context.Client.GetGuild(t.First().giveawayguild.guildID).GetUser(Convert.ToUInt64(userstring.Trim('<', '>', '@')));

                    if (reciv.Roles.Contains(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Admins")))
                    {
                        await Context.Channel.SendMessageAsync("Cannot ban Admins!");
                    }
                    else if (reciv.Roles.Contains(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Contestants")))
                    {
                        var recGU = t.First().giveawayguild.giveawayEntryMembers.FirstOrDefault(x => x.id == reciv.Id);
                        var sendGU = t.First().giveawayguild.giveawayEntryMembers.FirstOrDefault(x => x.id == Context.Message.Author.Id);
                        if (recGU.bannedUsers != null && sendGU.bannedUsers != null)
                        {
                            await Context.Channel.SendMessageAsync($"{reciv.Mention} HAS BEEN ELIMINATED BY {Context.Message.Author.Mention}: {Context.Guild.Users.Count(x => x.Roles.Contains(Context.Guild.Roles.FirstOrDefault(r => r.Name == "Contestants")))} Contestants left");

                            await Context.Guild.AddBanAsync(reciv);
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"Could not ban {recGU.DiscordName}! they were of the null type loser");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"Cannot ban {reciv.Mention} because there not a Contestant");
                    }
                }
                else { await Context.Channel.SendMessageAsync($"Giveaway not ready.."); }
            }
        }
        static internal int giveawayStep = 0;
        static internal bool giveawayinProg;
        static internal GiveAway currGiveaway;
        [Command("giveaway")]
        internal async Task giveaway()
        {
            //await Context.Channel.SendMessageAsync("Disabled because it will break when im gone lol");
            GiveAway ga = new GiveAway();
            currGiveaway = ga;
            currGiveaway.GiveAwayUser = Context.Message.Author.Id;

            EmbedBuilder eb = new EmbedBuilder();
            eb.Color = Color.Blue;
            eb.Title = "**Giveaway Builder**";
            eb.Description = $"Welcome {Context.Message.Author.Username}{Context.Message.Author.Discriminator} to the Giveaway Creator, follow these steps to create a giveaway. \n \n ***Step One*** \n `Enter the time in DD:HH:MM:SS format. ex 1:12:30:00 would be 1 day 12 hours and 30 minutes`";
            eb.Footer = new EmbedFooterBuilder();
            //eb.Footer.Text = "to redo a step type **\"redo**";
            await Context.Channel.SendMessageAsync("", false, eb.Build());
            giveawayStep++;
            giveawayinProg = true;
        }
        internal static async Task checkGiveaway(SocketMessage msg)
        {
            if (!msg.Author.IsBot)
            {
                if (giveawayinProg)
                {
                    if (msg.Channel.Id == Global.giveawayCreatorChanId)
                    {
                        if (msg.ToString() == "\"cancel")
                        {
                            giveawayinProg = false;
                            giveawayStep = 0;
                            await msg.Channel.SendMessageAsync("Cancelled giveaway");
                            return;
                        }
                        if (giveawayStep == 1)
                        {
                            try
                            {
                                string[] args = msg.ToString().Split(':');
                                int seconds = 0;
                                if (args.Length == 4)
                                {
                                    int days = Convert.ToInt32(args[0]); //days
                                    seconds = seconds + days * 24 * 60 * 60;

                                    int hours = Convert.ToInt32(args[1]);
                                    seconds = seconds + (hours * 60 * 60);

                                    int minutes = Convert.ToInt32(args[2]);
                                    seconds = seconds + (minutes * 60);

                                    int secs = Convert.ToInt32(args[3]);
                                    seconds = seconds + secs;
                                    Console.WriteLine($"{msg.Author.Username} Created a giveaway with the time of {seconds}");
                                    EmbedBuilder eb = new EmbedBuilder();
                                    eb.Color = Color.Blue;
                                    eb.Footer = new EmbedFooterBuilder();
                                    eb.Footer.Text = "to cancle a giveaway type **\"cancle**";
                                    eb.Title = "**Giveaway Step 1**";
                                    string time = "";
                                    if (days != 0)
                                        time += $"{days} Days, ";
                                    if (hours != 0)
                                        time += $"{hours} Hours, ";
                                    if (minutes != 0)
                                        time += $"{minutes} Minutes";
                                    if (secs != 0)
                                        time += $" and {secs} Seconds.";

                                    eb.Description = $"Time set to **{time}** ({seconds}) seconds \n\n **Next Step** \n What are you giving away?";
                                    currGiveaway.Seconds = seconds;
                                    await msg.Channel.SendMessageAsync("", false, eb.Build());
                                    giveawayStep++;
                                    return;
                                }
                                else
                                {
                                    await msg.Channel.SendMessageAsync("Invalad Time!");
                                }
                            }
                            catch (Exception ex)
                            {
                                Global.SendExeption(ex);
                            }
                        }
                        if (giveawayStep == 2)
                        {
                            try
                            {
                                currGiveaway.GiveAwayItem = msg.ToString();
                                EmbedBuilder eb = new EmbedBuilder();
                                eb.Title = "Giveaway Item";
                                eb.Color = Color.Blue;
                                eb.Description = $"The **Giveaway Item** is now set to: \n `{currGiveaway.GiveAwayItem}` \n\n **Next Step** \n how many winners should there be?";
                                eb.Footer = new EmbedFooterBuilder();
                                eb.Footer.Text = "to cancle a giveaway type **\"cancle**";
                                giveawayStep++;
                                await msg.Channel.SendMessageAsync("", false, eb.Build());
                                return;

                            }
                            catch (Exception ex)
                            {
                                Global.SendExeption(ex);
                            }
                        }
                        if (giveawayStep == 3)
                        {
                            try
                            {
                                int numPeople = Convert.ToInt32(msg.ToString());
                                currGiveaway.numWinners = numPeople;
                                EmbedBuilder eb = new EmbedBuilder();
                                eb.Title = "**Confirm?**";
                                eb.Color = Color.Blue;
                                string timefromsec = "";
                                TimeSpan ts = TimeSpan.FromSeconds(currGiveaway.Seconds);
                                if (ts.Days != 0)
                                    timefromsec += $"{ts.Days} Days, ";
                                if (ts.Hours != 0)
                                    timefromsec += $"{ts.Hours} Hours, ";
                                if (ts.Minutes != 0)
                                    timefromsec += $"{ts.Minutes} Minutes";
                                if (ts.Seconds != 0)
                                    timefromsec += $", and {ts.Seconds}";

                                eb.Description = $"Are you sure with these settings? \n\n **GiveawayItem** \n`{currGiveaway.GiveAwayItem}` \n \n **Winners** \n`{currGiveaway.numWinners}` \n\n **Giveawayer** \n `{currGiveaway.GiveAwayUser}` \n\n **Time**\n`{timefromsec}` \n\n to confirm these setting type `confirm`, to cancle a giveaway type **\"cancle**";
                                giveawayStep++;
                                await msg.Channel.SendMessageAsync("", false, eb.Build());
                                return;

                            }
                            catch (Exception ex)
                            {
                                await msg.Channel.SendMessageAsync($"Uh oh, Looks like we have had a boo boo: {ex.Message}");
                                Global.SendExeption(ex);
                            }
                        }
                        if (giveawayStep == 4)
                        {
                            if (msg.ToString() == "confirm")
                            {
                                //do the channel thing lol
                                Console.WriteLine("Creating Giveaway Guild...");
                                GiveawayGuild gg = new GiveawayGuild();
                                await gg.createguild(currGiveaway);
                                string url = gg.inviteURL;
                                currGiveaway.discordInvite = url;
                                EmbedBuilder eb = new EmbedBuilder();
                                string timefromsec = "";
                                TimeSpan ts = TimeSpan.FromSeconds(currGiveaway.Seconds);
                                if (ts.Days != 0)
                                    timefromsec += $"{ts.Days} Days, ";
                                if (ts.Hours != 0)
                                    timefromsec += $"{ts.Hours} Hours, ";
                                if (ts.Minutes != 0)
                                    timefromsec += $"{ts.Minutes} Minutes";
                                if (ts.Seconds != 0)
                                    timefromsec += $", and {ts.Seconds}";

                                eb.Title = "GIVEAWAY";
                                eb.Color = Color.Blue;
                                eb.Description = $"{Client.GetGuild(SwissGuildId).GetUser(currGiveaway.GiveAwayUser).Mention} Has started a giveaway for **{currGiveaway.GiveAwayItem}** with {currGiveaway.numWinners} winner(s), to enter the giveaway join {currGiveaway.discordInvite}\n\n **How does it work?** \n after the timer reaches 0 everyone will get access to the `{Preflix}ban <@user>` command, its like a FFA. the last person(s) remaining will get the giveaway item \n \n ***GIVEAWAY STARTS IN {timefromsec}***";
                                Console.WriteLine(url);
                                GiveawayTimer gt = new GiveawayTimer();
                                gt.currGiveaway = currGiveaway;
                                gt.gguild = gg;
                                await gt.StartTimer();
                                gt.Time = currGiveaway.Seconds;
                                var giveawaymsg = await Client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.giveawayChanID).SendMessageAsync("", false, eb.Build());
                                currGiveaway.giveawaymsg = giveawaymsg;
                                gg.currgiveaway = currGiveaway;
                                return;
                            }
                        }
                    }
                }
            }
        }
        [Command("autoslowmode")]
        public async Task aSlow(params string[] args)
        {
            var r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
            var adminrolepos = Context.Guild.Roles.FirstOrDefault(x => x.Id == 593106382111113232).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null || r.FirstOrDefault(x => x.Id == Global.DeveloperRoleId) != null)
            {
                if (args.Length == 1)
                {
                    var val = args.First();
                    if (val.ToLower() == "on")
                    {
                        var data = modifyJsonData(Global.CurrentJsonData, "AutoSlowmodeToggle", true);
                        Global.SaveConfig(data);
                        await Context.Channel.SendMessageAsync("Autoslowmode is now On!");
                    }
                    if (val.ToLower() == "off")
                    {
                        modifyJsonData(Global.CurrentJsonData, "AutoSlowmodeToggle", false);
                        await Context.Channel.SendMessageAsync("Autoslowmode is now Off!");
                    }
                }
                if (args.Length == 2)
                {
                    if (args.First().ToLower() == "set")
                    {
                        int val = 5;
                        try { val = Convert.ToInt32(args.Last()); }
                        catch (Exception ex) { await Context.Channel.SendMessageAsync("The number you provided is invalad!"); return; }
                        var data = modifyJsonData(Global.CurrentJsonData, "AutoSlowmodeTrigger", val);
                        Global.SaveConfig(data);
                        await Context.Channel.SendMessageAsync($"Set the trigger rate to **{val}** Messages a second!");

                    }
                }
            }

        }
        [Command("slowmode")]
        public async Task slowmode(string value)
        {
            //check user perms
            var r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
            var adminrolepos = Context.Guild.Roles.FirstOrDefault(x => x.Id == 593106382111113232).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null || r.FirstOrDefault(x => x.Id == Global.DeveloperRoleId) != null)
            {
                try
                {
                    int val = 0;
                    try
                    {
                        val = Convert.ToInt32(value);
                    }
                    catch { }
                    var chan = Context.Guild.GetTextChannel(Context.Channel.Id);
                    await chan.ModifyAsync(x =>
                    {
                        x.SlowModeInterval = val;
                    });
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Color = Color.Green,
                        Title = $"Set the slowmode to {value}!",
                        Description = $"{Context.Message.Author.Mention} successfully modified the slowmode of <#{Context.Channel.Id}> to {value} seconds!",
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = Context.Message.Author.ToString(),
                            IconUrl = Context.Message.Author.GetAvatarUrl(),
                            Url = Context.Message.GetJumpUrl()
                        }
                    }.Build());
                }
                catch (Exception ex)
                {
                    Global.SendExeption(ex);
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Color = Color.Red,
                    Title = "You dont have Permission!",
                    Description = $"Sorry {Context.Message.Author.Mention} but you do not have permission to change the slowmode of <#{Context.Channel.Id}> !",
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = Context.Message.Author.ToString(),
                        IconUrl = Context.Message.Author.GetAvatarUrl(),
                        Url = Context.Message.GetJumpUrl()
                    }
                }.Build());
            }
        }
        [Command("configperms")]
        public async Task configperm(string name, string newValue)
        {
            if (Context.Guild.Id == Global.SwissBotDevGuildID)
            {
                if (Global.ConfigSettings.Keys.Contains(name))
                {
                    bool perm = true ? newValue == "true" : false;
                    Global.ConfigSettings.Remove(name);
                    ConfigSettings.Add(name, perm);
                    EmbedBuilder eb = new EmbedBuilder();
                    eb.Title = "**Updated Config**";
                    eb.Footer = new EmbedFooterBuilder();
                    eb.Footer.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                    eb.Footer.Text = "Command Autogen";
                    eb.Color = Color.Green;
                    eb.Description = "Updated the Config Permissions, Here is the new Config Permissions";
                    string items = "";
                    foreach (var item in ConfigSettings)
                        items += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n";
                    eb.Description += $"\n{items}";
                    Global.SaveConfigPerms(ConfigSettings);
                    await Context.Channel.SendMessageAsync("", false, eb.Build());
                }
            }
        }
        [Command("configperms")]
        public async Task configperm(string name)
        {
            if (Context.Guild.Id == Global.SwissBotDevGuildID)
            {
                if (name == "list")
                {
                    string list = "";
                    foreach (var item in ConfigSettings)
                        list += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n";
                    EmbedBuilder eb = new EmbedBuilder()
                    {
                        Title = "**Config Permission List**",
                        Description = $"**here is the config list**\n {list}",
                        Color = Color.Green,
                        Footer = new EmbedFooterBuilder()
                        {
                            IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                            Text = "Command Autogen"
                        },
                    };
                    await Context.Channel.SendMessageAsync("", false, eb.Build());
                }
                else
                    await Context.Channel.SendMessageAsync($"Not a valad argument, please do `{Global.Preflix}configperms list` do view the config items, to change one type `{Global.Preflix}configperms (ITEM NAME) (VALUE)`");
            }
        }
        [Command("welcome")]
        public async Task welcome()
        {
            var arg = Context.Guild.GetUser(Context.Message.Author.Id);
            string welcomeMessage = CommandHandler.WelcomeMessageBuilder(Global.WelcomeMessage, arg);

            EmbedBuilder eb = new EmbedBuilder()
            {
                Title = $"***Welcome to Swiss001's Discord server!***",
                Footer = new EmbedFooterBuilder()
                {
                    IconUrl = arg.GetAvatarUrl(),
                    Text = $"{arg.Username}#{arg.Discriminator}"
                },
                Description = welcomeMessage,
                ThumbnailUrl = Global.WelcomeMessageURL,
                Color = Color.Green
            };
            await Context.Channel.SendMessageAsync("", false, eb.Build());
            Global.ConsoleLog($"WelcomeMessage for {arg.Username}#{arg.Discriminator}", ConsoleColor.Blue);
        }
        [Command("commandlogs")]
        public async Task logs(string name)
        {
            if (Context.Guild.Id == Global.SwissBotDevGuildID)
            {
                if (name == "list")
                {
                    string names = "";
                    foreach (var file in Directory.GetFiles(Global.CommandLogsDir))
                    {
                        names += file.Split(Global.systemSlash).Last() + "\n";
                    }
                    if (names == "")
                        names = "There are currently no Log files :\\";
                    EmbedBuilder eb = new EmbedBuilder();
                    eb.Color = Color.Green;
                    eb.Title = "**Command logs List**";
                    eb.Description = $"Here are the current Command Logs, To fetch one do `\"commandlogs (name)` \n ```{names}```";
                    await Context.Channel.SendMessageAsync("", false, eb.Build());
                }
                else
                {
                    if (File.Exists(Global.CommandLogsDir + $"{Global.systemSlash}{name}"))
                    {
                        await Context.Channel.SendFileAsync(Global.CommandLogsDir + $"{Global.systemSlash}{name}", $"Here is the Log **{name}**");
                    }
                    else
                    {
                        EmbedBuilder eb = new EmbedBuilder();
                        eb.Color = Color.Red;
                        eb.Title = "**Command logs List**";
                        eb.Description = $"The file {name} does not exist, try doing `\"commandlogs list` to view all the command logs";
                        await Context.Channel.SendMessageAsync("", false, eb.Build());
                    }
                }
            }
            else
            {
                if (Context.Channel.Id == Global.LogsChannelID)
                {
                    if (name == "list")
                    {
                        string names = "";
                        foreach (var file in Directory.GetFiles(Global.CommandLogsDir))
                        {
                            names += file.Split(Global.systemSlash).Last() + "\n";
                        }
                        if (names == "")
                            names = "There are currently no Log files :\\";
                        EmbedBuilder eb = new EmbedBuilder();
                        eb.Color = Color.Green;
                        eb.Title = "**Command logs List**";
                        eb.Description = $"Here are the current Command Logs, To fetch one do `\"commandlogs (name)` \n ```{names}```";
                        await Context.Channel.SendMessageAsync("", false, eb.Build());
                    }
                    else
                    {
                        if (File.Exists(Global.CommandLogsDir + $"{Global.systemSlash}{name}"))
                        {
                            await Context.Channel.SendFileAsync(Global.CommandLogsDir + $"{Global.systemSlash}{name}", $"Here is the Log **{name}**");
                        }
                        else
                        {
                            EmbedBuilder eb = new EmbedBuilder();
                            eb.Color = Color.Red;
                            eb.Title = "**Command logs List**";
                            eb.Description = $"The file {name} does not exist, try doing `\"commandlogs list` to view all the command logs";
                        }
                    }
                }
            }
        }
        [Command("messagelogs")]
        public async Task mlogs(string name, params string[] exp)
        {
            if (exp.Length >= 1)
            {
                var r = HandleExpression(exp, LogType.MessageLog, name);
                string final = "";
                foreach (var item in r)
                {
                    string loglMsg = $"[{item.date}] ";
                    loglMsg += $"USER: {item.username} CHANNEL: {item.channel} MESSAGE: {item.message}";
                    final += loglMsg + "\n";

                }
                File.WriteAllText($"{Environment.CurrentDirectory}{Global.systemSlash}MSGLogExp.txt", final);
                await Context.Channel.SendFileAsync($"{Environment.CurrentDirectory}{Global.systemSlash}MSGLogExp.txt", $"Here is the logs on {name} with the expression {string.Join(" ", exp)}");
            }
            if (Context.Guild.Id == Global.SwissBotDevGuildID)
            {
                if (name == "list")
                {
                    string names = "";
                    foreach (var file in Directory.GetFiles(Global.MessageLogsDir))
                    {
                        names += file.Split(Global.systemSlash).Last().Replace(".txt", "") + "\n";
                    }
                    if (names == "")
                        names = "There are currently no Log files :\\";
                    EmbedBuilder eb = new EmbedBuilder();
                    eb.Color = Color.Green;
                    eb.Title = "**Message logs List**";
                    eb.Description = $"Here are the current Message Logs, To fetch one do `\"messagelogs (name)` \n ```{names}```";

                }
                else
                {
                    if (File.Exists(Global.MessageLogsDir + $"{Global.systemSlash}{name}.txt"))
                    {
                        await Context.Channel.SendFileAsync(Global.MessageLogsDir + $"{Global.systemSlash}{name}.txt", $"Here is the Log **{name}**");
                    }
                    else
                    {
                        EmbedBuilder eb = new EmbedBuilder();
                        eb.Color = Color.Red;
                        eb.Title = "**Message logs List**";
                        eb.Description = $"The file {name} does not exist, try doing `\"messagelogs list` to view all the command logs";
                        await Context.Channel.SendMessageAsync("", false, eb.Build());
                    }
                }
            }
            else
            {
                if (Context.Channel.Id == Global.LogsChannelID)
                {
                    if (name == "list")
                    {
                        string names = "";
                        foreach (var file in Directory.GetFiles(Global.MessageLogsDir))
                        {
                            names += file.Split(Global.systemSlash).Last() + "\n";
                        }
                        if (names == "")
                            names = "There are currently no Log files :\\";
                        EmbedBuilder eb = new EmbedBuilder();
                        eb.Color = Color.Green;
                        eb.Title = "**Message logs List**";
                        eb.Description = $"Here are the current Message Logs, To fetch one do `\"messagelogs (name)` \n ```{names}```";
                        await Context.Channel.SendMessageAsync("", false, eb.Build());
                    }
                    else
                    {
                        if (File.Exists(Global.MessageLogsDir + $"{Global.systemSlash}{name}"))
                        {
                            await Context.Channel.SendFileAsync(Global.MessageLogsDir + $"{Global.systemSlash}{name}", $"Here is the Log **{name}**");
                        }
                        else
                        {
                            EmbedBuilder eb = new EmbedBuilder();
                            eb.Color = Color.Red;
                            eb.Title = "**Message logs List**";
                            eb.Description = $"The file {name} does not exist, try doing `\"messagelogs list` to view all the command logs";
                        }
                    }
                }
            }
        }
        [Command("modify")]
        public async Task modify(string configItem, params string[] input)
        {
            var r = Context.Guild.GetUser(Context.Message.Author.Id).Roles;
            var adminrolepos = Context.Guild.Roles.FirstOrDefault(x => x.Id == 593106382111113232).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null || r.FirstOrDefault(x => x.Id == Global.DeveloperRoleId) != null)
            {
                if (input.Length == 0)
                {
                    if (configItem == "list")
                    {
                        if (Context.Guild.Id == Global.SwissBotDevGuildID)
                        {
                            EmbedBuilder b = new EmbedBuilder();
                            b.Footer = new EmbedFooterBuilder();
                            b.Footer.Text = "**Dev Config**";
                            b.Title = "Dev Config List";
                            string list = "**Here is the current config file** \n";
                            foreach (var item in Global.JsonItemsListDevOps) { list += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n"; }
                            b.Description = list;
                            b.Color = Color.Green;
                            b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                            await Context.Channel.SendMessageAsync("", false, b.Build());
                        }
                        else
                        {

                            if (Context.Guild.GetCategoryChannel(Global.TestingCat).Channels.Contains(Context.Guild.GetTextChannel(Context.Channel.Id)))
                            {
                                EmbedBuilder b = new EmbedBuilder();
                                b.Footer = new EmbedFooterBuilder();
                                b.Footer.Text = "**Admin Config**";
                                b.Title = "Admin Config List";
                                string list = "**Here is the current config file, not all items are here, if you wish to view more items please contact Thomas or Swiss, because they control the config items you can modify!** \n";
                                string itemsL = "";
                                foreach (var item in Global.jsonItemsList) { itemsL += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n"; }
                                if (itemsL == "") { list = "**Sorry but there is nothing here or you do not have permission to change anything yet :/**"; }
                                b.Description = list + itemsL;
                                b.Color = Color.Green;
                                b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                                await Context.Channel.SendMessageAsync("", false, b.Build());
                            }
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"No value was provided for the variable `{configItem}`");
                    }
                }
                else
                {
                    var value = string.Join(" ", input);
                    string newvalue = value;
                    if (Context.Guild.Id == Global.SwissBotDevGuildID)//allow full modify
                    {
                        if (Global.JsonItemsListDevOps.Keys.Contains(configItem))
                        {
                            JsonItems data = Global.CurrentJsonData;
                            data = modifyJsonData(data, configItem, newvalue);
                            if (data.Token != null)
                            {
                                Global.SaveConfig(data);
                                await Context.Channel.SendMessageAsync($"Sucessfuly modified the config, Updated the item {configItem} with the new value of {value}");
                                EmbedBuilder b = new EmbedBuilder();
                                b.Footer = new EmbedFooterBuilder();
                                b.Footer.Text = "**Dev Config**";
                                b.Title = "Dev Config List";
                                string list = "**Here is the current config file** \n";
                                foreach (var item in Global.JsonItemsListDevOps) { list += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n"; }
                                b.Description = list;
                                b.Color = Color.Green;
                                b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                                await Context.Channel.SendMessageAsync("", false, b.Build());
                            }

                        }
                        else { await Context.Channel.SendMessageAsync($"Could not find the config item {configItem}! Try `{Global.Preflix}modify list` for a list of the Config!"); }
                    }
                    if (Context.Guild.Id == Global.SwissGuildId)
                    {
                        if (Context.Guild.GetCategoryChannel(Global.TestingCat).Channels.Contains(Context.Guild.GetTextChannel(Context.Channel.Id)))//allow some modify
                        {
                            if (Global.jsonItemsList.Keys.Contains(configItem))
                            {
                                JsonItems data = Global.CurrentJsonData;
                                data = modifyJsonData(data, configItem, newvalue);
                                if (data.Token != null)
                                {
                                    Global.SaveConfig(data);
                                    await Context.Channel.SendMessageAsync($"Sucessfuly modified the config, Updated the item {configItem} with the new value of {value}");
                                    EmbedBuilder b = new EmbedBuilder();
                                    b.Footer = new EmbedFooterBuilder();
                                    b.Footer.Text = "**Admin Config**";
                                    b.Title = "Admin Config List";
                                    string list = "**Here is the current config file** \n";
                                    foreach (var item in Global.jsonItemsList) { list += $"```json\n \"{item.Key}\" : \"{item.Value}\"```\n"; }
                                    b.Description = list;
                                    b.Color = Color.Green;
                                    b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                                    await Context.Channel.SendMessageAsync("", false, b.Build());
                                }
                            }
                            else
                            {
                                if (Global.JsonItemsListDevOps.Keys.Contains(configItem))
                                {
                                    EmbedBuilder b = new EmbedBuilder();
                                    b.Color = Color.Red;
                                    b.Title = "You need Better ***PERMISSION***";
                                    b.Description = "You do not have permission to modify this item, if you think this is incorrect you can DM quin#3017 for help";

                                    await Context.Channel.SendMessageAsync("", false, b.Build());
                                }
                                else { await Context.Channel.SendMessageAsync($"Could not find the config item {configItem}! Try `{Global.Preflix}modify list` for a list of the Config!"); }
                            }

                        }
                    }
                }
            }

        }
        [Command("images")]
        public async Task imgs()
        {
            await Context.Channel.SendMessageAsync("Getting imgages....");
            string htmlcont = "<!DOCTYPE html> <html lang=\"en\"> <head> </head> <body>\n";
            foreach (var chan in Context.Guild.TextChannels)
            {
                foreach (var msg in chan.GetMessagesAsync().FlattenAsync().Result)
                {
                    if (msg.Attachments.Count >= 1)
                    {
                        foreach (var att in msg.Attachments)
                        {
                            htmlcont += $"<img src = \"{att.ProxyUrl}\">\n";
                        }
                    }
                }
            }
            htmlcont += "</body> </html>";
            File.WriteAllText($"{Environment.CurrentDirectory}{Global.systemSlash}Data{Global.systemSlash}img.html", htmlcont);
            await Context.Channel.SendFileAsync($"{Environment.CurrentDirectory}{Global.systemSlash}Data{Global.systemSlash}img.html", "heres the immages");
        }
        internal JsonItems modifyJsonData(JsonItems data, string iName, object iValue)
        {
            try
            {
                var prop = data.GetType().GetProperty(iName);
                if (prop != null)
                {
                    Type t = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    object safeValue = (iValue == null) ? null : Convert.ChangeType(iValue, t);
                    prop.SetValue(data, safeValue, null);
                    return data;
                }
                else { throw new Exception($"Could not find the config item {iName}!"); }

            }
            catch (Exception ex)
            {
                EmbedBuilder b = new EmbedBuilder()
                {
                    Color = Color.Red,
                    Title = "Exeption!",
                    Description = $"**{ex}**"
                };
                Context.Channel.SendMessageAsync("", false, b.Build());
                return data;
            }
        }
    }
}
