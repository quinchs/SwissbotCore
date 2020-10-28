using Discord;
using Discord.WebSocket;
using SwissbotCore.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers.AutoMod
{
    [DiscordHandler]
    class NoTracker
    {
        public DiscordSocketClient client;
        public static List<string> aT = new List<string>();
        public NoTracker(DiscordSocketClient c)
        {
            client = c;
            client.MessageReceived += CheckTrackorBecauseItsAnnoying;
            client.MessageUpdated += Client_MessageUpdated;
            aT = Commands.loadAt();
        }

        private Task Client_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
            => CheckTrackorBecauseItsAnnoying(arg2);

        private async Task CheckTrackorBecauseItsAnnoying(SocketMessage arg)
        {
            if (DateTime.UtcNow.DayOfWeek != DayOfWeek.Tuesday)
                return;
            new Thread(async () =>
            {
                if (arg.Attachments.Any())
                {
                    foreach (var atch in arg.Attachments)
                    {
                        var url = atch.ProxyUrl;
                        string gurl = "https://www.google.com/searchbyimage?image_url=" + url;
                        HttpClient c = new HttpClient();
                        c.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36");
                        var g = await c.GetAsync(gurl);
                        string resp = await g.Content.ReadAsStringAsync();
                        Regex r = new Regex("title=\"Search\" value=\"(.*?)\" aria-label=\"Search\"");
                        if (r.IsMatch(resp))
                        {
                            var mtch = r.Match(resp);
                            var val = mtch.Groups[1].Value;
                            Console.WriteLine($"Tracktor? A: {arg.Author.ToString()} V: {val}");
                            if (aT.Any(x => val.Contains(x)) || val.Contains("virtus 120 dt cab") || val.Contains("major 80 opinie") || val.Contains("case ih") || val.Contains("workmaster") || val.Contains("tractor") || val.Contains("zetor") || val.Contains("truck") || val.Contains("kubota") || val.Contains("electric generator"))
                            {
                                await arg.DeleteAsync();
                                await arg.Channel.SendMessageAsync($"no traktor {arg.Author.Mention} >:(");
                                //await client.GetGuild(Global.SwissGuildId).GetTextChannel(665647956816429096).SendMessageAsync("", false, new EmbedBuilder()
                                //{
                                //    Title = "Someone posted a gay ass tractor!",
                                //    Fields = new List<EmbedFieldBuilder>()
                                //{
                                //    new EmbedFieldBuilder()
                                //    {
                                //        Name = "Poster:",
                                //        Value = arg.Author.Mention,
                                //    },
                                //    new EmbedFieldBuilder()
                                //    {
                                //        Name = "Image",
                                //        Value = url
                                //    }
                                //},
                                //    Color = Color.Orange,
                                //    ImageUrl = url,
                                //    Footer = new EmbedFooterBuilder()
                                //    {
                                //        Text = "im sick of these tractors"
                                //    }
                                //}.Build());
                            }
                        }
                    }
                }
            }).Start();
        }
    }
}
