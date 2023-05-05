using Client.Module;
using ModernWpf.Controls;
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
    public partial class LoginPage : System.Windows.Controls.Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                AnimationTools.OpacityAnimation(Values.AccountPage.LoadingRing, 0.5, new TimeSpan(0, 0, 0, 0, 300)); // 显示加载动画
            });

            string username = Username.Text;
            string password = Password.Password;

            if (string.IsNullOrEmpty(username) || username.Length < 1 || username.Length > 12 || username.Contains('-') || username.Contains(' ') || username.Contains('^'))
            {
                Dispatcher.Invoke(() =>
                {
                    AnimationTools.OpacityAnimation(Values.AccountPage.LoadingRing, 0, new TimeSpan(0, 0, 0, 0, 300)); // 隐藏加载动画
                });
                ContentDialog dialog = new ContentDialog
                {
                    Title = "提示",
                    Content = "用户名不可为空、小于1字符或大于 12 字符、或包含特殊符号 提示：（A-Z a-z 0-9 _）",
                    CloseButtonText = "确认",
                    DefaultButton = ContentDialogButton.Close
                };
                DialogTools.ShowDialog(dialog);
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    AnimationTools.OpacityAnimation(Values.AccountPage.LoadingRing, 0, new TimeSpan(0, 0, 0, 0, 300)); // 隐藏加载动画
                });
                if (string.IsNullOrEmpty(password) || password.Length < 6 || password.Length > 15)
                {
                    ContentDialog dialog = new ContentDialog
                    {
                        Title = "提示",
                        Content = "密码不可为空、小于6字符或大于15字符",
                        CloseButtonText = "确认",
                        DefaultButton = ContentDialogButton.Close
                    };
                    dialog.ShowAsync();
                }
                else
                {
                    Task.Run(() =>
                    {
                        Values.TcpSocket = new TcpSocket("127.0.0.1", 911, username, HashTools.GetSHA256(password));
                        Values.RaiseSocketInitialized();
                    });
                }
            }
        }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Values.AccountPage.Account_Frame.SourcePageType = typeof(RegisterPage);
            });
        }
    }
}
