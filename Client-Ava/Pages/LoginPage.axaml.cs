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
            ComboBoxItem item = (ComboBoxItem)ServerSelectionComboBox.SelectedItem;
            string ip = item.Tag as string;

            MainWindow.Connect(Username.Text, ip);
        }

        private void RegisterButton_Clicked(object sender, RoutedEventArgs e) => MainWindow.SwitchPage(MainWindow.PageType.RegisterPage);
    }
}
