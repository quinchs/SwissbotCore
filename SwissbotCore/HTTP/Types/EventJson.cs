using SwissbotCore.Handlers.EventVC;
using SwissbotCore.Handlers.EventVC.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Types
{
    public class EventJson
    {
        public List<EventVcUser> users { get; set; }
        public List<VoiceKickUser> kicks { get; set; }
    }
}
