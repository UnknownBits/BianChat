using Client.Module;
using ModernWpf;
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
