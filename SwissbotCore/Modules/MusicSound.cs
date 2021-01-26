using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwissbotCore.Modules
{
    [DiscordCommandClass]
    public class MusicSound : CommandModuleBase
    {
        [DiscordCommand("butt", commandHelp = "Liege has a nice ass lmao <3", description = "why are you reading this?")]
        public async Task Butt()
        {
            try
            {
                Console.WriteLine("running " + Environment.CurrentDirectory + $"{Path.DirectorySeparatorChar}Sound{Path.DirectorySeparatorChar}phelps.mp3");
                await PlaySoundFile(Environment.CurrentDirectory + $"{Path.DirectorySeparatorChar}Sound{Path.DirectorySeparatorChar}phelps.mp3");
            }
            catch(Exception x)
            {
                Console.WriteLine(x);
            }
        }

        private async Task PlaySoundFile(string path)
        {
            var vc = Context.Guild.GetUser(Context.Message.Author.Id).VoiceChannel;
            if (vc != null)
            {
                try
                {
                    var audioClient = await vc.ConnectAsync();
                    await SendAsync(audioClient, path);
                    await audioClient.StopAsync();
                    await vc.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    Global.SendExeption(ex);
                }

            }
            else
            {
                await Context.Channel.SendMessageAsync("You need to be in a voice channel");
            }
        }
        private Process CreateStream(string path)
        {
            try
            {
                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    RedirectStandardOutput = true,
                });
                Task.Run(async () =>
                {
                    Console.WriteLine($"Got error {await proc.StandardError.ReadToEndAsync()}");
                });
                proc.Exited += (object s, EventArgs a) =>
                {
                    Console.WriteLine("Prop exited");
                };
                return proc;
            }
            catch(Exception x)
            {
                Console.WriteLine(x);
                return null;
            }
        }
        private CancellationTokenSource _disposeToken;

        private async Task SendAsync(IAudioClient client, string path)
        {
            try
            {
                _disposeToken = new CancellationTokenSource();
                bool exit = false;
                // Create FFmpeg using the previous example
                using (var ffmpeg = CreateStream(path))
                {
                    if (ffmpeg == null)
                    {
                        Console.WriteLine("FFMPEG null");
                        return;
                    }
                    using (var output = ffmpeg.StandardOutput.BaseStream)
                    using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
                    {
                        try { await output.CopyToAsync(discord); }
                        finally { await discord.FlushAsync(); }
                        //int bufferSize = 1024;
                        //byte[] buffer = new byte[bufferSize];
                        //while (!_disposeToken.IsCancellationRequested && !exit)
                        //{
                        //    try
                        //    {
                        //        int read = await output.ReadAsync(buffer, _disposeToken.Token);
                        //        if (read == 0)
                        //        {
                        //            //No more data available
                        //            exit = true;
                        //            break;
                        //        }
                        //        await discord.WriteAsync(buffer, 0, read, _disposeToken.Token);
                        //    }
                        //    catch(Exception x)
                        //    {
                        //        exit = true;
                        //        Console.WriteLine(x);
                        //    }

                        //}
                        //await discord.FlushAsync();
                    }
                }
            }
            catch(Exception x)
            {
                Console.WriteLine(x);
            }
            
        }
    }
}
