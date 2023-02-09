﻿using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server_Console
{
    public class Threadtcpserver
    {
        private static Socket server;
        static void Main(string[] args)
        {
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, 911);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);
            server.Bind(iep);
            server.Listen(20);
            Console.WriteLine("等待客户机进行连接......");
            while (true)
            {
                //得到包含客户端信息的套接字  
                Socket client = server.Accept();
                //创建消息服务线程对象  
                ClientThread newclient = new ClientThread(client);
                //把ClientThread类的ClientService方法委托给线程  
                Thread newthread = new Thread(new ThreadStart(newclient.ClientService));
                //启动消息服务线程  
                newthread.Start();
            }
        }

        public class ClientThread
        {
            public long t0;
            public static List<ClientThread> clients = new List<ClientThread>();
            public Socket service;
            public bool connected = false;
            public string username = null;
            public ClientThread(Socket clientsocket)
            { 
                this.service = clientsocket;
            }
            public void ClientService()
            {
                String data = null;
                byte[] bytes = new byte[8193];

                //如果Socket不是空 
                if (service != null)
                {
                    connected = true;
                    clients.Add(this);
                }
                Console.WriteLine("新客户连接建立：{0} 个连接数", clients.Count);

                Task.Run(async () => {
                    await Task.Delay(5000);
                    if (username == null) { Disconnect(); }
                });

                Task.Run(async () => {
                    while (true)
                    {
                        await Task.Delay(5000);
                        try {
                            t0 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            service.Send(new byte[1] { 253 });
                        }
                        catch {
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
                        switch (buffer[0]) {
                            case 0: // 登录包
                                username = Encoding.UTF8.GetString(buffer, 1, buffer.Length - 1);
                                string notice = $"{username} 已上线";
                                Notice(notice);
                                service.Send(new byte[1] { 1 }.Concat(Encoding.UTF8.GetBytes($"{DateTime.Now} PID:{clients.Count}")).ToArray());
                                break;
                            case 1: // 聊天信息
                                Console.WriteLine($"数据包：{Encoding.UTF8.GetString(buffer, 1, buffer.Length - 1)}");
                                lock (clients) {
                                    foreach (var client in clients) {
                                        try { if (client != this) { client.service.Send(new byte[1] { 1 }.Concat(buffer.Skip(1)).ToArray()); } }
                                        catch { client.Disconnect(); }
                                    }
                                }
                                break;
                            case 255: // 返回 Ping 包
                                long t1 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                service.Send(new byte[1] { 254 }.Concat(BitConverter.GetBytes((t1 - t0))).ToArray());
                                break;
                        }
                    }
                    catch { Disconnect(); break; }
                }
            }
            public void Disconnect() {
                if (connected) {
                    try {
                        lock (clients) {
                            connected = false;
                            clients.Remove(this);
                            service.Close();
                        }
                        Notice($"{username} 已下线");
                        Console.WriteLine($"客户端已断开连接，当前连接数 {clients.Count}");
                    }
                    catch {}
                }
            }
            private static void Notice(string notice) { lock (clients) { foreach (var client in clients) { try { client.service.Send(new byte[1] { 9 }.Concat(Encoding.UTF8.GetBytes(notice)).ToArray()); } catch { client.Disconnect(); } } } Console.WriteLine($"公告包：{notice}"); }
        }
    }
}