using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SwissbotCore.HTTP.Websocket.Types
{
    public class Handshake : IWebsocketMessage
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
        /// The current page for this socket
        /// </summary>
        public string page { get; set; }

        /// <summary>
        /// The events this page wants to listen to
        /// </summary>
        public string[] events { get; set; }

        /// <summary>
        /// Used for SwissbotWorkers
        /// </summary>
        public int workerId { get; set; } = -1;

        public static Handshake fromContent(string c)
            => JsonConvert.DeserializeObject<Handshake>(c);
    }
}
