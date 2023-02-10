using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Client.Pages
{
    public partial class LoginPage : UserControl
    {
        public MainWindow MainWindow { get; set; }

        public LoginPage()
        {
            InitializeComponent();
        }

        private void ConnectButton_Clicked(object sender, RoutedEventArgs e)
        {
            ComboBoxItem item = ServerSelectionComboBox.SelectedItem as ComboBoxItem;
            string ip = item.Tag as string;
        }

        private void RegisterButton_Clicked(object sender, RoutedEventArgs e) {
        }
    }
}
