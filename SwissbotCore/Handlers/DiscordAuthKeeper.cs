using SwissbotCore.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers
{
    public class DiscordAuthKeeper
    {
        public static List<DiscordUser> Users;

        public static async Task Init()
        {
            try
            {
                Users = await SwissbotStateHandler.LoadObject<List<DiscordUser>>("WebUsers.json");
            }
            catch 
            {
                Users = new List<DiscordUser>();
            }
        }
        public static void Save()
        {
            SwissbotStateHandler.SaveObject("WebUsers.json", Users);
        }

        public static bool IsValidUser(Cookie cookie)
            => Users.Any(x => x.SessionToken == cookie.Value);

        public static DiscordUser GetUser(string session)
            => Users.First(x => x.SessionToken == session);

        public static void LogoutUser(string session)
        {
            Users.Remove(GetUser(session));

            Save();
        }
        
        public static void AddOrReplace(DiscordUser u)
        {
            if (Users.Any(x => x.ID == u.ID))
            {
                Users.Remove(Users.Find(x => x.ID == u.ID));
                AddUser(u);
            }
            else
                AddUser(u);
        }
        public static bool UserIsStaff(DiscordUser u)
        {
            var user = Global.Client.GetGuild(Global.SwissGuildId).GetUser(u.ID);

            return Program.UserHasPerm(user);
        }

        public static void AddUser(DiscordUser u)
        {
            Users.Add(u);

            Save();
        }
    }
}
