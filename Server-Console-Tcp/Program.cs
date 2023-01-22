using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server_Console_Tcp
{
    internal class Program
    {
        private static List<TcpClient> clients = new List<TcpClient>();

        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(System.Net.IPAddress.Any, 911);

            listener.Start();
            Console.WriteLine("等待客户端连接...");
            listener.BeginAcceptTcpClient(AcceptCallback, listener);

            string input = null;
            while (input != "exit")
            {
                input = Console.ReadLine();
            }
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);
            clients.Add(client);
            Console.WriteLine($"{(IPEndPoint)client.Client.RemoteEndPoint} 已连接，当前连接数 {clients.Count}");

            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        byte[] buffer = new byte[512];
                        int size = client.Client.Receive(buffer);
                        if (size <= 0)
                        {
                            throw new Exception();
                        }
                        Array.Resize(ref buffer, size);

                        Console.WriteLine($"收到数据：{Encoding.UTF8.GetString(buffer)}");

                        lock (clients)
                        {
                            foreach (var client1 in clients)
                            {
                                client1.Client.Send(buffer);
                            }
                        }
                    }
                    catch
                    {
                        clients.Remove(client);
                        client.Close();
                        Console.WriteLine($"客户端已断开连接，当前连接数 {clients.Count}");
                        break;
                    }
                }
            });

            listener.BeginAcceptTcpClient(AcceptCallback, listener);
        }
    }
}