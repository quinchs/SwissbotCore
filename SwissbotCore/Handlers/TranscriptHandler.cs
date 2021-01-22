using Discord;
using Discord.WebSocket;
using SwissbotCore.HTTP;
using SwissbotCore.HTTP.Websocket;
using SwissbotCore.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using static SwissbotCore.Handlers.SupportTicketHandler;

namespace SwissbotCore.Handlers
{
    
    public class TranscriptHandler
    {
        public TranscriptHandler()
        {
            if(!Directory.Exists(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Transcripts"))
                Directory.CreateDirectory(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Transcripts");
        }

        public static List<(string id, string[] dt)> ListTickets()
        {
            var lst = new List<(string id, string[] dt)>();

            foreach(var dir in Directory.GetDirectories(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Transcripts"))
            {
                var files = Directory.GetFiles(dir);
                lst.Add((dir.Split($"{Path.DirectorySeparatorChar}").Last(), files.Select(x => x.Split(Path.DirectorySeparatorChar).Last().Replace(".html", "")).ToArray()));
            }

            return lst;
        }

        public static string CreateTicketListHtml(DiscordUser requestingUser)
        {
            var u = Global.Client.GetUser(requestingUser.ID);
            string pfp = "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcTjA0Lpsg840JNGLaPgVWM9QofkvAYdFPLb-g&usqp=CAU";
            if(u != null)
            {
                pfp = u.GetAvatarUrl(Discord.ImageFormat.Jpeg, 256);
                if (pfp == null)
                    pfp = u.GetDefaultAvatarUrl();
            }

            string ticketLists = "";

            foreach(var item in ListTickets())
            {
                var ticketAuthor = Global.Client.GetUser(ulong.Parse(item.id));
                string username = "Username unavailable";
                string avatar = "https://encrypted-tbn0.gstatic.com/images?q=tbn%3AANd9GcTjA0Lpsg840JNGLaPgVWM9QofkvAYdFPLb-g&usqp=CAU";
                if(ticketAuthor != null)
                {
                    username = ticketAuthor.ToString();

                    avatar = ticketAuthor.GetAvatarUrl(Discord.ImageFormat.Jpeg, 256);
                    if (avatar == null)
                        avatar = ticketAuthor.GetDefaultAvatarUrl();
                }

                foreach (var ticket in item.dt)
                {
                    var dt = DateTime.FromFileTimeUtc(long.Parse(ticket));
                    ticketLists += Resources.ticketItem.Replace("{item.profile}", avatar)
                        .Replace("{item.username}", username)
                        .Replace("{item.id}", item.id)
                        .Replace("{item.date}", dt.ToString("R"))
                        .Replace("{item.url}", $"/apprentice/v1/tickets/{item.id}/{ticket}");
                }
            }

            string html = Resources.listingTickets;

            html = html.Replace("{user.username}", requestingUser.Username)
                   .Replace("{user.profile}", pfp)
                   .Replace("{tickets}", ticketLists)
                   .Replace("{bot.pfp}", Global.Client.CurrentUser.GetAvatarUrl(Discord.ImageFormat.Png));

            return html;
        }

        public static bool TranscriptExists(string uid, string timestamp)
            => File.Exists(Environment.CurrentDirectory
                + Path.DirectorySeparatorChar
                + "Transcripts"
                + Path.DirectorySeparatorChar 
                + $"{uid}" + Path.DirectorySeparatorChar
                + $"{timestamp}.html");

        public static string GetTranscript(string uid, string timestamp)
        {
            if (!TranscriptExists(uid, timestamp))
                return null;

            return File.ReadAllText(Environment.CurrentDirectory
                + Path.DirectorySeparatorChar
                + "Transcripts"
                + Path.DirectorySeparatorChar
                + $"{uid}" + Path.DirectorySeparatorChar
                + $"{timestamp}.html");
        }

        public static void SaveTranscript(TicketTranscript ticket)
        {
            var ticketerDir = Environment.CurrentDirectory
                + Path.DirectorySeparatorChar
                + "Transcripts"
                + Path.DirectorySeparatorChar
                + ticket.TicketAuther;

            if (!Directory.Exists(ticketerDir))
            {
                Directory.CreateDirectory(ticketerDir);
            }

            File.WriteAllText(ticketerDir 
                + Path.DirectorySeparatorChar
                + $"{ticket.creationTime.ToFileTimeUtc()}.html", ticket.compileHtml());
        }
    }

    public class TicketTranscript
    {
        public List<Message> msgs = new List<Message>();
        private SupportTicket t
            => CurrentTickets.Find(x => x.UserID == TicketAuther);

        public ulong TicketAuther;
        public DateTime creationTime;

        private SocketUser Author
            => Global.Client.GetUser(TicketAuther);
        public class Message
        {
            public ulong Author;
            public ulong Id;
            public string Content;
            public void AddAttachments(List<Attachment> l)
            {
                Attachments = new List<DiscordAttachment>();
                l.ForEach(x => Attachments.Add(new DiscordAttachment(x)));
            }
            public List<DiscordAttachment> Attachments = new List<DiscordAttachment>();
            public DateTimeOffset Timestamp;
            public class DiscordAttachment
            {
                public ulong Id { get; set; }
                public string Filename { get; set; }
                public string Url { get; set; }
                public string ProxyUrl { get; set; }
                public int Size { get; set; }
                public int? Height { get; set; }
                public int? Width { get; set; }

                public DiscordAttachment() { }

                public DiscordAttachment(Attachment t)
                {
                    var tpe = this.GetType();

                    foreach(var item in tpe.GetProperties())
                    {
                        item.SetValue(this, t.GetType().GetProperty(item.Name).GetValue(t));
                    }
                }
            }
            public SocketUser GetAuthor()
                => Global.Client.GetUser(Author);
        }

        private string sM;

        private string GetB64Image(string url, string style = "default")
        {
            try
            {
                using (var client = new WebClient())
                {
                    using(Stream stream = client.OpenRead(url))
                    {
                        using (Bitmap bitmap = new Bitmap(stream))
                        {
                            if (bitmap == null)
                                return "";

                            using (System.IO.MemoryStream ms = new MemoryStream()) 
                            {
                                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                                byte[] byteImage = ms.ToArray();
                                var b64 = Convert.ToBase64String(byteImage);

                                string stl = "width: fit-content;height: fit-content; padding-top: 1rem; max-width: 75%;";

                                if (style != "default")
                                    stl = style;

                                return $"<img src=\"data:image/jpg;base64, {b64}\" alt=\"img\" style=\"{stl}\"/>";
                            }
                        }
                    }
                }
            }
            catch
            {
                return "";
            }
        }
        public void AddMessage(SocketMessage m)
        {
            var msg = new Message()
            {
                Author = m.Author.Id,
                Content = m.Content.Replace("<", "&lt;").Replace(">", "&gt;"),
                Id = m.Id,
                Timestamp = m.Timestamp
            };

            msg.AddAttachments(m.Attachments.ToList());

            msgs.Add(msg);

            Global.SaveSupportTickets();

        }
        public void AddMessage(string content, SocketUser user, ulong mId, DateTimeOffset timeOffset)
        {
            msgs.Add(new Message()
            {
                Author = user.Id,
                Id = mId,
                Content = content.Replace("<", "&lt;").Replace(">", "&gt;"),
                Timestamp  = timeOffset
            });

            Global.SaveSupportTickets();

        }
        public TicketTranscript(SupportTicket ticket, string startingMessage)
        {
            if (ticket == null)
                return;
            TicketAuther = ticket.UserID;
            creationTime = DateTime.UtcNow;
            sM = startingMessage;
        }

        /// <summary>
        /// Compiles and saves the transcript
        /// </summary>
        /// <returns>The id and timestamp for this ticket</returns>
        public (string id, string timestamp) CompileAndSave()
        {
            TranscriptHandler.SaveTranscript(this);

            // Push the ticket event
            WebSocketServer.PushEvent("tickets.added", new
            {
                uid = this.Author.Id,
                date = this.creationTime.ToFileTimeUtc()
            });
            return (this.TicketAuther.ToString(), this.creationTime.ToFileTimeUtc().ToString());
        }
        public string compileHtml()
        {
            string html = "";

            // Add our header
            html += "<body style=\"background: #36393f; font-family: Arial, Helvetica, sans-serif\"> " +
                "<div class=\"header\" style=\"text-align: center;color: white;border-bottom: white 1px dashed;margin-bottom: 1rem;\">" +
                $"<a style=\"top: 10px; left: 10px; color: white; text-decoration: none; position: absolute;\" href=\"/apprentice/v1/tickets\">View all tickets</a> <h2>This ticket was created on {creationTime.ToString("g")} by {Author.ToString()}</h2>" +
                "<h3>This ticket is only accessable to staff and the ticket author. All time formats are in UTC</h3>" +
                "</div>";

            // Add the first message

            // Get role color
            string scolor = "white";

            var su = Global.GetSwissbotUser(TicketAuther).Result;
            if (su != null)
            {
                var sr = Global.Client.GetGuild(Global.SwissGuildId).Roles.First(x => x.Position == su.Hierarchy);
                if (sr != null)
                    scolor = sr.Color.ToString();
            }


            // mDiv
            html += $"<div class=\"m-start\" style=\"padding-right: 10rem !important; padding-left: 72px; padding-bottom: 2rem;\">";

            // Profile
            string spfp = su.GetAvatarUrl(Discord.ImageFormat.Jpeg);
            if (spfp == null)
                spfp = su.GetDefaultAvatarUrl();
            html += $"<img src=\"{spfp}\" aria-hidden=\"true\" alt=\"\" style=\"position: absolute;left: 16px;margin-top: calc(4px - 0.125rem);width: 40px;height: 40px;border-radius: 50%;overflow: hidden;cursor: pointer;-webkit-user-select: none;-moz-user-select: none;-ms-user-select: none;user-select: none;-webkit-box-flex: 0;-ms-flex: 0 0 auto;flex: 0 0 auto;pointer-events: none;z-index: 1;\"/>";

           
            
            // Message header
            html += $"<h2 class=\"h-start\" style=\"margin: 0; padding: 0; border: 0; vertical-align: baseline\"><span class=\"headerText\">" +
                $"<span class=\"username\" aria-controls=\"popout_102154\" aria-expanded=\"false\" role=\"button\" tabindex=\"0\" style=\"color: {scolor}; margin: 0; padding: 0; border: 0; font-size: 1.1rem; vertical-align: baseline;\">{su.ToString()}</span></span><span class=\"timestamp\">" +
                $"<span aria-label=\"{creationTime.ToString("g")}\"style=\"color: #72767d; font-size: 1rem; padding-left: 0.5rem;\">" +
                $"{creationTime.ToString("g")}</span></span></h2>";

            // Message content


            html += $"<div class=\"content - mID\" style=\"color: #dcddde; font-size: 1.2rem; display: flex; flex-direction: column;\">{sM}";

            html += $"</div></div>";

            foreach (var msg in msgs)
            {
                // mDiv
                html += $"<div class=\"m-{msg.Id}\" style=\"padding-right: 10rem !important; padding-left: 72px; padding-bottom: 2rem;\">";

                // Profile
                string pfp = msg.GetAuthor().GetAvatarUrl(Discord.ImageFormat.Jpeg);
                if (pfp == null)
                    pfp = msg.GetAuthor().GetDefaultAvatarUrl();
                html += $"<img src=\"{pfp}\" aria-hidden=\"true\" alt=\"\" style=\"position: absolute;left: 16px;margin-top: calc(4px - 0.125rem);width: 40px;height: 40px;border-radius: 50%;overflow: hidden;cursor: pointer;-webkit-user-select: none;-moz-user-select: none;-ms-user-select: none;user-select: none;-webkit-box-flex: 0;-ms-flex: 0 0 auto;flex: 0 0 auto;pointer-events: none;z-index: 1;\"/>";

                // Get role color
                string color = "white";

                var u = Global.GetSwissbotUser(msg.Author).Result;
                if(u != null)
                {
                    var r = Global.Client.GetGuild(Global.SwissGuildId).Roles.First(x => x.Position == u.Hierarchy);
                    if (r != null)
                        color = r.Color.ToString();
                }

                // Message header
                html += $"<h2 class=\"h-{msg.Id}\" style=\"margin: 0; padding: 0; border: 0; vertical-align: baseline\"><span class=\"headerText\">" +
                    $"<span class=\"username\" aria-controls=\"popout_102154\" aria-expanded=\"false\" role=\"button\" tabindex=\"0\" style=\"color: {color}; margin: 0; padding: 0; border: 0; font-size: 1.1rem; vertical-align: baseline;\">{msg.GetAuthor().ToString()}</span></span><span class=\"timestamp\">" +
                    $"<span aria-label=\"{msg.Timestamp.ToString("g")}\"style=\"color: #72767d; font-size: 1rem; padding-left: 0.5rem;\">" +
                    $"{msg.Timestamp.ToString("g")}</span></span></h2>";

                // Message content


                html += $"<div class=\"content - mID\" style=\"color: #dcddde; font-size: 1.2rem; display: flex; flex-direction: column;\">{msg.Content}";

                if (msg.Attachments.Count > 0)
                {
                    foreach(var atch in msg.Attachments)
                    {
                        var img = GetB64Image(atch.ProxyUrl);
                        if (img == "")
                            img = $"<div style=\"padding-top: 1rem;color: red;\">[Attachment: {atch.Filename}]</div>";

                        html += img;
                    }
                }
                html += $"</div></div>";
                
            }

            html += $"</div>";

            return html;
        }
    }
}
