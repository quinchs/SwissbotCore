using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.Handlers.EventVC
{
    public enum WorkerTask
    {
        Mute,
        Deafen
    }

    public class QueuedItem
    {
        public WorkerTask Task { get; set; }
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

        public static void CreateTask(WorkerTask task, ulong user, bool value)
        {
            QueuedItem q = new QueuedItem()
            {
                Task = task,
                UserId = user,
                Value = value
            };

            _queue.Enqueue(q);

                

        }

        
    }
}
