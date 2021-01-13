using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers.AutoMod
{
    [DiscordHandler]
    public class Censor
    {
        private DiscordSocketClient client;

        public static List<string> WhiteList = new List<string>();


        public Censor(DiscordSocketClient c)
        {
            client = c;

            client.MessageReceived += Client_MessageReceived;

            WhiteList = LoadWhiteList().Result;

            Task.Run(async () =>
            {
                try
                {
                    var cmd = await client.Rest.CreateGuildCommand(new SlashCommandCreationProperties()
                    {
                        Name = "whitelist",
                        Description = "Adds a word to the whitelist that blocks a word from the censor",
                        Options = new List<ApplicationCommandOptionProperties>()
                {
                    new ApplicationCommandOptionProperties()
                    {
                        Type = ApplicationCommandOptionType.SubCommand,
                        Description = "Adds a word to the whitelist",
                        Name = "Add",
                        Options = new List<ApplicationCommandOptionProperties>()
                        {
                            new ApplicationCommandOptionProperties()
                            {
                                Name = "Word",
                                Type = ApplicationCommandOptionType.String,
                                Required = true,
                                Description = "The word to add to the whitelist"
                            }
                        }
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Type = ApplicationCommandOptionType.SubCommand,
                        Description = "Removes a word to the whitelist",
                        Name = "Remove",
                        Options = new List<ApplicationCommandOptionProperties>()
                        {
                            new ApplicationCommandOptionProperties()
                            {
                                Name = "Word",
                                Type = ApplicationCommandOptionType.String,
                                Required = true,
                                Description = "The word to add to the whitelist"
                            }
                        }
                    },
                    new ApplicationCommandOptionProperties()
                    {
                        Type = ApplicationCommandOptionType.SubCommand,
                        Description = "Lists the current whitelist",
                        Name = "List",
                    },
                }
                    }, Global.SwissGuildId);
                }
                catch(Exception x)
                {

                }
            });

            client.InteractionCreated += Client_InteractionCreated;
        }

        private async Task Client_InteractionCreated(SocketInteraction arg)
        {
            if (arg.Data.Name == "whitelist")
            {
                if (!Program.UserHasPerm(arg.Member))
                {
                    await arg.FollowupAsync("Invalid permission fucker, go sit on a pinecone dickhead");
                    return;
                }

                if(arg.Data.Options.Count == 1)
                {
                    var opt = arg.Data.Options.First();
                    
                    switch (opt.Name)
                    {
                            
                        case "add":
                            {
                                var word = (string)opt.Options.First().Value;

                                if (WhiteList.Contains(word))
                                {
                                    await arg.FollowupAsync("", false, new EmbedBuilder()
                                    {
                                        Title = "No.. sex",
                                        Color = Color.Red,
                                        Description = $"The word `{word}` is already in the whitelist!"
                                    }.WithCurrentTimestamp().Build());
                                    return;
                                }

                                AddWhitelist(word);
                                await arg.FollowupAsync("", false, new EmbedBuilder()
                                {
                                    Title = "Success!",
                                    Color = Color.Green,
                                    Description = $"Added `{word}` to the whitelist!"
                                }.WithCurrentTimestamp().Build());
                                return;
                            }
                        case "remove":
                            {
                                var word = (string)opt.Options.First().Value;

                                if (!WhiteList.Contains(word))
                                {
                                    await arg.FollowupAsync("", false, new EmbedBuilder()
                                    {
                                        Title = "No.. sex",
                                        Color = Color.Red,
                                        Description = $"The word `{word}` is not in the whitelist!"
                                    }.WithCurrentTimestamp().Build());
                                    return;
                                }

                                RemoveWhitelist(word);
                                await arg.FollowupAsync("", false, new EmbedBuilder()
                                {
                                    Title = "Success!",
                                    Color = Color.Green,
                                    Description = $"Removed `{word}` from the whitelist!"
                                }.WithCurrentTimestamp().Build());
                                return;
                            }
                        case "list":

                            await arg.FollowupAsync("", false, new EmbedBuilder()
                            {
                                Title = "Whitelist",
                                Color = Color.Green,
                                Description = $"Heres the current whitelist:\n```\n{string.Join("\n", WhiteList)}```"
                            }.WithCurrentTimestamp().Build());
                            return;
                    }
                }
            }
        }

        public void AddWhitelist(string item)
        {
            if (!WhiteList.Contains(item))
                WhiteList.Add(item);

            SaveWhitelist();
        }
        public void RemoveWhitelist(string item)
        {
            if (WhiteList.Contains(item))
                WhiteList.Remove(item);

            SaveWhitelist();
        }

        public void SaveWhitelist()
            => SwissbotStateHandler.SaveObject("Whitelist.json", WhiteList);

        public async Task<List<string>> LoadWhiteList()
        {
            try
            {
                return await SwissbotStateHandler.LoadObject<List<string>>("Whitelist.json");
            }
            catch(Exception x)
            {
                Global.ConsoleLog("Failed to load whitelist, returning empty");
                return new List<string>();
            }
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            if (CheckCensor(arg))
            {
                var smsg = arg.Content;
                if (smsg.Length > 1023)
                {
                    var s = smsg.Take(1020);
                    smsg = new string(s.ToArray()) + "...";
                }
                EmbedBuilder b = new EmbedBuilder()
                {
                    Title = "Censor Alert",
                    Color = Color.Orange,
                    Fields = new List<EmbedFieldBuilder>()
                        {
                            {new EmbedFieldBuilder()
                            {
                                Name = "Message",
                                Value = smsg
                            } },
                            {new EmbedFieldBuilder()
                            {
                                Name = "Author",
                                Value = arg.Author.ToString() + $" ({arg.Author.Id})"
                            } },
                            {new EmbedFieldBuilder()
                            {
                                Name = "Action",
                                Value = "Message Deleted",
                            } },
                            {new EmbedFieldBuilder()
                            {
                                Name = "Channel",
                                Value = $"<#{arg.Channel.Id}>"
                            } },
                            { new EmbedFieldBuilder()
                            {
                                Name = "Jump to timeline",
                                Value = arg.Channel.GetMessagesAsync(2).FlattenAsync().Result.Last().GetJumpUrl()
                            } }

                        }
                };
                await arg.DeleteAsync();
                foreach (var item in Global.CensoredWords)
                    if (arg.Content.ToLower().Contains(item.ToLower()))
                        b.Fields.Add(new EmbedFieldBuilder() { Name = "Field", Value = item });

                await client.GetGuild(Global.SwissGuildId).GetTextChannel(665647956816429096).SendMessageAsync("", false, b.Build());

            }
        }

        public bool CheckCensor(SocketMessage arg)
        {
            if (arg.Author.IsBot) { return false; }
            var r = client.GetGuild(Global.SwissGuildId).GetUser(arg.Author.Id).Roles;
            var adminrolepos = client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == Global.ModeratorRoleID).Position;
            var rolepos = r.FirstOrDefault(x => x.Position >= adminrolepos);
            if (rolepos != null || r.Contains(client.GetGuild(Global.SwissGuildId).Roles.FirstOrDefault(x => x.Id == 622156934778454016)))
            { return false; }

            string cont = arg.Content.ToLower();
            foreach (var item in Global.CensoredWords)
                if (cont.Contains(item.ToLower()))
                {
                    string word = "";
                    if (cont.Contains(' '))
                        word = cont.Split(' ').FirstOrDefault(x => x.Contains(item.ToLower()));
                    else
                        word = cont;

                    if (WhiteList.Any(x => x.ToLower() == word.ToLower()))
                        return false;
                    return true;
                }
            return false;
        }
    }
}
