using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Rest;
using System.Text.RegularExpressions;

namespace SwissbotCore.Handlers
{
    [DiscordHandler]
    class SuggestionHandler
    {
        public Dictionary<ulong, DateTime> AskTimes = new Dictionary<ulong, DateTime>();

        public static DiscordSocketClient client { get; set; }
        public SuggestionHandler(DiscordSocketClient _client)
        {
            client = _client;

            AskTimes = LoadAskTimes();

            client.InteractionCreated += Client_InteractionCreated;

            Task.Run(async () =>
            {
                try
                {
                    await client.Rest.CreateGuildCommand(new SlashCommandCreationProperties()
                    {
                        Name = "suggestion-answer",
                        Description = "Answers a question asked in #server-suggestions",
                        Options = new List<ApplicationCommandOptionProperties>()
                        {
                            new ApplicationCommandOptionProperties()
                            {
                                Name = "Message",
                                Description = "The Message ID or the Link to the message containing the suggestion",
                                Required = true,
                                Type = ApplicationCommandOptionType.String
                            },
                            new ApplicationCommandOptionProperties()
                            {
                                Name = "Status",
                                Description = "The status for your review, for example: `On hold`",
                                Type = ApplicationCommandOptionType.String,
                                Required = true,
                                Choices = new List<ApplicationCommandOptionChoiceProperties>()
                                {
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Accepted - The request has been accepted",
                                        Value = "0"
                                    },
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Answered - The question has been answered",
                                        Value = "1"
                                    },
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "On Hold - The question is on hold",
                                        Value = "2"
                                    },
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Denied - The request has been denied",
                                        Value = "3"
                                    },
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Troll - The question is a troll question",
                                        Value = "4"
                                    }
                                }
                            },
                            new ApplicationCommandOptionProperties()
                            {
                                Name = "Review_Text",
                                Type = ApplicationCommandOptionType.String,
                                Description = "The message for your review, for example: \"We are currently working on that!\"",
                                Required = true,
                            }
                        }
                    }, Global.SwissGuildId);
                }
                catch (Exception x)
                {
                    Console.WriteLine($"Failed to create slash command: {x}");
                }
            });
        }

        private async Task Client_InteractionCreated(SocketInteraction arg)
        {
            if (arg.Data.Name == "suggestion-answer")
            {
                if (!Program.UserHasPerm(arg.Member.Id))
                {
                    await arg.Channel.SendMessageAsync("No lmao");
                    return;
                }

                // Get the args
                var messageIdArg = arg.Data.Options.FirstOrDefault(x => x.Name == "message").Value;

                var reviewTextArg = arg.Data.Options.FirstOrDefault(x => x.Name == "review_text");

                var statusArg = arg.Data.Options.FirstOrDefault(x => x.Name == "status");

                var status = (AskStaffStatus)int.Parse(statusArg.Value.ToString());

                if (reviewTextArg.Value.ToString().Length >= 1000)
                {
                    await arg.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Error",
                        Description = $"Your review text must be less than or equal to 1000 characters!",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }

                IMessage message;

                if (Regex.IsMatch(messageIdArg.ToString(), @"^\d{17,18}$"))
                {
                    var messageId = ulong.Parse(messageIdArg.ToString());
                    message = await Global.SuggestionChannel.GetMessageAsync(messageId);
                }
                else if (linkRegex.IsMatch(messageIdArg.ToString()))
                {
                    message = await GetMessageFromUrl(messageIdArg.ToString());
                }
                else
                {
                    await arg.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Error",
                        Description = $"The `MessageId` argument is not valid: `{messageIdArg} is neither a message id nor a url!`",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }

                if (message == null)
                {
                    // Unknown message
                    await arg.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Error",
                        Description = $"The `MessageId` argument is not valid: `The message with the id of `{messageIdArg}` does not exist`",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }

                if (message.Author.Id != client.CurrentUser.Id)
                {
                    // Not sent by us
                    await arg.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Error",
                        Description = $"The `MessageId` argument is not valid: `The message with the id of `{messageIdArg}` is not a suggestion message`",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }

                var embed = message.Embeds.First();

                if (embed.Fields.Length == EmbedBuilder.MaxFieldCount)
                {
                    await arg.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Error",
                        Description = $"The maximum amount of reviews has been reached!",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }

                var authorId = GetAskerId(embed);

                if (!authorId.HasValue)
                {
                    // Unable to get auther
                    await arg.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Error",
                        Description = $"Unable to check the auther of the question, This should not happen. Please contact quin!",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }

                var builder = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        IconUrl = embed.Author.Value.IconUrl,
                        Name = embed.Author.Value.Name
                    },
                    Description = embed.Description,
                    Color = status == AskStaffStatus.Accepted || status == AskStaffStatus.Answered ? Color.Green
                          : status == AskStaffStatus.Denied || status == AskStaffStatus.Troll ? Color.Red
                          : status == AskStaffStatus.OnHold ? Color.Orange
                          : CommandModuleBase.Blurple,
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = "Last reviewed at"
                    },
                    Timestamp = DateTime.UtcNow
                };

                foreach (var field in embed.Fields)
                    builder.AddField(field.Name, field.Value, field.Inline);

                builder.AddField($"{status} - {arg.Member}", $"{reviewTextArg.Value}");

                if (message is SocketUserMessage socketMessage)
                {
                    await socketMessage.ModifyAsync(x => x.Embed = builder.Build());
                }
                else if (message is RestUserMessage restMessage)
                {
                    await restMessage.ModifyAsync(x => x.Embed = builder.Build());
                }
                else
                {
                    // Cry!
                    Global.ConsoleLog($"Type fail reached: {nameof(message)} - {message.GetType()}");
                    await arg.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Error",
                        Description = $"Internal type error! just yell at quin please.",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }

                // Dm the user
                try
                {
                    var user = await Global.GetSwissbotUser(authorId.Value);
                    if (user != null)
                    {
                        await user.SendMessageAsync("", false, new EmbedBuilder()
                        {
                            Title = "Your suggestion to the staff has been reviewed!",
                            Description = $"{arg.Member} reviewed your suggestion \"{embed.Description.Replace($" []({user.Id})", "")}\"",
                            Fields = builder.Fields,
                            Color = builder.Color
                        }.WithCurrentTimestamp().Build());
                    }
                }
                catch
                {
                    // Do nothing
                }

                await arg.RespondAsync("Successfully added your review!");
            }
        }

        private ulong? GetAskerId(IEmbed b)
        {
            var r = Regex.Match(b.Description, @"\[\]\((\d{17,18})\)");
            if (!r.Success)
                return null;

            return ulong.Parse(r.Groups[1].Value);
        }

        public enum AskStaffStatus
        {
            Accepted = 0,
            Answered = 1,
            OnHold = 2,
            Denied = 3,
            Troll = 4,
        }

        private Regex linkRegex = new Regex(@"channels\/(\d{17,18})\/(\d{17,18})\/(\d{17,18})");
        private async Task<IMessage> GetMessageFromUrl(string url)
        {
            if (url == null)
                return null;

            var match = linkRegex.Match(url);

            if (!match.Success)
                return null;

            if (match.Groups.Count != 4)
                return null;

            var guild = ulong.Parse(match.Groups[1].Value);
            var channel = ulong.Parse(match.Groups[2].Value);
            var message = ulong.Parse(match.Groups[3].Value);

            if (guild != Global.SwissGuildId)
                return null;

            if (channel != Global.SuggestionChannelID)
                return null;

            return await Global.SuggestionChannel.GetMessageAsync(message);
        }

        public void SaveAskTimes()
            => SwissbotStateHandler.SaveObject("SuggestionTimes.json", AskTimes);

        public Dictionary<ulong, DateTime> LoadAskTimes()
        {
            try
            {
                return SwissbotStateHandler.LoadObject<Dictionary<ulong, DateTime>>("SuggestionTimes.json").GetAwaiter().GetResult();
            }
            catch
            {
                return new Dictionary<ulong, DateTime>();
            }
        }

        public void SetUserAsked(ulong user, DateTime asked)
        {
            if (AskTimes.ContainsKey(user))
                AskTimes.Remove(user);

            AskTimes.Add(user, asked);
            SaveAskTimes();
        }

        public (bool canAsk, DateTime lastAskTime) UserCanAsk(IUser user)
            => UserCanAsk(user.Id);
        public (bool canAsk, DateTime lastAskTime) UserCanAsk(ulong user)
        {
            if (AskTimes.ContainsKey(user))
            {
                var dates = AskTimes[user];

                if ((DateTime.UtcNow - dates).TotalHours >= 1)
                {
                    AskTimes.Remove(user);
                    SaveAskTimes();
                    return (true, dates);
                }
                else
                {
                    return (false, dates);
                }
            }
            else
                return (true, DateTime.Now);
        }

        [DiscordCommandClass()]
        public class SuggestionCommands : CommandModuleBase
        {
            [DiscordCommand("suggest", BotCanExecute = false, description = "Suggest somthing to the server", commandHelp = "Usage - `(PREFIX)suggest <suggestion>`", RequiredPermission = false)]
            public async Task suggest(params string[] args)
            {
                if (args.Length == 0)
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Huh?",
                        Description = "You didn't provide anything to suggest!",
                        Color = Color.Red
                    }.WithCurrentTimestamp().Build());
                    return;
                }

                string question = string.Join(' ', args);

                var handler = HandlerService.GetHandlerInstance<SuggestionHandler>();

                if (handler == null)
                {
                    Console.WriteLine("Handler null");

                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Command unavailable",
                        Description = "The bot needs to restart for this command to work. Please contact quin :/",
                        Color = Color.Red,
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = "`handler`"
                        },
                    }.WithCurrentTimestamp().Build());
                    return;
                }

                var result = handler.UserCanAsk(Context.User);

                if (!result.canAsk)
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Slow down there buddy!",
                        Description = "You can only suggest somthing once per hour!",
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = "You can ask again at"
                        },
                        Timestamp = result.lastAskTime.AddHours(1),
                        Color = Color.Red
                    }.Build());
                    return;
                }

                handler.SetUserAsked(Context.User.Id, DateTime.UtcNow);

                var ico = Context.User.GetAvatarUrl();
                if (ico == null)
                    ico = Context.User.GetDefaultAvatarUrl();

                var embed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        IconUrl = ico,
                        Name = $"{Context.User}'s suggestion:"
                    },
                    Description = question + $" []({Context.User.Id})",
                    Color = Blurple
                }.WithCurrentTimestamp();

                var message = await Global.SuggestionChannel.SendMessageAsync("", false, embed.Build());
                await message.AddReactionsAsync(new IEmote[] { new Emoji("✅"), new Emoji("❌") });
                
                var msg = await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = "Success!",
                    Description = $"Your question has been posted! You can click [Here]({message.GetJumpUrl()} \"liege is sexy also this is an easter egg?\") to view it!",
                    Color = Color.Green,
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = "Asked at"
                    }
                }.WithCurrentTimestamp().Build());
            }
        }
    }
}
