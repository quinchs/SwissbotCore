using Discord.WebSocket;
using SwissbotCore.Handlers.EventVC;
using SwissbotCore.HTTP.Websocket;
using SwissbotCore.HTTP.Websocket.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers
{
    [DiscordHandler]
    public class EventVCHandler
    {
        private static DiscordSocketClient client;
        public static List<EventVcUser> users = new List<EventVcUser>();
        public List<SocketGuildUser> CurrentVcUsers
            => client.GetGuild(Global.SwissGuildId).GetVoiceChannel(627906629047943238).Users.ToList();
        public EventVCHandler(DiscordSocketClient c)
        {
            client = c;
            
            client.UserVoiceStateUpdated += HandleVoiceStateUpdate;
            client.GuildMemberUpdated += GuildMemberUpdated;


            WebSocketServer.AddCustomEvent("event.user.mute", HandleUserActionEvent);
            WebSocketServer.AddCustomEvent("event.user.deafen", HandleUserActionEvent);
            WebSocketServer.AddCustomEvent("event.user.disconnect", HandleUserActionEvent);

            WebSocketServer.AddCustomEvent("event.channel.mute", HandleEntireChannelMute);
            WebSocketServer.AddCustomEvent("event.channel.unmute", HandleEntireChannelUnmute);


            // Load the users
            var channel = client.GetGuild(Global.SwissGuildId).GetVoiceChannel(627906629047943238);
            foreach(var user in channel.Users)
            {
                if (!user.VoiceState.HasValue)
                    continue;

                users.Add(new EventVcUser(user, user.VoiceState.Value));
            }
        }

        public static string GetCurrentUsersHTML()
        {
            var channel = client.GetGuild(Global.SwissGuildId).GetVoiceChannel(627906629047943238);

            string html = "";

            foreach(var gm in channel.Users)
            {
                var pfp = gm.GetAvatarUrl(Discord.ImageFormat.Jpeg, 256);
                if (pfp == null)
                    pfp = gm.GetDefaultAvatarUrl();

                string displayName = gm.Username;
                if (gm.Nickname != null)
                    displayName = gm.Nickname;

                html += Properties.Resources.eventUser
                    .Replace("{user.id}", gm.Id.ToString())
                    .Replace("{user.profile}", pfp)
                    .Replace("{user.displayName}", displayName);
            }

            return html;
        }

        private async Task HandleEntireChannelMute(RawWebsocketMessage msg)
        {
            // Here we dispatch the little workers of swissbot

            // First lets get everyone in VC
            var channel = client.GetGuild(Global.SwissGuildId).GetVoiceChannel(627906629047943238);
            
            // Check if there is anyone in VC
            if (channel.Users.Count == 0)
                return;

            // Mute them!
            await SwissbotWorkerHandler.AssignTasks(VoiceTask.Mute, true, channel.Users.Where(x => !x.IsMuted).Select(x => x.Id).ToArray());
        }
        private async Task HandleEntireChannelUnmute(RawWebsocketMessage msg)
        {
            // Here we dispatch the little workers of swissbot

            // First lets get everyone in VC
            var channel = client.GetGuild(Global.SwissGuildId).GetVoiceChannel(627906629047943238);

            // Check if there is anyone in VC
            if (channel.Users.Count == 0)
                return;

            // Unmute them!
            await SwissbotWorkerHandler.AssignTasks(VoiceTask.Mute, false, channel.Users.Where(x => x.IsMuted).Select(x => x.Id).ToArray());
        }

        private async Task HandleUserActionEvent(RawWebsocketMessage msg)
        {
            
            // Get the data
            var data = EventVcUserAction.FromRaw(msg.rawMessage);

            // Get the target user
            var user = Global.Client.GetGuild(Global.SwissGuildId).GetUser(data.targetUser);

            if (user == null)
                return;

            if (user.VoiceChannel == null)
                return;

            switch (data.action)
            {
                case EventVcUserAction.VcAction.Mute:
                    await user.ModifyAsync(x => x.Mute = data.value, new Discord.RequestOptions() { });
                    break;
                case EventVcUserAction.VcAction.Deafen:
                    await user.ModifyAsync(x => x.Deaf = data.value);
                    break;
                case EventVcUserAction.VcAction.Disconnect:
                    await user.ModifyAsync(x => x.Channel = null);
                    break;
            }

        }
        private async Task GuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
        {
            var u = users.FirstOrDefault(x => x.id == arg1.Id.ToString());

            if (u == null)
                return;

            if (!arg2.VoiceState.HasValue)
                return;

            string shouldBeDisplayName = arg2.Nickname != null ? arg2.Nickname : arg2.Username;
            if (u.displayName != shouldBeDisplayName)
            {
                u.displayName = shouldBeDisplayName;
                u.UpdateVoiceState(arg2.VoiceState.Value);
            }
        }

        private async Task HandleVoiceStateUpdate(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            if (arg2.VoiceChannel == null && arg3.VoiceChannel != null && arg3.VoiceChannel.Id == 627906629047943238 
                || arg2.VoiceChannel != null && arg3.VoiceChannel != null && arg2.VoiceChannel.Id != arg3.VoiceChannel.Id && arg3.VoiceChannel.Id == 627906629047943238)
            {
                // User joined the vc
                var user = new EventVcUser(client.GetGuild(Global.SwissGuildId).GetUser(arg1.Id), arg3);
                users.Add(user);
                return;
            } else if (arg2.VoiceChannel != null && arg2.VoiceChannel.Id == 627906629047943238 && arg3.VoiceChannel == null 
                || arg2.VoiceChannel != null && arg2.VoiceChannel.Id == 627906629047943238 && arg3.VoiceChannel.Id != 627906629047943238)
            {
                // Remove 
                var user = users.FirstOrDefault(x => x.id == arg1.Id.ToString());

                if (arg2.IsMuted)
                    await client.GetGuild(Global.SwissGuildId).GetUser(arg1.Id).ModifyAsync(x => x.Mute = false);

                if (user == null)
                    return;

                user.Remove();
                users.Remove(user);
            } else if(arg3.VoiceChannel != null && arg3.VoiceChannel.Id == 627906629047943238)
            {
                if (
                    arg3.IsSelfDeafened != arg2.IsSelfDeafened ||
                    arg3.IsSelfMuted != arg2.IsSelfMuted ||
                    arg3.IsMuted != arg2.IsMuted ||
                    arg3.IsDeafened != arg2.IsDeafened
                )
                {
                    var user = users.FirstOrDefault(x => x.id == arg1.Id.ToString());

                    if (user == null)
                        return;

                    user.UpdateVoiceState(arg3);
                }
            }
        }
    }
}
