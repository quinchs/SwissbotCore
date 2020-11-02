using IdentityModel.Client;
using SwissbotCore.Handlers;
using SwissbotCore.http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Routes
{
    /// <summary>
    /// This class will handle getting/returning tickets to requesting users
    /// </summary>
    class GetTicket
    {
        [Route(@"\/tickets\/(\d{18}|\d{17})\/(\d{18})", "GET", true)]
        public static async Task getTicket(HttpListenerContext c, MatchCollection m)
        {
            // Check if they have the discord auth
            if(!c.Request.Cookies.Any(x => x.Name == "csSessionID"))
            {
                c.Response.Redirect($"https://discord.com/api/oauth2/authorize?client_id=772314985979969596&redirect_uri=https%3A%2F%2Fapi.swissdev.team%2Fapprentice%2Fv1%2Fauth&response_type=code&scope=identify&state={UrlEncoder.Default.Encode(c.Request.RawUrl)}");
                c.Response.Close();

                return;
            }

            var sesh = c.Request.Cookies["csSessionID"];

            if (!DiscordAuthKeeper.IsValidUser(sesh))
            {
                c.Response.StatusCode = 401;
                c.Response.Close();
                return;
            }

            var user = DiscordAuthKeeper.GetUser(sesh.Value);

            if (!user.HasPermissions())
            {
                c.Response.StatusCode = 403;
                c.Response.Close();

                return;
            }

            if (m[0].Groups.Count != 3)
            {
                c.Response.StatusCode = 400;
                c.Response.Close();

                return;
            }

            // Fetch them the html!
            var uid = m[0].Groups[1].Value;
            var timestamp = m[0].Groups[2].Value;


            var ts = TranscriptHandler.GetTranscript(uid, timestamp);

            if(ts == null)
            {
                c.Response.StatusCode = 404;
                c.Response.Close();

                return;
            }

            c.Response.OutputStream.Write(Encoding.UTF8.GetBytes(ts));
            c.Response.ContentEncoding = Encoding.UTF8;
            c.Response.ContentType = "text/html";
            c.Response.StatusCode = 200;
            c.Response.Close();
        }
    }
}
