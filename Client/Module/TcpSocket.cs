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
    public class TcpSocket : IDisposable
    {
        private readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private bool disposedValue;
        public Task ReceiveTask;
        public bool Connected;
        public bool IsLogin = false;

        public TcpSocket(string server, int port)
        {
            Connected = socket.Connected;
            try
            {
                socket.Connect(server, port);
                Connected = true;
                socket.Send(new byte[0]);
            }
            catch { }
            Console.WriteLine(Connected);


        }

        /// <summary>
        /// 数据包类型
        /// </summary>
        public enum PacketType
        {
            Ping = 00,
            PingBack = 01,
            State_Account_Success = 10,
            State_Account_Error = 11,
            State_Server_Closing = 12,
            State_Server_Error = 13,
            Message_Notice = 20,
            Message_Login = 21,
            Message_Register = 22,
            Message_Messages = 23,
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
