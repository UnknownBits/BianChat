﻿using Client.Controls;
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
        public ChatPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MessageListBox.ItemsSource = Values.MessagesList;
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
            else
            {
                SendButton.IsEnabled = true;
                MessageTextBox.IsEnabled = true;
            }
        }

        /// <summary>
        /// 发送按钮处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendButton.IsEnabled = false;
            MessageTextBox.IsEnabled = false;

            if (string.IsNullOrEmpty(MessageTextBox.Text) || MessageTextBox.Text.Length >= 2048)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "提示",
                    Content = "发送消息不可为空或超过 2048 个字",
                    CloseButtonText = "确认",
                    DefaultButton = ContentDialogButton.Close
                };
                DialogTools.ShowDialog(dialog);
            }
            else
            {
                Values.MessagesList.Add($"你说：{MessageTextBox.Text}");
                Values.TcpSocket.SendPacket(TcpSocket.PacketType.Message_Messages, MessageTextBox.Text);
            }

            MessageTextBox.Text = "";
            SendButton.IsEnabled = true;
            MessageTextBox.IsEnabled = true;
        }
    }
}
