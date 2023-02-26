using BianChat.Controls;
using BianChat.Models;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// ChatPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChatPage : System.Windows.Controls.Page
    {
        private ObservableCollection<UserListItem> userList = new ObservableCollection<UserListItem>();

        public ChatPage()
        {
            InitializeComponent();

            Loaded += delegate
            {
                if (!AccountProfile.Connected)
                {
                    new ContentDialog
                    {
                        Title = "提示",
                        Content = "请到账户页进行登录。",
                        PrimaryButtonText = "跳转至账户页",
                        PrimaryButtonCommand = new CommandModel((obj) => { return true; }, (obj) =>
                        {
                            MainWindow window = (MainWindow)App.Current.MainWindow;
                            window.NavigateToPage(typeof(AccountPage), new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
                        }),
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                }
            };
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UserListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageTextBox.IsEnabled = true;
            SendButton.IsEnabled = true;
        }

        private void AddFriendButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
