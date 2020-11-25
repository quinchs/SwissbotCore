using Discord;
using Discord.WebSocket;
using SwissbotCore.Handlers.EventVC.Types;
using SwissbotCore.HTTP.Websocket;
using SwissbotCore.HTTP.Websocket.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers.EventVC
{
    [DiscordHandler]
    public class VoiceKickHandler
    {
        public static List<VoiceKickUser> CurrentVoiceKicked = new List<VoiceKickUser>();
        private DiscordSocketClient client;
        public VoiceKickHandler(DiscordSocketClient c)
        {
            client = c;

            WebSocketServer.AddCustomEvent("events.voicekick.add", HandleVoicekickAdd);
            WebSocketServer.AddCustomEvent("events.voicekick.revoke", HandleVoicekickRevoke);

            // Setup the events 
            client.UserVoiceStateUpdated += CheckIfVoiceKicked;

            // load from state
            try
            {
                CurrentVoiceKicked = SwissbotStateHandler.LoadObject<List<VoiceKickUser>>("VoiceKicked.json").Result;
            }
            catch { }

            // Setup the timer
            System.Timers.Timer t = new System.Timers.Timer()
            {
                AutoReset = true,
                Interval = 1000,
            };
            t.Elapsed += HandleElapsed;
            t.Start();
        }

        private async void HandleElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach(var item in CurrentVoiceKicked.ToList())
            {
                if(item.expires - DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds < 0)
                {
                    // Handle remove
                    CurrentVoiceKicked.RemoveAll(x => x.id == item.id);
                    SaveVoiceKicked();
                    WebSocketServer.PushEvent("events.voicekick.user.remove", new
                    {
                        id = item.id
                    });

                    var vc = client.GetGuild(Global.SwissGuildId).GetVoiceChannel(627906629047943238);
                    await vc.RemovePermissionOverwriteAsync(item.GetUser());

                    var channel = await item.GetUser().GetOrCreateDMChannelAsync();

                    await channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "You can now join event vc",
                        Description = "You are now able to join event vc, try not to get blocked from joining agan :/",
                        Color= Color.Green
                    }.WithCurrentTimestamp().Build());
                }
            }
        }

        private async Task CheckIfVoiceKicked(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            var user = CurrentVoiceKicked.FirstOrDefault(x => x.id == arg1.Id.ToString());

            if (user == null)
                return;

            if(arg3.VoiceChannel != null && arg3.VoiceChannel.Id == 627906629047943238)
            {
                // Disconnect them
                await arg3.VoiceChannel.Guild.GetUser(arg1.Id).ModifyAsync(x => x.Channel = null);

                // Send them a dm
                var date = (new DateTime(1970, 1, 1)).AddMilliseconds(user.expires);
                var tm = date - DateTime.UtcNow;

                var channel = await arg1.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = "Hey!",
                    Description = "You have been blocked from joining event vc, this could be because you were ruining the game for other people or you were being a general nuisance.\n" +
                    $"Dont worry tho, you will be able to join in {(int)tm.TotalMinutes} minutes, {tm.Seconds} seconds",
                    Color = Color.Red,
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder()
                        {
                            Name = "Expires:",
                            Value = date.ToString("R")
                        }
                    }
                }.WithCurrentTimestamp().Build());
            } 
        }

        public static void SaveVoiceKicked()
            => SwissbotStateHandler.SaveObject("VoiceKicked.json", CurrentVoiceKicked);

        public async Task HandleVoicekickRevoke(RawWebsocketMessage msg)
        {
            VoicekickPacket req;
            try
            {
                req = VoicekickPacket.FromRaw(msg.rawMessage);
            }
            catch(Exception e)
            {
                return;
            }

            var user = CurrentVoiceKicked.FirstOrDefault(x => x.id == req.target.ToString());

            if(user == null)
            {
                await msg.Sender.SendAsync(InvalidTargetPacket.Compile(msg.rawMessage, $"Invalid target, {req.target} does not exist!"), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                return;
            }

            CurrentVoiceKicked.Remove(user);
            SaveVoiceKicked();
            WebSocketServer.PushEvent("events.voicekick.user.remove", new 
            {
                id = req.target.ToString()
            });

            var vc = client.GetGuild(Global.SwissGuildId).GetVoiceChannel(627906629047943238);
            await vc.RemovePermissionOverwriteAsync(user.GetUser());

            var channel = await user.GetUser().GetOrCreateDMChannelAsync();


            await channel.SendMessageAsync("", false, new EmbedBuilder()
            {
                Title = "You can now join event vc",
                Description = "Your voicekick has been revoked by an event manager, you are now able to join back!",
                Color = Color.Green
            }.WithCurrentTimestamp().Build());
        }

        public async Task HandleVoicekickAdd(RawWebsocketMessage msg)
        {
            var req = VoicekickPacket.FromRaw(msg.rawMessage);

            var user = Global.Client.GetGuild(Global.SwissGuildId).GetUser(req.target);

            if (user == null)
            {
                await msg.Sender.SendAsync(InvalidTargetPacket.Compile(msg.rawMessage, $"Invalid target, {req.target} does not exist!"), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                return;
            }

            var VoicekickUser = new VoiceKickUser(user, req.expires);
            CurrentVoiceKicked.Add(VoicekickUser);
            SaveVoiceKicked();

            WebSocketServer.PushEvent("events.voicekick.user.add", VoicekickUser);

            // Add them to the channel override
            var vc = client.GetGuild(Global.SwissGuildId).GetVoiceChannel(627906629047943238);
            await vc.AddPermissionOverwriteAsync(user, new OverwritePermissions(connect: PermValue.Deny, viewChannel: PermValue.Allow));
            await user.ModifyAsync(x => x.Channel = null);
            // Get the times
            var date = (new DateTime(1970, 1, 1)).AddMilliseconds(req.expires);
            var tm = date - DateTime.UtcNow;

            // DM them
            var channel = await user.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync("", false, new EmbedBuilder()
            {
                Title = "Hey!",
                Description = "You have been blocked from joining event vc, this could be because you were ruining the game for other people or you were being a general nuisance.\n" +
                    $"Dont worry tho, you will be able to join in {Math.Floor(tm.TotalMinutes)} minutes, {tm.Seconds} seconds",
                Color = Color.Red,
                Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder()
                        {
                            Name = "Expires:",
                            Value = date.ToString("R")
                        }
                    }
            }.WithCurrentTimestamp().Build());
        }
    }
}
