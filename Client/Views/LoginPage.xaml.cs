using System;
using System.Windows;
using System.Threading.Tasks;
using Client.Module;
using System.Windows.Controls;
using ModernWpf.Media.Animation;
using System.Text;

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
                };
            });
        }
    }
}
