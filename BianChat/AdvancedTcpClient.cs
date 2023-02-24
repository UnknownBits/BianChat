using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BianChat
{
    public class AdvancedTcpClient : IDisposable
    {

        public event EventHandler<DataReceivedEventArgs> DataReceived = delegate { };
        public class DataReceivedEventArgs : EventArgs
        {
            public byte[] ReceivedData { get; set; }
            public PacketType DataType { get; set; }
        }

        public event EventHandler<PingReceivedEventArgs> PingReceived = delegate { };
        public class PingReceivedEventArgs : EventArgs
        {
            public int Ping { get; set; }
        }

        public event EventHandler<DisconnectedEventArgs> Disconnected = delegate { };
        public class DisconnectedEventArgs : EventArgs
        {
            public Exception ?Exception { get; init; }
        }

        private TcpClient client = new TcpClient();
        public Thread ?ReceiveTask;
        public bool Connected = false;
        private bool disposedValue;
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
                            if (buffer[0] == (int)PacketType.Ping)
                            {
                                client.Client.Send(new byte[1] { 0 });
                            }
                            else if (buffer[0] == (int)PacketType.PingBack) // Ping 包
                            {
                                int ping = BitConverter.ToInt32(buffer, 1);
                                PingReceived(client, new PingReceivedEventArgs { Ping = ping });
                            }
                            else
                            {
                                DataReceived(
                                    client, new DataReceivedEventArgs { ReceivedData = buffer.Skip(1).ToArray(), DataType = (PacketType)buffer[0] });
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

        public bool SendPacket(PacketType packetType, byte[] data)
        {
            return SendBytes(new byte[1] { (byte)packetType }.Concat(data).ToArray());
        }

        public enum PacketType
        {
            Ping = 0,
            PingBack = 1,
            State_Account_Success = 2,
            State_Closing = 3,
            Error_Unknown = 4,
            Error_Account = 5,
            Notice = 6,
            Login = 7,
            Register = 8,
            Message = 9
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