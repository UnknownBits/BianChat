using Client.Module;
using ModernWpf;
using System.Windows;
using System.Windows.Controls;

namespace Client.Views
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : System.Windows.Controls.Page
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ThemeSwitch_Button_Click(object sender, RoutedEventArgs e)
        {
            var tm = ThemeManager.Current;
            if (tm.ActualApplicationTheme == ApplicationTheme.Dark) tm.ApplicationTheme = ApplicationTheme.Light;
            else tm.ApplicationTheme = ApplicationTheme.Dark;
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            if ((string)((TabItem)RootTab.SelectedItem).Tag == "NetworkSettings")
            {
                if (Values.TcpSocket != null && Values.TcpSocket.IsLogin)
                {
                    Values.TcpSocket.PingPackageReceive += (s, e) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Node1_Status.Content = e.Ping;
                        });
                    };
                    Values.TcpSocket.SendPacket(TcpSocket.PacketType.Ping);
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        Node1_Status.Content = "Error";
                    });
                    DialogTools.ShowDialogWithCloseButton("警告", "未连接到服务器");
                }
            }
        }
    }
}
