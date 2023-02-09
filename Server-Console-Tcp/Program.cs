using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server_Console_Tcp
{
    internal class Program
    {
        private static List<TcpClient> clients = new List<TcpClient>();

        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 911);

            listener.Start();
            Console.WriteLine("等待客户端连接...");
            listener.BeginAcceptTcpClient(AcceptCallback, listener);

            string input = Console.ReadLine();
            while (input != "exit")
            {
                if(input.StartsWith("end "))
                {
                    if (input.Length > 4)
                    {
                        int i;
                        bool result = int.TryParse(input.AsSpan(4), out i);
                        if (result && 0 < i && i <= clients.Count)
                        {
                            lock (clients)
                            {
                                clients[i-1].Client.Close();
                            }
                            Console.WriteLine($"PID为{i}的客户端连接被结束");
                        }
                        else
                        {
                            Console.WriteLine("请输入正确的客户端pid");
                        }
                    }
                }
                else if (input.StartsWith("notice "))
                {
                    if (input.Length > 7)
                        Notice(input.Substring(7));
                }
                input = Console.ReadLine();
            }
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);
            long t0 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            bool connected = true;
            clients.Add(client);
            Console.WriteLine($"{(IPEndPoint)client.Client.RemoteEndPoint} 已连接，当前连接数 {clients.Count}");
            Task.Run(() =>
            {
                string username = null;
                Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    if (username == null)
                    {
                        Disconnect();
                    }
                });
                while (true)
                {
                    try
                    {
                        byte[] buffer = new byte[8193];
                        int size = client.Client.Receive(buffer);
                        if (size <= 0)
                        {
                            throw new Exception();
                        }
                        Array.Resize(ref buffer, size);
                        switch (buffer[0])
                        {
                            // 登录
                            case 0:
                                username = Encoding.UTF8.GetString(buffer, 1, buffer.Length - 1);
                                string notice = $"{username} 已上线";
                                Notice(notice);
                                client.Client.Send(new byte[1] { 1 }.Concat(Encoding.UTF8.GetBytes($"{DateTime.Now} PID:{clients.Count}")).ToArray());
                                break;

                            // 聊天信息
                            case 1:
                                Console.WriteLine($"数据包：{Encoding.UTF8.GetString(buffer, 1, buffer.Length - 1)}");
                                lock (clients)
                                {
                                    foreach (var client1 in clients)
                                    {
                                        try
                                        {
                                            if (client1 != client)
                                            {
                                                client1.Client.Send(new byte[1] { 1 }.Concat(buffer.Skip(1)).ToArray());
                                            }
                                        }
                                        catch
                                        {
                                            Disconnect();
                                        }
                                    }
                                }
                                break;

                            // 返回 Ping 包
                            case 255:
                                long t1 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                client.Client.Send(new byte[1] { 254 }.Concat(BitConverter.GetBytes((t1 - t0))).ToArray());
                                break;
                        }
                    }
                    catch
                    {
                        Disconnect();
                        break;
                    }
                }
            });

            void Disconnect()
            {
                if (connected)
                {
                    try
                    {
                        lock (clients)
                        {
                            connected = false;
                            clients.Remove(client);
                            client.Close();
                        }
                        Console.WriteLine($"客户端已断开连接，当前连接数 {clients.Count}");
                    }
                    catch { }
                }
            }

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(5000);
                    try
                    {
                        t0 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        client.Client.Send(new byte[1] { 253 });
                    }
                    catch
                    {
                        Disconnect();
                        break;
                    }
                }
            });

            listener.BeginAcceptTcpClient(AcceptCallback, listener);
        }

        private static void Notice(string notice)
        {
            Console.WriteLine($"公告：{notice}");
            lock (clients)
            {
                foreach (var client1 in clients)
                {
                    try
                    {
                        client1.Client.Send(new byte[1] { 9 }.Concat(Encoding.UTF8.GetBytes(notice)).ToArray());
                    }
                    catch
                    {
                        client1.Close();
                    }
                }
            }
        }
    }
}