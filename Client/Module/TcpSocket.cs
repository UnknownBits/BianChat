using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client.Module
{
    public class TcpSocket
    {
        public event EventHandler<PackageReceive_EventArgs> PackageReceive;
        public class PackageReceive_EventArgs : EventArgs
        {
            public PacketType packetType { get; set; }
            public byte[] data { get; set; }
        }

        public event EventHandler<PingPackageReceive_EventArgs> PingPackageReceive;
        public class PingPackageReceive_EventArgs : EventArgs
        {
            public int Ping { get; set; }
        }
        private readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private bool disposedValue;
        public Task ReceiveTask;
        public bool Connected;
        public bool IsLogin = false;

        public TcpSocket(string server, int port)
        {
            try
            {
                Connected = socket.Connected;
                socket.Connect(server, port);
                Connected = true;
            }
            catch(Exception ex) { }
            if (Connected)
            {
                ReceiveTask = Task.Run(() =>
                {
                    long timediff = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    while (socket != null || Connected)
                    {
                        try
                        {
                            // 接收
                            int size = 0;
                            byte[] buffer = new byte[8193];
                            size = socket.Receive(buffer);
                            if (size <= 0) { throw new SocketException(10054); }
                            Array.Resize(ref buffer, size);
                            Trace.WriteLine($"[TcpSocket] 接收到类型为 {(PacketType)buffer[0]} 的数据");
                            switch ((PacketType)buffer[0])
                            {
                                case PacketType.PingBack:
                                    int ping = BitConverter.ToInt32(buffer, 1);
                                    Task.Run(() => PingPackageReceive(socket, new PingPackageReceive_EventArgs { Ping = ping }));
                                    break;
                                default:
                                    Task.Run(() => PackageReceive(socket, new PackageReceive_EventArgs { packetType = (PacketType)buffer[0], data = buffer.Skip(1).ToArray() }));
                                    break;
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
                    if (Connected)
                    {
                        Dispose();
                    }
                });
            }
        }

        /// <summary>
        /// 数据包类型
        /// </summary>
        public enum PacketType
        {
            Ping,
            PingBack,
            State_Account_Success,
            State_Account_Error,
            State_Server_Closing,
            State_Server_Error,
            Message_Notice,
            Message_Login,
            Message_Register,
            Message_Messages,
        }

        //断开连接
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

        protected virtual void Dispose(bool disposing, Exception exception)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Connected)
                    {
                        socket.Close();
                    }
                }
                socket.Dispose();
                IsLogin = false;
                Connected = false;
                disposedValue = true;
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Connected)
                    {
                        socket.Close();
                    }
                }
                socket.Dispose();
                IsLogin = false;
                Connected = false;
                disposedValue = true;
            }
        }
    }
}
