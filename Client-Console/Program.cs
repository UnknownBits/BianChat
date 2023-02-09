using System.Net.Sockets;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Client_Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AdvancedTcpClient advancedTcpClient = new AdvancedTcpClient();
            advancedTcpClient.Connect("127.0.0.1",911);
            advancedTcpClient.BeginReceive();
            advancedTcpClient.DataReceived += ((client, args) => {
                switch (args.ReceivedData[0])
                {
                    // 公告
                    case 9:
                        string notice = Encoding.UTF8.GetString(args.ReceivedData, 1, args.ReceivedData.Length - 1);
                        Console.WriteLine(notice);
                        break;

                    // 消息
                    case 1:
                        string message = Encoding.UTF8.GetString(args.ReceivedData, 1, args.ReceivedData.Length - 1);
                        Console.WriteLine($"接收到信息：{message}");
                        break;
                }
            });

            while (true)
            {
                string a = Console.ReadLine();
                if (a != null)
                {
                    advancedTcpClient.Send(a);
                }
                else
                {
                    Console.WriteLine("不能发送空字符");
                }
            }
        }

       
    }
}