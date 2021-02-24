using SwissbotCore.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Routes
{
    /// <summary>
    /// This class will manage the logout function for users
    /// </summary>
    public class Logout
    {
        [Route("/logout", "GET")]
        public static async Task logout(HttpListenerContext c)
        {
            // Check if we're even logged in
            if (!c.Request.Cookies.Any(x => x.Name == "csSessionID"))
            {
                // Return the "not logged in" html
                c.Response.OutputStream.Write(Encoding.UTF8.GetBytes(Properties.Resources.notLoggedIn));
                c.Response.ContentType = "text/html";
                c.Response.ContentEncoding = Encoding.UTF8;
                c.Response.StatusCode = 200;
                c.Response.Close();
                return;
            }

            var sesh = c.Request.Cookies["csSessionID"];

            if (!DiscordAuthKeeper.IsValidUser(sesh))
            {
                c.Response.Headers.Add("Set-Cookie", "csSessionID=deleted; expires=Thu, 01 Jan 1970 00:00:00 GMT");
                c.Response.StatusCode = 401;
                c.Response.Close();
                return;
            }

            // Log them out!
            DiscordAuthKeeper.LogoutUser(sesh.Value);

            c.Response.Headers.Add("Set-Cookie", "csSessionID=deleted; expires=Thu, 01 Jan 1970 00:00:00 GMT");
            c.Response.OutputStream.Write(Encoding.UTF8.GetBytes(Properties.Resources.loggedOut));
            c.Response.ContentType = "text/html";
            c.Response.ContentEncoding = Encoding.UTF8;
            c.Response.StatusCode = 200;
            c.Response.Close();
        }
    }
}
