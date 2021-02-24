﻿using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Discord.Rest;
using Discord;
using System.Net.Http;
using System.Net;
using SwissbotCore.Handlers;
using static SwissbotCore.Handlers.SupportTicketHandler;
using SwissbotCore.Modules;
using static SwissbotCore.Modules.ModDatabase;

namespace SwissbotCore
{
    class Global
    {
        public static char Preflix { get; set; }
        public static string systemSlash = "/";
        private static string ConfigPath = $"{Environment.CurrentDirectory}{systemSlash}Data{systemSlash}Config.json";
        private static string cMSGPath = $"{Environment.CurrentDirectory}{systemSlash}Data{systemSlash}CashedMSG.MSG";
        private static string ConfigSettingsPath = $"{Environment.CurrentDirectory}{Global.systemSlash}Data{Global.systemSlash}ConfigPerms.json";
        //public static string aiResponsePath = $"{Environment.CurrentDirectory}{Global.systemSlash}Data{Global.systemSlash}Responses.AI";
        public static string LinksDirpath = $"{Environment.CurrentDirectory}{Global.systemSlash}LinkLogs";
        public static string CensorPath = $"{Environment.CurrentDirectory}{Global.systemSlash}Data{Global.systemSlash}Censor.txt";
        public static string HelpMessagefilepath = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}HelpCards.txt";
        public static string RoleCardFilepath = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}RoleCard.json";
        public static string SnipsFilePath = $"{Environment.CurrentDirectory}\\Data\\Snippets.json";
        public static string SupportTicketJsonPath = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}SupportTickets.json";
        public static string SnippetsFilePath = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}Snippets.json";
        public static string BlockedUsersPath = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}Data{Path.DirectorySeparatorChar}BlockedUsers.json";
        public static string Status { get; set; }
        public static string Token { get; set; }
        public static DiscordSocketClient Client { get; set; }
        public static ulong SwissGuildId { get; set; }
        public static List<string> CensoredWords { get; set; }

        internal static JsonItems CurrentJsonData;
        public static ulong DeveloperRoleId { get; set; }
        public static int UserCount { get; set; }
        public static ulong SwissBotDevGuildID { get; set; }
        public static string MessageLogsDir = $"{Environment.CurrentDirectory}{Global.systemSlash}Messagelogs";
        public static string CommandLogsDir = $"{Environment.CurrentDirectory}{Global.systemSlash}Commandlogs";
        public static string ButterFile = $"{Environment.CurrentDirectory}{Global.systemSlash}Data{Global.systemSlash}Landings.butter";
        public static string ButterFilesDirPath = $"{Environment.CurrentDirectory}{Global.systemSlash}Data{Global.systemSlash}ButterFiles";
        public static ulong LogsChannelID { get; set; }
        public static ulong DebugChanID { get; set; }
        public static List<UnnaprovedSubs> SubsList = new List<UnnaprovedSubs>();
        public static ulong TestingCat { get; set; }
        public static Dictionary<string, bool> ConfigSettings { get; set; }
        public static ulong StatsChanID { get; set; }
        public static ulong StatsTotChanID { get; set; }
        public static ulong WelcomeMessageChanID { get; set; }
        public static string WelcomeMessage { get; set; }
        public static ulong SubmissionChanID { get; set; }
        public static string WelcomeMessageURL { get; set; }
        public static ulong ModeratorRoleID { get; set; }
        public static SocketRole ModeratorRole
            => SwissGuild.GetRole(ModeratorRoleID);
        public static ulong MemberRoleID { get; set; }
        public static ulong UnverifiedRoleID { get; set; }
        public static ulong VerificationChanID { get; set; }
        public static List<GiveAway> GiveAwayGuilds { get; set; }
        public static ulong VerificationLogChanID { get; set; }
        public static ulong SubmissionsLogChanID { get; set; }
        public static ulong MilestonechanID { get; set; }
        public static int AutoSlowmodeTrigger { get; set; }
        public static bool AutoSlowmodeToggle { get; set; }
        public static ulong giveawayCreatorChanId { get; set; }
        public static ulong giveawayChanID { get; set; }
        public static ulong BotAiChanID { get; set; }
        public static ulong MutedRoleID { get; set; }
        public static bool VerifyAlts { get; set; }
        public static int AltVerificationHours { get; set; }
        public static ulong TicketCategoryID { get; set; }
        public static ulong SuggestionChannelID { get; set; }
        public static Dictionary<string, string> TicketSnippets { get; set; }
        public static Dictionary<string, List<LogItem>> linkLogs { get; set; }
        public static Dictionary<string, List<LogItem>> messageLogs { get; set; }
        public static Dictionary<string, List<LogItem>> commandLogs { get; set; }
        public static List<ulong> MutedMembers { get; set; }

        public static string[] Workers { get; set; }

        public static string ApiKey { get; set; }
        internal static Dictionary<string, string> jsonItemsList { get; private set; }
        internal static Dictionary<string, string> JsonItemsListDevOps { get; private set; }
        internal static SocketGuild SwissGuild
            => Client.GetGuild(Global.SwissGuildId);

        public static async Task SendAlertMessage(string text = "", bool tts = false, Embed embed = null, RequestOptions options = null)
            => await SwissGuild.GetTextChannel(665647956816429096).SendMessageAsync(text, tts, embed, options);

        public static async Task<SocketGuildUser> GetSwissbotUser(ulong id)
        {
            await SwissGuild.DownloadUsersAsync();

            return SwissGuild.GetUser(id);
        }

        public struct GiveAway
        {
            public int Seconds { get; set; }
            public string GiveAwayItem { get; set; }
            public ulong GiveAwayUser { get; set; }
            public string discordInvite { get; set; }
            public int numWinners { get; set; }
            public RestUserMessage giveawaymsg { get; set; }
            public GiveawayGuildObj giveawayguild { get; set; }
        }
        public class LogItem
        {
            public string username { get; set; }
            public string id { get; set; }
            public string date { get; set; }
            public string channel { get; set; }
            public string message { get; set; }
        }
        public class GiveawayGuildObj
        {
            public ulong guildID { get; set; }
            public bool bansActive { get; set; }
            public List<GiveawayUser> giveawayEntryMembers { get; set; }
            public RestGuild guildOBJ { get; set; }
            public GiveawayGuildObj create(RestGuild guild)
            {
                this.guildID = guild.Id;
                this.bansActive = false;
                giveawayEntryMembers = new List<GiveawayUser>();
                guildOBJ = guild;
                return this;
            }
            public void startBans() { this.bansActive = true; }

            public void removeUser(GiveawayUser bannedUser, GiveawayUser remainingUser)
            {
                if (giveawayEntryMembers.Contains(bannedUser))
                {
                    if (giveawayEntryMembers.Contains(remainingUser))
                    {
                        remainingUser.bans++;
                        remainingUser.bannedUsers.Add(bannedUser);
                        giveawayEntryMembers.Remove(bannedUser);
                    }
                }
            }
        }
        public class GiveawayUser
        {
            public int bans { get; set; }
            public string DiscordName { get; set; }
            public SocketGuildUser user { get; set; }
            public ulong id { get; set; }
            public List<GiveawayUser> bannedUsers { get; set; }

        }
        public static void SaveCensor()
        {
            SwissbotStateHandler.SaveObject("Censor.json", CensoredWords);

        }
        public static IEnumerable<string> SplitDescriptions(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize).Select(i => str.Substring(i * chunkSize, chunkSize));
        }
        public static void ReadConfig()
        {
            if (!Directory.Exists(MessageLogsDir)) { Directory.CreateDirectory(MessageLogsDir); }
            if (!Directory.Exists(CommandLogsDir)) { Directory.CreateDirectory(CommandLogsDir); }
            //if (!File.Exists(aiResponsePath)) { File.Create(aiResponsePath); }
            if (!File.Exists(CensorPath)) { File.Create(CensorPath); }
            if (!File.Exists(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Data" + Path.DirectorySeparatorChar + "AltVerifyCards.txt")) { File.Create(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Data" + Path.DirectorySeparatorChar + "AltVerifyCards.txt").Close(); }
            if (!File.Exists(HelpMessagefilepath)) { File.Create(HelpMessagefilepath).Close(); }
            if (!File.Exists(RoleCardFilepath)) { File.Create(RoleCardFilepath).Close(); }
            if (!File.Exists(SupportTicketJsonPath)) { File.Create(SupportTicketJsonPath).Close(); }
            if (!File.Exists(SnippetsFilePath)) { File.Create(SnippetsFilePath).Close(); }
            if (!File.Exists(BlockedUsersPath)) { File.Create(BlockedUsersPath).Close(); }
            var data = JsonConvert.DeserializeObject<JsonItems>(File.ReadAllText(ConfigPath));
            jsonItemsList = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ConfigPath));
            JsonItemsListDevOps = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ConfigPath));
            ConfigSettings = JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText(ConfigSettingsPath));
            foreach (var item in ConfigSettings)
                if (item.Value == false)
                    jsonItemsList.Remove(item.Key);

            JsonItemsListDevOps.Remove("Token");
            CurrentJsonData = data;
            SwissbotStateHandler.APIKey = data.StateAPIKey;
            Preflix = data.Preflix;
            WelcomeMessageChanID = data.WelcomeMessageChanID;
            WelcomeMessage = data.WelcomeMessage;
            WelcomeMessageURL = data.WelcomeMessageURL;
            Status = data.Status;
            giveawayChanID = data.giveawayChanID;
            giveawayCreatorChanId = data.giveawayCreatorChanId;
            Token = data.Token;
            StatsChanID = data.StatsChanID;
            SwissGuildId = data.SwissGuildID;
            DeveloperRoleId = data.DeveloperRoleId;
            SwissBotDevGuildID = data.SwissTestingGuildID;
            LogsChannelID = data.LogsChannelID;
            DebugChanID = data.DebugChanID;
            SubmissionChanID = data.SubmissionChanID;
            TestingCat = data.TestingCatigoryID;
            ModeratorRoleID = data.ModeratorRoleID;
            MemberRoleID = data.MemberRoleID;
            AutoSlowmodeTrigger = data.AutoSlowmodeTrigger;
            ApiKey = data.ApiKey;
            AutoSlowmodeToggle = data.AutoSlowmodeToggle;
            UnverifiedRoleID = data.UnverifiedRoleID;
            VerificationChanID = data.VerificationChanID;
            VerificationLogChanID = data.VerificationLogChanID;
            SubmissionsLogChanID = data.SubmissionsLogChanID;
            MilestonechanID = data.MilestonechanID;
            BotAiChanID = data.BotAiChanID;
            VerifyAlts = data.VerifyAlts;
            AltVerificationHours = data.AltVerificationHours;
            StatsTotChanID = data.StatsTotChanID;
            MutedRoleID = data.MutedRoleID;
            TicketCategoryID = data.TicketCategoryID;
            SuggestionChannelID = data.SuggestionChannelID;
            TicketSnippets = data.TicketSnippets;
            Workers = data.Workers.Split(' ');

            try
            {
                CensoredWords = SwissbotStateHandler.LoadObject<List<string>>("Censor.json").Result;
            }
            catch
            {
                CensoredWords = new List<string>(); 
            }


        }
        public static void SaveConfigPerms(Dictionary<string, bool> nConfigPerm)
        {
            string json = JsonConvert.SerializeObject(nConfigPerm, Formatting.Indented);
            File.WriteAllText(ConfigSettingsPath, json);
            ConsoleLog("Saved New configPerm items. here is the new JSON \n " + json + "\n Saving...", ConsoleColor.Black, ConsoleColor.DarkYellow);
            ReadConfig();
        }

        
        public static void SaveHelpMessageCards()
        {
            string s = "";
            foreach (var item in HelpMessageHandler.CurrentHelpMessages)
                s += item.Key + "," + item.Value + "\n";
            File.WriteAllText(HelpMessagefilepath, s);
        }
        public static Dictionary<ulong, ulong> LoadHelpMessageCards()
        {
            var t = File.ReadAllText(HelpMessagefilepath);
            Dictionary<ulong, ulong> ulist = new Dictionary<ulong, ulong>();
            if (t == "")
                return ulist;
            foreach (var i in t.Split("\n"))
            {
                if (i != "")
                {
                    var spl = i.Split(",");
                    ulist.Add(ulong.Parse(spl[0]), ulong.Parse(spl[1]));
                }
            }

            return ulist;
        }
        public static List<string> getUnvertCash()
        {
            return File.ReadAllLines(cMSGPath).ToList();
        }
        public static void saveUnvertCash(List<string> newDat)
        {
            string nw = "";
            foreach (var id in newDat)
                nw += $"{id}\n";
            File.WriteAllText(cMSGPath, nw);
        }
        public static void SaveConfig(JsonItems newData)
        {
            string jsonS = JsonConvert.SerializeObject(newData, Formatting.Indented);
            newData.Token = "N#########################";
            string conJson = JsonConvert.SerializeObject(newData, Formatting.Indented);
            File.WriteAllText(ConfigPath, jsonS);
            ConsoleLog("Saved New config items. here is the new JSON \n " + conJson + "\n Saving...", ConsoleColor.DarkYellow);
            ReadConfig();
        }
        public static void SaveConfig(Dictionary<string, bool> newPerms)
        {
            string jsonS = JsonConvert.SerializeObject(newPerms);
            ConsoleLog("Saved New configPerms items. here is the new JSON \n " + jsonS + "\n Saving...", ConsoleColor.Blue);
            File.WriteAllText(ConfigSettingsPath, jsonS);
        }
        public class JsonItems
        {
            public string Token { get; set; }
            public string Status { get; set; }
            public char Preflix { get; set; }
            public Dictionary<string, string> TicketSnippets { get; set; }
            public ulong SwissGuildID { get; set; }
            public ulong SwissTestingGuildID { get; set; }
            public ulong TestingCatigoryID { get; set; }
            public ulong DeveloperRoleId { get; set; }
            public ulong LogsChannelID { get; set; }
            public ulong DebugChanID { get; set; }
            public ulong StatsChanID { get; set; }
            public ulong SubmissionChanID { get; set; }
            public ulong WelcomeMessageChanID { get; set; }
            public string WelcomeMessage { get; set; }
            public string WelcomeMessageURL { get; set; }
            public ulong VerificationLogChanID { get; set; }
            public ulong ModeratorRoleID { get; set; }
            public int AltVerificationHours { get; set; }
            public ulong MemberRoleID { get; set; }
            public ulong UnverifiedRoleID { get; set; }
            public ulong VerificationChanID { get; set; }
            public ulong SubmissionsLogChanID { get; set; }
            public ulong SuggestionChannelID { get; set; }
            public ulong MilestonechanID { get; set; }
            public bool AutoSlowmodeToggle { get; set; }
            public ulong BotAiChanID { get; set; }
            public ulong StatsTotChanID { get; set; }
            public ulong giveawayChanID { get; set; }
            public ulong giveawayCreatorChanId { get; set; }
            public string ApiKey { get; set; }
            public int AutoSlowmodeTrigger { get; set; }
            public bool VerifyAlts { get; set; }
            public ulong MutedRoleID { get; set; }
            public ulong TicketCategoryID { get; set; }
            public string StateAPIKey { get; set; }
            public string Workers { get; set; }

        }
        public static void ConsoleLog(string ConsoleMessage, ConsoleColor FColor = ConsoleColor.Green, ConsoleColor BColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = FColor;
            Console.BackgroundColor = BColor;
            Console.WriteLine("[ - Internal - ] - " + ConsoleMessage);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
        }
        public static async void SendExeption(Exception ex)
        {
            try
            {
                EmbedBuilder b = new EmbedBuilder();
                b.Color = Color.Red;
                b.Description = $"The following info is for an Exeption, `TARGET`\n\n```{ex.TargetSite}```\n`EXEPTION`\n\n```{ex.Message}```\n`SOURCE`\n\n```{ex.Source}```\n";
                b.Footer = new EmbedFooterBuilder();
                b.Footer.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " ZULU";
                b.Title = "Bot Command Error!";
                await Client.GetGuild(Global.SwissGuildId).GetTextChannel(Global.DebugChanID).SendMessageAsync("", false, b.Build());
                await Client.GetGuild(Global.SwissBotDevGuildID).GetTextChannel(622164033902084145).SendMessageAsync("", false, b.Build());
            }
            catch (Exception excp)
            {
                Console.WriteLine(excp);
            }
        }

        internal static RoleAssignerHandler.RoleCard ReadRoleCard()
        {
            //return File.ReadAllText(RoleCardFilepath) != "" ? JsonConvert.DeserializeObject<RoleAssignerHandler.RoleCard>(File.ReadAllText(RoleCardFilepath)) : null;
            try
            {
                return SwissbotStateHandler.LoadObject<RoleAssignerHandler.RoleCard>("RoleCards.json").GetAwaiter().GetResult();
            }
            catch(Exception ex)
            {
                return new RoleAssignerHandler.RoleCard();
            }
        }
        public static void SaveRoleCard()
        {
            string json = JsonConvert.SerializeObject(RoleAssignerHandler.Rolecard, Formatting.Indented);
            File.WriteAllText(RoleCardFilepath, json);
            SwissbotStateHandler.SaveObject("RoleCards.json", RoleAssignerHandler.Rolecard);
        }
        public static List<SupportTicket> ReadSupportTickets()
        {
            //var json =  File.ReadAllText(SupportTicketJsonPath);
            //if (json == "")
            //    return new List<SupportTicket>();
            //else
            //    return JsonConvert.DeserializeObject<List<SupportTicket>>(json);
            return SwissbotStateHandler.LoadObject<List<SupportTicket>>("Tickets.json").GetAwaiter().GetResult();
        }
        public static void SaveSupportTickets()
        {
            var ticks = SupportTicketHandler.CurrentTickets;
            ticks.ForEach(x => x.DMTyping.TypingObject = null);
            //ticks.ForEach(x => x.DmTyping.TypingObject = null);
            //string json = JsonConvert.SerializeObject(ticks, Formatting.Indented);
            //File.WriteAllText(SupportTicketJsonPath, json);
            SwissbotStateHandler.SaveObject("Tickets.json", ticks);
        }
        public static Dictionary<string, string> LoadSnippets()
        {
            //string json = File.ReadAllText(SnippetsFilePath);
            //if (json == "")
            //    return new Dictionary<string, string>();
            //else
            //    return JsonConvert.DeserializeObject<Dictionary<string,string>>(json);
            return SwissbotStateHandler.LoadObject<Dictionary<string, string>>("Snippets.json").GetAwaiter().GetResult();

        }
        public static void SaveSnippets()
        {
            SwissbotStateHandler.SaveObject("Snippets.json", SupportTicketHandler.Snippets);
        }
        public static List<ulong> LoadBlockedUsers()
        {
            //string json = File.ReadAllText(BlockedUsersPath);
            //if (json == "")
            //    return new List<ulong>();
            //else
            //    return JsonConvert.DeserializeObject<>(json);
            return SwissbotStateHandler.LoadObject<List<ulong>>("BlockedUsers.json").GetAwaiter().GetResult();
        }
        public static void SaveBlockedUsers()
        {
            SwissbotStateHandler.SaveObject("BlockedUsers.json", SupportTicketHandler.BlockedUsers);
        }
        public static Dictionary<ulong, DateTime> LoadMuted()
        {
            try
            {
                return SwissbotStateHandler.LoadObject<Dictionary<ulong, DateTime>>("MutedUsers.json").GetAwaiter().GetResult();
            }
            catch(Exception x)
            {
                return new Dictionary<ulong, DateTime>();
            }
             
        }
        public static void SaveMutedUsers()
        {
            SwissbotStateHandler.SaveObject("MutedUsers.json", MutedHandler.CurrentMuted);
        }
        //public static ModlogsJson LoadModlogs()
        //{
        //    try
        //    {
        //        return SwissbotStateHandler.LoadObject<ModlogsJson>("Modlogs.json").GetAwaiter().GetResult();
        //    }
        //    catch (Exception x)
        //    {
        //        return new ModlogsJson();
        //    }

        //}
        //public static void SaveModlogs()
        //{
        //    SwissbotStateHandler.SaveObject("Modlogs.json", ModDatabase.currentLogs);
        //}
        public static List<SuggestionHandler.Suggestion> LoadSuggestions()
        {
            try
            {
                return SwissbotStateHandler.LoadObject<List<SuggestionHandler.Suggestion>>("Suggestions.json").GetAwaiter().GetResult();
            }
            catch
            {
                return new List<SuggestionHandler.Suggestion>();
            }
        }
        public static void SaveSuggestions()
        {
            SwissbotStateHandler.SaveObject("Suggestions.json", SuggestionHandler.CurrentSuggestions);
        }
        public static void SaveAutoSlowmode()
        {
            SwissbotStateHandler.SaveObject("AutoSlowmode.json", AutoModHandler.CurrentSlowmodes);
        }
        public static List<AutoModHandler.SlowmodeChannel> LoadAutoSlowmode()
        {
            try
            {
                return SwissbotStateHandler.LoadObject<List<AutoModHandler.SlowmodeChannel>>("AutoSlowmode.json").GetAwaiter().GetResult();
            }
            catch
            {
                return new List<AutoModHandler.SlowmodeChannel>();
            }
        }
        public struct UnnaprovedSubs
        {
            public IMessage linkMsg { get; set; }
            public IMessage botMSG { get; set; }
            public string url { get; set; }
            public Emoji checkmark { get; set; }
            public Emoji Xmark { get; set; }
            public ulong SubmitterID { get; set; }
        }
    }
}