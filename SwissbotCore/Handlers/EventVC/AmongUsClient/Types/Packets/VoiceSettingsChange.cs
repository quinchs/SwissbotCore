using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers.EventVC.AmongUsClient.Types.Packets
{
    public class VoiceSettingsChange : GameStatePacket
    {
        public bool CanTalk { get; set; }
    }
}
