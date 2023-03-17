using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BianChat.Tools
{
    public static class PublicValues
    {
        public static Dispatcher UIDispatcher;
        public static MainWindow MainWindow;
        public static ChatClient Client = new ChatClient();
        public static Dictionary<string, List<ModernWpf.Controls.ListViewItem>> UserMessagePairs = new Dictionary<string, List<ModernWpf.Controls.ListViewItem>>();
    }
}
