using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Client.Module
{
    internal static class Values
    {
        internal static TcpSocket TcpSocket;
        internal static MainWindow MainWindow;
        internal static Dispatcher UIDispatcher;
        internal static ObservableCollection<string> MessagesList = new ObservableCollection<string>();
    }
}
