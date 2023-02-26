using BianChat.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace BianChat.Views
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
            if (Username.Text == "") // 判断用户名
            {
                DialogTools.ShowDialogWithCloseButton("提示", "用户名不可为空");
            }
            else if (Username.Text.Length < 3 || Username.Text.Length > 12)
            {
                DialogTools.ShowDialogWithCloseButton("提示", "用户名长度需在 3-16 位内");
            }
            else if (!Regex.IsMatch(Username.Text, "^[-_a-zA-Z0-9]{3,16}$"))
            {
                DialogTools.ShowDialogWithCloseButton("提示", "用户名只能包含大小写字母或数字或下划线");
            }
            else if (Password.Password == "") // 判断密码
            {
                DialogTools.ShowDialogWithCloseButton("提示", "密码不能为空");
            }
            else if (Password.Password.Length < 8 || Password.Password.Length > 20)
            {
                DialogTools.ShowDialogWithCloseButton("提示", "密码长度需在 8-30 位内");
            }
            else if (!Regex.IsMatch(Password.Password, "(?=.*[A-Z])(?=.*[a-z])(?=.*\\d)(?=.*[$@!%*#?&])[A-Za-z\\d$@!%*#?&]{8,30}$"))
            {
                DialogTools.ShowDialogWithCloseButton("提示", "密码需包含大小写字母和数字和以下标点符号：$@!%*#?&");
            }
            else // 验证通过
            {
                AnimationTools.OpacityAnimation(LoadingRing, 0.5, new TimeSpan(0, 0, 0, 0, 300)); // 显示加载动画
                string username = Username.Text;
                string password = Password.Password;
                Task.Run(() =>
                {
                    Exception? ex = null;
                    ChatClient.LoginCompletedEventArgs.State? state = null;

                    AccountProfile profile = new AccountProfile();
                    profile.Client.Disconnected += (s, e) => { ex = e.Exception; };
                    profile.Client.LoginCompleted += (s, e) => { state = e.LoginState; };
                    profile.Connect(username, password);

                    AnimationTools.OpacityAnimation(LoadingRing, 0, new TimeSpan(0, 0, 0, 0, 300)); // 显示加载动画
                    if (ex != null)
                    {
                        DialogTools.ShowDialogWithCloseButton("错误", $"尝试连接到服务器时出现错误：{ex.Message}");
                    }
                    else
                    {
                        switch (state)
                        {
                            case ChatClient.LoginCompletedEventArgs.State.Success:
                                DialogTools.ShowDialogWithCloseButton("提示", "登录成功");
                                PublicValues.MainWindow.NavigateToPage(typeof(AccountPage));
                                break;
                            case ChatClient.LoginCompletedEventArgs.State.Failed_Account:
                                DialogTools.ShowDialogWithCloseButton("错误", "用户名或密码错误");
                                break;
                            case ChatClient.LoginCompletedEventArgs.State.Failed_Unknown:
                                DialogTools.ShowDialogWithCloseButton("错误", "服务器内部错误");
                                break;
                        }
                    }
                });
            }
        }
    }
}
