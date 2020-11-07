using Discord;
using Newtonsoft.Json;
using SwissbotCore.Handlers;
using SwissbotCore.http;
using SwissbotCore.HTTP.Types;
using SwissbotCore.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Routes
{
    public class DeleteModlogs
    {
        public static Dictionary<string, List<DateTime>> rate = new Dictionary<string, List<DateTime>>();


        [Route("/modlog", "DELETE")]
        public static async Task deleteModlog(HttpListenerContext c)
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

            // Check the ratelimit
            if (rate.ContainsKey(user.SessionToken))
            {
                var dt = rate[user.SessionToken];

                if(dt.Where(x => (DateTime.UtcNow - x).TotalSeconds < 15).Count() >= 3)
                {
                    c.Response.StatusCode = 429;
                    c.Response.Close();
                    return;
                }

                dt.RemoveAll(x => (DateTime.UtcNow - x).TotalSeconds > 15);

                dt.Add(DateTime.UtcNow);
            }
            else
            {
                rate.Add(user.SessionToken, new List<DateTime>() { DateTime.UtcNow });
            }

            // Check the body
            if (!c.Request.HasEntityBody)
            {
                c.Response.StatusCode = 400;
                c.Response.Close();
                return;
            }

            string cont = "";
            using (var sr = new StreamReader(c.Request.InputStream))
            {
                cont = sr.ReadToEnd();
            }

            ModlogDeleteBody body;

            try
            {
                body = JsonConvert.DeserializeObject<ModlogDeleteBody>(cont);
            }
            catch
            {
                c.Response.StatusCode = 400;
                c.Response.Close();
                return;
            }

            if(body.modlog == null)
            {
                c.Response.StatusCode = 400;
                c.Response.Close();
                return;
            }

            if(body.modlog.Length != 32)
            {
                c.Response.StatusCode = 400;
                c.Response.Close();
                return;
            }

            if (body.uid == null)
            {
                c.Response.StatusCode = 400;
                c.Response.Close();
                return;
            }

            ulong uid = 0;

            if(!ulong.TryParse(body.uid, out uid))
            {
                c.Response.StatusCode = 400;
                c.Response.Close();
                return;
            }

            // Check if there is even a modlog for the requested modlog id

            var modlogUser = ModDatabase.currentLogs.Users.FirstOrDefault(x => x.userId == uid);

            if(modlogUser == null)
            {
                c.Response.StatusCode = 404;
                c.Response.Close();
                return;
            }

            var log = modlogUser.Logs.FirstOrDefault(x => x.InfractionID == body.modlog);

            if(log == null)
            {
                c.Response.StatusCode = 404;
                c.Response.Close();
                return;
            }

            // Delete the log!
            modlogUser.Logs.Remove(log);

            // Save the log
            ModDatabase.SaveModLogs();

            // Post an alert
            await Global.Client.GetGuild(Global.SwissGuildId).GetTextChannel(665647956816429096).SendMessageAsync("", false, new EmbedBuilder()
            {
                Title = "Modlog deleted",
                Description = $"Modlog {log.InfractionID} was deleted by {user.Username} (<@{user.ID}>)",
                Fields = new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    {
                        Name = "Log Details",
                        Value = $"User: {modlogUser.username} ({modlogUser.userId})\n" +
                        $"Action: {log.Action}\n" +
                        $"Reason: {log.Reason}\n" +
                        $"Moderator: <@{log.ModeratorID}>\n" +
                        $"Date: {log.Date}",
                    }
                },
                Color = Color.Orange
            }.WithCurrentTimestamp().Build());
            // Return an OK
            c.Response.StatusCode = 200;
            c.Response.Close();
            return;
        }
    }
}
