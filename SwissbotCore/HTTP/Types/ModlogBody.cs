using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissbotCore.HTTP.Types
{
    public class ModlogBody
    {
        public ulong userId { get; set; }
        public SwissbotCore.Modules.ModDatabase.Action type { get; set; }
        public ulong moderatorId { get; set; }
        public string reason { get; set; }
        public string username { get; set; }
    }
}
