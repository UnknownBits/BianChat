using Client.Controls;
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

namespace Client.Views
{
    /// <summary>
    /// ChatPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChatPage : Page
    {
        private ObservableCollection<UserListItem> userList = new ObservableCollection<UserListItem>();

        public ChatPage()
        {
            InitializeComponent();
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
