using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers.EventVC
{
    [DiscordHandler]
    public class AmongUsClientMapper
    {
        public Dictionary<ulong, ulong> ClientMaps { get; set; }

        private DiscordSocketClient client;

        public AmongUsClientMapper(DiscordSocketClient c)
        {
            Task.Run(async () =>
            {
                ClientMaps = await SwissbotStateHandler.LoadObject<Dictionary<ulong, ulong>>("ClientMap.json");
            });

            client = c;
        }

        public void SaveMap()
            => SwissbotStateHandler.SaveObject("ClientMap.json", ClientMaps);

        public ulong? GetClientIdByDiscordId(ulong discordUserId)
        {
            var user = ClientMaps.FirstOrDefault(x => x.Value == discordUserId);
            if (user.Key == default)
                return null;
            return user.Key;
        }

        public void MapUser(ulong clientId, IUser discordUser)
        {
            ClientMaps.Add(clientId, discordUser.Id);
            SaveMap();
        }

        public void MapUser(ulong clientId, ulong discordUserId)
        {
            ClientMaps.Add(clientId, discordUserId);
            SaveMap();
        }
        public void RemoveClientMap(ulong clientId)
        {
            ClientMaps.Remove(clientId);
            SaveMap();
        }
    }
}
