using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Client.Pages
{
    public partial class InfoPage : UserControl
    {
        public InfoPage()
        {
            InitializeComponent();
        }
        private void DisconnectButton_Click(object sender, RoutedEventArgs args)
        {
            MainWindow.Disconnect();
        }
    }
}
