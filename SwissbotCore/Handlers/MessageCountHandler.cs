using Discord;
using Discord.WebSocket;
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
        public List<StaffMember> Records;
        public List<ulong> ChannelBlacklists = new List<ulong>();
        public Settings Settings = new Settings();
        public Timer timer = new Timer();

        public MessageCountHandler(DiscordSocketClient c)
        {
            this.client = c;

            client.MessageReceived += MessageCounter;

            timer.Elapsed += HandleCheck;
            timer.Interval = 1800000;
            timer.Start();

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
                    Records = await SwissbotStateHandler.LoadObject<List<StaffMember>>("StaffMessagesCount.json");
                }
                catch
                {
                    Records = new List<StaffMember>();
                }

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
                var users = Records.Where(x => x.CountPast7Days < Settings.MinimumMessages && x.IsCheckReady);

                foreach(var user in users)
                {
                    user.Infractions.Add(new CountInfraction()
                    {
                        Count = user.CountPast7Days,
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
                SaveRecords();
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
                if (!(arg.Member.Roles.Any(x => x.Id == 592464345322094593) || arg.Member.Id == 259053800755691520))
                {
                    await arg.FollowupAsync("No... Bad!");
                    return;
                }

                if(arg.Data.Options.Count == 1)
                {
                    var opt = arg.Data.Options.First();
                    switch (opt.Name)
                    {
                        case "blacklist":
                            {
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

                                var record = Records.FirstOrDefault(x => x.Id == id);

                                

                                if (record == null)
                                {
                                    await arg.FollowupAsync("", false, new EmbedBuilder()
                                    {
                                        Title = "404, suck me off",
                                        Description = $"The user <@{id}> does not have any messages on record. Either they are not staff or they have 0 messages :/",
                                        Color = Color.Red
                                    }.Build());
                                    return;
                                }

                                var past24 = record.Messages.Count(x => (DateTime.UtcNow - x.Time).TotalDays < 1);
                                var past7days = record.Messages.Count(x => (DateTime.UtcNow - x.Time).TotalDays < 7);
                                var past31day = record.Messages.Count(x => (DateTime.UtcNow - x.Time).TotalDays < 31);

                                var oldest = record.Messages.First();

                                await arg.FollowupAsync("", false, new EmbedBuilder() 
                                { 
                                    Title = "Message count",
                                    Description = $"Here's the message count for <@{id}>",
                                    Fields = new List<EmbedFieldBuilder>()
                                    {
                                        new EmbedFieldBuilder()
                                        {
                                            Name = "Past 24 hours",
                                            Value = $"{past24}"
                                        },
                                        new EmbedFieldBuilder()
                                        {
                                            Name = "Past 7 days",
                                            Value = $"{past7days}"
                                        },
                                        new EmbedFieldBuilder()
                                        {
                                            Name = "Past 31 days",
                                            Value = $"{past31day}",
                                        },
                                        new EmbedFieldBuilder()
                                        {
                                            Name = "Oldest record",
                                            Value = $"{oldest.Time.ToString("R")}"
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

                    await UpdateUserRecord(arg);
                }
            }

        }

        private async Task UpdateUserRecord(SocketMessage arg)
        {
            if(Records.Any(x => x.Id == arg.Author.Id))
            {
                Records[Records.FindIndex(x => x.Id == arg.Author.Id)].Messages.Add(new MessageRecord()
                {
                    Channel = arg.Channel.Id,
                    Time = arg.Timestamp.UtcDateTime
                });
            }
            else
            {
                Records.Add(new StaffMember()
                {
                    Id = arg.Author.Id,
                    Messages = new List<MessageRecord>()
                    {
                        new MessageRecord()
                        {
                            Channel = arg.Channel.Id,
                            Time = arg.Timestamp.UtcDateTime
                        }
                    }
                });
            }

            SaveRecords();
        }

        public void SaveBlacklist()
            => SwissbotStateHandler.SaveObject("StaffMessagesCountBlacklist.json", ChannelBlacklists);

        public void SaveSettings()
           => SwissbotStateHandler.SaveObject("StaffMessagesSettings.json", Settings);

        public void SaveRecords()
            => SwissbotStateHandler.SaveObject("StaffMessagesCount.json", Records);

    }

   
    public class Settings
    {
        public int MinimumMessages { get; set; } = 500;
        public bool HasCompletedCheck { get; set; } = false;

    }
    
    public class StaffMember
    {
        public List<CountInfraction> Infractions { get; set; } = new List<CountInfraction>();
        public List<MessageRecord> Messages { get; set; } = new List<MessageRecord>();
        public ulong Id { get; set; }

        public bool IsCheckReady
            => Messages.Any() ? (DateTime.UtcNow - Messages.First().Time).TotalDays >= 7 : true;

        public int CountPast24Hours
            => Messages.Count(x => (DateTime.UtcNow - x.Time).TotalDays < 1);
        public int CountPast7Days
            => Messages.Count(x => (DateTime.UtcNow - x.Time).TotalDays < 7);
        public int CountPast31Days
            => Messages.Count(x => (DateTime.UtcNow - x.Time).TotalDays < 31);
    }
    public class CountInfraction
    {
        public DateTime Date { get; set; }
        public int MinimumAtTime { get; set; }
        public int Count { get; set; }
    }
    public class MessageRecord
    {
        public DateTime Time { get; set; }
        public ulong Channel { get; set; }
    }
}
