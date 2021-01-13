using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers.EventVC
{
    
    public class QueuedItem
    {
        public WorkerTask Task { get; set; }
        public ulong UserId { get; set; }
        public bool Value { get; set; }
        public ulong[] Roles { get; set; }
    }



    class WorkerTaskCreator
    {
        /// <summary>
        /// -1 is this bot, 0 and above is worker id's
        /// </summary>
        private static int CurrentWorker = -1;

        private static int _workerCount = Global.Workers.Length;
        private static ConcurrentQueue<QueuedItem> _queue = new ConcurrentQueue<QueuedItem>();

        public static void CreateTask(WorkerTask task, ulong user, bool value)
        {
            QueuedItem q = new QueuedItem()
            {
                Task = task,
                UserId = user,
                Value = value
            };

            _queue.Enqueue(q);

            HandleDequeue();
        }
        public static void CreateTask(WorkerTask task, ulong user, string action, params ulong[] roles)
        {
            QueuedItem q = new QueuedItem()
            {
                Task = task,
                UserId = user,
                Roles = roles,
            };

            _queue.Enqueue(q);

            HandleDequeue();
        }

        private static async void HandleDequeue()
        {
            while(_queue.TryDequeue(out var item))
            {
                CurrentWorker++;

                if (CurrentWorker == Global.Workers.Length)
                    CurrentWorker = -1;

                if(CurrentWorker == -1)
                {
                    var user = await Global.GetSwissbotUser(item.UserId);
                    if (user == null)
                        continue;

                    switch (item.Task)
                    {
                        case WorkerTask.Deafen:
                            await user.ModifyAsync(x => x.Deaf = item.Value);
                            break;
                        case WorkerTask.Mute:
                            await user.ModifyAsync(x => x.Mute = item.Value);
                            break;
                        case WorkerTask.AddRoles:
                            List<IRole> roles = new List<IRole>();
                            foreach (var r in item.Roles)
                                roles.Add(user.Guild.GetRole(r));
                            await user.AddRolesAsync(roles);
                            break;
                        case WorkerTask.RemoveRoles:
                            List<IRole> rs = new List<IRole>();
                            foreach (var r in item.Roles)
                                rs.Add(user.Guild.GetRole(r));
                            await user.RemoveRolesAsync(rs);
                            break;
                    }
                }
                else
                {
                    if(item.Task == WorkerTask.RemoveRoles || item.Task == WorkerTask.AddRoles)
                    {
                        await SwissbotWorkerHandler.AssignTask(item.Task, item.Task == WorkerTask.AddRoles ? "add" : "remove", item.Roles, CurrentWorker, item.UserId);
                    }
                    else
                        await SwissbotWorkerHandler.AssignTask(item.Task, item.Value, CurrentWorker, item.UserId);
                }
            }
        }       
    }
}
