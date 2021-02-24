using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static SwissbotCore.Handlers.VerificationHandler;

namespace SwissbotCore.Handlers
{
    [DiscordHandler]
    class AltAccountHandler
    {
        public DiscordSocketClient _client;
        public List<(ulong id, DateTime joinDate)> JoinWatchList = new List<(ulong id, DateTime joinDate)>();
        public static List<AltMessage> AltMessages = new List<AltMessage>();
        public Dictionary<ulong, List<SocketMessage>> Spammers = new Dictionary<ulong, List<SocketMessage>>();

        public AltAccountHandler(DiscordSocketClient client)
        {
            _client = client;

            _client.UserJoined += _client_UserJoined;

            _client.MessageReceived += CheckAltRaid;

            //_client.MessageReceived += CheckSpamRaid;

            Timer t = new Timer();
            t.Interval = 60000 * 60;
            t.Elapsed += T_Elapsed;

            //Timer t2 = new Timer();
            //t2.Interval = 10000;
            //t2.Elapsed += T2_Elapsed;
        }

        //private void T2_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    var d = Spammers.ToList();

        //    foreach(var item in d.Where(x => (DateTime.UtcNow - x.Value.Last().Timestamp.UtcDateTime).TotalSeconds >= 10))
        //    {
        //        Spammers.Remove(item.Key);
        //    }
        //}

        //public async Task CheckSpammers()
        //{
        //    var d = Spammers.ToList();

        //    foreach()
        //}

        //private async Task CheckSpamRaid(SocketMessage arg)
        //{
        //    if (arg.Channel is SocketDMChannel)
        //        return;

        //    if (arg.Author.IsBot)
        //        return;

        //    if (arg.Channel is SocketTextChannel channel)
        //    {
        //        if(Spammers.ContainsKey(arg.Author.Id))
        //        {
        //            Spammers[arg.Author.Id].Add(arg);

        //            Task.Run(async () =>
        //            {
        //                await CheckSpammers();
        //            });
        //        }
        //        else
        //        {
        //            Spammers.Add(arg.Author.Id, new List<SocketMessage>() { arg });
        //        }
        //    }
        //}

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach(var item in JoinWatchList.ToArray())
            {
                if((DateTime.UtcNow - item.joinDate).TotalHours >= 1)
                {
                    JoinWatchList.Remove(item);
                }
            }
        }

        private async Task CheckAltRaid(SocketMessage arg)
        {
            if (arg.Channel is SocketDMChannel)
                return;

            if(arg.Channel is SocketTextChannel channel)
            {
                if (channel.Guild.Id != Global.SwissGuildId)
                    return;

                if (JoinWatchList.Any(x => x.id == arg.Author.Id))
                {
                    var watch = JoinWatchList.FirstOrDefault(x => x.id == arg.Author.Id);
                    var msg = new AltMessage(arg);
                    AltMessages.Add(msg);
                    var allMessages = msg.Messages.ToList();

                    if (allMessages.Count > 3)
                    {
                        // Check for spam
                        var time = GetOldestNewest(allMessages);
                        var avgMsg = allMessages.Count / time.TotalSeconds;
                        var amountPingRoles = allMessages.Count(x => x.MentionedRoles > 0) == 0 ? 0 : allMessages.Count(x => x.MentionedRoles > 0) / allMessages.Count;
                        var amountPingUsers = allMessages.Count(x => x.MentionedUsers > 0) == 0 ? 0 : allMessages.Count(x => x.MentionedUsers > 0) / allMessages.Count;

                        if ((amountPingRoles > 0.2 || amountPingUsers > 0.2))
                        {
                            JoinWatchList.Remove(watch);
                            AltMessages.RemoveAll(x => x.Author == arg.Author.Id);
                            // Lockout
                            await LockoutUser(arg.Author.Id,
                                    $"**Raid trigger**\n" +
                                    $"```\n" +
                                    $"Total messages:     {allMessages.Count}\n" +
                                    $"Join date:          {watch.joinDate.ToString("R")}\n" +
                                    $"Avg msg's/sec       {avgMsg}\n" +
                                    $"Role ping count:    {amountPingRoles * 100}%\n" +
                                    $"User ping count:    {amountPingUsers * 100}%\n" +
                                    $"Total account time: {time.TotalSeconds}s\n```\n\n" +
                                    $"**Trigger Score**\n" +
                                    $"`{amountPingRoles} > 0.2 || {amountPingUsers} > 0.2`\n" +
                                    $"Total: **{amountPingUsers + amountPingRoles + avgMsg}**");
                            return;
                        }

                        double total = 0;
                        for (int i = 0; i != allMessages.Count - 1; i++)
                        {
                            var s = allMessages[i].Content;
                            var t = allMessages[i + 1].Content;
                            var v = CalculateSimilarity(s, t);
                            total += v;
                        }

                        double contentAverage = total / allMessages.Count;

                        if (contentAverage >= 0.55d && avgMsg >= 0.25)
                        {
                            JoinWatchList.Remove(watch);
                            AltMessages.RemoveAll(x => x.Author == arg.Author.Id);

                            await LockoutUser(arg.Author.Id,
                                     $"**Raid trigger**\n" +
                                     $"```\n" +
                                     $"Total messages:     {allMessages.Count}\n" +
                                     $"Join date:          {watch.joinDate.ToString("R")}\n" +
                                     $"Avg msg's/sec       {avgMsg}\n" +
                                     $"Content Similarity: {contentAverage}\n" +
                                     $"Total sim calc:     {total}/{allMessages.Count}\n" +
                                     $"Total account time: {time.TotalSeconds}s\n```\n\n" +
                                     $"**Trigger Score**\n" +
                                     $"`{contentAverage} >= 0.55d && {avgMsg} >= 0.25d`\n" +
                                     $"Total: **{contentAverage + total + avgMsg}**");
                            return;
                        }
                    }
                }
            }
        }

        public async Task LockoutUser(ulong id, string details)
            => await LockoutUser(await Global.GetSwissbotUser(id), details);

        public async Task LockoutUser(SocketGuildUser user, string details)
        {
            if (user == null)
                return;

            await user.AddRoleAsync(Global.SwissGuild.GetRole(Global.MutedRoleID));

            try
            {
                await user.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = "You have been locked out of the server",
                    Description = "You we're locked out of the server for suspicious activity. " +
                             "The system isn't perfect so if you feel like this is incorrect," +
                             " please open a support ticket by sending a message here.",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
            }
            catch { }

            int channels = 0;
            int messages = 0;

            try
            {
                var s = Global.SwissGuild;
                SocketTextChannel c = null;
                Dictionary<SocketTextChannel, List<ulong>> m = new Dictionary<SocketTextChannel, List<ulong>>();
                foreach (var msg in AltMessages.Where(x => x.Author == user.Id))
                {
                    c = s.GetTextChannel(msg.Channel);
                    if (c == null)
                        continue;
                    if (m.ContainsKey(c))
                        m[c].Add(msg.Id);
                    else
                        m.Add(c, new List<ulong>() { msg.Id });
                }

                foreach(var item in m)
                {
                    channels++;
                    messages += item.Value.Count;
                    try
                    {
                        await item.Key.DeleteMessagesAsync(item.Value);
                    }
                    catch { }
                }
            }
            catch(Exception x) 
            {
                Console.WriteLine(x);
            }

            // Send the high fidelity alert
            await Global.SendAlertMessage("@here", false, new EmbedBuilder()
            {
                Title = "High fidelity alert",
                Color = Color.Orange,
                Description = $"This high fidelity alert was triggered by user {user.Mention}.",
                Fields = new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    {
                        Name = "Details",
                        Value = details
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "Actions",
                        Value = $"Muted {user}\nDeleted {messages} messages in {channels} channel(s)"
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "How to resolve",
                        Value = $"If this is incorrect, or the issue has been resolved, you can use `*unmute {user.Id}` to unmute them."
                    }
                }
            }.WithCurrentTimestamp().Build());
        }

        public TimeSpan GetOldestNewest(List<AltMessage> msg)
        {
            var old = msg.Min(x => x.Time);
            var newest = msg.Max(x => x.Time);

            return newest - old;
        }

        private async Task _client_UserJoined(SocketGuildUser arg)
        {
            if (IsAlt(arg))
                JoinWatchList.Add((arg.Id, arg.JoinedAt.HasValue ? arg.JoinedAt.Value.UtcDateTime : DateTime.UtcNow));
        }

        public static bool IsAlt(SocketGuildUser arg)
        {
            if ((DateTime.UtcNow - arg.CreatedAt.UtcDateTime).TotalHours < Global.AltVerificationHours)
                return true;
            return false;
        }

        /// <summary>
        /// Calculate percentage similarity of two strings
        /// <param name="source">Source String to Compare with</param>
        /// <param name="target">Targeted String to Compare</param>
        /// <returns>Return Similarity between two strings from 0 to 1.0</returns>
        /// </summary>
        public static double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = LevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }

        public static int LevenshteinDistance(string source, string target)
        {
            // degenerate cases
            if (source == target) return 0;
            if (source.Length == 0) return target.Length;
            if (target.Length == 0) return source.Length;

            // create two work vectors of integer distances
            int[] v0 = new int[target.Length + 1];
            int[] v1 = new int[target.Length + 1];

            // initialize v0 (the previous row of distances)
            // this row is A[0][i]: edit distance for an empty s
            // the distance is just the number of characters to delete from t
            for (int i = 0; i < v0.Length; i++)
                v0[i] = i;

            for (int i = 0; i < source.Length; i++)
            {
                // calculate v1 (current row distances) from the previous row v0

                // first element of v1 is A[i+1][0]
                //   edit distance is delete (i+1) chars from s to match empty t
                v1[0] = i + 1;

                // use formula to fill in the rest of the row
                for (int j = 0; j < target.Length; j++)
                {
                    var cost = (source[i] == target[j]) ? 0 : 1;
                    v1[j + 1] = Math.Min(v1[j] + 1, Math.Min(v0[j + 1] + 1, v0[j] + cost));
                }

                // copy v1 (current row) to v0 (previous row) for next iteration
                for (int j = 0; j < v0.Length; j++)
                    v0[j] = v1[j];
            }

            return v1[target.Length];
        }

        public class AltMessage
        {
            public ulong Id { get; set; }
            public int MentionedRoles { get; set; }
            public string Content { get; set; }
            public int MentionedUsers { get; set; }
            public DateTime Time { get; set; }
            public ulong Author { get; set; }
            public ulong Channel { get; set; }

            public List<AltMessage> Messages
                => AltMessages.Where(x => x.Author == this.Author).ToList();

            public AltMessage() { }

            public AltMessage(SocketMessage m)
            {
                this.Id = m.Id;
                this.MentionedRoles = m.MentionedRoles.Count;
                this.MentionedUsers = m.MentionedUsers.Count;
                this.Content = m.Content;
                this.Time = m.Timestamp.UtcDateTime;
                this.Author = m.Author.Id;
                this.Channel = m.Channel.Id;
            }
        }
    }

    
}
