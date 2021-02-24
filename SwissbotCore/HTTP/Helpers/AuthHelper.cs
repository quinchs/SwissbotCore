﻿using SwissbotCore.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP
{
    public static class AuthHelper
    {
        public static string GetUsername(this HttpListenerContext c)
        {
            if (!c.Request.Cookies.Any(x => x.Name == "csSessionID"))
                return null;

            var sesh = c.Request.Cookies["csSessionID"];

            if (!DiscordAuthKeeper.IsValidUser(sesh))
                return null;

            var user = DiscordAuthKeeper.GetUser(sesh.Value);

            if (user == null)
                return null;
            return user.Username;
        }
        public static DiscordUser GetSwissbotAuth(this HttpListenerContext c)
        {
            // Check if they have the discord auth
            if (!c.Request.Cookies.Any(x => x.Name == "csSessionID"))
            {
                c.Response.Redirect($"https://discord.com/api/oauth2/authorize?client_id=772314985979969596&redirect_uri=https%3A%2F%2Fapi.swissdev.team%2Fapprentice%2Fv1%2Fauth&response_type=code&scope=identify&state={UrlEncoder.Default.Encode(c.Request.RawUrl)}");
                c.Response.Close();

                return null;
            }

            var sesh = c.Request.Cookies["csSessionID"];

            if (!DiscordAuthKeeper.IsValidUser(sesh))
            {
                c.Response.Redirect($"https://discord.com/api/oauth2/authorize?client_id=772314985979969596&redirect_uri=https%3A%2F%2Fapi.swissdev.team%2Fapprentice%2Fv1%2Fauth&response_type=code&scope=identify&state={UrlEncoder.Default.Encode(c.Request.RawUrl)}");
                c.Response.Close();
                return null;
            }

            var user = DiscordAuthKeeper.GetUser(sesh.Value);

            if (!user.HasPermissions())
            {
                c.Response.StatusCode = 403;
                c.Response.Close();

                return null;
            }

            return user;
        }
        public static DiscordUser GetEventManagerAuth(this HttpListenerContext c)
        {
            if (!c.Request.Cookies.Any(x => x.Name == "csSessionID"))
            {
                c.Response.Redirect($"https://discord.com/api/oauth2/authorize?client_id=772314985979969596&redirect_uri=https%3A%2F%2Fapi.swissdev.team%2Fapprentice%2Fv1%2Fauth&response_type=code&scope=identify&state={UrlEncoder.Default.Encode(c.Request.RawUrl)}");
                c.Response.Close();

                return null;
            }

            var sesh = c.Request.Cookies["csSessionID"];

            if (!DiscordAuthKeeper.IsValidUser(sesh))
            {
                c.Response.Redirect($"https://discord.com/api/oauth2/authorize?client_id=772314985979969596&redirect_uri=https%3A%2F%2Fapi.swissdev.team%2Fapprentice%2Fv1%2Fauth&response_type=code&scope=identify&state={UrlEncoder.Default.Encode(c.Request.RawUrl)}");
                c.Response.Close();
                return null;
            }

            var user = DiscordAuthKeeper.GetUser(sesh.Value);

            if (user.HasPermissions())
            {
                return user;
            }
            else
            {
                var discordUser = Global.GetSwissbotUser(user.ID).Result;
                if (discordUser == null)
                {
                    c.Response.StatusCode = 401;
                    c.Response.Close();
                    return null;
                }
                if (discordUser.Roles.Any(x => x.Id == 779943693951565824))
                    return user;
                else
                {
                    c.Response.StatusCode = 401;
                    c.Response.Close();
                    return null; 
                }
            }
        }
    }
}
