using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
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
            public Exception? Exception { get; init; }
        }

        private TcpClient client = new TcpClient();
        public Thread? ReceiveTask;
        public bool Connected = false;
        private bool disposedValue = false;

        public AdvancedTcpClient() { }

        public void Connect(string ip)
        {
            Task.Delay(10).Wait();
            int idx = ip.LastIndexOf(':');
            string ip1 = ip[..idx];
            int port = int.Parse(ip[(idx + 1)..]);
            client.Connect(ip1, port);
            Connected = true;
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
                                Dispose(ex);
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
            Message = 9,
            Message_Send_Success = 10,
            Update_Value = 11,
            Get_Value = 12,
            Get_Value_Result = 13,
            Get_Account_Info = 14,
            Get_Account_Info_Result = 15
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
                catch (Exception ex)
                {
                    Dispose(ex);
                    return false;
                }
            }
            return false;
        }

        public void Dispose(Exception exception)
        {
            Dispose(disposing: true, exception);
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing, Exception exception = null)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Connected)
                    {
                        client?.Close();
                        Disconnected(this, new DisconnectedEventArgs { Exception = exception });
                    }
                }
                Connected = false;
                client?.Dispose();
                disposedValue = true;
            }
        }
    }
}