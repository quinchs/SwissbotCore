using Newtonsoft.Json;
using SwissbotCore.Handlers;
using SwissbotCore.Handlers.EventVC;
using SwissbotCore.http;
using SwissbotCore.HTTP.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Routes
{
    public class EventVC
    {
        [Route("/event", "GET")]
        public static async Task eventManagerPage(HttpListenerContext c)
        {
            var user = c.GetEventManagerAuth();

            if (user == null)
                return;

            // Serve the page

            string html = Properties.Resources.EventManager;

            c.Response.ContentType = "text/html";
            c.Response.ContentEncoding = Encoding.UTF8;
            c.Response.OutputStream.Write(Encoding.UTF8.GetBytes(html));
            c.Response.StatusCode = 200;
            c.Response.Close();
        }
        [Route(@"/event.json", "GET")]
        public static async Task json(HttpListenerContext c)
        {
            // This will be used to populate the javascript arrays on the page load
            var user = c.GetEventManagerAuth();

            if (user == null)
                return;

            // Generate the json

            EventJson json = new EventJson();
            json.users = EventVCHandler.users;
            json.kicks = VoiceKickHandler.CurrentVoiceKicked;

            string jsonString = JsonConvert.SerializeObject(json);

            c.Response.ContentEncoding = Encoding.UTF8;
            c.Response.ContentType = "text/json";
            c.Response.OutputStream.Write(Encoding.UTF8.GetBytes(jsonString));
            c.Response.StatusCode = 200;
            c.Response.Close();
        }

        [Route(@"^\/event\/user\?id=(\d{17,18})$", "GET", true)]
        public static async Task eventVc(HttpListenerContext c, MatchCollection m)
        {
            var user = c.GetEventManagerAuth();

            if (user == null)
                return;

            // Get the requested user id 
            var id = ulong.Parse(m[0].Groups[1].Value);

            // Get the user
            var gm = await Global.GetSwissbotUser(id);

            if(gm == null)
            {
                c.Response.StatusCode = 404;
                c.Response.Close();
                return;
            }


            var pfp = gm.GetAvatarUrl(Discord.ImageFormat.Jpeg, 256);
            if (pfp == null)
                pfp = gm.GetDefaultAvatarUrl();

            string displayName = gm.Username;
            if (gm.Nickname != null)
                displayName = gm.Nickname;

            var html = Properties.Resources.eventUser
                .Replace("{user.id}", gm.Id.ToString())
                .Replace("{user.profile}", pfp)
                .Replace("{user.displayName}", displayName);

            c.Response.ContentType = "text/html";
            c.Response.ContentEncoding = Encoding.UTF8;
            c.Response.OutputStream.Write(Encoding.UTF8.GetBytes(html));
            c.Response.StatusCode = 200;
        }
    }
}
