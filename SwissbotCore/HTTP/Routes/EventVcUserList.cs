using SwissbotCore.Handlers;
using SwissbotCore.http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Routes
{
    public class EventVcUserList
    {
        [Route("/events/users", "GET")]
        public static async Task eventVcUserList(HttpListenerContext c)
        {
            var auth = c.GetEventManagerAuth();

            if (auth == null)
                return;

            // Get the users
            var users = EventVCHandler.CurrentVcUsers;

            string html = "";

            foreach (var user in users)
            {
                string pfp = user.GetAvatarUrl(Discord.ImageFormat.Jpeg);
                if (pfp == null)
                    pfp = user.GetDefaultAvatarUrl();

                html += Properties.Resources.VoicekickPopupUser
                    .Replace("{user.profile}", pfp)
                    .Replace("{user.id}", user.Id.ToString())
                    .Replace("{user.displayName}", user.Nickname != null ? user.Nickname : user.Username);
            }

            c.Response.ContentType = "text/html";
            c.Response.OutputStream.Write(Encoding.UTF8.GetBytes(html));
            c.Response.ContentEncoding = Encoding.UTF8;
            c.Response.StatusCode = 200;
            c.Response.Close();
        }
    }
}
