using System.Net.Sockets;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Security.Cryptography;

namespace Client_Console
{
    internal class Program
    {
        public static bool state = false;
        static void Main(string[] args)
        {
            while (!state)
            {
                Console.WriteLine("Please input your account");
                string account = Console.ReadLine();
                string password = Console.ReadLine();
                if (account != null)
                {
                    state = true;

                }
            }
            Start();
        }
        static void Start()
        {
            AdvancedTcpClient advancedTcpClient = new AdvancedTcpClient();
            advancedTcpClient.Connect("test.biannetwork.top", 5000);
            advancedTcpClient.BeginReceive();
            advancedTcpClient.DataReceived += ((client, data) =>
            {
                Console.WriteLine($"接收到信息：{Encoding.UTF8.GetString(data.ReceivedData)}");
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
        /// <summary>
        /// SHA256加密
        /// </summary>
        /// <param name="strIN">要加密的string字符串</param>
        /// <returns>SHA256加密之后的密文</returns>
        public static string GetHash(string strIN)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(strIN));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                var sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }
        public static bool VerifyHash(string strIN,string hash)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                // Hash the input.
                var hashOfInput = GetHash(strIN);

                // Create a StringComparer an compare the hashes.
                StringComparer comparer = StringComparer.OrdinalIgnoreCase;

                return comparer.Compare(hashOfInput, hash) == 0;
            }
        }
    }
}