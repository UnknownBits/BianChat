using Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Client.Views.Account_Pages
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
                        case TcpSocket.PacketType.State_Account_Success:
                            Dispatcher.Invoke(() =>
                            {
                                Values.MainWindow.RootFrame.SourcePageType = typeof(AccountPage);
                            });
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
