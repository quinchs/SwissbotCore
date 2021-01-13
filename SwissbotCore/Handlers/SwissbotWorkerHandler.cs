using Discord.WebSocket;
using Newtonsoft.Json;
using SwissbotCore.HTTP.Websocket.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers
{
    [DiscordHandler]
    class SwissbotWorkerHandler
    {
        private DiscordSocketClient client;
#if DEBUG
        private static string WorkerPath = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}Worker{Path.DirectorySeparatorChar}SwissbotWorker.exe";
#else
        private static string WorkerPath = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}Worker{Path.DirectorySeparatorChar}SwissbotWorker";
#endif
        private static Dictionary<(int id, string auth), (Process proc, WebSocket client)> Workers = new Dictionary<(int id, string auth), (Process proc, WebSocket client)>();

        private static Random random = new Random();

        public static int WorkerCount 
            => Global.Workers.Length;

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static bool isValidHandshake(Handshake hs)
            => Workers.ContainsKey((hs.workerId, hs.session));

        public static void AcceptHandshake(Handshake hs, WebSocket client)
        {
            var proc = Workers[(hs.workerId, hs.session)].proc;

            Workers[(hs.workerId, hs.session)] = (proc, client);

            proc.Exited += HandleWorkerExit;
        }

        private static void HandleWorkerExit(object sender, EventArgs e)
        {
            // Get the worker
            var worker = Workers.FirstOrDefault(x => x.Value.proc.HasExited);
            if(worker.Key.auth == null)
            {
                Global.ConsoleLog("Got bad handle for worker exit", ConsoleColor.Red);
                return;
            }

            WorkerLog($"Worker {worker.Key.id} Exited, Restarting worker...");

            Workers.Remove(worker.Key);

            string auth = RandomString(32);
            var proc = new Process();
            proc.StartInfo.FileName = WorkerPath;
            proc.StartInfo.Arguments = $"{auth} {Global.Workers[worker.Key.id]} {worker.Key.id}";
            proc.EnableRaisingEvents = true;
#if DEBUG
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.CreateNoWindow = false;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
#else
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
#endif

            proc.Start();
            Workers.Add((worker.Key.id, auth), (proc, null));
        }

        public static async Task AssignTask(WorkerTask task, bool value, int workerId, params ulong[] users)
        {
            var worker = Workers.ElementAt(workerId);

            string content = JsonConvert.SerializeObject(new MuteUsers()
            {
                Type = "MuteUsers",
                Action = task.ToString(),
                Value = value,
                Users = users
            });

            byte[] packet = Encoding.UTF8.GetBytes(content);

            await worker.Value.client.SendAsync(packet, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        public static async Task AssignTask(WorkerTask task, string action, ulong[] roles, int workerId, ulong user)
        {
            var worker = Workers.ElementAt(workerId);

            string content = JsonConvert.SerializeObject(new RolesUpdate()
            {
                Type = task.ToString(),
                Action = action,
                Roles = roles,
                User = user
            });

            byte[] packet = Encoding.UTF8.GetBytes(content);

            await worker.Value.client.SendAsync(packet, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        public static async Task AssignTasks(WorkerTask task, string action, ulong[] roles, ulong user)
        {
            var items = Split(roles, WorkerCount);

            foreach (var worker in Workers)
            {
                var workerUsers = items[worker.Key.id];
                if (workerUsers.Length == 0)
                    continue;

                string content = JsonConvert.SerializeObject(new RolesUpdate()
                {
                    Type = task.ToString(),
                    Action = action,
                    Roles = workerUsers,
                    User = user
                });

                byte[] packet = Encoding.UTF8.GetBytes(content);

                await worker.Value.client.SendAsync(packet, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public static async Task AssignTasks(WorkerTask task, bool value, params ulong[] users)
        {
            var items = Split(users, WorkerCount);

            foreach (var worker in Workers)
            {
                var workerUsers = items[worker.Key.id];
                if (workerUsers.Length == 0)
                    continue;

                string content = JsonConvert.SerializeObject(new MuteUsers()
                {
                    Type = "MuteUsers",
                    Action = task.ToString(),
                    Value = value,
                    Users = workerUsers
                });

                byte[] packet = Encoding.UTF8.GetBytes(content);

                await worker.Value.client.SendAsync(packet, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
        public static ulong[][] Split(ulong[] array, int size)
        {
            List<List<ulong>> data = new List<List<ulong>>();

            int dataIndex = 0;
            for (int i = 0; i != size; i++)
                data.Add(new List<ulong>());
            for(int i = 0; i != array.Length; i++)
            {
                if (dataIndex == size)
                    dataIndex = 0;
                
                data[dataIndex].Add(array[i]);
                dataIndex++;
            }
            return data.Select(x => x.ToArray()).ToArray();

        }
        public static void WorkerLog(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("[ Worker Log ] " + message);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        public SwissbotWorkerHandler(DiscordSocketClient c)
        {
            client = c;

            // Start the workers
            for (int i = 0; i != WorkerCount; i++)
            {
                string auth = RandomString(32);
                var proc = new Process();
                proc.StartInfo.FileName = WorkerPath;
                proc.StartInfo.Arguments = $"{auth} {Global.Workers[i]} {i}";
                proc.EnableRaisingEvents = true;
#if DEBUG
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.CreateNoWindow = false;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
#else
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
#endif
                proc.Start();
                Workers.Add((i, auth), (proc, null));
            }
        }
        private class MuteUsers
        {
            public string Type { get; set; } = "MuteUsers";
            public string Action { get; set; }
            public bool Value { get; set; }
            public ulong[] Users { get; set; }
        }
        public class RolesUpdate
        {
            public string Type { get; set; } = "RolesUpdate";
            public string Action { get; set; }
            public ulong User { get; set; }
            public ulong[] Roles { get; set; }
        }
    }

    public enum WorkerTask
    {
        Mute,
        Deafen,
        RemoveRoles,
        AddRoles
    }
}
