using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AdvancedTcpClient advancedTcpClient = new AdvancedTcpClient();

        public MainWindow()
        {
            InitializeComponent();
            advancedTcpClient.Connect("127.0.0.1", 911);
            advancedTcpClient.BeginReceive();
            advancedTcpClient.DataReceived += ((client, data) => {
                this.logs.Dispatcher.BeginInvoke(() => { logs.Text = $"{Encoding.UTF8.GetString(data.ReceivedData)}{Environment.NewLine}" + logs.Text; });
            });
            button.IsEnabled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (text.Text != "")
            {
                advancedTcpClient.Send($"{Name.Text} 说：{text.Text}");
                logs.Text = $"你说：{text.Text}{Environment.NewLine}" + logs.Text;
                text.Text = null;
            }
            else
            {
                logs.Text = $"不能发送空字符{Environment.NewLine}" + logs.Text;
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
                            byte[] buffer = new byte[8193];
                            obj.ToString();
                            if (client.Client != null)
                            {
                                int size = client.Client.Receive(buffer);
                                Array.Resize(ref buffer, size);
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
