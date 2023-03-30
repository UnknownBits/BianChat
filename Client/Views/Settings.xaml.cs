using Client.Module;
using ModernWpf;
using System.Windows;
using System.Windows.Controls;

namespace Client.Views
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
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
        }

        private void ThemeSwitch_Button_Click(object sender, RoutedEventArgs e)
        {
            var tm = ThemeManager.Current;
            if (tm.ActualApplicationTheme == ApplicationTheme.Dark) tm.ApplicationTheme = ApplicationTheme.Light;
            else tm.ApplicationTheme = ApplicationTheme.Dark;
        }
    }
}
