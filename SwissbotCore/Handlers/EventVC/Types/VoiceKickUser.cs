using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers.EventVC.Types
{
    public class VoiceKickUser
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public ulong expires { get; set; }
        public VoiceKickUser() { }
        public VoiceKickUser(SocketGuildUser user, ulong duration)
        {
            this.expires = duration;
            this.id = user.Id.ToString();
            this.displayName = user.Nickname != null ? user.Nickname : user.Username;
        }

        public void Update(SocketGuildUser user)
        {
            this.displayName = user.Nickname != null ? user.Nickname : user.Username;
        }


        public string ToHTML()
        {
            var user = GetUser();
            string pfp = user.GetAvatarUrl(Discord.ImageFormat.Jpeg);
            if (pfp == null)
                pfp = user.GetDefaultAvatarUrl();

            return Properties.Resources.VoiceKickMenuUser
                .Replace("{user.profile}", pfp)
                .Replace("{user.displayName}", user.Nickname != null ? user.Nickname : user.Username)
                .Replace("{user.id}", user.Id.ToString());
        }
        public SocketGuildUser GetUser()
            => Global.GetSwissbotUser(ulong.Parse(this.id)).GetAwaiter().GetResult();
    }
}
