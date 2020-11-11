using System;
using System.Collections.Generic;
using System.Text;

namespace SwissbotCore.HTTP.Websocket.Types
{
    public class WebsocketMessage : IWebsocketMessage
    {
        public string session { get; set; }
        public string type { get; set; }
    }
}
