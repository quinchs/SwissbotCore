using Newtonsoft.Json;
using SwissbotCore.Handlers;
using SwissbotCore.HTTP.Websocket.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Websocket
{
    public class WebSocketServer
    {
        public static List<WebsocketUser> CurrentClients = new List<WebsocketUser>();

        private static event EventHandler<WebSocketReceiveEventArgs> MessageReceived;

        public static void PushEvent(string eventName, object eventData)
        {
            Task.Run(() => _pushEvent(eventName, eventData).ConfigureAwait(false));    
        }
        private static async Task _pushEvent(string eventName, object eventData)
        {
            byte[] packet = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
            {
                type = eventName,
                data = eventData
            }));

            foreach (var client in CurrentClients.Where(x => x.Events.Contains(eventName)))
            {
                if(client.Socket.State == WebSocketState.Open)
                {
                    await client.Socket.SendAsync(packet, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        private static WebsocketUser GetUser(string cookie)
            => CurrentClients.FirstOrDefault(x => x.User.SessionToken == cookie);

        private static bool CanResumeSession(Handshake hs)
            => CurrentClients.Any(x => x.User.SessionToken == hs.session.Replace("csSessionID=", "") && x.isDisconnected && x.CanResume);
        public static WebsocketUser GetResumedSession(Handshake hs)
            => CurrentClients.FirstOrDefault(x => x.User.SessionToken == hs.session.Replace("csSessionID=", "") && x.isDisconnected && x.CanResume);

        public static void Create()
        {
            MessageReceived += WebSocketServer_MessageReceived;
            Global.ConsoleLog("Created Websocket Server", ConsoleColor.Cyan, ConsoleColor.Black);
        }

        private static async void WebSocketServer_MessageReceived(object sender, WebSocketReceiveEventArgs e)
        {
            Global.ConsoleLog("Received new message from websocket client", ConsoleColor.Cyan, ConsoleColor.Black);

            if(e.result.MessageType == WebSocketMessageType.Close)
            {
                Global.ConsoleLog($"Got a close, Updating session", ConsoleColor.Cyan, ConsoleColor.Black);

                var user = CurrentClients.FirstOrDefault(x => x.Socket.Equals(e.socket));

                if (user == null)
                    return;

                user.SetDisconnected();
                return;
            }

            // Deserialize the data
            string content = Encoding.UTF8.GetString(e.data);

            IWebsocketMessage message;
            try
            {
                message = JsonConvert.DeserializeObject<WebsocketMessage>(content);
            }
            catch(Exception x)
            {
                Global.ConsoleLog("Closing client with bad message type", ConsoleColor.Cyan, ConsoleColor.Black);
                // Close the socket 
                CurrentClients.RemoveAll(x => x.Socket.Equals(e.socket));
                await e.socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Bad Handshake", CancellationToken.None);
                return;
            }
            switch (message.type)
            {
                case "handshake":
                    Global.ConsoleLog("Got new handshake", ConsoleColor.Cyan, ConsoleColor.Black);

                    // Parse the handshake
                    Handshake hs = Handshake.fromContent(content);

                    // Check if the user has a valid session
                    var u = DiscordAuthKeeper.GetUser(hs.session.Replace("csSessionID=", ""));
                    if(u == null)
                    {
                        await e.socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Invalid session", CancellationToken.None);
                    }

                    // Check if this is a resumed session
                    if (CanResumeSession(hs))
                    {
                        // Resume the session
                        var session = GetResumedSession(hs);
                        session.ResumeSession(hs, e.socket);
                        Global.ConsoleLog($"Resumed {session.User.Username}'s session", ConsoleColor.Cyan, ConsoleColor.Black);

                    }
                    else
                    {
                        Global.ConsoleLog("Creating new WebsocketUser", ConsoleColor.Cyan, ConsoleColor.Black);
                        // Add the client
                        CurrentClients.Add(new WebsocketUser(hs, e.socket));
                    }

                    // Send an OK status
                    byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new {
                        status = "OK",
                        type = "handshake_accept"
                    }));

                    await e.socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
                    break;
            }
        }
        
        public static void AcceptSocket(HttpListenerWebSocketContext c)
        {
            Task.Run(() => handleReceiving(c.WebSocket, c).ConfigureAwait(false));
        }

        private class WebSocketReceiveEventArgs
        {
            public WebSocket socket;
            public WebSocketReceiveResult result;
            public byte[] data;
        }

        private static async Task handleReceiving(WebSocket s, HttpListenerWebSocketContext contx)
        {
            while(s.State == WebSocketState.Open)
            {
                try
                {
                    byte[] _buff = new byte[1024];
                    var r = await s.ReceiveAsync(_buff, CancellationToken.None);

                    MessageReceived?.Invoke(null, new WebSocketReceiveEventArgs()
                    {
                        result = r,
                        data = _buff,
                        socket = s
                    });
                }
                catch(Exception x)
                {
                    Global.ConsoleLog($"{x}");
                    await s.CloseAsync(WebSocketCloseStatus.NormalClosure, $"Bad socket", CancellationToken.None);
                    return;
                }
            }
        }
    }
}
