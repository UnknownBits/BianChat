using Avalonia.Controls;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;

namespace Client_Ava
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }

    public class AdvancedTcpClient : IDisposable
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
        /// <summary>
        /// 是否连接
        /// </summary>
        public bool Connected = false;
        private bool disposedValue;

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
                            byte[] buffer = new byte[512];
                            if (client.Client != null)
                            {
                                client.Client.Receive(buffer);
                            }
                            else
                            {
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    client.Close();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~AdvancedTcpClient()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
