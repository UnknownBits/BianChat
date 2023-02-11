using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Client_Ava.Pages
{
    public partial class RegisterPage : UserControl
    {
        public MainWindow MainWindow { get; set; }

        public RegisterPage()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SwitchPage(MainWindow.PageType.LoginPage);
        }

        private void RegisterButton_Clicked(object sender, RoutedEventArgs e)
        {
            string ip = ((ComboBoxItem)ServerSelectionComboBox.SelectedItem).Tag.ToString();
            MainWindow.Register(Username.Text, Password.Text, ip);
        }
    }
}
