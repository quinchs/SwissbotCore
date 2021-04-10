using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SwissbotCore.Handlers;
using SwissbotCore.HTTP;
using SwissbotCore.HTTP.Websocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SwissbotCore
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    //CustomCommandService s = new CustomCommandService(new CustomCommandService.Settings() { DefaultPrefix = '*'});
                    new Program().StartAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Global.ConsoleLog($"Exception: {ex}\n\n Retrying...", ConsoleColor.Red, ConsoleColor.Black);
                    Thread.Sleep(5000);
                }
            }
        }
        private DiscordSocketClient _client;
        private CustomCommandService _commands;
        private CommandHandler _handler;
        private HandlerService handlerService;
        private HttpServer _server;
        private TranscriptHandler _t;

        public async Task StartAsync()
        {
            WebSocketServer.Create();
            
            Global.systemSlash = "/";
            
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("[" + DateTime.Now.TimeOfDay + "] - " + "Welcome, " + Environment.UserName);

            Global.ReadConfig();

            _t = new TranscriptHandler();
            await DiscordAuthKeeper.Init();
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                AlwaysDownloadUsers = true,
            });

            _client.Log += Log;


            await _client.LoginAsync(TokenType.Bot, Global.Token);

            await _client.StartAsync();

            Global.Client = _client;

            _commands = new CustomCommandService(new Settings()
            {
                DefaultPrefix = Global.Preflix,
                HasPermissionMethod = HasPerms,
                CustomGuildPermissionMethod = new Dictionary<ulong, Func<SocketCommandContext, bool>>()
                {
                    { 592458779006730264, HasPerms},
                    { 622150031092350976, (SocketCommandContext c) => { return true; } },
                    { 726857672942420070, (SocketCommandContext c) => { return false;} },
                    { 706397254000443392, (SocketCommandContext c) => { return true; } }
                },
                AllowCommandExecutionOnInvalidPermissions = true,
                DMCommands = false
            });
            handlerService = new HandlerService(_client);

            _handler = new CommandHandler(_client, _commands, handlerService);

            Global.ConsoleLog("Creating Server...");
            _server = new HttpServer(3000);
            Global.ConsoleLog("Server running!");

            await Task.Delay(-1);   

            //jabibot

            
        }
        public static bool HasPerms(SocketCommandContext c)
        {
            if (c.Guild.Id == Global.SwissBotDevGuildID) { return true; }
            else return UserHasPerm(Global.GetSwissbotUser(c.User.Id).Result);
        }

        public static bool UserHasPerm(ulong id)
        {
            return Task.Run<bool>(async () =>
            {
                var r = await Global.GetSwissbotUser(id);
                if (r == null)
                    return false;
                return UserHasPerm(r);
            }).GetAwaiter().GetResult();
        }

        public static bool UserHasPerm(IGuildUser user)
        {
            if (user.Id == 221204198287605770)
                return false;
            if (user.Id == 259053800755691520)
                return true;
            if (user.Id == 393448221944315915)
                return true;
            else if (user.Guild.GetRole(Global.ModeratorRoleID).Position <= user.Hierarchy)
                return true;
            else if (user.RoleIds.Any(x => x == 706397254000443392))
                return true;
            else
                return false;
        }
        private async Task Log(LogMessage msg)
        {
            if (msg.Message == null)
                return;
            if (!msg.Message.StartsWith("Received Dispatch"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[Svt: {msg.Severity} Src: {msg.Source} Ex: {msg.Exception}] - " + msg.Message);
            }
        }
    }
}
