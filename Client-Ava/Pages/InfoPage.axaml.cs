using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;

namespace Client_Ava.Pages
{
    public partial class InfoPage : UserControl
    {
        public MainWindow MainWindow;
        public ObservableCollection<ListBoxItem> Notices = new ObservableCollection<ListBoxItem>();
        public InfoPage()
        {
            InitializeComponent();
            NoticeListBox.Items = Notices;
        }
        private void DisconnectButton_Click(object sender, RoutedEventArgs args)
        {
            MainWindow.Disconnect();
        }

        private void ChangePasswdButton_Click(object sender, RoutedEventArgs args)
        {

        }
    }
}
