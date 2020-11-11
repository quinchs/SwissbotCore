using SwissbotCore.Handlers;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace SwissbotCore.HTTP.Websocket.Types
{
    public class WebsocketUser
    {
        public WebSocket Socket;
        public DiscordUser User;
        public string[] Events;
        public string CurrentPage;

        public bool isDisconnected = false;
        public DateTime DisconnectTime;

        public bool CanResume
            => (DateTime.UtcNow - DisconnectTime).TotalMinutes < 15;


        public void SetDisconnected()
        {
            isDisconnected = true;
            DisconnectTime = DateTime.UtcNow;
        }

        public void ResumeSession(Handshake shake, WebSocket _socket)
        {
            isDisconnected = false;

            this.Socket = _socket;

            this.CurrentPage = shake.page;

            this.Events = shake.events;
        }

        public WebsocketUser(Handshake shake, WebSocket _socket)
        {
            this.Socket = _socket;

            // Get their session
            string sessionId = shake.session.Replace("csSessionID=", "");

            this.User = DiscordAuthKeeper.GetUser(sessionId);

            this.CurrentPage = shake.page;

            this.Events = shake.events;
        }
    }
}
