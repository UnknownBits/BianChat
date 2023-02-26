using BianChat.Tools;
using BianChat.Views;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace BianChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += delegate
            {
                Initialize();
            };
        }

        private void Initialize()
        {
            PublicValues.UIDispatcher = Dispatcher;
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            Type navigatePage;
            switch (selectedItem.Tag)
            {
                case "Home":
                    navigatePage = typeof(HomePage);
                    break;
                case "Chat":
                    navigatePage = typeof(ChatPage);
                    break;
                case "Account":
                    navigatePage = typeof(LoginPage);
                    break;
                case "Settings":
                    navigatePage = typeof(Settings);
                    break;
                default:
                    goto case "Home";
            }
            NavigateToPage(navigatePage, args.RecommendedNavigationTransitionInfo);
        }

        public void NavigateToPage(Type pageType, NavigationTransitionInfo transInfo = null)
            => RootFrame.Navigate(pageType, null, transInfo);
    }
}
