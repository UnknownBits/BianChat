using System;
using Client.Module;
using System.Windows;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Values.MainWindow = this;
        }

        private void RootNavigation_Loaded(object sender, RoutedEventArgs e)
        {
            Values.UIDispatcher = Dispatcher;
            RootNavigation.SelectedItem = RootNavigation.MenuItems[0];
        }

        private void RootNavigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            Type navigatePage;
            switch (((NavigationViewItem)args.SelectedItem).Name)
            {
                case "Home":
                    navigatePage = typeof(Views.HomePage);
                    break;
                case "Chat":
                    navigatePage = typeof(Views.ChatPage);
                    break;
                case "Account":
                    if (Values.TcpSocket != null && Values.TcpSocket.IsLogin) navigatePage = typeof(Views.AccountPage);
                    else navigatePage = typeof(Views.LoginPage);
                    break;
                case "Settings":
                    navigatePage = typeof(Views.Settings);
                    break;
                default:
                    goto case "Home";
            }
            RootFrame.SourcePageType = navigatePage;
        }
    }
}
