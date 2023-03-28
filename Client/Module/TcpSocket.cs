using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client.Module
{
    public class TcpSocket
    {
        public event EventHandler<PackageReceive_EventArgs> PackageReceive = delegate { };
        public class PackageReceive_EventArgs : EventArgs
        {
            public PacketType packetType { get; set; }
            public byte[] Data { get; set; }
        }

        public event EventHandler<PingPackageReceive_EventArgs> PingPackageReceive = delegate { };
        public class PingPackageReceive_EventArgs : EventArgs
        {
            public int Ping { get; set; }
        }
        public event EventHandler<ErrorReceive_EventArgs> ErrorReceive = delegate { };
        public class ErrorReceive_EventArgs : EventArgs
        {
            public Exception exception { get; set; }
        }

        public event EventHandler<LoginCompletedEventArgs> LoginCompleted = delegate { };
        public class LoginCompletedEventArgs : EventArgs
        {
            public State LoginState { get; set; }
            public enum State
            {
                Success,
                Failed_Account,
                Failed_Unknown
            }
        }


        private readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public bool Connected;
        public bool IsLogin = false;

        public TcpSocket(string server, int port,string username,string passwordHash)
        {
            try
            {
                Connected = socket.Connected;
                socket.Connect(server, port);
                Connected = true;

                Task ReceiveTask = Task.Run(() =>
                {
                    long timediff = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    while (socket != null && Connected)
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
                                case PacketType.Ping:
                                    SendPacket(PacketType.PingBack);
                                    break;
                                case PacketType.PingBack:
                                    int ping = BitConverter.ToInt32(buffer, 1);
                                    Task.Run(() => PingPackageReceive(this, new PingPackageReceive_EventArgs { Ping = ping }));
                                    break;
                                case PacketType.State_Account_Success:
                                    IsLogin = true;
                                    Task.Run(() =>
                                    {
                                        LoginCompleted(this, new LoginCompletedEventArgs
                                        {
                                            LoginState = LoginCompletedEventArgs.State.Success
                                        });
                                    });
                                    break;
                                default:
                                    Task.Run(() => PackageReceive(this, new PackageReceive_EventArgs { packetType = (PacketType)buffer[0], Data = buffer.Skip(1).ToArray() }));
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
                    Dispose(new Exception("断开连接"));
                });

                SendPacket(PacketType.Message_Login,$"{username}^{passwordHash}");
            }
            catch (Exception ex) { Dispose(ex); }
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


        public void SendPacket(PacketType type)
        {
            try { socket.Send(new byte[1] { (byte)type }); }
            catch (Exception ex) { Dispose(ex); }
            Trace.WriteLine($"发送 {type} 包");
        }

        public void SendPacket(PacketType type,string data)
        {
            try { socket.Send(new byte[1] { (byte)type }.Concat(Encoding.UTF8.GetBytes(data)).ToArray()); }
            catch(Exception ex) { Dispose(ex); }
            Console.WriteLine($"{type} 包：{data}");
        }

        //断开连接
        public void Dispose(Exception exception)
        {
            try
            {
                if (Connected)
                {
                    Connected = false;
                    IsLogin = false;
                    socket.Close();
                }
                Trace.WriteLine($"客户端已断开连接");
                Task.Run(() => ErrorReceive(this, new ErrorReceive_EventArgs { exception = exception }));
                
            }
            catch (Exception ex) { Dispose(ex); }
        }
    }
}
