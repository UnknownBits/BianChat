using System.Net.Sockets;
using System.Text;

namespace Client_Console
{
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
                    long timediff = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    while (true)
                    {
                        try
                        {
                            // 接收
                            int size = 0;
                            byte[] buffer = new byte[8193];
                            if (client.Client != null)
                            {
                                size = client.Client.Receive(buffer);
                                Array.Resize(ref buffer, size);
                            }
                            else
                            {
                                Connected = false;
                                break;
                            }
                            if (size <= 0)
                            {
                                throw new SocketException(10054);
                            }
                            if (buffer[0] == 253)
                            {
                            }
                            else if (buffer[0] == 254) // Ping 包
                            {
                            }
                            else
                            {
                                DataReceived(
                                    client, new DataReceivedEventArgs { ReceivedData = buffer });
                            }
                        }
                        catch (Exception ex)
                        {
                            if (Connected)
                            {
                                Connected = false;
                                client.Close();
                            }
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
                lock (obj)
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
            return SendBytes(new byte[1] { 1 }.Concat(Encoding.UTF8.GetBytes(message)).ToArray());
        }

        public bool SendBytes(byte[] data)
        {
            if (Connected)
            {
                try
                {
                    client.Client.Send(data);
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