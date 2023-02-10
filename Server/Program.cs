using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Server
{
    public class Threadtcpserver
    {
        public static List<ClientThread> clients = new List<ClientThread>();
        private static Socket? server;
        static void Main(string[] args)
        {
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, 911);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(iep);
            server.Listen(20);
            Task.Run(() =>
            {
                while (true)
                {
                    Socket client = server.Accept();
                    ClientThread newclient = new ClientThread(client);
                    Thread newthread = new Thread(new ThreadStart(newclient.ClientService));
                    newthread.Start();
                }
            });
            Console.WriteLine("欢迎使用BianChat V1.5.0 寒假特供版");
            Console.WriteLine("等待客户机进行连接......");
            while (true)
            {
                string? command = Console.ReadLine();

                if (!string.IsNullOrEmpty(command))
                {
                    string[] command_args = command.Split(' ');
                    if (command_args.Length > 0)
                    {
                        switch (command_args[0])
                        {
                            case "kill":
                                if (command_args.Length == 2) {
                                    try
                                    {
                                        lock (clients)
                                        {
                                            foreach (var client in clients)
                                            {
                                                if (client.Uid == int.Parse(command_args[1]))
                                                {
                                                    client.Disconnect();
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex) { }
                                }
                                else Console_Helper(CommandType.kill);
                                break;
                            case "notice":
                                if (command_args.Length == 2) ClientThread.SendData($"{command_args[1]}", ClientThread.DataType.Message);
                                else Console_Helper(CommandType.notice);
                                break;
                            case "end":
                                try { lock (clients) { foreach (var client in clients) { client.Disconnect(); } } }
                                catch { }
                                while(clients.Count == 0) { System.Environment.Exit(System.Environment.ExitCode); }
                                break;
                            case "say":
                                if (command_args.Length == 2) ClientThread.SendData($"服务器说：{command_args[1]}", ClientThread.DataType.Message);
                                else Console_Helper(CommandType.say);
                                break;
                            default:
                                Console_Helper();
                                break;
                        }
                    }
                }
            }
        }
        public static void Console_Helper(CommandType commandType = CommandType.all)
        {
            switch(commandType) {
                case CommandType.all:
                    Console.WriteLine("未知的指令 指令集：");
                    Console.WriteLine(" say");
                    Console.WriteLine(" kill");
                    Console.WriteLine(" end");
                    Console.WriteLine(" notice");
                    break;
                case CommandType.say:
                    Console.WriteLine("say [message]");
                    Console.WriteLine("message: 向客户端发送的消息");
                    break;
            }
        }
        public enum CommandType { all,say,kill,end,notice }
        public class ClientThread
        {
            public long t0;
            public Socket service;
            public bool connected = false;
            public string? username;
            public string? password_sha256;
            public int Uid;
            public ClientThread(Socket clientsocket)
            {
                this.service = clientsocket;
            }

            public void ClientService()
            {
                if (service != null)
                {
                    byte[] bytes = new byte[8193];
                    lock (clients)
                    {
                        connected = true;
                        clients.Add(this);
                    }
                    Console.WriteLine("新客户连接建立：{0} 个连接数", clients.Count);

                    Task.Run(async () =>
                    {
                        await Task.Delay(5000);
                        if (username == null) { Disconnect(); }
                    });

                    Task.Run(async () =>
                    {
                        while (true)
                        {
                            await Task.Delay(5000);
                            try
                            {
                                t0 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                service.Send(new byte[1] { 253 });
                            }
                            catch
                            {
                                Disconnect();
                                break;
                            }
                        }
                    });

                    while (true)
                    {
                        try
                        {
                            byte[] buffer = new byte[8193];
                            int size = service.Receive(buffer);
                            if (size <= 0) { throw new Exception(); }
                            Array.Resize(ref buffer, size);
                            switch (buffer[0])
                            {
                                case 0: // 登录包
                                    string[] login_info = Encoding.UTF8.GetString(buffer, 1, buffer.Length - 1).Split('^');
                                    username = login_info[0];
                                    password_sha256 = login_info[1];
                                    try
                                    {
                                        if (Environment.Mode != Environment.ModeType.Local)
                                        {
                                            using var mySql = new MySql();
                                            if (!(mySql.GetUserId(username, out Uid) && mySql.Vaild_Password(Uid, password_sha256)))
                                            {
                                                service.Send(new byte[2] { 255, 1 });
                                                Disconnect();
                                                break;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        service.Send(new byte[2] { 255, 255 });
                                        Disconnect();
                                        break;
                                    }
                                    service.Send(new byte[2] { 255, 0 });
                                    Task.Delay(10).Wait();
                                    Task.Run(async () =>
                                    {
                                        if (Environment.Mode == Environment.ModeType.Local) { await Task.Delay(10); }
                                        SendData($"{username} 已上线", DataType.Notice);
                                        service.Send(new byte[1] { 1 }.Concat(Encoding.UTF8.GetBytes($"{DateTime.Now} PID:{this.Uid}")).ToArray());
                                    });
                                    break;
                                case 1: // 聊天信息
                                    Console.WriteLine($"数据包：{Encoding.UTF8.GetString(buffer, 1, buffer.Length - 1)}");
                                    lock (clients)
                                    {
                                        foreach (var client in clients)
                                        {
                                            try { if (client != this) { client.service.Send(new byte[1] { 1 }.Concat(buffer.Skip(1)).ToArray()); } }
                                            catch { client.Disconnect(); }
                                        }
                                    }
                                    break;
                                case 255: // 返回 Ping 包
                                    service.Send(new byte[1] { 254 }.Concat(BitConverter.GetBytes(DateTimeOffset.Now.ToUnixTimeMilliseconds() - t0)).ToArray());
                                    break;
                            }
                        }
                        catch { Disconnect(); break; }
                    }
                }
            }

            public void Disconnect() { if (connected) { try { lock (clients) { connected = false; clients.Remove(this); service.Close(); } SendData($"{username} 已下线",DataType.Notice); Console.WriteLine($"客户端已断开连接，当前连接数 {clients.Count}"); } catch { } } }
            public enum DataType { Message = 1, Notice = 9 }
            public static void SendData(string data,DataType type) { lock (clients) { foreach (var client in clients) { try { client.service.Send(new byte[1] { (byte)type }.Concat(Encoding.UTF8.GetBytes(data)).ToArray()); } catch { client.Disconnect(); } } } Console.WriteLine($"{type}包：{data}"); }
        }
    }
}