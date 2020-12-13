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


        public void Modify(bool AutoMuteNewUsers = false, List<ulong> PermaMutes = null)
        {
            this.AutoMuteNewUsers = AutoMuteNewUsers;

            if (PermaMutes != null)
                this.PermaMutes = PermaMutes;

            Save();

            BrodcastUpdatedSettings();
        }


        public EventVcSettings Clone()
        {
            return new EventVcSettings()
            {
                AutoMuteNewUsers = this.AutoMuteNewUsers,
                PermaMutes = this.PermaMutes
            };
        }

        public void BrodcastUpdatedSettings()
            => WebSocketServer.PushEvent("event.settings.update", this);

        public void Save()
            => SwissbotStateHandler.SaveObject("EventSettings", this);
    }
}
