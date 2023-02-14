namespace Server
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

        public static string DatabaseFileName = @"Database.db";
    }
}
