using Newtonsoft.Json;
using SwissbotCore.Handlers;
using SwissbotCore.HTTP.Types;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Timers;

namespace SwissbotCore.HTTP
{
    public class DiscordUser
    {

        public string SessionToken;
        public SessionPermission Permission = SessionPermission.Staff;
        public ulong ID
            => User.id;
        public string Username
            => $"{User.username}#{User.discriminator}";
        public User User { get; set; }
        public string CurrentToken;
        public string Refresh;
        public string authType;

        public bool HasPermissions()
            => DiscordAuthKeeper.UserIsStaff(this);

        public DiscordUser() { }
        public DiscordUser(TokenResponse resp)
        {
            this.CurrentToken = resp.access_token;
            this.Refresh = resp.refresh_token;
            this.authType = resp.token_type;

            SessionToken = GenerateToken();

            getId();

            if (HasPermissions())
            {
                Permission = SessionPermission.Staff;
            }
            else
            {
                var discordUser = Global.Client.GetGuild(Global.SwissGuildId).GetUser(this.ID);
                if (discordUser == null)
                {
                    return;
                }
                if (discordUser.Roles.Any(x => x.Id == 779943693951565824))
                    Permission = SessionPermission.EventManager;
            }
            
        }

        private static RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
        public static string GenerateToken()
        {
            byte[] token = new byte[32];

            random.GetBytes(token);

            return BitConverter.ToString(token).Replace("-", "");
        }

        private bool useRefresh()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/oauth2/token");
            webRequest.Method = "POST";
            string parameters = "client_id=772314985979969596&client_secret=s-y9VIm0gEScVqnL2pTN6O7gXUtxYpIP&grant_type=refresh_token&refresh_token=" + Refresh + "&redirect_uri=https://api.swissdev.team/apprentice/v1/auth";
            byte[] byteArray = Encoding.UTF8.GetBytes(parameters);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = byteArray.Length;
            Stream postStream = webRequest.GetRequestStream();

            postStream.Write(byteArray, 0, byteArray.Length);
            postStream.Close();
            WebResponse response = webRequest.GetResponse();
            postStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(postStream);
            string responseFromServer = reader.ReadToEnd();

            TokenResponse t = JsonConvert.DeserializeObject<TokenResponse>(responseFromServer);

            CurrentToken = t.access_token;
            Refresh = t.refresh_token;

            return CurrentToken == t.access_token;
        }

        private bool isSecond = false;
        private ulong getId()
        {
            HttpClient c = new HttpClient();
            c.DefaultRequestHeaders.Add("Authorization", $"{authType} {CurrentToken}");
            var resp = c.GetAsync("https://discord.com/api/users/@me").Result;


            if(resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Resfresh our token

                if (!isSecond)
                {
                    useRefresh();
                    isSecond = true;
                    return getId();
                }
                else
                {
                    isSecond = false;
                    throw new Exception("Bad Token");
                }
            }
            else
            {
                if (resp.IsSuccessStatusCode)
                {
                    string data = resp.Content.ReadAsStringAsync().Result;
                    User = JsonConvert.DeserializeObject<User>(data);
                    return User.id;
                }
            }

            throw new Exception("Bad Token");
        }

        /// <summary>
        /// Converts the current object to a string
        /// </summary>
        /// <returns>The users username</returns>
        public override string ToString()
        {
            return this.Username;
        }
    }
}
