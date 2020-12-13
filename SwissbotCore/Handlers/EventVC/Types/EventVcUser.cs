using Discord.WebSocket;
using SwissbotCore.HTTP.Websocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers.EventVC
{
    public class EventVcUser
    {
        public string displayName { get; set; }
        public string id { get; set; }
        public bool isSelfDeafened { get; set; }
        public bool isSelfMuted { get; set; }
        public bool isServerMuted { get; set; }
        public bool isServerDeafened { get; set; }
        public bool isConnected { get; set; }
        private SocketGuildUser _user { get;}

        public SocketGuildUser GetUser()
            => _user;


        public EventVcUser(SocketGuildUser user, SocketVoiceState state)
        {
            this._user = user;
            this.displayName = user.Nickname != null ? user.Nickname : user.Username;
            this.id = user.Id.ToString();
            this.isSelfMuted = state.IsSelfMuted;
            this.isSelfDeafened = state.IsSelfDeafened;
            this.isServerDeafened = state.IsDeafened;
            this.isServerMuted = state.IsMuted;
            this.isConnected = true;

            // Update the current clients with a new user
            WebSocketServer.PushEvent("event.user.added", this);
        }

        public void Remove()
        {
            this.isConnected = false;

            // Update the current clients with the disconnected
            WebSocketServer.PushEvent("event.user.updated", this);
        }
        public void UpdateVoiceState(SocketVoiceState state)
        {
            var user = this._user;
            this.displayName = user.Nickname != null ? user.Nickname : user.Username;
            this.id = user.Id.ToString();
            this.isSelfMuted = state.IsSelfMuted;
            this.isSelfDeafened = state.IsSelfDeafened;
            this.isServerDeafened = state.IsDeafened;
            this.isServerMuted = state.IsMuted;
            this.isConnected = true;

            // Update the current clients with the updated user
            WebSocketServer.PushEvent("event.user.updated", this);
        }
    }
}
