using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using RedditNet.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace SwissbotCore.Handlers
{
    class RoleAssignerHandler
    {
        public static DiscordSocketClient client;
        public static RoleCard Rolecard = Global.ReadRoleCard();
        public RoleAssignerHandler(DiscordSocketClient c)
        {
            //client = c;

            //client.ReactionAdded += CheckReactAdd;

            //client.ReactionRemoved += CheckReactRemove;
        }
        
        private async Task CheckReactRemove(Discord.Cacheable<Discord.IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg2.Id != Rolecard.ChannelID)
                return;
            var usr = client.GetGuild(Rolecard.ServerID).GetUser(arg3.UserId);
            var guild = client.GetGuild(Rolecard.ServerID);

            var msg = await guild.GetTextChannel(Rolecard.ChannelID).GetMessageAsync(Rolecard.MessageID);
            if (arg3.MessageId != msg.Id)
                return;
            if (arg3.User.Value.IsBot)
                return;

            //doesnt contain "a" tag
            RoleCard.RoleEmoteDesc redVal = null;
            var sem = arg3.Emote.ToString();
            redVal = Rolecard.RoleEmojiIDs.Any(x => x.Emote == sem) ? Rolecard.RoleEmojiIDs.First(x => x.Emote == sem) : null;

            if (redVal == null)
            {
                await msg.RemoveReactionAsync(arg3.Emote, usr);
                return;
            }
            var role = guild.GetRole(redVal.RoleID);
            if (!usr.Roles.Contains(role))
                return;
            await usr.RemoveRoleAsync(role, new RequestOptions() { AuditLogReason = "Self-Assigned role" });
            try
            {
                await usr.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = $"**{guild.Name}**",
                    Description = $"You were removed from the role **\"{role.Name}\"**",
                    Color = Color.Green
                }.Build());
            }
            catch { }
        }

        private async Task CheckReactAdd(Discord.Cacheable<Discord.IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg2.Id != Rolecard.ChannelID)
                return;

            var usr = client.GetGuild(Rolecard.ServerID).GetUser(arg3.UserId);
            var guild = client.GetGuild(Rolecard.ServerID);

            var msg = await guild.GetTextChannel(Rolecard.ChannelID).GetMessageAsync(Rolecard.MessageID);
            if (arg3.MessageId != msg.Id)
                return;
            if (usr.IsBot)
                return;

            RoleCard.RoleEmoteDesc redVal = null;

            redVal = Rolecard.RoleEmojiIDs.Any(x => x.Emote == arg3.Emote.ToString()) ? Rolecard.RoleEmojiIDs.First(x => x.Emote == arg3.Emote.ToString()) : null;

            if(redVal == null)
            {
                await msg.RemoveReactionAsync(arg3.Emote, usr);
                return;
            }   
            
            var role = guild.GetRole(redVal.RoleID);
            await usr.AddRoleAsync(role, new RequestOptions() { AuditLogReason = "Self-Assigned role" });
            try
            {
                await usr.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = $"**{guild.Name}**",
                    Description = $"You were assigned the role **\"{role.Name}\"**",
                    Color = Color.Green
                }.Build());
            }
            catch { }
        }

        public async Task ChangeRoles(SocketGuildUser usr, IRole role, RoleAction act)
        {
            if(act == RoleAction.Add)
                await usr.AddRoleAsync(role);
            if (act == RoleAction.Remove)
                await usr.RemoveRoleAsync(role);

        }
        public enum RoleAction
        {
            Add,
            Remove
        }
        
        public class RoleCard
        {
            public ulong ServerID { get; set; }
            public List<RoleEmoteDesc> RoleEmojiIDs { get; set; }
            public ulong MessageID { get; set; }
            public ulong ChannelID { get; set; }
            public class RoleEmoteDesc
            {
                public ulong RoleID { get; set; }
                public string Emote { get; set; }
                public string Description { get; set; }
            }
        }
        [DiscordCommandClass()]
        public class RoleCommands : CommandModuleBase
        {
            [DiscordCommand("createrolecard", 
            description = "Create a new role card, use the command `addassignablerole` to start adding roles and reactions <3",
            commandHelp = "Parameters - `(PREFIX)createrollcard <#channel> <@Role> <Emote> <Description>,`\n" +
                "you can add more than one role, to do this have your role emote and description then a comma `,`\n" +
                "Example:\n" +
                "--------------\n" +
                "`(PREFIX)createrolecard #general @custom-role-1 🅱 this is a role description followed by a comma, @custom-role-2 🙂 this is the second description`\n" +
                "--------------",
            RequiredPermission = true,
            prefixes = new char[] {'?', '*' }
            )]
            public async Task CreateRoleCard(params string[] args)
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = "Sorry bud",
                    Description = $"This has been disabled bucko",
                    Color = Color.Red
                }.Build());
                return;
                if (!HasExecutePermission)
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Waaiitt a minute.. who are you?",
                        Description = $"Looks like you dont have permission to execute this command. if this is wrong yell at quin>",
                        Color = Color.Red
                    }.Build());
                    return;
                }

                if (args.Length == 0)
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "What? where?",
                        Description = $"Looks like you didnt provide a channel :/ do so please!",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                if(args.Length == 1)
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "What should i add?!",
                        Description = $"Uhhhhh. epic swissbot here, you need to tell me the roles you want to add plus there emoji/description ok bye <3",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                var channel = Context.Message.MentionedChannels.First();
                var redargs = args.Skip(1).ToArray();
                var tmpRolecard = new RoleCard();
                tmpRolecard.RoleEmojiIDs = new List<RoleCard.RoleEmoteDesc>();
                if (redargs.Any(x => x.Contains(",")))//multiple
                {
                    int c = 0;
                    var msg = Context.Channel.SendMessageAsync("this message is here to test the emotes... (dont delete)").Result;

                    foreach (var item in string.Join(' ', redargs).Split(", "))
                    {
                        if(Context.Message.MentionedRoles.Count <= c)
                        {
                            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                            {
                                Title = "What is that?",
                                Description = "The Role you provided is invalid :/",
                                Color = Color.Red
                            }.Build());
                            return;
                        }
                        IRole role = Context.Message.MentionedRoles.ToArray()[c];
                        c++;
                        string emote = item.Split(' ')[1];
                        string description = string.Join(' ', item.Split(' ').Skip(2));

                        if (GuildEmote.TryParse(emote, out var outemote))
                        {
                            tmpRolecard.RoleEmojiIDs.Add(new RoleCard.RoleEmoteDesc() { Description = description, Emote = emote, RoleID = role.Id});
                            try
                            {
                                await msg.AddReactionAsync(outemote);
                            }
                            catch (Exception ex)
                            {
                                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                {
                                    Title = "What is that?",
                                    Description = "The emote you provided is invalid or is broken :/",
                                    Color = Color.Red
                                }.Build());
                                await msg.DeleteAsync();
                                return;
                            }
                            //Global.SaveRoleCard();
                            //await UpdateRoleCard();
                        }
                        else
                        {
                            var m = new Emoji(emote);
                            try
                            {
                                await msg.AddReactionAsync(m);
                                tmpRolecard.RoleEmojiIDs.Add(new RoleCard.RoleEmoteDesc() { Description = description, Emote = m.Name, RoleID = role.Id });
                                //Global.SaveRoleCard();
                                //await UpdateRoleCard();
                            }
                            catch (Exception ex)
                            {
                                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                                {
                                    Title = "What is that?",
                                    Description = "The emote you provided is invalid or is broken :/",
                                    Color = Color.Red
                                }.Build());
                                await msg.DeleteAsync();
                                return;
                            }
                        }
                    }

                    await msg.ModifyAsync(x => x.Content = "Ok good you used valid emotes <3 card will be created very shortly. ps il delete this in 5 seconds lol");
                    new Timer() { AutoReset = false, Interval = 5000, Enabled = true }.Elapsed += (object s, ElapsedEventArgs a) =>
                    {
                        msg.DeleteAsync().Wait();
                    };
                    await DeployCard(tmpRolecard);

                }
                else//single
                {   
                    IRole role = Context.Message.MentionedRoles.First();
                    string emote = redargs[1];
                    string description = string.Join(' ', redargs.Skip(2));

                    if(Emote.TryParse(emote, out var outemote))
                    {
                        tmpRolecard.RoleEmojiIDs.Add(new RoleCard.RoleEmoteDesc() { Description = description, Emote = emote, RoleID = role.Id });
                        //Global.SaveRoleCard();
                        //await UpdateRoleCard();
                    }
                    else
                    {
                        var m = new Emoji(emote);
                        try
                        {
                            var msg = Context.Channel.SendMessageAsync("Testing the emote...").Result;
                            await msg.AddReactionAsync(m);
                            await msg.ModifyAsync(x => x.Content = "Ok good you used a valid emote <3. ps il delete this in 5 seconds lol");
                            new Timer() { AutoReset = false, Interval = 5000, Enabled = true }.Elapsed += (object s, ElapsedEventArgs a) =>
                            {
                                msg.DeleteAsync().Wait();
                            };
                            tmpRolecard.RoleEmojiIDs.Add(new RoleCard.RoleEmoteDesc() { Description = description, Emote = m.Name, RoleID = role.Id });
                            //Global.SaveRoleCard();
                            //await UpdateRoleCard();
                        }
                        catch (Exception ex)
                        {
                            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                            {
                                Title = "What is that?",
                                Description = "The emote you provided is invalid or is broken :/",
                                Color = Color.Red
                            }.Build());
                            return;
                        }
                    }

                    await DeployCard(tmpRolecard);
                }
            }
            public async Task DeployCard(RoleCard c)
            {
                var eb = new EmbedBuilder()
                {
                    Title = "Self-Assignable Role Menu",
                    Color = Color.DarkBlue,
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = "Created on " + DateTime.UtcNow.ToLongDateString() + ", " + DateTime.UtcNow.ToShortTimeString()
                    },
                    Timestamp = DateTime.Now
                };
                string des = "**Here are all the Self-Assignable roles**\n\n";
                List<IEmote> em = new List<IEmote>();
                var sa = Context.Guild.Emotes;
                foreach (var r in c.RoleEmojiIDs)
                {
                    des += $"{r.Emote} - <@&{r.RoleID}>\n{r.Description}\n\n";
                    var m = new Regex(@"\<.?\:(.*?)\:(\d*?)\>").Match(r.Emote);
                    if (m.Success)
                    {
                        if (sa.Any(x => x.Id == ulong.Parse(m.Groups[2].Value)))
                        {
                            em.Add(sa.FirstOrDefault(x => x.Id == ulong.Parse(m.Groups[2].Value)));
                            continue;
                        }
                    }
                    if (Emote.TryParse(r.Emote, out var s))
                    {
                        var name = new Regex(@"\<\:(.*?)\:(\d*?)\>").Match(r.Emote).Groups[1].Value;
                    }    
                    else
                        em.Add(new Emoji(r.Emote));
                }
                eb.Description = des;
                var msg = await Context.Channel.SendMessageAsync("", false, eb.Build());
                await msg.AddReactionsAsync(em.ToArray());
                c.ServerID = Context.Guild.Id;
                c.MessageID = msg.Id;
                c.ChannelID = msg.Channel.Id;
                Rolecard = c;
                Global.SaveRoleCard();
            }
            [DiscordCommand("addassignablerole", 
            commandHelp = "Parameter - `(PREFIX)addassignablerole <@role / roleID> <Emote> <roleDescription>`", 
            description = "Adds a role to the assignrole card", 
            RequiredPermission = true, 
            prefixes = new char[] { '?', '*' })]
            
            public async Task AddRoleToCard(params string[] args)
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = "Sorry bud",
                    Description = $"This has been disabled bucko",
                    Color = Color.Red
                }.Build());
                return;
                if (!HasExecutePermission)
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Waaiitt a minute.. who are you?",
                        Description = $"Looks like you dont have permission to execute this command. if this is wrong yell at quin>",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                if(Rolecard == null)
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Waaiitt a minute.. who are you?",
                        Description = $"Role card is empty, meaning we dont have one yet. try adding one with `{string.Join('|', Commands.First(x => x.CommandName == "createrollcard").Prefixes)}createrollcard <#channel>",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                if(args.Length == 0)
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "What do you want me to add?",
                        Description = "You didnt provide a role and emote :/",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                if(args.Length == 1)
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "What do you want me to add?",
                        Description = "You didnt provide a emote :/",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                if (args.Length == 2)
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "What does this role do?",
                        Description = "You didnt provide description :/",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                if (Context.Message.MentionedRoles.Count != 1)
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "What is that?",
                        Description = "The role you provided is invalid or is not a role :/",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                if (Emote.TryParse(args[1], out var outemote))
                {
                    Rolecard.RoleEmojiIDs.Add(new RoleCard.RoleEmoteDesc() { RoleID = Context.Message.MentionedRoles.First().Id, Emote = args[1], Description = string.Join(' ', args.Skip(2)) });
                    Global.SaveRoleCard();
                    await UpdateRoleCard();
                }
                else
                {
                    var m = new Emoji(args[1]);
                    try
                    {
                        var msg = Context.Channel.SendMessageAsync("Testing the emote...").Result;
                        await msg.AddReactionAsync(m);
                        await msg.ModifyAsync(x => x.Content = "Ok good you used a valid emote <3. ps il delete this in 5 seconds lol");
                        new Timer() { AutoReset = false, Interval = 5000, Enabled = true }.Elapsed += (object s, ElapsedEventArgs a) =>
                         {
                             msg.DeleteAsync().Wait();
                         };
                        Rolecard.RoleEmojiIDs.Add(new RoleCard.RoleEmoteDesc() { RoleID = Context.Message.MentionedRoles.First().Id, Emote = m.Name, Description = string.Join(' ', args.Skip(2)) });
                        Global.SaveRoleCard();
                        await UpdateRoleCard();
                    }
                    catch (Exception ex)
                    {
                        await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                        {
                            Title = "What is that?",
                            Description = "The emote you provided is invalid or is broken :/",
                            Fields = new List<EmbedFieldBuilder>()
                            {
                                { new EmbedFieldBuilder(){ Name = "Exception", Value = ex.Message} }
                            },
                            Color = Color.Red
                        }.Build());
                        return;
                    }
                    
                }
            }
            [DiscordCommand("removeassignablerole", 
            description = "Removes a Self-Assignable role from the Role card",
            commandHelp = "Parameters - `(PREFIX)removeassignablerole <@role>`", 
            RequiredPermission = true,
            prefixes = new char[] {'?', '*' }
            )]
            public async Task RemoveRoleFromCard(string r)
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = "Sorry bud",
                    Description = $"This has been disabled bucko",
                    Color = Color.Red
                }.Build());
                return;
                if (!HasExecutePermission)
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Waaiitt a minute.. who are you?",
                        Description = $"Looks like you dont have permission to execute this command. if this is wrong yell at quin>",
                        Color = Color.Red
                    }.Build());
                    return;
                }
                IRole role = Context.Message.MentionedRoles.First();
                if(Rolecard.RoleEmojiIDs.Any(x => x.RoleID == role.Id))
                {
                    Rolecard.RoleEmojiIDs.Remove(Rolecard.RoleEmojiIDs.First(x => x.RoleID == role.Id));
                    Global.SaveRoleCard();
                    await UpdateRoleCard();
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Done!",
                        Description = $"thanks to swissbots big brain we were able to remove it from the card <3",
                        Color = Color.Green
                    }.Build());
                    return;
                }
                else
                {
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "What is that?",
                        Description = "The role you provided isnt on the card",
                        Color = Color.Red
                    }.Build());
                    return;
                }
            }
            public async Task UpdateRoleCard()
            {
                var tmsg = await Context.Guild.GetTextChannel(Rolecard.ChannelID).GetMessageAsync(Rolecard.MessageID);
                var msg = (RestUserMessage)tmsg;
                var emb = new EmbedBuilder()
                {
                    Title = "Self-Assignable Role Menu",
                    Color = Color.DarkBlue,
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = "Updated on " + DateTime.UtcNow.ToLongDateString() + ", " + DateTime.UtcNow.ToShortTimeString()
                    },
                    Timestamp = DateTime.Now
                };
                string ds = "**Here are all the Self-Assignable roles**\n\n";
                List<IEmote> em = new List<IEmote>();
                var sa = Context.Guild.Emotes;
                foreach (var r in Rolecard.RoleEmojiIDs)
                {
                    ds += $"{r.Emote} - <@&{r.RoleID}>\n{r.Description}\n\n";
                    var m = new Regex(@"\<.?\:(.*?)\:(\d*?)\>").Match(r.Emote);
                    if (m.Success)
                    {
                        if (sa.Any(x => x.Id == ulong.Parse(m.Groups[2].Value)))
                        {
                            em.Add(sa.FirstOrDefault(x => x.Id == ulong.Parse(m.Groups[2].Value)));
                            continue;
                        }
                    }
                    if (Emote.TryParse(r.Emote, out var s))
                    {
                        var name = new Regex(@"\<\:(.*?)\:(\d*?)\>").Match(r.Emote).Groups[1].Value;
                    }
                    else
                        em.Add(new Emoji(r.Emote));
                }
                emb.Description = ds;
                await msg.ModifyAsync(x => x.Embed = emb.Build());
                foreach(var i in em)
                {
                    if (!msg.Reactions.Keys.Contains(i))
                        await msg.AddReactionAsync(i);
                }
            }
        }
    }
}
