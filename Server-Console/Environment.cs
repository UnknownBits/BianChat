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
        public static string Sql_Server()
        {
            if (Mode == ModeType.Online || Mode == ModeType.Maintenance)
            { return "127.0.0.1"; }
            else return "221.224.90.88";
        }
    }
}
