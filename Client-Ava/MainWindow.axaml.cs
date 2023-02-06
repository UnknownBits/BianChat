using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using Client_Ava.Pages;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Client_Ava
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<ListBoxItem> ChatList = new ObservableCollection<ListBoxItem>();
        private AdvancedTcpClient Client = new AdvancedTcpClient();
        private LoginPage LoginPage;
        private InfoPage InfoPage = new InfoPage();

        public MainWindow()
        {
            InitializeComponent();

            LoginPage = (LoginPage)Login.Content;
            LoginPage.MainWindow = this;
            InfoPage.MainWindow = this;
            Client.DataReceived += DataReceivedCallback;
            Client.Disconnected += (s, e) =>
            {
                Task.Run(() =>
                {
                    if (e.Exception != null)
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            ContentDialog dialog = new ContentDialog
                            {
                                CloseButtonText = "确定",
                                DefaultButton = ContentDialogButton.Close,
                                Title = "错误",
                                Content = $@"连接异常终止。错误信息：{e.Exception.Message}"
                            };
                            dialog.ShowAsync();
                        });
                        Task.Delay(200).Wait();
                    }
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        SendTextBox.IsEnabled = false;
                        SendButton.IsEnabled = false;
                        ChatList.Clear();
                        Login.IsHitTestVisible = false;
                        OpacityAnimation(Login, 0, TimeSpan.FromMilliseconds(300));
                    });
                    Task.Delay(300).Wait();

                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Login.Content = LoginPage;
                        OpacityAnimation(Login, 1, TimeSpan.FromMilliseconds(300));
                        Login.IsHitTestVisible = true;
                    });
                });
            };
            Client.PingReceived += (s, e) =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    InfoPage.PingText.Text = $"延迟：{e.Ping} ms";
                });
            };
        }

        private void OpacityAnimation(Animatable control, double opacity, TimeSpan duration)
        {
            Animation animation = new Animation
            {
                Duration = duration,
                PlaybackDirection = PlaybackDirection.Normal,
                FillMode = FillMode.Both
            };
            var kf = new KeyFrame
            {
                Cue = new Cue(1.0)
            };
            kf.Setters.Add(new Setter
            {
                Property = OpacityProperty,
                Value = opacity
            });
            animation.Children.Add(kf);
            animation.RunAsync(Login, null);
        }

        private void DataReceivedCallback(object? sender, AdvancedTcpClient.DataReceivedEventArgs args)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                switch (args.ReceivedData[0])
                {
                    // 公告
                    case 0:
                        string notice = Encoding.UTF8.GetString(args.ReceivedData, 1, args.ReceivedData.Length - 1);
                        InfoPage.Notices.Add(new ListBoxItem { FontSize = 20, Content = notice, IsHitTestVisible = false });
                        break;

                    // 消息
                    case 1:
                        string message = Encoding.UTF8.GetString(args.ReceivedData, 1, args.ReceivedData.Length - 1);
                        ChatList.Add(new ListBoxItem
                        {
                            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                            Content = new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                            IsHitTestVisible = false
                        });
                        break;

                    case 255:
                        string stamp = Encoding.UTF8.GetString(args.ReceivedData, 1, args.ReceivedData.Length - 1);
                        ChatList.Add(new ListBoxItem
                        {
                            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                            Content = new TextBlock { Text = stamp, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                            IsHitTestVisible = false
                        });
                        Client.Send($"{LoginPage.Username.Text} 说：{stamp}");
                        break;
                }
            });
        }

        public void Connect(string username, string ip)
        {
            if (string.IsNullOrEmpty(username) || username.Length >= 12)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "提示",
                    Content = "用户名不可为空或大于 12 字符",
                    CloseButtonText = "确认",
                    DefaultButton = ContentDialogButton.Close
                };
                dialog.ShowAsync();
            }
            else
            {
                Login.IsHitTestVisible = false;
                OpacityAnimation(Login, 0, TimeSpan.FromMilliseconds(300));
                Task.Run(() =>
                {
                    Task.Delay(500).Wait();

                    try
                    {
                        Client.Connect(ip);
                        Client.BeginReceive();
                        Client.SendBytes(new byte[1] { 0 }.Concat(Encoding.UTF8.GetBytes(LoginPage.Username.Text)).ToArray());

                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            ChatList.Clear();
                            ChatListBox.Items = ChatList;
                            SendTextBox.IsEnabled = true;
                            SendButton.IsEnabled = true;
                            InfoPage.Notices.Clear();
                        }).Wait();
                    }
                    catch
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            ContentDialog dialog = new ContentDialog
                            {
                                CloseButtonText = "确定",
                                DefaultButton = ContentDialogButton.Close,
                                Content = "无法连接到服务器",
                                Title = "错误"
                            };
                            dialog.ShowAsync();

                            SendTextBox.IsEnabled = false;
                            SendButton.IsEnabled = false;
                            OpacityAnimation(Login, 1, TimeSpan.FromMilliseconds(300));
                            Login.IsHitTestVisible = true;
                        });
                        return;
                    }

                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        InfoPage.Username.Text = $"用户名：{LoginPage.Username.Text}";
                        var selectedItem = (FluentAvalonia.UI.Controls.ComboBoxItem)LoginPage.ServerSelectionComboBox.SelectedItem;
                        InfoPage.ServerName.Text = $"服务器：{selectedItem.Content}";
                        Login.Content = InfoPage;

                        OpacityAnimation(Login, 1, TimeSpan.FromMilliseconds(300));
                        Login.IsHitTestVisible = true;
                    });
                });
            }
        }

        public void Disconnect()
        {
            if (Client.Connected)
            {
                Client.Disconnect();
            }
        }

        private void SendButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SendTextBox.Text) || SendTextBox.Text.Length >= 2048)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "提示",
                    Content = "发送消息不可为空或超过 2048 个字",
                    CloseButtonText = "确认",
                    DefaultButton = ContentDialogButton.Close
                };
                dialog.ShowAsync();
            }
            else
            {
                Client.Send($"{LoginPage.Username.Text} 说：{SendTextBox.Text}");
                ChatList.Add(new ListBoxItem
                {
                    HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Content = new TextBlock { Text = $"你说：{SendTextBox.Text}", TextWrapping = Avalonia.Media.TextWrapping.Wrap }
                });
                SendTextBox.Text = "";
            }
        }
    }

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

        public AdvancedTcpClient() { }

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
                                timediff = BitConverter.ToInt64(buffer, 1);
                            }
                            else if (buffer[0] == 254)
                            {
                                long timestamp = BitConverter.ToInt64(buffer, 1);
                                PingReceived(client, new PingReceivedEventArgs { Ping = (int)(timestamp - timediff - 500) });
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
