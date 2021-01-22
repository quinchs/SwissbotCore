using SwissbotCore.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Websocket
{
    public class EventPermissions
    {
        private static string[] eventManagerEvents = new string[]
        {
            "event.user.added",
            "event.user.updated",
            "event.user.mute",
            "event.user.deafen",
            "event.user.disconnect",
            "events.voicekick.user.remove",
            "events.voicekick.user.add",
            "event.channel.mute",
            "event.channel.unmute"
        };
        private static string[] staffEvents = new string[] 
        { 
            "modlog.added",
            "modlog.removed",
            "tickets.added",
        }.Concat(eventManagerEvents).ToArray();


        public static bool hasPermissionForEvent(string session, params string[] events)
            => hasPermissionForEvent(DiscordAuthKeeper.GetUser(session));
        public static bool hasPermissionForEvent(DiscordUser user, params string[] events)
        {
            switch (user.Permission)
            {
                case HTTP.Types.SessionPermission.EventManager:
                    foreach(var item in events)
                    {
                        if (!eventManagerEvents.Contains(item))
                            return false;
                    }
                    return true;

                case HTTP.Types.SessionPermission.Staff:
                    foreach (var item in events)
                    {
                        if (!staffEvents.Contains(item))
                            return false;
                    }
                    return true;
                case HTTP.Types.SessionPermission.None:
                    return false;
            }
            return false;
        }
    }
}
