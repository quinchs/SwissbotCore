using Discord.WebSocket;
using Newtonsoft.Json;
using SwissbotCore.Handlers.EventVC.AmongUsClient.Types.Packets;
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
    class AmongUsClientHandler
    {
        private DiscordSocketClient client;
        public AmongUsClientHandler(DiscordSocketClient c)
        {
            this.client = c;

            WebSocketServer.AddCustomEvent("amongus.game.start", HandleGameStart);
            WebSocketServer.AddCustomEvent("amongus.voicestate.update", HandleVoicestateUpdate);
            WebSocketServer.AddCustomEvent("amongus.game.end", HandleGameEnd);
        }



        public async Task HandleVoicestateUpdate(RawWebsocketMessage message)
        {
            // Get the packet
            VoiceSettingsChange packet;
            try
            {
                packet = JsonConvert.DeserializeObject<VoiceSettingsChange>(message.rawMessage);
            }
            catch(Exception x)
            {

            }

            // Update the voice channel
            foreach(var user in EventVCHandler.CurrentVcUsers)
            {
                
            }

        }

        public async Task HandleGameEnd(RawWebsocketMessage message)
        {

        }

        public async Task HandleGameStart(RawWebsocketMessage message)
        {

        }
    }
}
