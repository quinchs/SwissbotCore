using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SwissbotCore.Modules
{
    [DiscordCommandClass]
    public class WaddleCommand : CommandModuleBase
    {
        [DiscordCommand("waddle", commandHelp = "(Prefix)waddle", description = "uses cool image magic to put your face on a penguin...you asked for it.")]
        public async Task Waddle()
        {
            WebClient wc = new WebClient();
            byte[] bytes = wc.DownloadData("https://cdn.discordapp.com/attachments/592463507124125706/719941828476010606/wqqJzxAeASEAAAAASUVORK5CYII.png");
            MemoryStream ms = new MemoryStream(bytes);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            string purl = Context.Message.Author.GetAvatarUrl();
            byte[] bytes2 = wc.DownloadData(purl);
            MemoryStream ms2 = new MemoryStream(bytes2);
            System.Drawing.Image img2 = System.Drawing.Image.FromStream(ms2);
            int width = img.Width;
            int height = img.Height;

            using (img)
            {
                using var bitmap = new Bitmap(img.Width, img.Height);
                using (var canvas = Graphics.FromImage(bitmap))
                {
                    canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    canvas.DrawImage(img,
                                     new Rectangle(0, //100
                                                   0, //-30
                                                   width,
                                                   height),
                                     new Rectangle(0,
                                                   0,
                                                   img.Width,
                                                   img.Height),
                                     GraphicsUnit.Pixel);

                    canvas.DrawImage(img2, (img.Width / 2) - (img2.Width / 2), (img.Height / 2) - img2.Height - 60, 120, 110);
                    canvas.Save();
                }

                try
                {
                    bitmap.Save($"{Environment.CurrentDirectory}\\img.jpg",
                                System.Drawing.Imaging.ImageFormat.Jpeg);
                    await Context.Channel.SendFileAsync($"{Environment.CurrentDirectory}\\img.jpg");
                }

                catch (Exception ex) { }
            }
        }
    }
}
