using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Websocket.Types
{
    class WorkerLog
    {
        public string message { get; set; }
        public int workerId { get; set; }
        public string type { get; set; }
        public string session { get; set; }
    }
}
