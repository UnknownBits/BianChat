using BianChat.Models;
using ModernWpf.Controls;
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

namespace BianChat.Views
{
    /// <summary>
    /// AccountPage.xaml 的交互逻辑
    /// </summary>
    public partial class AccountPage : System.Windows.Controls.Page
    {
        public AccountPage()
        {
            InitializeComponent();

            if (AccountProfile.Current == null)
            {
                LoginDialogPage page = new LoginDialogPage();
                new ContentDialog
                {
                    Content = page,
                    Title = "登录",
                    PrimaryButtonText = "确定",
                    PrimaryButtonCommand = new CommandModel(new Predicate<object>((obj) => { return true; }), (obj) =>
                    {
                        
                    }),
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
        }
    }
}
