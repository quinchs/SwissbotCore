using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers
{
    class VerificationHandler
    {
        public static Dictionary<ulong, ulong> FList =new Dictionary<ulong, ulong>();

        public DiscordSocketClient _client;
        public VerificationHandler(DiscordSocketClient client)
        {
            FList = Global.ReadAltCards();

            _client = client;
            try { CheckVerts().GetAwaiter().GetResult(); } catch (Exception ex) { Global.ConsoleLog($"Ex,{ex} ", ConsoleColor.Red); }

        }
        public  async Task CheckVerification(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var tuser = _client.GetGuild(Global.SwissGuildId).GetUser(arg3.UserId);

            if (!Global.VerifyAlts && AltAccountHandler.IsAlt(tuser))
                return;
            if (arg2.Id == Global.VerificationChanID)
            {
                var user = _client.GetGuild(Global.SwissGuildId).GetUser(arg3.UserId);
                var emote = new Emoji("✅");
                if (arg3.Emote.Name == emote.Name)
                {
                    if (user == null) { user = _client.GetGuild(Global.SwissGuildId).GetUser(arg1.Value.Author.Id); }
                    string nick = "";
                    if (user.Nickname != null)
                        nick = user.Nickname;
                    SocketRole unVertRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.UnverifiedRoleID);
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
                        await user.SendMessageAsync("", false, new EmbedBuilder()
                        {
                            Title = "***Welcome to Swiss001's Discord Server!***",
                            Description = "Welcome, we hope you read the rules, if not please watch this video https://www.youtube.com/watch?v=RFOkRDpF8dY thanks!",
                            Color = Color.Green

                        }.Build());
                        Global.ConsoleLog($"WelcomeMessage for {user.Username}#{user.Discriminator}", ConsoleColor.Blue);
                    }
                }
            }
            else if (arg2.Id == 692909459831390268)
            {
                //rewrite man verification

                if (arg3.User.Value.IsBot) { return; }
                if (FList.Keys.Contains(arg3.MessageId))
                {
                    var k = FList[arg3.MessageId];
                    var user = _client.GetGuild(Global.SwissGuildId).GetUser(k);
                    var msg = _client.GetGuild(Global.SwissGuildId).GetTextChannel(692909459831390268).GetMessageAsync(arg3.MessageId).Result;
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
                            Global.SaveAltCards();
                            await _client.GetGuild(Global.SwissGuildId).GetTextChannel(662142405377654804).SendMessageAsync($"Allowed {user.ToString()}. Moderator: {arg3.User.ToString()}");
                        }
                    }
                    if (arg3.Emote.Name == ex.Name)
                    {
                        try
                        {
                            await user.SendMessageAsync("", false, new EmbedBuilder()
                            {
                                Title = "**You have been Stopped!**",
                                Color = Color.Red,
                                Description = "You've been banned from **Swiss001 Official Discord Server.**",
                                Fields = new List<EmbedFieldBuilder>()
                                    {
                                        { new EmbedFieldBuilder(){
                                        Name = "To appeal",
                                        Value = "https://neoney.xyz/swiss001/unban"
                                        } }
                                    },
                            }.Build());
                        }
                        catch { }
                        await user.BanAsync(7, "Denied by Staff");
                        await msg.DeleteAsync();
                        FList.Remove(arg3.MessageId);
                        Global.SaveAltCards();
                        await _client.GetGuild(Global.SwissGuildId).GetTextChannel(692909459831390268).SendMessageAsync($"Banned {user.ToString()}. Moderator: {arg3.User.ToString()}");

                    }
                }
            }
        }
        public async Task CheckVerts()
        {
            var unVertRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.UnverifiedRoleID);
            var userRole = _client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.MemberRoleID);
            Global.ConsoleLog("GUID: " + Global.SwissGuildId + ". vcuid: " + Global.VerificationChanID);

            var tpm = await _client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.VerificationChanID).GetMessageAsync(627680940155469844);
            var sMessage = (IUserMessage)tpm;

            var emote = new Emoji("✅");
            var reActs = await sMessage.GetReactionUsersAsync(emote, 7500).FlattenAsync();
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
                        string welcomeMessage = VerificationHandler.WelcomeMessageBuilder(Global.WelcomeMessage, user);
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

        internal static string WelcomeMessageBuilder(string orig, SocketGuildUser user)
        {
            if (orig.Contains("(USER)"))
                orig = orig.Replace("(USER)", $"<@{user.Id}>");

            if (orig.Contains("(USERCOUNT)"))
                orig = orig.Replace("(USERCOUNT)", Global.UserCount.ToString());
            return orig;
        }
    }
}
