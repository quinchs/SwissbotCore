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
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static SwissbotCore.Global;
using Discord.Audio;
using System.Diagnostics;
using static SwissbotCore.RedditHandler;
using SwissbotCore;
using System.Drawing;
using System.Drawing.Drawing2D;
using Color = Discord.Color;
using static SwissbotCore.CustomCommandService;
using System.Security.Cryptography;
using SwissbotCore.Handlers;

namespace SwissbotCore.Modules
{
    [DiscordCommandClass()]
    public class Commands : CommandModuleBase
    {
        [DiscordCommand("help")]
        public async Task help()
        {   
            var msg = await Context.Channel.SendMessageAsync("", false, HelpMessageHandler.HelpEmbedBuilder(1, HelpMessageHandler.CalcHelpPage(Context.Guild.GetUser(Context.Message.Author.Id))));
            var emote1 = new Emoji("\U000027A1");
            var emote2 = new Emoji("\U00002B05");
            await msg.AddReactionAsync(emote2);
            await msg.AddReactionAsync(emote1);
            HelpMessageHandler.CurrentHelpMessages.Add(msg.Id, Context.Message.Author.Id);
            Global.SaveHelpMessageCards();
        }
        [DiscordCommand("altverify", RequiredPermission = true, commandHelp = "Parameters - `(PREFIX)altverify <on/off/true/false>`\n If on or true alt accounts can verify and will still post an alert in <#665647956816429096>. if off or false it will post a verify msg in <#692909459831390268>")]
        public async Task AltVerify(string param)
        {
            if (!await HasPerms(Context)) { await Context.Channel.SendMessageAsync("You do not have permission!"); return; }

            bool setting;
            switch (param.ToLower())
            {
                case "true":
                    setting = true;
                    break;
                case "on":
                    setting = true;
                    break;
                case "false":
                    setting = false;
                    break;
                case "off":
                    setting = false;
                    break;
                default:
                    {
                        await Context.Channel.SendMessageAsync($"Not a valid parameter, either use \"on/true\" or \"off/false\". see {Global.Preflix}help for help");
                        return;
                    }
            }

            Global.VerifyAlts = setting;
            await Context.Channel.SendMessageAsync("Set the alt verification to " + param);
        }
        [DiscordCommand("support", description = "i am once again asking for you financial support")]
        public async Task b()
        {
            await PlaySoundFile(@"C:\Users\plynch\Downloads\I_am_once_again_asking_for_your_financial_support_Bernie_Sanders_Green_Screen.mp3");
        }
        [DiscordCommand("newvideo")]
        public async Task nv()
        {
            await PlaySoundFile(@"C:\Users\plynch\Downloads\update.mp3");
        }
        [DiscordCommand("alexa", description ="gravy train")]
        public async Task al()
        {
            await PlaySoundFile(@"C:\Users\plynch\Downloads\whip_a_cessna_1.mp3");
        }
        private async Task PlaySoundFile(string path)
        {
            var vc = Context.Guild.GetUser(Context.Message.Author.Id).VoiceChannel;
            if (vc != null)
            {
                try
                {
                    var audioClient = await vc.ConnectAsync();
                    await SendAsync(audioClient, path);
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
        [DiscordCommand("modwars", description = "MODWARS! (were not in a fight)")]
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

        [DiscordCommand("censor", RequiredPermission = true, commandHelp = "`(PREFIX)censor list` will show you the current censored words\nTo add a censored word type `(PREFIX)censor add <word/sentence>`\nTo remove a censored word type `(PREFIX)censor remove <word/sentence>`", description = "adds, removes, or lists the current censor")]
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
        [DiscordCommand("butter", description = "use this command to get or submit butter landings", commandHelp = "Parameters - `(PREFIX)butter <link>`\n adds a link to the butter list")]
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
        [DiscordCommand("testmilestone")]
        public async Task testMilestone(string count)
        {
            if (Context.Guild.GetCategoryChannel(Global.TestingCat).Channels.Contains(Context.Guild.GetTextChannel(Context.Channel.Id)))
            {
                await CommandHandler.SendMilestone(Convert.ToInt32(count), Context.Channel.Id);
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
        [DiscordCommand("quinasking", description ="*loads gun*")]
        public async Task quinasking()
        {
            await Context.Channel.SendFileAsync(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Data" + Path.DirectorySeparatorChar + "quinasking.jpg");
        }
        [DiscordCommand("asking", description ="does cool image generation stuff")]
        public async Task bernie()
        {
            WebClient wc = new WebClient();
            byte[] bytes = wc.DownloadData("https://cdn.discordapp.com/attachments/592768337407115264/678407124794998795/bernie.jpg");
            MemoryStream ms = new MemoryStream(bytes);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            string purl = Context.Message.Author.GetAvatarUrl();
            byte[] bytes2 = wc.DownloadData(purl);
            MemoryStream ms2 = new MemoryStream(bytes2);
            System.Drawing.Image img2 = System.Drawing.Image.FromStream(ms2);

            int width = img.Width;
            int height = img.Height;

            using (img)
            {
                using (var bitmap = new Bitmap(img.Width, img.Height))
                {
                    using (var canvas = Graphics.FromImage(bitmap))
                    {
                        canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        canvas.DrawImage(img,
                                         new Rectangle(0,
                                                       0,
                                                       width,
                                                       height),
                                         new Rectangle(0,
                                                       0,
                                                       img.Width,
                                                       img.Height),
                                         GraphicsUnit.Pixel);
                        canvas.DrawImage(img2, (img.Width / 2) - (img2.Width / 2), (img.Height / 2) - img2.Height - 110, 256, 256);
                        canvas.Save();
                    }
                    try
                    {
                        bitmap.Save($"{Environment.CurrentDirectory}\\img.jpg",
                                    System.Drawing.Imaging.ImageFormat.Jpeg);
                        await Context.Channel.SendFileAsync($"{Environment.CurrentDirectory}\\img.jpg");
                        //this.BackgroundImage = bitmap;
                    }
                    catch (Exception ex) { }
                }
            }
        }
        [DiscordCommand("george", description ="more cool image gen stuff")]
        public async Task g()
        {
            //baseurl https://cdn.discordapp.com/attachments/592463507124125706/682686064229613593/george.jpg
            WebClient wc = new WebClient();
            byte[] bytes = wc.DownloadData("https://cdn.discordapp.com/attachments/592463507124125706/682686064229613593/george.jpg");
            MemoryStream ms = new MemoryStream(bytes);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);

            //get profile
            byte[] bytes2 = wc.DownloadData(Context.Message.Author.GetAvatarUrl());
            MemoryStream ms2 = new MemoryStream(bytes2);
            System.Drawing.Image img2 = System.Drawing.Image.FromStream(ms2);
            
            int width = img.Width;
            int height = img.Height;

            using (img)
            {
                using (var bitmap = new Bitmap(img.Width, img.Height))
                {
                    using (var canvas = Graphics.FromImage(bitmap))
                    {
                        canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        canvas.DrawImage(img,
                                         new Rectangle(0,
                                                       0,
                                                       width,
                                                       height),
                                         new Rectangle(0,
                                                       0,
                                                       img.Width,
                                                       img.Height),
                                         GraphicsUnit.Pixel);
                        canvas.DrawImage(img2, (img.Width / 2) - (img2.Width) + 27, (img.Height / 2) - (img2.Height + 33));
                        canvas.Save();
                    }
                    try
                    {
                        bitmap.Save($"{Environment.CurrentDirectory}\\img.jpg",
                                    System.Drawing.Imaging.ImageFormat.Jpeg);
                        await Context.Channel.SendFileAsync($"{Environment.CurrentDirectory}\\img.jpg");
                        //this.BackgroundImage = bitmap;
                    }
                    catch (Exception ex) { }
                }
            }
        }
        [DiscordCommand("fate", description = "generates the fate image", commandHelp = "Parameters - `(PREFIX)fate <@user>`")]
        public async Task fate(string user)
        {
            //baseurl https://cdn.discordapp.com/attachments/592807608499437665/678443510210363442/council.jpg
            WebClient wc = new WebClient();
            byte[] bytes = wc.DownloadData("https://cdn.discordapp.com/attachments/620673311122391040/682691076649517087/image0.png");
            MemoryStream ms = new MemoryStream(bytes);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            string purl = Context.Message.MentionedUsers.First().GetAvatarUrl();
            byte[] bytes2 = wc.DownloadData(purl);
            MemoryStream ms2 = new MemoryStream(bytes2);
            System.Drawing.Image img2 = System.Drawing.Image.FromStream(ms2);
            

            int width = img.Width;
            int height = img.Height;

            using (img)
            {
                using (var bitmap = new Bitmap(img.Width, img.Height))
                {
                    using (var canvas = Graphics.FromImage(bitmap))
                    {
                        canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        canvas.DrawImage(img,
                                         new Rectangle(0,
                                                       0,
                                                       width,
                                                       height),
                                         new Rectangle(0,
                                                       0,
                                                       img.Width,
                                                       img.Height),
                                         GraphicsUnit.Pixel);
                        canvas.DrawImage(img2, (img.Width / 2) - (img2.Width / 2)-10, (img.Height / 2) - (img2.Height / 2) + 100);
                        canvas.Save();
                    }
                    try
                    {
                        bitmap.Save($"{Environment.CurrentDirectory}\\img.jpg",
                                    System.Drawing.Imaging.ImageFormat.Jpeg);
                        await Context.Channel.SendFileAsync($"{Environment.CurrentDirectory}\\img.jpg");
                        //this.BackgroundImage = bitmap;
                    }
                    catch (Exception ex) { }
                }
            }
        }

        [DiscordCommand("terminate")]
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
        [DiscordCommand("ping", description = "Gets the ping of the bot")]
        public async Task ping()
        {
            await Context.Channel.SendMessageAsync($"Pong: {Context.Client.Latency}ms!");
        }
        
        [DiscordCommand("vcmute", RequiredPermission = true, description = "Mutes all memebrs in vc")]
        public async Task muteusers()
        {
            Global.MutedMembers = new List<ulong>();
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
                            if (!user.IsMuted)
                            {
                                await user.ModifyAsync(x => x.Mute = true);
                                if (!MutedMembers.Contains(user.Id))
                                    Global.MutedMembers.Add(user.Id);
                                u++;
                            }
                        }
                    }
                    await Context.Channel.SendMessageAsync($"Muted {u} members");
                }
            }
        }
        [DiscordCommand("vcunmute", RequiredPermission = true, description = "Unmutes all members in vc")]
        public async Task unmuteusers()
        {
            try
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
                        var vcusers = Context.Guild.GetUser(Context.Message.Author.Id).VoiceChannel.Users;
                        foreach (var item in vcusers)
                        {
                            SocketGuildUser user = null;
                            if (vcusers.Any(x => x.Id == item.Id))
                                user = vcusers.FirstOrDefault(x => x.Id == item.Id);
                            else
                            { user = Context.Guild.GetUser(item.Id); }
                            if (user.IsMuted)
                            {
                                await user.ModifyAsync(x => x.Mute = false);
                                u++;
                            }
                        }
                        await Context.Channel.SendMessageAsync($"UnMuted {u} members");
                    }
                }
                Global.MutedMembers = null;
            }
            catch(Exception ex)
            {

            }
        }
        static int rot = 0;
        [DiscordCommand("reddit", description = "Pulls a random reddit post from the r/swissbot reddit")]
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
        [DiscordCommand("guess", description = "Guesses a image", commandHelp = "Parameters - `(PREFIX)guess <imagelink>` or use `(PREFIX)guess` with an image attachment")]
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
                    Regex r = new Regex("title=\"(Sök|Search)\" value=\"(.*?)\" aria-label=\"(Sök|Search)\"");
                    //File.Create($"{Environment.CurrentDirectory}\\Debug.txt").Close();
                    //File.WriteAllText($"{Environment.CurrentDirectory}\\Debug.txt", $"--DEBUG--\n\nRegex debug:\n    IsMatch: {r.IsMatch(resp)}\n    RegexString: {r.ToString()}\n\n--Start Resp--\n\n {resp}\n\n--End Resp--");
                    //await Context.Message.Author.SendFileAsync($"{Environment.CurrentDirectory}\\Debug.txt");
                    if (r.IsMatch(resp))
                    {
                        var mtch = r.Match(resp);
                        var val = mtch.Groups[2].Value;
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
        [DiscordCommand("butter", description = "Fetches a random butter submission made by you wonderful people <3")]
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
        static internal int giveawayStep = 0;
        static internal bool giveawayinProg;
        static internal GiveAway currGiveaway;
        [DiscordCommand("giveaway")]
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
        [DiscordCommand("autoslowmode", RequiredPermission = true, description = "Toggles autoslowmode and sets it to `x` ammount of messages per second", commandHelp = "Parameters - `(PREFIX)autoslowmode <on|off>` or `(PREFIX)autoslowmode set <numberOfMessagesPerSecond>`")]
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
        [DiscordCommand("slowmode", RequiredPermission = true, commandHelp = "Parameters - `(PREFIX)slowmode <Seconds>`\nTo disable slowmode just type `slowmode off`", description = "This command sets the slowmode")]
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
        [DiscordCommand("configperms")]
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
        [DiscordCommand("configperms")]
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
        [DiscordCommand("welcome", description = "Generates a test welcome message")]
        public async Task welcome()
        {
            var arg = Context.Guild.GetUser(Context.Message.Author.Id);
            string welcomeMessage = Handlers.VerificationHandler.WelcomeMessageBuilder(Global.WelcomeMessage, arg);

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
        public async Task<bool> HasPerms(SocketCommandContext c)
        {
             if(c.Guild.Id == Global.SwissBotDevGuildID) { return true; }
             var user = c.Guild.GetUser(c.Message.Author.Id);
             if (user.Guild.GetRole(Global.ModeratorRoleID).Position <= user.Hierarchy)
                return true;
            else
                return false;
        }
        [DiscordCommand("modify")]
        public async Task modify(string configItem, params string[] input)
        {
            if (!HasPerms(Context).Result) { await Context.Channel.SendMessageAsync("Invalad permissions!"); return; }

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
        [DiscordCommand("images")]
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
