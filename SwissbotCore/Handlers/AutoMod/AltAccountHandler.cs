using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static SwissbotCore.Handlers.VerificationHandler;

namespace SwissbotCore.Handlers
{
    [DiscordHandler]
    class AltAccountHandler
    {
        public DiscordSocketClient _client;
        public AltAccountHandler(DiscordSocketClient client)
        {
            _client = client;

            _client.UserJoined += CheckAlt;
        }
        public async Task CheckAlt(SocketGuildUser arg)
        {
            if (IsAlt(arg))
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

                if (!Global.VerifyAlts)
                {
                    EmbedBuilder eb = new EmbedBuilder()
                    {

                        Title = "Manual Verification",
                        Description = "This user was flagged down because it was created less than 12 hours ago",
                        Color = Color.Red
                    };
                    b.Fields.Add(new EmbedFieldBuilder()
                    {
                        Name = "Join at UTC",
                        Value = arg.JoinedAt.Value.UtcDateTime
                    });
                    b.Fields.Add(new EmbedFieldBuilder()
                    {
                        Name = "Created at UTC",
                        Value = arg.CreatedAt.UtcDateTime
                    });
                    b.Fields.Add(new EmbedFieldBuilder()
                    {
                        Name = "Username",
                        Value = arg.ToString()
                    });
                    string anick = "None";
                    if (arg.Nickname != null)
                        anick = arg.Nickname;
                    b.Fields.Add(new EmbedFieldBuilder()
                    {
                        Name = "Nickname",
                        Value = anick
                    });
                    b.Fields.Add(new EmbedFieldBuilder()
                    {
                        Name = "ID",
                        Value = arg.Id
                    });

                    var msg = await _client.GetGuild(Global.SwissGuildId).GetTextChannel(692909459831390268).SendMessageAsync("", false, b.Build());
                    await msg.AddReactionAsync(new Emoji("✅"));
                    await msg.AddReactionAsync(new Emoji("❌"));
                    FList.Add(msg.Id, arg.Id);
                    Global.SaveAltCards();
                    await arg.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Color = Color.Red,
                        Title = "**Uh oh, am I smelling an alt?**",
                        Description = "Dont worry, you are not under arrest.\nYour account was detected to be an alt. The staff team will manually have to verify you.\n\nSit Tight!"
                    }.Build());
                    return;
                }
            }
        }

        public static bool IsAlt(SocketGuildUser arg)
        {
            if ((DateTime.UtcNow - arg.CreatedAt.UtcDateTime).TotalHours < Global.AltVerificationHours)
                return true;
            return false;
        }
    }
}
