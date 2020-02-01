using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwissBot
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
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
        private CommandService _commands;
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

            _commands = new CommandService();

            _handler = new CommandHandler(_client);

            Console.WriteLine("[" + DateTime.Now.TimeOfDay + "] - " + "Command Handler ready");

            await Task.Delay(-1);

        }

        private async Task Log(LogMessage msg)
        {
            if (!msg.Message.StartsWith("Received Dispatch"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[" + DateTime.Now.TimeOfDay + "] - " + msg.Message);
            }
        }
    }
}
