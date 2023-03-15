using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Server_Ava
{
    public class Threadtcpserver
    {
        public static Dictionary<int, ClientThread> clients = new Dictionary<int, ClientThread>();
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
            SQLite.CreateDatabase();
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
                                            foreach (var client in clients.Values)
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
                                if (command_args.Length == 2) ClientThread.SendData($"{command_args[1]}", ClientThread.DataType.Notice);
                                else Console_Helper(CommandType.notice);
                                break;
                            case "end":
                                try { lock (clients) { foreach (var client in clients.Values) { client.Disconnect(); } } }
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
            public Socket service;
            public bool isLogin = false;   //登录状态
            public bool connected = false; //连接状态
            public string? username;
            public string? password_sha256;
            public int Uid;
            public long t0;

            public ClientThread(Socket clientsocket)
            {
                service = clientsocket;
            }

            public void ClientService()
            {
                if (service == null)
                {
                    connected = true; //连接状态为正常
                    byte[] bytes = new byte[8193];
                    Console.WriteLine("新客户连接建立：{0} 个连接数", clients.Count + 1);
                }
                else Disconnect();

                #region 登录超时模块
                Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    if (!isLogin) { Disconnect(); }
                });
                #endregion

                Task.Run(async () =>
                {
                    await Task.Delay(200);
                    while (true)
                    {
                        try
                        {
                            t0 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            service.Send(new byte[1] { (int)DataType.Ping });
                        }
                        catch
                        {
                            Disconnect();
                            break;
                        }
                        await Task.Delay(5000);
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
                        switch ((DataType)buffer[0])
                        {
                            case DataType.Login: // 登录包
                                if (buffer[1] == 0) // 登录
                                {
                                    string[] login_info = Encoding.UTF8.GetString(buffer, 2, buffer.Length - 2).Split('^');
                                    if (login_info.Length > 2)
                                    {
                                        service.Send(new byte[1] { (int)DataType.Error_Account });
                                        Console.WriteLine("登录失败：账号或密码错误");
                                        Task.Delay(100).Wait();
                                        Disconnect();
                                    }
                                    username = login_info[0];
                                    password_sha256 = login_info[1];
                                    if (password_sha256.Length != 64) Disconnect();
                                    if (username.Contains(' ') || password_sha256.Contains(' '))
                                    {
                                        service.Send(new byte[1] { (int)DataType.Error_Account });
                                        Console.WriteLine("登录失败：账号或密码包含空格");
                                        Task.Delay(100).Wait();
                                        Disconnect();
                                        break;
                                    }
                                    if (Environment.Mode != Environment.ModeType.Local)
                                    {
                                        try
                                        {
                                            using var sql = new SQLite();
                                            if (!(sql.GetUserId(username, out Uid) && sql.Vaild_Password(Uid, password_sha256)) && !clients.ContainsKey(Uid))
                                            {
                                                service.Send(new byte[1] { (int)DataType.Error_Account });
                                                Console.WriteLine("登录失败：账号或密码错误或已在线");
                                                Task.Delay(100).Wait();
                                                Disconnect();
                                                break;
                                            }
                                        }
                                        catch
                                        {
                                            service.Send(new byte[1] { (int)DataType.Error_Unknown });
                                            Console.WriteLine("登录失败：未知错误");
                                            Task.Delay(100).Wait();
                                            Disconnect();
                                            break;
                                        }
                                    }
                                    Task.Run(async () =>
                                    {
                                        using var sql = new SQLite();
                                        string friendList = sql.GetValue(Uid, SQLite.ValuesType.FriendList);
                                        string email = sql.GetValue(Uid, SQLite.ValuesType.Email);
                                        string account_info = $"{Uid}|{username}|{email}|{friendList}";
                                        isLogin = true;
                                        service.Send(new byte[1] { (int)DataType.State_Account_Success }
                                        .Concat(Encoding.UTF8.GetBytes(account_info)).ToArray());
                                        lock (clients)
                                        {
                                            clients.Add(Uid, this);
                                        }
                                        await Task.Delay(100);
                                        SendData($"{username} 已上线", DataType.Notice);
                                    });
                                }
                                else if (buffer[1] == 1) // 注册
                                {
                                    if (Environment.Mode != Environment.ModeType.Local)
                                    {
                                        try
                                        {
                                            string[] login_info = Encoding.UTF8.GetString(buffer, 2, buffer.Length - 2).Split('^');
                                            if (login_info.Length != 2)
                                            {
                                                service.Send(new byte[1] { (int)DataType.Error_Account });
                                                Task.Delay(100).Wait();
                                                Disconnect();
                                            }
                                            username = login_info[0];
                                            password_sha256 = login_info[1];
                                            if (password_sha256.Length != 64) Disconnect();
                                            using var sql = new SQLite();
                                            if (sql.GetUserId(username, out Uid))
                                            {
                                                service.Send(new byte[1] { (int)DataType.Error_Account });
                                                Console.WriteLine("注册失败：用户已存在");
                                                Task.Delay(100).Wait();
                                                Disconnect();
                                                break;
                                            }
                                            if (!sql.AddValue(username, password_sha256, ""))
                                            {
                                                service.Send(new byte[1] { (int)DataType.Error_Unknown });
                                                Console.WriteLine("注册失败：服务器内部错误");
                                                Task.Delay(100).Wait();
                                                Disconnect();
                                                break;
                                            }
                                        }
                                        catch
                                        {
                                            service.Send(new byte[1] { (int)DataType.Error_Unknown });
                                            Console.WriteLine("注册失败：服务器内部错误");
                                            Task.Delay(100).Wait();
                                            Disconnect();
                                            break;
                                        }
                                    }
                                    Task.Run(() =>
                                    {
                                        service.Send(new byte[1] { (int)DataType.State_Account_Success });
                                        Console.WriteLine($"新用户注册：{username}");
                                        Task.Delay(100).Wait();
                                        Disconnect();
                                    });
                                }
                                break;
                            case DataType.Message: // 聊天信息
                                if (isLogin)
                                {
                                    string[] message_info = Encoding.UTF8.GetString(buffer, 1, buffer.Length - 1).Split('^');
                                    int uid = int.Parse(message_info[0]);
                                    string message = message_info[1];
                                    if (message_info.Length != 2) continue;
                                    Console.WriteLine($"发送方：{username}, 接收方：{clients[uid].username}, 消息：{message}");
                                    using SQLite sql = new SQLite();
                                    string[] friends = sql.GetValue(uid, SQLite.ValuesType.FriendList).Split('^');
                                    if (!clients.ContainsKey(uid))
                                    {
                                        service.Send(new byte[2] { (byte)DataType.Message_Send_Status, 1 });
                                        break;
                                    }
                                    else if (!friends.Contains(Uid.ToString()))
                                    {
                                        Disconnect();
                                        break;
                                    }
                                    lock (clients[uid])
                                    {
                                        clients[uid].service.Send(new byte[1] { (byte)DataType.Message }.Concat(Encoding.UTF8.GetBytes($"{Uid}^{message}")).ToArray());
                                    }
                                    service.Send(new byte[2] { (byte)DataType.Message_Send_Status, 0 });
                                }
                                break;
                            case DataType.Ping_Result: // 返回 Ping 包
                                service.Send(new byte[1] { (int)DataType.Ping_Result }.Concat(BitConverter.GetBytes(DateTimeOffset.Now.ToUnixTimeMilliseconds() - t0)).ToArray());
                                break;
                            case DataType.Get_Value: // 获取账户信息
                                if (isLogin)
                                {
                                    if (buffer[1] > (byte)SQLite.ValuesType.MaxValue) Disconnect();
                                    SQLite.ValuesType type = (SQLite.ValuesType)buffer[1];
                                    using SQLite sql = new SQLite();
                                    string result = sql.GetValue(Uid, type);
                                    if (result == null)
                                    {
                                        service.Send(new byte[2] { (byte)DataType.Get_Value_Result, (byte)type });
                                        break;
                                    }
                                    service.Send(new byte[2] { (byte)DataType.Get_Value_Result, (byte)type }
                                    .Concat(Encoding.UTF8.GetBytes(result)).ToArray());
                                }
                                break;
                            case DataType.Get_Account_Info: // 获取账户全部信息（可获取其他人）
                                if (isLogin)
                                {
                                    int uid = BitConverter.ToInt32(buffer, 1);
                                    using SQLite sql = new SQLite();
                                    string username = sql.GetValue(uid, SQLite.ValuesType.Username);
                                    if (username == null)
                                    {
                                        service.Send(new byte[1] { (byte)DataType.Get_Account_Info_Result }.Concat(buffer.Skip(1)).ToArray());
                                        break;
                                    }
                                    string result = $"{username}";
                                    service.Send(new byte[1] { (byte)DataType.Get_Account_Info_Result }
                                    .Concat(buffer.Skip(1))
                                    .Concat(Encoding.UTF8.GetBytes(result)).ToArray());
                                }
                                break;
                            case DataType.Update_Value: // 修改账户信息
                                if (isLogin)
                                {
                                    if (buffer[1] > (byte)SQLite.ValuesType.MaxValue) Disconnect();
                                    if (buffer[1] == (byte)SQLite.ValuesType.FriendList)
                                        service.Send(new byte[2] { (byte)DataType.Update_Value_Result, 3 });
                                    SQLite.ValuesType type = (SQLite.ValuesType)buffer[1];
                                    string content = Encoding.UTF8.GetString(buffer, 2, buffer.Length - 2);
                                    using SQLite sql = new SQLite();
                                    if (type == SQLite.ValuesType.FriendList && content.Split('^').Contains(Uid.ToString()))
                                        service.Send(new byte[2] { (byte)DataType.Update_Value_Result, 2 });
                                    if (sql.UpdateValue(Uid, type, content)) service.Send(new byte[2] { (byte)DataType.Update_Value_Result, 1 });
                                    else service.Send(new byte[2] { (byte)DataType.Update_Value_Result, 0 });
                                }
                                break;
                            case DataType.Add_Friend: // 添加好友
                                if (isLogin)
                                {
                                    int uid = BitConverter.ToInt32(buffer, 1);
                                    using SQLite sql = new SQLite();
                                    List<string> processes = sql.GetValue(uid, SQLite.ValuesType.UnprocessedRequests).Split('^').ToList();
                                    processes.Add($"{RequestType.To_Add_Friend}|{Uid}");
                                    sql.UpdateValue(uid, SQLite.ValuesType.UnprocessedRequests, string.Join('^', processes));
                                    processes = sql.GetValue(Uid, SQLite.ValuesType.UnprocessedRequests).Split('^').ToList();
                                    processes.Add($"{RequestType.From_Add_Friend}|{uid}");
                                    sql.UpdateValue(Uid, SQLite.ValuesType.UnprocessedRequests, string.Join('^', processes));
                                    service.Send(new byte[2] { (byte)DataType.Add_Friend_Result, 0 });
                                }
                                break;
                        }
                    }
                    catch { Disconnect(); break; }
                }
            }

            public void Disconnect()
            {
                if (connected)
                {
                    try
                    {
                        lock (clients)
                        {
                            connected = false;
                            clients.Remove(Uid);
                            service.Close();
                        }
                        if (isLogin) SendData($"{username} 已下线",DataType.Notice);
                        Console.WriteLine($"客户端已断开连接，当前连接数 {clients.Count}");
                    }
                    catch { }
                }
            }

            public enum DataType
            {
                Ping = 0, // Ping包
                Ping_Result = 1, // Ping
                State_Account_Success = 2,
                State_Closing = 3,
                Error_Unknown = 4,
                Error_Account = 5,
                Notice = 6,
                Login = 7,
                Register = 8,
                Message = 9,
                Message_Send_Status = 10,
                Get_Value = 11,
                Get_Value_Result = 12,
                Get_Account_Info = 13,
                Get_Account_Info_Result = 14,
                Update_Value = 15,
                Update_Value_Result = 16,
                Add_Friend = 17,
                Add_Friend_Result = 18
            }

            public enum RequestType
            {
                From_Add_Friend = 0,
                To_Add_Friend = 1
            }

            public static void SendData(string data, DataType type)
            {
                lock (clients)
                {
                    foreach (var client in clients.Values)
                    {
                        try { client.service.Send(new byte[1] { (byte)type }.Concat(Encoding.UTF8.GetBytes(data)).ToArray()); }
                        catch
                        {
                            client.Disconnect();
                        }
                    }
                }
                Console.WriteLine($"{type} 包：{data}");
            }
            public static void SendData(DataType type)
            {
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
        }
    }
}