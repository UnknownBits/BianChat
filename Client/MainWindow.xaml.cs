using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
