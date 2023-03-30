using System;
using Client.Module;
using System.Windows;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using System.Text;
using Client.Views;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Values.MainWindow = this;

            // 初始化
            Values.TcpSocket.PackageReceive += (s, e) =>
            {
                if (e.packetType == TcpSocket.PacketType.Message_Messages)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Values.MessagesList.Add(Encoding.UTF8.GetString(e.Data));
                    });
                }
            };
            Values.TcpSocket.LoginCompleted += (s, e) =>
            {
                if (Values.MainWindow.RootFrame.SourcePageType == typeof(LoginPage))
                {
                    switch (e.LoginState)
                    {
                        case TcpSocket.PacketType.State_Account_Success:
                            Dispatcher.Invoke(() =>
                            {
                                Values.MainWindow.RootFrame.SourcePageType = typeof(AccountPage);
                            });
                            DialogTools.ShowDialogWithCloseButton("提示", "登录成功");
                            break;
                        case TcpSocket.PacketType.State_Account_Error:
                            DialogTools.ShowDialogWithCloseButton("错误", "用户名或密码错误");
                            break;
                        case TcpSocket.PacketType.State_Server_Error:
                            DialogTools.ShowDialogWithCloseButton("错误", "服务器内部错误");
                            break;
                    }
                }
            };
        }

        private void RootNavigation_Loaded(object sender, RoutedEventArgs e)
        {
            Values.UIDispatcher = Dispatcher;
            RootNavigation.SelectedItem = RootNavigation.MenuItems[0];
        }

        private void RootNavigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            Type navigatePage;
            switch (((NavigationViewItem)args.SelectedItem).Name)
            {
                case "Home":
                    navigatePage = typeof(Views.HomePage);
                    break;
                case "Chat":
                    navigatePage = typeof(Views.ChatPage);
                    break;
                case "Account":
                    if (Values.TcpSocket != null && Values.TcpSocket.IsLogin) navigatePage = typeof(Views.AccountPage);
                    else navigatePage = typeof(Views.LoginPage);
                    break;
                case "Settings":
                    navigatePage = typeof(Views.Settings);
                    break;
                default:
                    goto case "Home";
            }
            RootFrame.SourcePageType = navigatePage;
        }
    }
}
