using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers.EventVC.AmongUsClient.Types
{
    public class AmongUsGameState
    {
        public GameState State { get; private set; }
        public string GameCode { get; }
        public List<Player> Players { get; } = new List<Player>();
        public bool IsHost { get; }
        public uint ClientId { get; }
        public uint HostId { get; }
        public int PlayerCount { get; }
        public int Imposters { get; }
        public int Crewmates { get; }
        public int DeadPlayers { get; }

        internal bool ExiledCausesEnd = false;

        internal uint allPlayers;
        public bool InGame { get; set; }
        internal bool CanTalk { get; set; }
        public bool ConnectedToServer { get; set; }
    }

    public class Player
    {
        public byte Id { get; set; }
        public string Name { get; set; }
        public byte ColorId { get; set; }
        public uint HatId { get; set; }
        public uint PetId { get; set; }
        public uint SkinId { get; set; }
        public bool Disconnected { get; set; }
        public bool IsImposter { get; set; }
        public bool IsDead { get; set; }
        public bool InVent { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public override string ToString()
        {
            return $"Id: {this.Id} - {this.Name}";
        }
    }

    public enum GameState
    {
        Lobby,
        Tasks,
        Discussion,
        Menu,
        Unknown,
    }
}
