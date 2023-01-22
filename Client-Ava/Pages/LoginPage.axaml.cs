using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Client_Ava.Pages
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
            FluentAvalonia.UI.Controls.ComboBoxItem item = ServerSelectionComboBox.SelectedItem as FluentAvalonia.UI.Controls.ComboBoxItem;
            string ip = item.Tag as string;

            MainWindow.Connect(Username.Text, ip);
        }
    }
}
