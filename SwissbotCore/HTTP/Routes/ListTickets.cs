using SwissbotCore.Handlers;
using SwissbotCore.http;
using SwissbotCore.Properties;
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
    class ListTickets
    {
        [Route(@"\/tickets(\?uid=(\d{17,18})&dt=(\d{18}))?$", "GET", true)]
        public static async Task listTicket(HttpListenerContext c, MatchCollection m)
        {
            var user = c.GetSwissbotAuth();

            if (user == null)
                return;

            if (c.Request.QueryString.Count == 0)
            {
                // List our tickets
                var html = TranscriptHandler.CreateTicketListHtml(user);

                c.Response.ContentType = "text/html";
                c.Response.OutputStream.Write(Encoding.UTF8.GetBytes(html));
                c.Response.ContentEncoding = Encoding.UTF8;
                c.Response.StatusCode = 200;
                c.Response.Close();
            }
            else
            {
                // Get the uid and dt
                var uid = c.Request.QueryString["uid"];
                var dt = c.Request.QueryString["dt"];

                // Get the user
                string username = "Username Unavailable";
                string avatar = "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcTjA0Lpsg840JNGLaPgVWM9QofkvAYdFPLb-g&usqp=CAU";
                var ticketAuthor = Global.Client.GetUser(ulong.Parse(uid));
                if (ticketAuthor != null)
                {
                    username = ticketAuthor.ToString();

                    avatar = ticketAuthor.GetAvatarUrl(Discord.ImageFormat.Jpeg, 256);
                    if (avatar == null)
                        avatar = ticketAuthor.GetDefaultAvatarUrl();
                }

                // Create the item
                var html = Resources.ticketItem.Replace("{item.profile}", avatar)
                        .Replace("{item.username}", username)
                        .Replace("{item.id}", uid)
                        .Replace("{item.date}", DateTime.FromFileTimeUtc(long.Parse(dt)).ToString("R"))
                        .Replace("{item.url}", $"/apprentice/v1/tickets/{uid}/{dt}");

                c.Response.ContentEncoding = Encoding.UTF8;
                c.Response.OutputStream.Write(Encoding.UTF8.GetBytes(html));
                c.Response.StatusCode = 200;
                c.Response.Close();
            }
        }
    }
}
