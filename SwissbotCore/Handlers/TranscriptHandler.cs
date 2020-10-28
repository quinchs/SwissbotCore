using Discord;
using Discord.WebSocket;
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
    public class TicketTranscript
    {
        private List<Message> msgs = new List<Message>();
        private SupportTicket t;
        private SocketUser tAuther;
        private DateTime creationTime;

        public class Message
        {
            public SocketUser Author;
            public ulong Id;
            public string Content;
            public List<Attachment> Attachments = new List<Attachment>();
            public DateTimeOffset Timestamp;
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

                                string stl = "width: fit-content;height: fit-content; padding-top: 1rem;";

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
            msgs.Add(new Message()
            {
                Attachments = m.Attachments.ToList(),
                Author = m.Author,
                Content = m.Content,
                Id = m.Id
            });
        }
        public void AddMessage(string content, SocketUser user, ulong mId, DateTimeOffset timeOffset)
        {
            msgs.Add(new Message()
            {
                Author = user,
                Id = mId,
                Content = content,
                Timestamp  = timeOffset
            });
        }
        public TicketTranscript(SupportTicket ticket, string startingMessage)
        {
            if (ticket == null)
                return;

            t = ticket;
            creationTime = DateTime.UtcNow;
            sM = startingMessage;
            tAuther = Global.Client.GetUser(ticket.UserID);
        }

        public string compileHtml()
        {
            string html = "";

            // Add our header
            html += "<body style=\"background: #36393f; font-family: Arial, Helvetica, sans-serif\"> " +
                "<div class=\"header\" style=\"text-align: center;color: white;border-bottom: white 1px dashed;margin-bottom: 1rem;\">" +
                $"<h2>This ticket was created on {creationTime.ToString("g")} by {tAuther.ToString()}</h2>" +
                "<h3>This ticket is only accessable to staff and the ticket author</h3>" +
                "</div>";

            // Add the first message

            // Get role color
            string scolor = "white";

            var su = Global.Client.GetGuild(Global.SwissGuildId).GetUser(t.UserID);
            if (su != null)
            {
                var sr = Global.Client.GetGuild(Global.SwissGuildId).Roles.ElementAt(su.Hierarchy - 1);
                if (sr != null)
                    scolor = sr.Color.ToString();
            }


            // mDiv
            html += $"<div class=\"m-start\" style=\"padding-right: 10rem !important; padding-left: 72px; padding-bottom: 1rem;\">";

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
                html += $"<div class=\"m-{msg.Id}\" style=\"padding-right: 10rem !important; padding-left: 72px; padding-bottom: 1rem;\">";

                // Profile
                string pfp = msg.Author.GetAvatarUrl(Discord.ImageFormat.Jpeg);
                if (pfp == null)
                    pfp = msg.Author.GetDefaultAvatarUrl();
                html += $"<img src=\"{pfp}\" aria-hidden=\"true\" alt=\"\" style=\"position: absolute;left: 16px;margin-top: calc(4px - 0.125rem);width: 40px;height: 40px;border-radius: 50%;overflow: hidden;cursor: pointer;-webkit-user-select: none;-moz-user-select: none;-ms-user-select: none;user-select: none;-webkit-box-flex: 0;-ms-flex: 0 0 auto;flex: 0 0 auto;pointer-events: none;z-index: 1;\"/>";

                // Get role color
                string color = "white";

                var u = Global.Client.GetGuild(Global.SwissGuildId).GetUser(msg.Author.Id);
                if(u != null)
                {
                    var r = Global.Client.GetGuild(Global.SwissGuildId).Roles.ElementAt(u.Hierarchy -1);
                    if(r != null)
                        color = r.Color.ToString();
                }

                // Message header
                html += $"<h2 class=\"h-{msg.Id}\" style=\"margin: 0; padding: 0; border: 0; vertical-align: baseline\"><span class=\"headerText\">" +
                    $"<span class=\"username\" aria-controls=\"popout_102154\" aria-expanded=\"false\" role=\"button\" tabindex=\"0\" style=\"color: {color}; margin: 0; padding: 0; border: 0; font-size: 1.1rem; vertical-align: baseline;\">{msg.Author.ToString()}</span></span><span class=\"timestamp\">" +
                    $"<span aria-label=\"{msg.Timestamp.UtcDateTime.ToString("g")}\"style=\"color: #72767d; font-size: 1rem; padding-left: 0.5rem;\">" +
                    $"{msg.Timestamp.UtcDateTime.ToString("g")}</span></span></h2>";

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
