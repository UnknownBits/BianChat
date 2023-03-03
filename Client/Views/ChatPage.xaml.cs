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

namespace Client.Views
{
    /// <summary>
    /// ChatPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChatPage : Page
    {
        public ChatPage()
        {
            InitializeComponent();
        }
        private void UserListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageTextBox.IsEnabled = true;
            SendButton.IsEnabled = true;
        }
        private void AddFriendButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
