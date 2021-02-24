using IdentityModel.Client;
using SwissbotCore.Handlers;
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
            var user = c.GetSwissbotAuth();

            if (user == null)
                return;

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
