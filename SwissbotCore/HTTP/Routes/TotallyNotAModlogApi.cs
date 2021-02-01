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
            var r = await QuinsCryptoLol.IsValidRequest(c);
            if (!r.result)
            {
                c.Response.StatusCode = 401;
                c.Response.Close();
                return;
            }

            string cont = r.payload;
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
                Global.ConsoleLog(x.ToString());
                c.Response.StatusCode = 500;
                c.Response.Close();
            }
        }
    }
}
