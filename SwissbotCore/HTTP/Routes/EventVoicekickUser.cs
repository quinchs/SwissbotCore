using SwissbotCore.Handlers.EventVC;
using SwissbotCore.http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Routes
{
    class EventVoicekickUser
    {
        [Route(@"^\/events\/voicekick\/users\?id=(\d{17,18})$", "GET", true)]
        public static async Task eventVoicekickUser(HttpListenerContext c, MatchCollection m)
        {
            // Check auth
            var user = c.GetEventManagerAuth();

            if (user == null)
                return;

            // Get the id
            var id = c.Request.QueryString["id"];

            var vcUser = VoiceKickHandler.CurrentVoiceKicked.FirstOrDefault(x => x.id == id);

            if(vcUser == null)
            {
                c.Response.StatusCode = 404;
                c.Response.Close();
                return;
            }

            string html = vcUser.ToHTML();

            c.Response.ContentType = "text/html";
            c.Response.ContentEncoding = Encoding.UTF8;
            c.Response.OutputStream.Write(Encoding.UTF8.GetBytes(html));
            c.Response.StatusCode = 200;
            c.Response.Close();

        }
    }
}
