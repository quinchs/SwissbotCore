using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Websocket.Types
{
    class EventVcUserAction : IWebsocketMessage
    { 
        public enum VcAction
        {
            Mute,
            Deafen,
            Disconnect
        }
        public string session { get; set; }
        public string type { get; set; }
        public VcAction action { get; set; }
        public bool value { get; set; }
        public ulong targetUser { get; set; }

        public static EventVcUserAction FromRaw(string raw)
            => JsonConvert.DeserializeObject<EventVcUserAction>(raw);
    }
}
