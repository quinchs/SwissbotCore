using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
       
        public async Task StartAsync()
        {
            Global.systemSlash = "/";
            
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("[" + DateTime.Now.TimeOfDay + "] - " + "Welcome, " + Environment.UserName);

            Global.ReadConfig();

            //_services = new ServiceCollection().AddSingleton(new AudioService());

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                AlwaysDownloadUsers = true,

                //MessageCacheSize = 99999,
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
                    { 622150031092350976, (SocketCommandContext c) => { return true; } }
                }
            });

            _handler = new CommandHandler(_client, _commands);

            Console.WriteLine("[" + DateTime.Now.TimeOfDay + "] - " + "Command Handler ready");
           
            await Task.Delay(-1);   

            //jabibot

            
        }
        public static bool HasPerms(SocketCommandContext c)
        {
            if (c.Guild.Id == Global.SwissBotDevGuildID) { return true; }
            var user = c.Guild.GetUser(c.Message.Author.Id);
            if (user.Guild.GetRole(Global.ModeratorRoleID).Position <= user.Hierarchy)
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
