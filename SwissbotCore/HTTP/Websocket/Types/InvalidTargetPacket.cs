using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Websocket.Types
{
    class InvalidTargetPacket
    {
        public string type { get; set; } = "error.invalid.target";
        public string packet { get; set; }
        public string error { get; set; }

        public InvalidTargetPacket(string packet, string errorMessage)
        {
            this.packet = packet;
            this.error = errorMessage;
        }

        public static byte[] Compile(string packet, string errorMessage)
        {
            var c = JsonConvert.SerializeObject(new InvalidTargetPacket(packet, errorMessage));
            return Encoding.UTF8.GetBytes(c);
        }
    }
}
