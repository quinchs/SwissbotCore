using Discord;
using Discord.WebSocket;
using SwissbotCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwissbotCore
{
    [DiscordHandler]
    public class UserCache
    {
        public static uint CacheSize = 250;
        public static DiscordSocketClient client;
        public UserCache(DiscordSocketClient c)
        {
            client = c;
        }
        private static DoubleIDEntityCache<IGuildUser> Users = new DoubleIDEntityCache<IGuildUser>();

        public static bool TryGetUser(ulong Id, out IGuildUser user)
        {
            user = null;
            if (UserExistsInCache(Id))
            {
                user = GetUser(Id);
                return true;
            }
            else
                return false;
        }

        public static int Count { get => Users.Count; }
       
        /// <summary>
        /// Creates a User
        /// </summary>
        /// <param name="user">The user to create</param>
        /// <returns>The newly created user</returns>
        public static IGuildUser AddUser(IGuildUser user)
        {
            Users.Add(user);
            Global.ConsoleLog($"Added {(user.Nickname == null ? user.ToString() : user.Nickname)} to the cache: {Count}/{CacheSize}");
            if (Count > CacheSize)
            { 
                var popped = Users.Pop();
                Global.ConsoleLog($"Removed {(popped.Nickname == null ? popped.ToString() : popped.Nickname)} to the cache: {Count}/{CacheSize}");
            }

            return user;
        }

        public static bool UserExistsInCache(ulong UserId)
            => Users.Any(x => x.Id == UserId && x.GuildId == Global.SwissGuildId);

        /// <summary>
        /// Updates a user in the cache only
        /// </summary>
        /// <param name="u">The user to update</param>
        public static void UpdateUser(IGuildUser u)
        {
            if(Users.Any(x => x != null && x.Id == u.Id))
            {
                Users.Replace(u);
            }
        }
        public static IGuildUser GetUser(ulong UserId)
        {
            if (Users.Any(x => x != null && x.Id == UserId))
                return Users[UserId, Global.SwissGuildId];
            return null;
        }
    }
}
