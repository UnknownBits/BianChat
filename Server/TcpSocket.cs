using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class TcpSocket
    {
        public static Dictionary<int, TcpSocket> clients = new Dictionary<int, TcpSocket>();
        public bool connected = false; //连接状态
        public bool isLogin = false;   //登录状态
        public Socket service;

        public string? username;
        public string? password_sha256;

        public int Uid;
        public long t0;

        public TcpSocket(Socket socket)
        {
            service = socket;
        }

        public void MessageService()
        {
            if (service != null)
            {
                connected = true; //连接状态为正常
                clients.Add(0, this);
                byte[] bytes = new byte[8193];
                Console.WriteLine("新客户连接建立：{0} 个连接数", clients.Count);
            }
            else Disconnect();

            #region 登录超时模块
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                if (!isLogin) { Disconnect(); }
            });
            #endregion

            while (connected)
            {
                try
                {
                    byte[] buffer = new byte[8193];
                    int size = service.Receive(buffer);
                    if (size <= 0) { throw new Exception(); }
                    Array.Resize(ref buffer, size);
                    Console.WriteLine(buffer[0]);
                    switch ((PacketType)buffer[0])
                    {
                        case PacketType.Ping:
                            t0 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            service.Send(new byte[1] { (byte)PacketType.Ping });
                            break;
                        case PacketType.PingBack:
                            service.Send(new byte[1] { (int)PacketType.PingBack }.Concat(BitConverter.GetBytes(DateTimeOffset.Now.ToUnixTimeMilliseconds() - t0)).ToArray());
                            Console.WriteLine(DateTimeOffset.Now.ToUnixTimeMilliseconds() - t0);
                            break;
                        case PacketType.Message_Login:
                            string[] loginInfo = Encoding.UTF8.GetString(buffer, 2, buffer.Length - 2).Split('^');
                            if (loginInfo.Length > 2) {
                                service.Send(new byte[1] { (int)PacketType.State_Server_Error });
                                // 登录失败：账号或密码错误
                                Task.Delay(100).Wait();
                                Disconnect();
                            }
                            else if (loginInfo[1].Length != 64) Disconnect();
                            username = loginInfo[0];
                            password_sha256 = loginInfo[1];

                            try {
                                using var sql = new SQLite();
                                if (!(sql.GetUserId(username, out Uid) && sql.Vaild_Password(Uid, password_sha256)) && !clients.ContainsKey(Uid)) {
                                    service.Send(new byte[1] { (int)PacketType.State_Account_Error });
                                    // 登录失败：账号或密码错误或已在线
                                    Task.Delay(100).Wait();
                                    Disconnect();
                                    break;
                                }
                            }
                            catch {
                                service.Send(new byte[1] { (int)PacketType.State_Server_Error });
                                // 登录失败 未知错误
                                Task.Delay(100).Wait();
                                Disconnect();
                                break;
                            }
                            break;
                        case PacketType.Message_Messages:

                            break;
                    }
                }
                catch { Disconnect(); break; }
            }

            Disconnect();
        }

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

        public static void SendPacket(string data, PacketType type) {
            lock (clients) {
                foreach (var client in clients.Values) {
                    try { client.service.Send(new byte[1] { (byte)type }.Concat(Encoding.UTF8.GetBytes(data)).ToArray()); }
                    catch { client.Disconnect(); }
                }
            }
            Console.WriteLine($"{type} 包：{data}");
        }

        public static void SendPacket(PacketType type) {
            lock (clients)
            {
                foreach (var client in clients.Values)
                {
                    try { client.service.Send(new byte[1] { (byte)type }); }
                    catch
                    {
                        client.Disconnect();
                    }
                }
            }
            Console.WriteLine($"发送 {type} 包");
        }

        public void Disconnect() {
            if (connected) {
                try {
                    lock (clients) {
                        connected = false;
                        clients.Remove(Uid);
                        service.Close();
                    }
                    if (isLogin) SendPacket($"{username} 已下线", PacketType.Message_Notice);
                    Console.WriteLine($"客户端已断开连接，当前连接数 {clients.Count}");
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
        }

    }
}
