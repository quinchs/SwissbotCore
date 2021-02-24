using Newtonsoft.Json;
using SwissbotCore.Handlers;
using SwissbotCore.HTTP.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Routes
{
    /// <summary>
    /// This class will handle all the http requests from discord that corilate to discord OAuth2
    /// </summary>
    class DiscordCallback
    {
        [Route(@"^\/auth", "GET", true)]
        public static async Task auth(HttpListenerContext c, MatchCollection m)
        {
            try
            {
                var code = c.Request.QueryString.Get("code");

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://discordapp.com/api/oauth2/token");
                webRequest.Method = "POST";
                string parameters = "client_id=772314985979969596&client_secret=s-y9VIm0gEScVqnL2pTN6O7gXUtxYpIP&grant_type=authorization_code&code=" + code + "&redirect_uri=https://api.swissdev.team/apprentice/v1/auth";
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

                var user = new DiscordUser(JsonConvert.DeserializeObject<TokenResponse>(responseFromServer));

                DiscordAuthKeeper.AddOrReplace(user);

                if (c.Request.QueryString.AllKeys.Contains("state"))
                    c.Response.Redirect(c.Request.QueryString["state"]);

                c.Response.Headers.Add("Set-Cookie", $"csSessionID={user.SessionToken}; Expires={DateTime.UtcNow.AddDays(7).ToString("R")}");
                c.Response.Close();
            }
            catch(Exception x)
            {

            }
        }
    }
}
