using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SwissbotCore.Modules
{
    [DiscordCommandClass()]
    public class CommandClass : CommandModuleBase
    {
        [DiscordCommand("topic", commandHelp = "(PREFIX)topic", description = "Generates a random topic to keep the chat active")]
        public async Task Topic()
        {
            var typingChannel = Context.Channel;
            string[] topic = {"What's your favorite TV show?", "What are your hobby's?", "Are you working on anything exciting?", "What's your biggest fear?", "Who's your role model?", "What's your biggest regret?",
            "What's your dream job?", "Do you believe in aliens?", "what's the biggest thing you've hid from someone?", "What do you plan to do this weekend?", "What's something not many people know about you?",
            "What are you most passionate about?", "What makes you laugh out loud?", "What was your favorite thing to do as a kid?", "What is your favorite book?", "What is the worst present you have ever received and why",
            "What is the weirdest food combination you’ve ever tried?", "Who’s your favorite comedian?", "If you could be famous, would you want to? Why?", "Are you a cat person or a dog person?", "How did you hear about this sever",
            "Do you know a good joke, if so, what is it?"};
            Random rand = new Random();
            int index = rand.Next(topic.Length);
            await typingChannel.TriggerTypingAsync();
            await Context.Channel.SendMessageAsync(topic[index]);
        }

        [DiscordCommand("8ball", commandHelp = "(PREFIX)8ball <question>", description = "Replies to your question with the standard 8ball responses")]
        public async Task EightBall(params string[] args)
        {
            string qstn = string.Join(' ', args);
            var typingChannel = Context.Channel;
            if (string.IsNullOrEmpty(qstn))
            {
                await typingChannel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync("Please enter a parameter");
            }

            else
            {
                string[] answers = { "As I see it, yes.", "Ask again later.", "It is certain.", "It is decidedly so.", "Don't count on it.", "Better not tell you now.", "Concentrate and ask again.", " Cannot predict now.",
            "Most likely.", "My reply is no", "Yes.", "You may rely on it.", "Yes - definitely.", "Very doubtful.", "Without a doubt.", " My sources say no.", " Outlook not so good.", "Outlook good.", "Reply hazy, try again",
            "Signs point to yes"};

                Random rand = new Random();
                int index = rand.Next(answers.Length);
                await typingChannel.TriggerTypingAsync();
                await Context.Channel.SendMessageAsync(answers[index]);
            }
        }

        [DiscordCommand("roll", commandHelp = "(PREFIX)roll <input number>", description = "Replies with a random number between 0 and your input number")]
        public async Task Roll(int max_number)
        {
            var typingChannel = Context.Channel;
            Random r = new Random();
            int ans = r.Next(0, max_number);
            await typingChannel.TriggerTypingAsync();
            await Context.Channel.SendMessageAsync($"Number: {ans}");
        }

        [DiscordCommand("userinfo", commandHelp = "(PREFIX)userinfo <user>", description = "Shows information about a user such as when they joined")]
        //[Command("userinfo")]
        public async Task Userinfo([Optional] string inputUser)
        {
            if (!Context.Message.MentionedUsers.Any())
            {
                await Context.Channel.SendMessageAsync("Can't give you any information if you don't specify a member");
            }

            var user = Context.Message.MentionedUsers.First();
            SocketGuildUser SGU = (SocketGuildUser)Context.Message.MentionedUsers.First();
            var GuildUser = Context.Guild.GetUser(Context.User.Id);
            var typingChannel = Context.Channel;
            await typingChannel.TriggerTypingAsync();
            string nickState = "";

            if (SGU.Nickname == null)
            {
                await Context.Channel.SendMessageAsync($"null");
                string nul = "null";
                nickState = nul;
            }

            else
            {
                await Context.Channel.SendMessageAsync(SGU.Nickname);
                nickState = SGU.Nickname;

            }

            EmbedBuilder eb = new EmbedBuilder()
            {
                Title = "I am a title!",
                Description = "I am a description set by initializer.",
            };
            eb.AddField($"Roles:[{SGU.Roles.Count}]", $"{String.Join(separator: ", @", values: SGU.Roles.Select(r => r.ToString()))}");
            eb.AddField("ID:", $"{user.Id}")
                .WithAuthor(Context.Client.CurrentUser)
                .WithColor(Color.DarkPurple)
                .WithTitle($"***{user.Username}***")
                .WithDescription($"{user.Username}#{user.Discriminator}\n Nickname: {nickState}\n status: {user.Status}\n Account created at date: {user.CreatedAt}\nJoined server at: {SGU.JoinedAt}")// +

            .WithCurrentTimestamp()
            .Build();

            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }
    }
}
