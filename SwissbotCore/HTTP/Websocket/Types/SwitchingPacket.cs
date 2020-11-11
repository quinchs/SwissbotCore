using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SwissbotCore.HTTP.Websocket.Types
{
    public class SwitchingPacket : IWebsocketMessage
    {
        /// <summary>
        /// The current session
        /// </summary>
        public string session { get; set; }

        /// <summary>
        /// The type of the websocket message
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// The route that the client is switching to
        /// </summary>
        public string route { get; set; }

        public static SwitchingPacket fromContent(string c)
            => JsonConvert.DeserializeObject<SwitchingPacket>(c);
    }
}
