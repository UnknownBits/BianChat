using System;
using System.Windows;
using System.Threading.Tasks;
using Client.Module;
using System.Windows.Controls;

namespace Client.Views
{
    /// <summary>
    /// LoginPage.xaml 的交互逻辑
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            AnimationTools.OpacityAnimation(LoadingRing, 0.5, new TimeSpan(0, 0, 0, 0, 300)); // 显示加载动画
            string username = Username.Text;
            string password = Password.Password;
            
            
            Task.Run(() =>
            {
                Values.TcpSocket = new TcpSocket("127.0.0.1", 911, username, HashTools.GetSHA256(password));
                Values.TcpSocket.LoginCompleted += (s, e) =>
                {
                    AnimationTools.OpacityAnimation(LoadingRing, 0, new TimeSpan(0, 0, 0, 0, 300)); // 隐藏加载动画
                    switch (e.LoginState)
                    {
                        case TcpSocket.LoginCompletedEventArgs.State.Success:
                            Values.MainWindow.RootFrame.SourcePageType = typeof(AccountPage);
                            DialogTools.ShowDialogWithCloseButton("提示", "登录成功");
                            break;
                        case TcpSocket.LoginCompletedEventArgs.State.Failed_Account:
                            DialogTools.ShowDialogWithCloseButton("错误", "用户名或密码错误");
                            break;
                        case TcpSocket.LoginCompletedEventArgs.State.Failed_Unknown:
                            DialogTools.ShowDialogWithCloseButton("错误", "服务器内部错误");
                            break;
                    }
                };
            });
        }
    }
}
