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
            ComboBoxItem ?item = ServerSelectionComboBox.SelectedItem as ComboBoxItem;
            string ?ip = item?.Tag as string;
            if (ip != null)
            {
                MainWindow.Connect(Username.Text,Password.Text, ip);
            }
        }

        private void RegisterButton_Clicked(object sender, RoutedEventArgs e) => MainWindow.SwitchPage(MainWindow.PageType.RegisterPage);
    }
}
