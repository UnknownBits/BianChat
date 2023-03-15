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
                Console.WriteLine("新客户连接建立：{0} 个连接数", clients.Count + 1);
            }
            else Disconnect();
            SendData(PacketType.Ping);
            SendData(PacketType.PingBack);
            SendData(PacketType.Message_Notice);
            SendData(PacketType.Ping);
            SendData(PacketType.PingBack);
            SendData(PacketType.Message_Notice); SendData(PacketType.Ping);
            SendData(PacketType.PingBack);
            SendData(PacketType.Message_Notice); SendData(PacketType.Ping);
            SendData(PacketType.PingBack);
            SendData(PacketType.Message_Notice);
        }

        /// <summary>
        /// 数据包类型
        /// </summary>
        public enum PacketType
        {
            Ping = 00,
            PingBack = 01,
            State_Account_Success = 10,
            State_Account_Error = 11,
            State_Server_Closing = 12,
            State_Server_Error = 13,
            Message_Notice = 20,
            Message_Login = 21,
            Message_Register = 22,
            Message_Messages = 23,
        }

        public static void SendData(string data, PacketType type) {
            lock (clients) {
                foreach (var client in clients.Values) {
                    try { client.service.Send(new byte[1] { (byte)type }.Concat(Encoding.UTF8.GetBytes(data)).ToArray()); }
                    catch { client.Disconnect(); }
                }
            }
            Console.WriteLine($"{type} 包：{data}");
        }

        public static void SendData(PacketType type) {
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
                    if (isLogin) SendData($"{username} 已下线", PacketType.Message_Notice);
                    Console.WriteLine($"客户端已断开连接，当前连接数 {clients.Count}");
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
        }

    }
}
