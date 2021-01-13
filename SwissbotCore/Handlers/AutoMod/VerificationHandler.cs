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
    [DiscordHandler]
    class VerificationHandler
    {
        public DiscordSocketClient _client;
        public VerificationHandler(DiscordSocketClient client)
        {
            _client = client;

            // Deprecated
            //_client.ReactionAdded += CheckVerification;
            //try { CheckVerts().GetAwaiter().GetResult(); } catch (Exception ex) { Global.ConsoleLog($"Ex,{ex} ", ConsoleColor.Red); }

            _client.GuildMemberUpdated += _client_GuildMemberUpdated;
        }

        private async Task _client_GuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
        {
            if (arg2.Guild.Id != Global.SwissGuildId)
                return;

            if(arg1.IsPending.HasValue && arg2.IsPending.HasValue)
            {
                if(arg1.IsPending.Value && !arg2.IsPending.Value)
                {
                    await arg2.AddRoleAsync(arg2.Guild.GetRole(Global.MemberRoleID), new RequestOptions() 
                    {
                        AuditLogReason = "Completed screening"
                    });

                    await arg2.Guild.GetTextChannel(Global.WelcomeMessageChanID).SendMessageAsync("", false, BuildWelcomeEmbed(arg2));
                }
            }
        }

        internal static string WelcomeMessageBuilder(string orig, SocketGuildUser user)
        {
            if (orig.Contains("(USER)"))
                orig = orig.Replace("(USER)", $"<@{user.Id}>");

            if (orig.Contains("(USERCOUNT)"))
                orig = orig.Replace("(USERCOUNT)", user.Guild.MemberCount.ToString());
            return orig;
        }

        internal static Embed BuildWelcomeEmbed(SocketGuildUser user)
        {
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
            return eb.Build();
        }
    }
}
