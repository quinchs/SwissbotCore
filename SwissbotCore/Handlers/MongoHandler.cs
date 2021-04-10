using Discord.WebSocket;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers
{
    [DiscordHandler]
    public class MongoHandler
    {
        public MongoClient Client;
        public IMongoDatabase SwissbotDatabase
            => Client.GetDatabase("swissbot");
        public IMongoCollection<StaffMemberMessage> MessageCountCollection
            => SwissbotDatabase.GetCollection<StaffMemberMessage>("staff-messages");
        public IMongoCollection<StaffMember> StaffMemberCollection
            => SwissbotDatabase.GetCollection<StaffMember>("staff-member");

        public MongoHandler(DiscordSocketClient c)
        {
            Global.ConsoleLog("Connecting to mongo...");
            Client = new MongoClient(Global.MongoConnectionString);
            Global.ConsoleLog("Connected to mongo!");
        }
    }
}
