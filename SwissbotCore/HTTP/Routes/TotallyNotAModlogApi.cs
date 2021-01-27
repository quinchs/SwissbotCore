using Newtonsoft.Json;
using SwissbotCore.http;
using SwissbotCore.HTTP.Helpers;
using SwissbotCore.HTTP.Types;
using SwissbotCore.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Routes
{
    public class TotallyNotAModlogApi
    {
        [Route("/modlog", "POST")]
        public static async Task WhyReadThisLol(HttpListenerContext c)
        {
            if (!await QuinsCryptoLol.IsValidRequest(c))
            {
                c.Response.StatusCode = 401;
                c.Response.Close();
            }

            string cont = "";
            using (var sr = new StreamReader(c.Request.InputStream))
            {
                cont = sr.ReadToEnd();
            }

            ModlogBody body;

            try
            {
                body = JsonConvert.DeserializeObject<ModlogBody>(cont);
            }
            catch
            {
                c.Response.StatusCode = 400;
                c.Response.Close();
                return;
            }

            try
            {
                await ModDatabase.AddModlogs(body.userId, body.type, body.moderatorId, body.reason, body.username);
                c.Response.StatusCode = 200;
                c.Response.Close();
            }
            catch(Exception x)
            {
                Global.ConsoleLog(x);
                c.Response.StatusCode = 500;
                c.Response.Close();
            }
        }
    }
}
