using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal class Program
    {
        private static readonly Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        static void Main(string[] args)
        {
            Console.WriteLine("欢迎使用Bian Chat 急急国王版 v2.0");
            SQLite.CreateDatabase();
            Console.Write("输入监听端口号:");
            string? command = Console.ReadLine();
            if (!string.IsNullOrEmpty(command)) 
            {
                IPEndPoint iep = new IPEndPoint(IPAddress.Any, int.Parse(command));
                server.Bind(iep);
                server.Listen();

                Task.Run(() => { //消息线程
                    while (true)
                    {
                        Socket client = server.Accept();
                        TcpSocket newclient = new TcpSocket(client);
                        Thread newthread = new Thread(new ThreadStart(newclient.MessageService));
                        newthread.Start();
                    }
                });
                Console.WriteLine("等待客户机进行连接......");
                Console.ReadKey();
            }
        }
    }
}