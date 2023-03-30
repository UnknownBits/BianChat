using Client.Controls;
using Client.Module;
using ModernWpf.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Client.Views
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
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Values.TcpSocket == null || !Values.TcpSocket.IsLogin) {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "提示",
                    Content = "请到登录页进行登录。",
                    PrimaryButtonText = "跳转至登录页",
                    DefaultButton = ContentDialogButton.Primary
                };
                dialog.Closing += delegate { Values.MainWindow.RootNavigation.SelectedItem = Values.MainWindow.Account; };
                DialogTools.ShowDialog(dialog);
            }
        }

        /// <summary>
        /// UserListBox 选中处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageTextBox.IsEnabled = true;
            SendButton.IsEnabled = true;
        }
        /// <summary>
        /// 新增好友按键处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddFriendButton_Click(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 发送按钮处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
