using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Websocket.Types
{
    public class VoicekickPacket : IWebsocketMessage
    {
        public string type { get; set; }
        public string session { get; set; }
        public ulong target { get; set; }
        public ulong expires { get; set; }

        public static VoicekickPacket FromRaw(string raw)
            => JsonConvert.DeserializeObject<VoicekickPacket>(raw);
    }
}
