using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Websocket.Types
{
    public class RawWebsocketMessage
    {
        public string session { get; }
        public string type { get; }
        public string rawMessage { get; }
        public WebSocket Sender { get; set; }

        public RawWebsocketMessage(IWebsocketMessage message, string raw, WebSocket socket)
        {
            this.session = message.session;
            this.type = message.type;
            this.rawMessage = raw;
            this.Sender = socket;
        }

        public static RawWebsocketMessage FromIWebsocketMessage(IWebsocketMessage message, string raw, WebSocket socket)
            => new RawWebsocketMessage(message, raw, socket);
    }
}
