using Newtonsoft.Json.Serialization;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class TcpSocket
    {
        public static Dictionary<int, TcpSocket> clients = new Dictionary<int, TcpSocket>();
        public bool connected; //连接状态
        public bool isLogin;   //登录状态
        public Socket socket;

        public string? username;
        public string? password_sha256;
        public string? email;

        public int UID;
        private long UnixTime;

        #region PacketType 数据包类型
        /// <summary>
        /// 数据包类型
        /// </summary>
        public enum PacketType
        {
            Ping,
            PingBack,
            State_Account_Success,
            State_Account_Error,
            State_Server_Closing,
            State_Server_Error,
            Message_Notice,
            Message_Login,
            Message_Register,
            Message_Messages,
        }
        #endregion

        public TcpSocket(Socket socket)
        {
            this.socket = socket;
            connected = socket.Connected;
            isLogin = false;
        }

        public void MessageService()
        {
            connected = true; //连接状态为正常
            Console.WriteLine("新客户连接建立");

            // 登录超时
            Task.Run(async () =>
            {
                await Task.Delay(10000);
                socket.Close();
            });

            while (connected)
                try {
                    byte[] buffer = new byte[8193];
                    int size = socket.Receive(buffer);
                    if (size <= 0) { throw new Exception(); }
                    Array.Resize(ref buffer, size);

                    switch ((PacketType)buffer[0]) {
                        case PacketType.Ping:
                            UnixTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            SendPacket(PacketType.Ping, this);
                            break;
                        case PacketType.PingBack:
                            SendPacket(PacketType.PingBack, this, BitConverter.GetBytes(DateTimeOffset.Now.ToUnixTimeMilliseconds() - UnixTime));
                            break;

                        case PacketType.Message_Login:
                            string[] loginInfo = Encoding.UTF8.GetString(buffer, 1, buffer.Length - 1).Split('^');
                            if (loginInfo.Length != 2 | loginInfo[1].Length != 64)
                            {
                                SendPacket(PacketType.State_Server_Error, this); // 登录失败：不规范的登录包
                                Task.Delay(100).Wait();
                                Disconnect();
                            }
                            username = loginInfo[0];
                            password_sha256 = loginInfo[1];
                            try 
                            {
                                using var sql = new SQLite();
                                if (!(sql.GetUserId(username, out UID) && sql.GetValue(UID,SQLite.ValuesType.Email,out email) && sql.Vaild_Password(UID, password_sha256) && clients.ContainsKey(UID))) 
                                {
                                    SendPacket(PacketType.State_Account_Error, this); // 登录失败：账号或密码错误或已在线
                                    Task.Delay(100).Wait();
                                    Disconnect();
                                    break;
                                }
                            }
                            catch 
                            {
                                SendPacket(PacketType.State_Server_Error,this); // 登录失败 未知错误
                                Task.Delay(100).Wait();
                                Disconnect();
                                break;
                            }

                            Task.Run(async () => 
                            {
                                isLogin = true;
                                SendPacket(PacketType.State_Account_Success,this);
                                lock (clients) { clients.Add(UID, this); }
                                await Task.Delay(100);
                                SendPacket(PacketType.Message_Notice,this, $"{username} 已上线");
                            });
                            break;
                        case PacketType.Message_Register:
                            string[] registerInfo = Encoding.UTF8.GetString(buffer, 1, buffer.Length - 1).Split('^');
                            if (registerInfo.Length != 3 | registerInfo[1].Length != 64)
                            {
                                SendPacket(PacketType.State_Server_Error, this); // 注册失败：不规范的注册包
                                Task.Delay(100).Wait();
                                Disconnect();
                            }

                            username = registerInfo[0];
                            password_sha256 = registerInfo[1];
                            email= registerInfo[2];

                            try
                            {
                                using var sql = new SQLite();
                                if (!(sql.AddValue(username,password_sha256,email,out UID) && clients.ContainsKey(UID)))
                                {
                                    SendPacket(PacketType.State_Account_Error, this); // 注册失败：账号已在线
                                    Task.Delay(100).Wait();
                                    Disconnect();
                                    break;
                                }
                            }
                            catch
                            {
                                SendPacket(PacketType.State_Server_Error, this); // 登录失败 未知错误
                                Task.Delay(100).Wait();
                                Disconnect();
                                break;
                            }

                            Task.Run(async () =>
                            {
                                isLogin = true;
                                SendPacket(PacketType.State_Account_Success, this);
                                lock (clients) { clients.Add(UID, this); }
                                await Task.Delay(100);
                                SendPacket(PacketType.Message_Notice, this, $"{username} 已上线");
                            });

                            break;
                        case PacketType.Message_Messages:
                            if (isLogin) {
                                string content = Encoding.UTF8.GetString(buffer, 1, buffer.Length - 1);
                                BroadcastPacket(PacketType.Message_Messages, $"{username} 说：{content}");
                            }
                            break;
                    }
                }
                catch { Disconnect(); break; }
            Disconnect();
        }

        /// <summary>
        /// 广播 type类型 的数据包
        /// </summary>
        /// <param name="type">数据包类型</param>
        public static void BroadcastPacket(PacketType type)
        {
            lock (clients)
                foreach (var client in clients.Values)
                    try { client.socket.Send(new byte[1] { (byte)type }); }
                    catch { client.Disconnect(); }
            Console.WriteLine($"广播 {type} 包");
        }

        /// <summary>
        /// 广播 type类型 data数据 的数据包
        /// </summary>
        /// <param name="type">数据包类型</param>
        /// <param name="data">自定义数据</param>
        public static void BroadcastPacket(PacketType type,string data) {
            lock (clients) 
                foreach (var client in clients.Values)
                    try { client.socket.Send(new byte[1] { (byte)type }.Concat(Encoding.UTF8.GetBytes(data)).ToArray()); }
                    catch { client.Disconnect(); }
            Console.WriteLine($"广播{type} 包：{data}");
        }

        public static void SendPacket(PacketType type, int UID, byte[] data)
        {
            foreach (var client in clients.Values)
                if (client.UID == UID)
                    try { client.socket.Send(new byte[1] { (byte)type }.Concat(data).ToArray()); }
                    catch { client.Disconnect(); }
            Console.WriteLine($"{type} 包：{data}");
        }

        public static void SendPacket(PacketType type, TcpSocket tcpSocket, byte[] data)
        {
            try { tcpSocket.socket.Send(new byte[1] { (byte)type }.Concat(data).ToArray()); }
            catch { tcpSocket.Disconnect(); }
            Console.WriteLine($"{type} 包：{data}");
        }

        public static void SendPacket(PacketType type, int UID, string data)
        {
            foreach (var client in clients.Values)
                if (client.UID == UID)
                    try { client.socket.Send(new byte[1] { (byte)type }.Concat(Encoding.UTF8.GetBytes(data)).ToArray()); }
                    catch { client.Disconnect(); }
            Console.WriteLine($"{type} 包：{data}");
        }

        public static void SendPacket(PacketType type, TcpSocket tcpSocket, string data)
        {
            try { tcpSocket.socket.Send(new byte[1] { (byte)type }.Concat(Encoding.UTF8.GetBytes(data)).ToArray()); }
            catch { tcpSocket.Disconnect(); }
            Console.WriteLine($"{type} 包：{data}");
        }


        public static void SendPacket(PacketType type, int UID)
        {
            foreach (var client in clients.Values)
                if (client.UID == UID)
                    try { client.socket.Send(new byte[1] { (byte)type }); }
                    catch { client.Disconnect(); }
            Console.WriteLine($"发送 {type} 包");
        }

        public static void SendPacket(PacketType type, TcpSocket tcpSocket)
        {
            try { tcpSocket.socket.Send(new byte[1] { (byte)type }); }
            catch { tcpSocket.Disconnect(); }
            Console.WriteLine($"发送 {type} 包");
        }

        #region Disconnect 断开连接模块
        public void Disconnect()
        {
            if (connected)
                try {
                    if (isLogin)
                    {
                        lock (clients) clients.Remove(UID);
                        isLogin = false;
                        BroadcastPacket(PacketType.Message_Notice, $"{username} 已下线");
                    }
                    connected = false;
                    socket.Close();
                    Console.WriteLine($"连接数发生变化，当前连接数 {clients.Count}");
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }
        #endregion
    }
}
