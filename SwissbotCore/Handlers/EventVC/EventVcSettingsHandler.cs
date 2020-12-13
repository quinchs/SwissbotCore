using Discord.WebSocket;
using Newtonsoft.Json;
using SwissbotCore.Handlers.EventVC.Types;
using SwissbotCore.HTTP.Websocket;
using SwissbotCore.HTTP.Websocket.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers.EventVC
{
    [DiscordHandler]
    public class EventVcSettingsHandler
    {
        private DiscordSocketClient client;

        private EventVcSettings currentSettings;

        public EventVcSettingsHandler(DiscordSocketClient c)
        {
            this.client = c;

            // Add the events to the sockets
            WebSocketServer.AddCustomEvent("event.settings.update", HandleSettingsUpdate);
        }

        private async Task HandleSettingsUpdate(RawWebsocketMessage msg)
        {
            var newSettings = SettingsUpdate.Create(msg);

            if (newSettings.data == null)
                return;

            var SettingsType = typeof(EventVcSettings);
            var SettingsProps = SettingsType.GetProperties();


            var dt = newSettings.data.GetType();

            var tmpSettings = currentSettings.Clone(); 

            foreach(var prop in dt.GetProperties())
            {
                if(SettingsProps.Any(x => x.Name == prop.Name))
                {
                    var setProp = SettingsProps.First(x => x.Name == prop.Name);

                    setProp.SetValue(tmpSettings, prop.GetValue(newSettings.data));
                }
            }


        }
    }
}
