using SwissbotCore.Handlers;
using SwissbotCore.http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Routes
{
    class ListTickets
    {
        [Route("/tickets", "GET")]
        public static async Task listTicket(HttpListenerContext c)
        {
            // Check if they have the discord auth
            if (!c.Request.Cookies.Any(x => x.Name == "csSessionID"))
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

            // List our tickets
            var html = TranscriptHandler.CreateTicketListHtml(user);

            c.Response.ContentType = "text/html";
            c.Response.OutputStream.Write(Encoding.UTF8.GetBytes(html));
            c.Response.ContentEncoding = Encoding.UTF8;
            c.Response.StatusCode = 200;
            c.Response.Close();
        }
    }
}
