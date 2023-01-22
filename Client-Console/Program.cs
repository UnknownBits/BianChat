using System.Net.Sockets;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Client_Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AdvancedTcpClient advancedTcpClient = new AdvancedTcpClient();
            advancedTcpClient.Connect("127.0.0.1", 911);
            advancedTcpClient.BeginReceive();
            advancedTcpClient.DataReceived += ((client, data) => {
                Console.WriteLine($"接收到信息：{Encoding.UTF8.GetString(data.ReceivedData)}");
            });

            while (true)
            {
                string a = Console.ReadLine();
                if (a != null)
                {
                    advancedTcpClient.Send(a);
                }
                else
                {
                    Console.WriteLine("不能发送空字符");
                }
            }
        }

        public class AdvancedTcpClient
        {
            // EventArgs
            public class DataReceivedEventArgs : EventArgs
            {
                public byte[] ReceivedData { get; set; } = new byte[0];
            }

            // TCP 客户端
            private TcpClient client;

            /// <summary>
            /// 接收线程（任务）
            /// </summary>
            public Thread ReceiveTask;
            private static readonly object obj = new object();
            /// <summary>
            /// 是否连接
            /// </summary>
            public bool Connected = false;

            /// <summary>
            /// 数据接收事件
            /// </summary>
            public event EventHandler<DataReceivedEventArgs> DataReceived = delegate { };

            public AdvancedTcpClient()
            {
                client = new TcpClient();
            }

            public void Connect(string ip, int port)
            {
                client.Connect(ip, port);
                Connected = true;
            }

            public void BeginReceive()
            {
                if (Connected)
                {
                    ReceiveTask = new Thread(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                // 接收
                                byte[] buffer = new byte[1026];
                                obj.ToString();
                                if(client.Client != null)
                                {
                                    int size = client.Client.Receive(buffer);
                                    Array.Resize(ref buffer, size);
                                }
                                else {
                                    Connected = false;
                                    break;
                                }
                                DataReceived(
                                    client, new DataReceivedEventArgs { ReceivedData = buffer });
                            }
                            catch
                            {
                                client.Close();
                                Connected = false;
                                break;
                            }
                        }
                    });
                    ReceiveTask.IsBackground = true;
                    ReceiveTask.Start();
                }
            }

            public void EndReceive()
            {
                if (Connected)
                {
                    lock(obj)
                    {
                        client.Close();
                        try 
                        {
                            ReceiveTask.Abort();
                        } 
                        catch { }
                    }
                }

                Connected = false;
            }

            public bool Send(string message)
            {
                if (Connected)
                {
                    try
                    {
                        client.Client.Send(Encoding.UTF8.GetBytes(message));
                        return true;
                    }
                    catch
                    {
                        Connected = false;
                        return false;
                    }
                }

                return false;
            }
        }
    }
}