using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Console
{
    internal static class Environment
    {
        public static ModeType Mode = ModeType.Online; 

        public enum ModeType
        {
            Online,// 在线模式
            Local, // 本地模式
            Maintenance // 维护模式
        }
    }
}
