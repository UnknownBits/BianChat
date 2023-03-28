using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using System;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static Module.TcpSocket? tcpSocket;
        internal static MainWindow mainWindow;
        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;
            NavigateToPage(typeof(Views.HomePage));
        }
        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            Type navigatePage;
            switch (selectedItem.Name)
            {
                case "Home":
                    navigatePage = typeof(Views.HomePage);
                    break;
                case "Chat":
                    navigatePage = typeof(Views.ChatPage);
                    break;
                case "Account":
                    if (tcpSocket != null && tcpSocket.IsLogin)
                    {
                        navigatePage = typeof(Views.AccountPage);
                    }
                    else
                    {
                        navigatePage = typeof(Views.LoginPage);
                    }
                    break;
                case "Settings":
                    navigatePage = typeof(Views.Settings);
                    break;
                default:
                    goto case "Home";
            }
            NavigateToPage(navigatePage, args.RecommendedNavigationTransitionInfo);
        }

        public void NavigateToPage(Type pageType, NavigationTransitionInfo transInfo = null)
        {
            Dispatcher.Invoke(() =>
            {
                if (transInfo == null) transInfo = new DrillInNavigationTransitionInfo();
                RootFrame.Navigate(pageType, null, transInfo);
            });
        }
    }
}
