using System;
using System.Collections.Generic;
using System.Text;

namespace SwissbotCore.HTTP.Websocket.Types
{
    public interface IWebsocketMessage
    {
        string session { get; }
        string type { get; }
    }
}
