using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Websocket.Types
{
    public class SettingsUpdate
    {
        public string session { get; }
        public string type { get; }
        public object data { get; }
        public WebSocket Sender { get; set; }

        public static SettingsUpdate Create(RawWebsocketMessage msg)
        {
            var s = JsonConvert.DeserializeObject<SettingsUpdate>(msg.rawMessage);
            s.Sender = msg.Sender;
            return s;
        }
    }
}
