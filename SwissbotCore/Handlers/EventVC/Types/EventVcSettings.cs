using SwissbotCore.HTTP.Websocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers.EventVC.Types
{
    public class EventVcSettings
    {
        public bool AutoMuteNewUsers { get; set; } = false;

        public List<ulong> PermaMutes { get; set; } = new List<ulong>();

        public MuteMode VoiceMuteMode { get; set; }


        public enum MuteMode
        {
            Mute,
            DeafenAlive
        }

        public void Modify(bool AutoMuteNewUsers = false, List<ulong> PermaMutes = null, MuteMode? muteMode = null)
        {
            this.AutoMuteNewUsers = AutoMuteNewUsers;

            if (PermaMutes != null)
                this.PermaMutes = PermaMutes;

            if (muteMode.HasValue)
                this.VoiceMuteMode = muteMode.Value;

            Save();

            BrodcastUpdatedSettings();
        }


        public EventVcSettings Clone()
        {
            return new EventVcSettings()
            {
                AutoMuteNewUsers = this.AutoMuteNewUsers,
                PermaMutes = this.PermaMutes,
                VoiceMuteMode = this.VoiceMuteMode
            };
        }

        public void BrodcastUpdatedSettings()
            => WebSocketServer.PushEvent("event.settings.update", this);

        public void Save()
            => SwissbotStateHandler.SaveObject("EventSettings", this);
    }
}
