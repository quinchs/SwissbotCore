using Discord;
using SwissbotCore.Handlers;
using SwissbotCore.Handlers.Swissplus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Modules
{
    [DiscordCommandClass]
    public class Swissplus : CommandModuleBase
    {
        [DiscordCommand("afk", prefixes = new char[] { '*', '+' })]
        public async Task afk(params string[] args)
        {
            string reason = "No reason provided";

            if(args.Length > 0)
                reason = string.Join(' ', args);

            // Get the handler
            var handler = HandlerService.GetHandlerInstance<AfkHandler>();

            if(handler == null)
            {
                Console.WriteLine("Handler null");

                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = "Command unavailable",
                    Description = "The bot needs to restart for this command to work :/",
                    Color = Color.Red,
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = "handler"
                    },
                }.WithCurrentTimestamp().Build());
                return;
            }

            if (handler.AfkStatus == null)
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = "Command unavailable",
                    Description = "The bot needs to restart for this command to work :/",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = "handler.AfkStatus"
                    },
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }

            if (handler.AfkStatus.ContainsKey(Context.User.Id))
                return;

            handler.SetUserStatus(Context.User.Id, reason);


            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
            {
                Title = $":wave: Goodbye {Context.User}",
                Description = "I've successfully set your AFK Status!",
                Fields = new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    {
                        Name = "Reason",
                        Value = reason,
                    }
                },
                Footer = new EmbedFooterBuilder()
                {
                    Text = "Afk set at"
                },
                Color = Blurple
            }.WithCurrentTimestamp().Build());  
        }

        [DiscordCommand("ask-staff", prefixes = new char[] { '*', '+'})]
        public async Task askstaff(params string[] args)
        {
            if(args.Length == 0)
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = "Huh?",
                    Description = "You didn't provide any question to ask the staff",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }

            string question = string.Join(' ', args);

            var handler = HandlerService.GetHandlerInstance<AskStaffHandler>();

            if(handler == null)
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
                    Description = "You can only ask one question per hour!",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = "You can ask again at"
                    },
                    Timestamp = result.lastAskTime.AddHours(1),
                    Color = Color.Red
                }.Build());
                return;
            }

            var ico = Context.User.GetAvatarUrl();
            if (ico == null)
                ico = Context.User.GetDefaultAvatarUrl();

            var embed = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    IconUrl = ico,
                    Name = $"{Context.User}'s Question:"
                },
                Description = question + $" []({Context.User.Id})",
                Color = Blurple
            }.WithCurrentTimestamp();

            var message = await Global.AskStaffChannel.SendMessageAsync("", false, embed.Build());

            handler.SetUserAsked(Context.User.Id, DateTime.UtcNow);

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

        [DiscordCommand("aeroboy", prefixes = new char[] {'*', '+'}, description = "bean me daddy")]
        public async Task aeroboy(params string[] args)
        {
            if(Context.User.Id != 599759136493797377)
            {
                await Context.Channel.SendMessageAsync("This command has been reserved for King Aeroboy! Fuck off you shit!");
                return;
            }

            if(args.Length == 0)
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = "Who do you want to aeroboy?",
                    Description = "Please provide a user and reason",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }

            var user = await GetUser(args[0]);

            if(user == null)
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = "Unknown user",
                    Description = $"Cant find \"{args[0]}\"",
                    Color = Color.Red
                }.WithCurrentTimestamp().Build());
                return;
            }

            string reason = "No reason provided";

            if(args.Length > 1)
                reason = string.Join(' ', args);

            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
            {
                Title = $"Sucessfuly Areoboy'd {user} ({user.Id})",
                Description = $"The user {user} has been successfully **Aeroboy'd**",
                Fields = new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    {
                        Name = "Offender",
                        Value = Context.User.ToString(),
                        IsInline = true
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "Reason",
                        Value = reason,
                        IsInline = true
                    }
                },
                ImageUrl = "https://cdn.discordapp.com/attachments/726863858496700527/796463605230469130/SPOILER_IMG_20200702_183230.jpg",
                Color = Blurple
            }.WithCurrentTimestamp().Build());
        }
    }
}
