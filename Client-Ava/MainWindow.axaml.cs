using System.Drawing;
using Avalonia.Controls;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using System.Collections.ObjectModel;
using Avalonia.Layout;

namespace Client_Ava
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> ChatList = new ObservableCollection<string>();
        private AdvancedTcpClient Client = new AdvancedTcpClient();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (UserName.Text == "" || UserName.Text.Length == 0 || UserName.Text.Length >= 12)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "提示",
                    Content = "用户名不为空或大于12字符",
                    CloseButtonText = "确认",
                    DefaultButton = ContentDialogButton.Close
                };
                dialog.ShowAsync();
                return;
            }
            else
            {
                Login.IsVisible = false;
                ChatList.Clear();
                ChatListBox.Items = ChatList;

                FluentAvalonia.UI.Controls.ComboBoxItem item = ServerSelectionComboBox.SelectedItem as FluentAvalonia.UI.Controls.ComboBoxItem;
                string ip = item.Tag as string;
                Client.Connect(ip);
                Client.BeginReceive();
                Client.DataReceived += (s,e) =>
                {
                    string message = Encoding.UTF8.GetString(e.ReceivedData,0,e.size);
                    ChatList.Add(message);
                };
            }

        }
    }

    public class AdvancedTcpClient : IDisposable
    {
        // EventArgs
        public class DataReceivedEventArgs : EventArgs
        {
            public byte[] ReceivedData { get; set; }
            public int size { get; set; }
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

        public AdvancedTcpClient() { }

        public void Connect(string ip)
        {
            client?.Close();
            client = new TcpClient();
            int idx = ip.LastIndexOf(':');
            string ip1 = ip[..(idx)];
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
                    while (true)
                    {
                        try
                        {
                            // 接收
                            int size = 0;
                            byte[] buffer = new byte[512];
                            if (client.Client != null)
                            {
                                size = client.Client.Receive(buffer);
                            }
                            else
                            {
                                Connected = false;
                                break;
                            }
                            DataReceived(
                                client,new DataReceivedEventArgs { ReceivedData = buffer,size = size });
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

        public void Disconnect()
        {
            client?.Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    Disconnect();
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
