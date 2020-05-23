using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Linq;
using System.Text.RegularExpressions;
using Discord.Rest;
using Discord.Commands;

namespace SwissbotCore.Handlers
{
    public class HelpMessageHandler
    {
        public static int HelpmsgPerPage = 8;
        public static List<string> HelpPagesPublic = new List<string>();
        public static List<string> HelpPagesStaff = new List<string>();
        public static List<string> HelpPagesDev = new List<string>();

        public static int HelpPagesPublicCount = 0;
        public static int HelpPagesStaffCount = 0;
        public static int HelpPagesDevCount = 0;

        public static Dictionary<ulong, ulong> CurrentHelpMessages = Global.LoadHelpMessageCards();
        private DiscordSocketClient client;
        public HelpMessageHandler(DiscordSocketClient client)
        {
            this.client = client;
            BuildHelpPages();
            HelpPagesPublicCount = (int)Math.Ceiling((double)HelpPagesPublic.Count / (double)HelpmsgPerPage);
            HelpPagesStaffCount = (int)Math.Ceiling((double)HelpPagesStaff.Count / (double)HelpmsgPerPage);
            HelpPagesDevCount = (int)Math.Ceiling((double)HelpPagesDev.Count / (double)HelpmsgPerPage);
        }

        public async Task HandleHelpMessage(Cacheable<Discord.IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.User.Value.IsBot)
                return;
            if (!CurrentHelpMessages.Keys.Any(x => x == arg3.MessageId))
                return;
            var msg = (RestUserMessage)client.GetGuild(Global.SwissGuildId).GetTextChannel(arg3.Channel.Id).GetMessageAsync(arg3.MessageId).Result;
            if (CurrentHelpMessages.Keys.Contains(arg3.MessageId) && msg != null)
            {
                if(arg3.UserId != CurrentHelpMessages[arg3.MessageId])
                {
                    await msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                    return;
                }
                //is a valid card, lets check what page were on
                var s = msg.Embeds.First().Title;

                Regex r = new Regex(@"\*\*Help \((\d)\/(\d)\)");
                var mtc = r.Match(s);
                var curpage = int.Parse(mtc.Groups[1].Value);

                if (arg3.Emote.Name == "⬅")
                {
                    //check if the message is > 2 weeks old or exists in swiss server
                    if (curpage == 1)
                    {
                        await msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                        return;
                    }

                    await msg.ModifyAsync(x => x.Embed = HelpEmbedBuilder(curpage - 1, CalcHelpPage(client.GetGuild(Global.SwissGuildId).GetUser(arg3.User.Value.Id))));
                    await msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);

                }
                else if (arg3.Emote.Name == "➡")
                {
                    await msg.ModifyAsync(x => x.Embed = HelpEmbedBuilder(curpage + 1, CalcHelpPage(client.GetGuild(Global.SwissGuildId).GetUser(arg3.User.Value.Id))));
                    await msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                }
                else
                {
                    await msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                }

            }
        }
        public static void BuildHelpPages()
        {
            foreach (var item in CommandModuleBase.Commands.Where(x => x.RequiresPermission ==false))
            {
                if (item.CommandHelpMessage == null && item.CommandDescription == null)
                    continue;
                string n = "";
                foreach (var pfl in item.Prefixes)
                {
                    n += $"**\\{pfl}{item.CommandName}**, ";
                }
                n = n.Remove(n.Length - 2);
                string desc = "";
                desc = n + "\n";
                string tmp = "";
                if (item.CommandDescription != null)
                {
                    tmp += $"{item.CommandDescription}";
                }
                if (item.CommandHelpMessage != null)
                {
                    if (tmp == "")
                        tmp += $"{item.CommandHelpMessage}";
                    else
                        tmp += $"\n\n{item.CommandHelpMessage}";
                }
                HelpPagesPublic.Add(desc + tmp + "\n");
            }
            foreach (var item in CommandModuleBase.Commands)
            {
                if (item.CommandHelpMessage == null && item.CommandDescription == null)
                    continue;
                string n = "";
                foreach (var pfl in item.Prefixes)
                {
                    n += $"**\\{pfl}{item.CommandName}**, ";
                }
                n = n.Remove(n.Length - 2);
                string desc = "";
                desc = n + "\n";
                string tmp = "";
                if (item.CommandDescription != null)
                {
                    tmp += $"{item.CommandDescription}";
                }
                if (item.CommandHelpMessage != null)
                {
                    if (tmp == "")
                        tmp += $"{item.CommandHelpMessage}";
                    else
                        tmp += $"\n\n{item.CommandHelpMessage}";
                }
                HelpPagesStaff.Add(desc + tmp + "\n");
            }
            foreach (var item in CommandModuleBase.Commands)
            {
                string n = "";
                foreach (var pfl in item.Prefixes)
                {
                    n += $"**\\{pfl}{item.CommandName}**, ";
                }
                n = n.Remove(n.Length - 2);
                string desc = "";
                desc = n + "\n";
                string tmp = "";
                if (item.CommandDescription != null)
                {
                    tmp += $"{item.CommandDescription}";
                }
                if (item.CommandHelpMessage != null)
                {
                    if (tmp == "")
                        tmp += $"{item.CommandHelpMessage}";
                    else
                        tmp += $"\n\n{item.CommandHelpMessage}";
                }
                HelpPagesDev.Add(desc + tmp + "\n");
            }
        }
        public static Embed HelpEmbedBuilder(int page, HelpPages p)
        {
            if(p == HelpPages.Public)
            {
                //if (curpage == HelpPagesPublicCount)
                //{
                //    await msg.RemoveReactionAsync(arg3.Emote, arg3.User.Value);
                //    return;
                //}
                if (page > HelpPagesPublicCount)
                    page = page - 1;
                var rs = HelpPagesPublic.Skip((page - 1) * HelpmsgPerPage).Take(HelpmsgPerPage);
                var em = new EmbedBuilder()
                {
                    Title = $"**Help ({page}/{HelpPagesPublicCount})**",
                    Color = Color.Green,
                    Description = string.Join("\n", rs),
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = "Public help message"
                    }
                };
                
                return em.Build();
            }
            else if(p == HelpPages.Staff)
            {

                if (page > HelpPagesStaffCount)
                    page = page - 1;
                var em = new EmbedBuilder()
                {
                    Title = $"**Help ({page}/{HelpPagesStaffCount})**",
                    Color = Color.Green,
                    Description = string.Join("\n", HelpPagesStaff.Skip((page - 1) * HelpmsgPerPage).Take(HelpmsgPerPage)),
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = "Staff help message"
                    }
                };
                return em.Build();
            }
            else if (p == HelpPages.Dev)
            {

                if (page > HelpPagesDevCount)
                    page = page - 1;
                var em = new EmbedBuilder()
                {
                    Title = $"**Help ({page}/{HelpPagesDevCount})**",
                    Color = Color.Green,
                    Description = string.Join("\n", HelpPagesDev.Skip((page - 1) * HelpmsgPerPage).Take(HelpmsgPerPage)),
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = "Dev help message"
                    }
                };
                return em.Build();
            }
            else
                return null;
        }
        public static HelpPages CalcHelpPage(SocketGuildUser usr)
        {
            if (usr.Guild.Id == Global.SwissBotDevGuildID)
                return HelpPages.Dev;
            if (usr.Roles.Any(x => x.Id == Global.DeveloperRoleId))
                return HelpPages.Dev;
            if (usr.Guild.GetRole(Global.ModeratorRoleID).Position <= usr.Hierarchy)
                return HelpPages.Staff;
            else
                return HelpPages.Public;
        }
        public enum HelpPages
        {
            Public,
            Staff,
            Dev
        }
    }
}
