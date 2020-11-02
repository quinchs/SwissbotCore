using System;
using System.Collections.Generic;
using System.Text;

namespace SwissbotCore.HTTP.Types
{
    public class User
    {
        public ulong id { get; set; }
        public string username { get; set; }
        public string avatar { get; set; }
        public string discriminator { get; set; }
        public int public_flags { get; set; }
        public int flags { get; set; }
        public string locale { get; set; }
        public bool mfa_enabled { get; set; }
        public int premium_type { get; set; }
    }
}
