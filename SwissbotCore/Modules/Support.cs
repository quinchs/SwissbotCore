using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Modules
{
    public class Support : ModuleBase<SocketCommandContext>
    {
        static DiscordSocketClient client { get; set; }
        static string SnipsFilePath = $"{Environment.CurrentDirectory}\\Data\\Snippets.json";
        /// <summary>
        /// Key:DMChanID, Val:SwissTextChannelID
        /// </summary>
        static Dictionary<ulong, ulong> CurrentTickets = new Dictionary<ulong, ulong>();
        static Snippets CurrentSnippets { get; set; }
        //setup dm support
        public class Snippets
        {
            public Dictionary<string, string> snippets { get; set; }
        }
        public static void start(DiscordSocketClient c)
        {
            client = c;
            client.MessageReceived += CheckDM;
            //load snippets
            if (!File.Exists(SnipsFilePath))
                File.Create(SnipsFilePath).Close();


        }
        public static void SaveSnips()
        {
            string json = JsonConvert.SerializeObject(CurrentSnippets);
            File.WriteAllText(SnipsFilePath, json);
        }
        public static void LoadSnips()
        {
            string json = File.ReadAllText(SnipsFilePath);
            CurrentSnippets = JsonConvert.DeserializeObject<Snippets>(json);
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

        [Command("snippets")]
        public async Task snippets(params string[] args)
        {
            if (!await HasPerms(Context.Guild.GetUser(Context.Message.Author.Id)))
                return;
            if(args.Length == 0)
            {
                EmbedBuilder b = new EmbedBuilder()
                {
                    Title = $"These are the current snippets.",
                    Description = $"to add one do `{Global.Preflix}snippets add <snippetname> <snippetvalue>`\nto remove one do `{Global.Preflix}snippets remove <snippetname>`"
                };
                foreach(var item in CurrentSnippets.snippets)
                {
                    b.Fields.Add(new EmbedFieldBuilder()
                    {
                        Name = item.Key,
                        Value = item.Value
                    });
                }
                await Context.Channel.SendMessageAsync("", false, b.Build());
            }
            else
            {
                if(args.Length > 1)
                {
                    if(args[1] == "add")
                    {
                        string sname = args[2];
                        string sval = String.Join(" ", args).Replace($"add {sname} ", "");
                        if (CurrentSnippets.snippets.Keys.Contains(sname))
                        {
                            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                            {
                                Title = "Uh oh... ",
                                Description = "That snippet already exists!",
                                Color = Color.Red

                            }.Build());
                        }
                        if (CurrentSnippets.snippets.Count < 26)
                        {
                            CurrentSnippets.snippets.Add(sname, sval);
                            SaveSnips();
                            return;
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                            {
                                Title = "Uh oh... ",
                                Description = "Looks like there is too many snippets, please talk to quin to get him to write some super duper code to split the feilds",
                                Color = Color.Red

                            }.Build()); 
                        }
                    }
                    if(args[1] == "remove")
                    {
                        string sname = args[2];

                        if (CurrentSnippets.snippets.Keys.Contains(sname))
                        {
                            CurrentSnippets.snippets.Remove(sname);
                            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                            {
                                Title = "**Succ**ess!",
                                Description = $"Removed **{sname}** from the snippets!",
                                Color = Color.Red

                            }.Build());
                            SaveSnips();
                            return;
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                            {
                                Title = "Uh oh... ",
                                Description = "That snippet does not exist!",
                                Color = Color.Red

                            }.Build());
                        }
                    }
                }
            }
        }

        private static async Task CheckDM(SocketMessage arg)
        {
            if (arg.Channel.GetType() == typeof(SocketDMChannel))
            {
                if (arg.Content.StartsWith("*echo"))
                    return;

                if (CurrentTickets.Keys.Contains(arg.Channel.Id))
                    await SendFromDM(arg);
                else
                    await CreateTicket(arg);
            }
        }
        [Command("r")]
        public async Task SendToDM(params string[] msg)
        {
            var chan = Context.Channel;
        }
        public static async Task SendFromDM(SocketMessage arg)
        {
            var chan = client.GetGuild(Global.SwissGuildId).GetTextChannel(CurrentTickets[arg.Id]);
            await chan.SendMessageAsync($"**{arg.Author.ToString()}** - " + arg.Content);
        }
        public static async Task CreateTicket(SocketMessage arg)
        {
            await arg.Author.SendMessageAsync("Creating ticket...");

            //ticket cat: 606557115758411807
            var chan = await client.GetGuild(Global.SwissGuildId).CreateTextChannelAsync(arg.Author.Username + "-" + arg.Author.Discriminator, x => x.CategoryId = 606557115758411807);

            await chan.SendMessageAsync($"@here New tread! User: {arg.Author.ToString()} (<@{arg.Author.Id}>");
            await chan.SendMessageAsync($"**{arg.Author.ToString()}** - " + arg.Content);

            //create ticket
        }
    }
}
