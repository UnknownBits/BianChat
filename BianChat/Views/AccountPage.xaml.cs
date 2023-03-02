using BianChat.Models;
using BianChat.Tools;
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

namespace BianChat.Views
{
    /// <summary>
    /// AccountPage.xaml 的交互逻辑
    /// </summary>
    public partial class AccountPage : System.Windows.Controls.Page
    {
        public AccountPage()
        {
            InitializeComponent();

            Loaded += delegate
            {
                AccountProfile.Current.Client.Disconnected += (s, e) =>
                {
                    if (e.Exception != null)
                    {
                        DialogTools.ShowDialogWithCloseButton("错误", $"连接异常退出：{e.Exception.Message}");
                        PublicValues.UIDispatcher.Invoke(() =>
                        {
                            PublicValues.MainWindow.RootNavigation.SelectedItem = PublicValues.MainWindow.Home;
                        });
                    }
                };

                Username.Text = AccountProfile.Current.UserInfo.Username;
                // TODO: 头像
            };
        }
    }
}
