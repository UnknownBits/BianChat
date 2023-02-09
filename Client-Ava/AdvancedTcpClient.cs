﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client_Ava
{
    public class AdvancedTcpClient : IDisposable
    {
        // EventArgs
        public class DataReceivedEventArgs : EventArgs
        {
            public byte[] ReceivedData { get; set; }
        }

        public class PingReceivedEventArgs : EventArgs
        {
            public int Ping { get; set; }
        }

        public class DisconnectedEventArgs : EventArgs
        {
            public Exception Exception { get; init; }
        }

        // TCP 客户端
        private TcpClient client;

        /// <summary>
        /// 接收线程（任务）
        /// </summary>
        public Thread ReceiveTask;
        /// <summary>
        /// 是否连接
        /// </summary>
        public bool Connected = false;
        private bool disposedValue;

        /// <summary>
        /// 数据接收事件
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> DataReceived = delegate { };

        public event EventHandler<PingReceivedEventArgs> PingReceived = delegate { };

        public event EventHandler<DisconnectedEventArgs> Disconnected = delegate { };

        public void Connect(string ip)
        {
            client?.Close();
            Task.Delay(10).Wait();
            client = new TcpClient();
            int idx = ip.LastIndexOf(':');
            string ip1 = ip[..idx];
            int port = int.Parse(ip[(idx + 1)..]);
            client.Connect(ip1, port);
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
                                client.Client.Send(new byte[1] { 255 });
                            }
                            else if (buffer[0] == 254) // Ping 包
                            {
                                int ping = BitConverter.ToInt32(buffer, 1);
                                PingReceived(client, new PingReceivedEventArgs { Ping = ping });
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
                                Disconnected(this, new DisconnectedEventArgs { Exception = ex });
                            }
                            break;
                        }
                    }
                });
                ReceiveTask.IsBackground = true;
                ReceiveTask.Start();
            }
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

        public void Disconnect()
        {
            if (Connected)
            {
                Connected = false;
                client?.Close();
                Disconnected(this, new DisconnectedEventArgs());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Disconnect();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}