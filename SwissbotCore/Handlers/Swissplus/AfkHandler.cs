using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers
{
    [DiscordHandler]
    public class AfkHandler
    {
        public Dictionary<ulong, string> AfkStatus = new Dictionary<ulong, string>();
        private DiscordSocketClient client;

        public AfkHandler(DiscordSocketClient c)
        {
            this.client = c;

            this.AfkStatus = LoadAfkStatus();

            client.MessageReceived += Client_MessageReceived;
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg.Author.IsBot)
                return;

            if(arg.Channel is SocketTextChannel channel)
            {
                if(channel.Guild.Id == Global.SwissGuildId)
                {
                    if (AfkStatus.ContainsKey(arg.Author.Id))
                    {
                        AfkStatus.Remove(arg.Author.Id);
                        SaveAfkStatus();
                        _ = Task.Run(async () =>
                        {
                            var msg = await channel.SendMessageAsync($"{arg.Author.Mention}", false, new EmbedBuilder()
                            {
                                Color = CommandModuleBase.Blurple,
                                Title = $":wave: Welcome back {arg.Author.Username}!",
                                Description = "I've cleared your afk status."
                            }.WithCurrentTimestamp().Build());
                            await Task.Delay(5000);
                            await msg.DeleteAsync();
                        });
                    }

                    foreach(var mu in arg.MentionedUsers)
                    {
                        if (AfkStatus.ContainsKey(mu.Id))
                        {
                            var status = AfkStatus[mu.Id];
                            _ = Task.Run(async () =>
                            {
                                var msg = await channel.SendMessageAsync($"{arg.Author.Mention} Hey! {mu.Username} is currently afk: `{status.Replace('`', '\'')}`");
                                await Task.Delay(5000);
                                await msg.DeleteAsync();
                            });
                        }
                    }
                }
            }
        }

        private Dictionary<ulong, string> LoadAfkStatus()
        {
            try
            {
                return SwissbotStateHandler.LoadObject<Dictionary<ulong, string>>("afk.json").GetAwaiter().GetResult();
            }
            catch
            {
                return new Dictionary<ulong, string>();
            }
        }

        private void SaveAfkStatus()
            => SwissbotStateHandler.SaveObject("afk.json", AfkStatus);

        public void SetUserStatus(ulong user, string reason)
        {
            if (AfkStatus.ContainsKey(user))
                AfkStatus[user] = reason;
            else
                AfkStatus.Add(user, reason);

            SaveAfkStatus();
        }
    }

    
}
