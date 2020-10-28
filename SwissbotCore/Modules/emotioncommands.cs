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
    public class EmotionClass : CommandModuleBase
    {
        [DiscordCommand("angry", commandHelp = "(PREFIX)angry", description = "WHY YOU LITTLE MUPPET...")]
        public async Task angry()
            => await Context.Message.Channel.SendMessageAsync("OK I'm really angry,what do you want noob, get lost!");
        [DiscordCommand("furious", commandHelp = "(PREFIX)furious", description = "Ok ok swissbot is real angry now....")]
        public async Task furious()
            => await Context.Message.Channel.SendMessageAsync("WHY YOU LITTLE PIECE OF PASTA, I TOLD YOU TO GO AWAY!!!");
        [DiscordCommand("cry", commandHelp = "(PREFIX)cry", description = "Now now swissbot...")]
        public async Task cry()
            => await Context.Message.Channel.SendMessageAsync("**WHAAAAAAAAAA WHHAAAAA** I'm telling Daddy Quin now!");
        [DiscordCommand("punch", commandHelp = "(PREFIX)punch", description = "Why you bully him?")]
        public async Task punch()
            => await Context.Message.Channel.SendMessageAsync("**OWWWWWWW** What was that for?");
        [DiscordCommand("depress", commandHelp = "(PREFIX)depress", description = ":/ Why? Just why?")]
        public async Task depress()
            => await Context.Message.Channel.SendMessageAsync("*cries in the corner*");
    }
}
 
