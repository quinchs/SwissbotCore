using Discord;
using Discord.WebSocket;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SwissbotCore.Handlers
{
    [DiscordHandler]
    public class MessageCountHandler
    {
        private DiscordSocketClient client;
        public List<ulong> ChannelBlacklists = new List<ulong>();
        public Settings Settings = new Settings();
        public Timer timer = new Timer();
        public Timer VcTimer = new Timer();

        

        public MessageCountHandler(DiscordSocketClient c)
        {
            this.client = c;

            client.MessageReceived += MessageCounter;

            timer.Elapsed += HandleCheck;
            timer.Interval = 1800000;
            timer.Start();

            //VcTimer.Elapsed += HandleVcCheck;
            //VcTimer.Interval = 60000;
            //VcTimer.Start();

            try
            {
                client.Rest.CreateGuildCommand(new Discord.SlashCommandCreationProperties()
                {
                    Name = "staff-activity",
                    Description = "Changes the count filer for staff messasges",
                    Options = new List<Discord.ApplicationCommandOptionProperties>()
                    {
                        new Discord.ApplicationCommandOptionProperties()
                        {
                            Name = "blacklist",
                            Description = "blacklists a channel from adding to message count",
                            //Required = true,
                            Type = Discord.ApplicationCommandOptionType.SubCommandGroup,
                            Options = new List<Discord.ApplicationCommandOptionProperties>()
                            {
                                new Discord.ApplicationCommandOptionProperties()
                                {
                                    Name = "add",
                                    Description = "adds a channel to the staff count blacklist",
                                    Type = Discord.ApplicationCommandOptionType.SubCommand,
                                    Options = new List<Discord.ApplicationCommandOptionProperties>()
                                    {
                                        new Discord.ApplicationCommandOptionProperties()
                                        {
                                            Name = "Channel",
                                            Description = "The channel to blacklist",
                                            Type = Discord.ApplicationCommandOptionType.Channel,
                                            Required = true,
                                        }
                                    }
                                },
                                new Discord.ApplicationCommandOptionProperties()
                                {
                                    Name = "remove",
                                    Description = "removes a channel to the staff count blacklist",
                                    Type = Discord.ApplicationCommandOptionType.SubCommand,
                                    Options = new List<Discord.ApplicationCommandOptionProperties>()
                                    {
                                        new Discord.ApplicationCommandOptionProperties()
                                        {
                                            Name = "Channel",
                                            Description = "The channel to remove from the blacklist",
                                            Type = Discord.ApplicationCommandOptionType.Channel,
                                            Required = true,
                                        }
                                    }
                                },
                                new Discord.ApplicationCommandOptionProperties()
                                {
                                    Name = "list",
                                    Description = "lists all channels in the staff count blacklist",
                                    Type = Discord.ApplicationCommandOptionType.SubCommand,
                                }
                            }
                        },
                        new ApplicationCommandOptionProperties()
                        {
                            Name = "check",
                            Description = "checks a staff members activity",
                            Type = ApplicationCommandOptionType.SubCommand,
                            Options = new List<ApplicationCommandOptionProperties>()
                            {
                                new ApplicationCommandOptionProperties()
                                {
                                    Name = "user",
                                    Description = "The user to check the activity of",
                                    Type = ApplicationCommandOptionType.User,
                                    Required = true
                                }
                            }
                        },
                        new ApplicationCommandOptionProperties()
                        {
                            Name = "settings",
                            Description = "Change the settings of the count filter",
                            Type = ApplicationCommandOptionType.SubCommandGroup,
                            Options = new List<ApplicationCommandOptionProperties>()
                            {
                                new ApplicationCommandOptionProperties()
                                {
                                    Name = "minimum-messages",
                                    Description = "change the minimum amount of messages to trigger a infraction",
                                    Type = ApplicationCommandOptionType.SubCommand,
                                    Options = new List<ApplicationCommandOptionProperties>()
                                    {
                                        new ApplicationCommandOptionProperties()
                                        {
                                            Name = "set",
                                            Type = ApplicationCommandOptionType.Integer,
                                            Description = "sets the minimum messages to trigger a infraction",
                                        }
                                    }
                                }
                            }
                        }
                    }
                }, Global.SwissGuildId);
            }
            catch(Exception x)
            {

            }

            client.InteractionCreated += Client_InteractionCreated;

            Task.Run(async () =>
            {
                // Load the state
                try
                {
                    ChannelBlacklists = await SwissbotStateHandler.LoadObject<List<ulong>>("StaffMessagesCountBlacklist.json");
                }
                catch
                {
                    ChannelBlacklists = new List<ulong>();
                }

                try
                {
                    Settings = await SwissbotStateHandler.LoadObject<Settings>("StaffMessagesSettings.json");
                }
                catch
                {
                    Settings = new Settings();
                }
            }); 
        }

        private async void HandleCheck(object sender, ElapsedEventArgs e)
        {
            if(!Settings.HasCompletedCheck && DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday && DateTime.UtcNow.Hour == 12)
            {
                var alertsChannel = Global.SwissGuild.GetTextChannel(665647956816429096);
                // do check for infractions
                var users = StaffMember.StaffMemberCollection.Find(x => x.GetMessageCount(DateTime.UtcNow.AddDays(-7)) < Settings.MinimumMessages && x.IsCheckReady);

                foreach(var user in users.ToList())
                {
                    user.Infractions.Add(new CountInfraction()
                    {
                        Count = user.GetMessageCount(DateTime.UtcNow.AddDays(-7)),
                        Date = DateTime.UtcNow,
                        MinimumAtTime = Settings.MinimumMessages
                    });

                    var u = await Global.GetSwissbotUser(user.Id);

                    await alertsChannel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Inactivity report",
                        Description = $"{(u == null ? $"<@{user.Id}>" : $"{u.ToString()}")} has failed to meet the activity requirement of {Settings.MinimumMessages} messages per week.",
                        Color = CommandModuleBase.Blurple,

                    }.WithCurrentTimestamp().Build());
                }
                Settings.HasCompletedCheck = true;
                SaveSettings();
            }
            else if (Settings.HasCompletedCheck)
            {
                Settings.HasCompletedCheck = false;
                SaveSettings();
            }
        }

        private async Task Client_InteractionCreated(SocketInteraction arg)
        {
            if(arg.Data.Name == "staff-activity")
            {
                if(!Program.UserHasPerm(arg.Member))
                {
                    await arg.FollowupAsync("No... Bad!");
                    return;
                }

                
                if (arg.Data.Options.Count == 1)
                {
                    var opt = arg.Data.Options.First();
                    
                    switch (opt.Name)
                    {
                        case "blacklist":
                            {
                                if (!(arg.Member.Roles.Any(x => x.Id == 592464345322094593) || arg.Member.Id == 259053800755691520))
                                {
                                    await arg.FollowupAsync("No... Bad!");
                                    return;
                                }

                                var sub = opt.Options.First();
                                switch (sub.Name)
                                {
                                    case "add":
                                        {
                                            var channel = ulong.Parse(sub.Options.First().Value.ToString());
                                            if (ChannelBlacklists.Any(x => x == channel))
                                            {
                                                await arg.FollowupAsync("", false, new EmbedBuilder()
                                                {
                                                    Title = "Can't do that!",
                                                    Description = $"The channel <#{channel}> is already in the blacklist.",
                                                    Color = Color.Red
                                                }.Build());
                                                return;
                                            }

                                            ChannelBlacklists.Add(channel);
                                            SaveBlacklist();

                                            await arg.FollowupAsync("", false, new EmbedBuilder()
                                            {
                                                Title = "Done!",
                                                Description = $"The channel <#{channel}> is now in the blacklist.",
                                                Color = Color.Green,
                                            }.Build());
                                            return;
                                        }
                                    case "remove":
                                        {
                                            var channel = ulong.Parse(sub.Options.First().Value.ToString());
                                            if (!ChannelBlacklists.Any(x => x == channel))
                                            {
                                                await arg.FollowupAsync("", false, new EmbedBuilder()
                                                {
                                                    Title = "Can't do that!",
                                                    Description = $"The channel <#{channel}> is not in the blacklist.",
                                                    Color = Color.Red
                                                }.Build());
                                                return;
                                            }

                                            ChannelBlacklists.Remove(channel);
                                            SaveBlacklist();

                                            await arg.FollowupAsync("", false, new EmbedBuilder()
                                            {
                                                Title = "Done!",
                                                Description = $"The channel <#{channel}> is now removed from the blacklist.",
                                                Color = Color.Green,
                                            }.Build());
                                            return;
                                        }
                                    case "list":

                                        List<string> channels = new List<string>();
                                        foreach (var item in ChannelBlacklists)
                                        {
                                            var chan = Global.SwissGuild.GetChannel(item);
                                            if (chan == null)
                                                channels.Add($"{item} - #unknown");
                                            else
                                                channels.Add($"{item} - #{chan.Name}");
                                        }

                                        await arg.FollowupAsync("", false, new EmbedBuilder()
                                        {
                                            Title = "Blacklist",
                                            Description = channels.Count > 0 ? $"Here's the channel blacklist:\n```\n{string.Join("\n", channels)}```" : "There are no blacklisted channels",

                                        }.Build());

                                        break;
                                }
                            }
                            break;
                        case "check":
                            {
                                var id = ulong.Parse(opt.Options.First().Value.ToString());

                                var user = await Global.GetSwissbotUser(id);

                                var record = StaffMember.GetStaffMember(id);

                                if (record == null || user == null)
                                {
                                    await arg.FollowupAsync("", false, new EmbedBuilder()
                                    {
                                        Title = "404, suck me off",
                                        Description = $"The user <@{id}> does not have any messages on record. Either they are not staff or they have 0 messages :/",
                                        Color = Color.Red
                                    }.Build());
                                    return;
                                }

                                var av = user.GetAvatarUrl();
                                if (av == null)
                                    av = user.GetDefaultAvatarUrl();

                                await arg.RespondAsync("", false, new EmbedBuilder()
                                {
                                    Author = new EmbedAuthorBuilder()
                                    {
                                        Name = user.ToString(),
                                        Url = av
                                    },
                                    Title = "Message count",
                                    Description = $"Here's the message count for <@{id}>",
                                    Fields = new List<EmbedFieldBuilder>()
                                    {
                                        new EmbedFieldBuilder()
                                        {
                                            Name = "Past 24 Hours:",
                                            Value = $"> {record.GetMessageCount(DateTime.UtcNow.AddDays(-1))}"
                                        },
                                        new EmbedFieldBuilder()
                                        {
                                            Name = "Past 7 Days:",
                                            Value = $"> {record.GetMessageCount(DateTime.UtcNow.AddDays(-7))}"
                                        },
                                        new EmbedFieldBuilder()
                                        {
                                            Name = "Past 31 Days:",
                                            Value = $"> {record.GetMessageCount(DateTime.UtcNow.AddDays(-31))}"
                                        },
                                        new EmbedFieldBuilder()
                                        {
                                            Name = "Infractions",
                                            Value = $"{(record.Infractions.Any() ? $"```\n{string.Join("\n", record.Infractions.Select(x => $"{x.Date.ToString("R")} - {x.Count}/{x.MinimumAtTime}"))}```" : "none")}"
                                        }
                                    },
                                    Color = Color.Green
                                }.WithCurrentTimestamp().Build());

                            }
                            break;
                        case "settings":
                            {
                                if (!(arg.Member.Roles.Any(x => x.Id == 592464345322094593) || arg.Member.Id == 259053800755691520))
                                {
                                    await arg.FollowupAsync("No... Bad!");
                                    return;
                                }

                                var sub = opt.Options.First();
                                switch (sub.Name)
                                {
                                    case "minimum-messages":
                                        if(sub.Options == null)
                                        {
                                            await arg.FollowupAsync("", false, new EmbedBuilder() 
                                            {
                                                Title = "Minimum Messages",
                                                Description = $"The minimum messages per week is currently set at {Settings.MinimumMessages}",
                                                Color = Color.Green,
                                            }.Build());
                                            return;
                                        }
                                        else
                                        {
                                            var c = int.Parse(sub.Options.First().Value.ToString());

                                            Settings.MinimumMessages = c;

                                            SaveSettings();

                                            await arg.FollowupAsync("", false, new EmbedBuilder()
                                            {
                                                Title = "Success!",
                                                Description = $"The minimum messages per week is now set at {Settings.MinimumMessages}",
                                                Color = Color.Green,
                                            }.Build());
                                            return;
                                        }
                                }
                                break;
                            }
                    }
                   
                }
            }
        }

        private async Task MessageCounter(SocketMessage arg)
        {
            // Check if we are in a guild
            if (arg.Channel is SocketTextChannel channel) // Liege fat
            {
                if (arg.Author.IsBot)
                    return;

                if (channel.Guild.Id != Global.SwissGuildId)
                    return;

                // Check if author cuck is staff
                var user = await Global.GetSwissbotUser(arg.Author.Id);

                if (user == null)
                    return;

                if (user.Hierarchy >= Global.ModeratorRole.Position)
                {
                    if(ChannelBlacklists.Any(x => x == arg.Channel.Id))
                        return;

                    var member = StaffMember.GetOrCreateStaffMember(user);
                    member.AddMessageRecord(arg);
                }
            }

        }

        public enum ActivityType
        {
            Voice,
            Message
        }
       
        public void SaveBlacklist()
            => SwissbotStateHandler.SaveObject("StaffMessagesCountBlacklist.json", ChannelBlacklists);

        public void SaveSettings()
           => SwissbotStateHandler.SaveObject("StaffMessagesSettings.json", Settings);

    }

   
    public class Settings
    {
        public int MinimumMessages { get; set; } = 500;
        public bool HasCompletedCheck { get; set; } = false;

    }

    [BsonIgnoreExtraElements]
    public class StaffMemberMessage
    {
        public DateTime Time { get; set; }
        public ulong ChannelId { get; set; }
        public ulong AuthorId { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class StaffMember
    {
        [BsonIgnore]
        public static IMongoCollection<StaffMemberMessage> MessageCollection
            => HandlerService.GetHandlerInstance<MongoHandler>().MessageCountCollection;

        [BsonIgnore]
        public static IMongoCollection<StaffMember> StaffMemberCollection
            => HandlerService.GetHandlerInstance<MongoHandler>().StaffMemberCollection;

        public List<CountInfraction> Infractions { get; set; } = new List<CountInfraction>();
        
        public DateTime CreatedAt { get; set; }

        public ulong Id { get; set; }

        [BsonIgnore]
        public bool IsCheckReady
            => (DateTime.UtcNow - this.CreatedAt).TotalDays >= 7;

        public StaffMember(IGuildUser staff)
        {
            this.Id = staff.Id;
            this.CreatedAt = DateTime.UtcNow;
            SaveThis();
        }

        public StaffMember(IGuildUser staff, IMessage message)
        {
            this.Id = staff.Id;
            this.CreatedAt = DateTime.UtcNow;
            this.AddMessageRecord(message);
            SaveThis();
        }

        public long GetMessageCount(DateTime from)
            => MessageCollection.CountDocuments(x => x.AuthorId == this.Id && x.Time > from);

        public void AddMessageRecord(IMessage m)
        {
            MessageCollection.InsertOne(new StaffMemberMessage()
            {
                AuthorId = this.Id,
                ChannelId = m.Channel.Id,
                Time = m.Timestamp.UtcDateTime
            });
        }

        public void AddInfraction(CountInfraction inf)
        {
            this.Infractions.Add(inf);
            SaveThis();
        }

        public static bool StaffMemberExists(ulong id)
        {
            return 0 < StaffMemberCollection.CountDocuments(x => x.Id == id);
        }

        public static StaffMember GetStaffMember(ulong id)
        {
            var f = StaffMemberCollection.Find(x => x.Id == id);

            if (f.Any())
                return f.First();
            else return null;
        }

        public static StaffMember GetOrCreateStaffMember(IGuildUser user)
        {
            if (StaffMemberExists(user.Id))
                return StaffMemberCollection.Find(x => x.Id == user.Id).First();
            else
                return new StaffMember(user);
        }

        private ReplaceOneResult SaveThis()
            => StaffMemberCollection.ReplaceOne<StaffMember>(x => x.Id == this.Id, this, new ReplaceOptions() { IsUpsert = true });
    }
    public class CountInfraction
    {
        public DateTime Date { get; set; }
        public int MinimumAtTime { get; set; }
        public long Count { get; set; }
    }
   
}
