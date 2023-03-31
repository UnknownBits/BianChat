using Client.Module;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Client.Views
{
    /// <summary>
    /// AccountPage.xaml 的交互逻辑
    /// </summary>
    public partial class AccountPage : Page
    {
        private bool _isBusy = false;
        public AccountPage()
        {
            InitializeComponent();
            Values.AccountPage = this;
            Values.SocketInitialized += Values_SocketInitialized;
            if (Values.TcpSocket != null && Values.TcpSocket.IsLogin)
            {
                Account_Frame.IsEnabled = false;
                Values.AccountPage.UserName.Content = $"当前登录用户名:{Values.TcpSocket.username}";
            }
        }

        private void Values_SocketInitialized(object? sender, System.EventArgs e)
        {
            Task.Run(() =>
            {
                Values.TcpSocket.LoginCompleted += (s, e) =>
                {
                    AnimationTools.OpacityAnimation(LoadingRing, 0, new TimeSpan(0, 0, 0, 0, 300)); // 隐藏加载动画
                    switch (e.LoginState)
                    {
                        case TcpSocket.PacketType.State_Account_Success:
                            Dispatcher.Invoke(() =>
                            {
                                Values.MainWindow.RootNavigation.SelectedItem = Values.MainWindow.Chat;
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
                };
            });
        }
    }
}
