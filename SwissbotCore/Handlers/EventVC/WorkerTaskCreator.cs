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
        public VoiceTask Task { get; set; }
        public ulong UserId { get; set; }
        public bool Value { get; set; }
    }

    class WorkerTaskCreator
    {
        /// <summary>
        /// -1 is this bot, 0 and above is worker id's
        /// </summary>
        private static int CurrentWorker = -1;

        private static int _workerCount = Global.Workers.Length;
        private static ConcurrentQueue<QueuedItem> _queue = new ConcurrentQueue<QueuedItem>();

        public static void CreateTask(VoiceTask task, ulong user, bool value)
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

        private static async void HandleDequeue()
        {
            while(_queue.TryDequeue(out var item))
            {
                CurrentWorker++;

                if (CurrentWorker == Global.Workers.Length)
                    CurrentWorker = -1;

                if(CurrentWorker == -1)
                {
                    var user = Global.Client.GetGuild(Global.SwissGuildId).GetUser(item.UserId);
                    if (user == null)
                        continue;

                    switch (item.Task)
                    {
                        case VoiceTask.Deafen:
                            await user.ModifyAsync(x => x.Deaf = item.Value);
                            break;
                        case VoiceTask.Mute:
                            await user.ModifyAsync(x => x.Mute = item.Value);
                            break;
                    }
                }
                else
                {
                    await SwissbotWorkerHandler.AssignTask(item.Task, item.Value, CurrentWorker, item.UserId);
                }
            }
        }       
    }
}
