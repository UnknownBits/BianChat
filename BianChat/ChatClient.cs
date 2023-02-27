using BianChat.Tools;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BianChat
{
    public class ChatClient
    {
        public class MessageReceivedEventArgs : EventArgs
        {
            public string Message { get; set; }
            public AdvancedTcpClient.PacketType MessageType { get; set; }
        }

        public class LoginCompletedEventArgs : EventArgs
        {
            public State LoginState { get; set; }
            public UserInfo UserInfo { get; set; }

            public enum State
            {
                Success,
                Failed_Account,
                Failed_Unknown
            }
        }

        public AdvancedTcpClient client;
        public bool Connected = false;

        public event EventHandler<AdvancedTcpClient.PingReceivedEventArgs> PingReceived = delegate { };
        public event EventHandler<LoginCompletedEventArgs> LoginCompleted = delegate { };
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<AdvancedTcpClient.DisconnectedEventArgs> Disconnected = delegate { };

        public ChatClient() { }

        public void Connect(string username, string password, string ip = "127.0.0.1:911")
        {
            // 断开已有连接
            client = new AdvancedTcpClient();
            client.PingReceived += (s, e) => { PingReceived(s, e); };
            client.DataReceived += DataReceivedCallback;
            client.Disconnected += (s, e) =>
            {
                Connected = false;

                Disconnected(s, e);
            };
            try
            {
                client.Connect(ip);
                Connected = true;
            }
            catch (Exception ex)
            {
                Connected = false;
                Disconnected(null, new AdvancedTcpClient.DisconnectedEventArgs { Exception = ex });
            }

            // 登录
            string passwdHash = HashTools.GetSHA256(password);
            client.SendPacket(AdvancedTcpClient.PacketType.Login,
                new byte[1] { 0 }.Concat(Encoding.UTF8.GetBytes(username + '^' + passwdHash)).ToArray());
        }

        private void DataReceivedCallback(object sender, AdvancedTcpClient.DataReceivedEventArgs e)
        {
            switch (e.DataType)
            {
                // 登录成功
                case AdvancedTcpClient.PacketType.State_Account_Success:
                    string[] account_info = Encoding.UTF8.GetString(e.ReceivedData).Split('^');
                    int uid = int.Parse(account_info[0]);
                    string username = account_info[1];
                    
                    LoginCompleted(null, new LoginCompletedEventArgs
                    {
                        LoginState = LoginCompletedEventArgs.State.Success
                    });
                    break;

                // 登录失败——账号或密码错误
                case AdvancedTcpClient.PacketType.Error_Account:
                    LoginCompleted(null, new LoginCompletedEventArgs { LoginState = LoginCompletedEventArgs.State.Failed_Account });
                    break;

                // 登录失败——未知错误
                case AdvancedTcpClient.PacketType.Error_Unknown:
                    LoginCompleted(null, new LoginCompletedEventArgs { LoginState = LoginCompletedEventArgs.State.Failed_Unknown });
                    break;

                // 接收到消息
                case AdvancedTcpClient.PacketType.Message:
                    MessageReceived(null, new MessageReceivedEventArgs
                    {
                        Message = Encoding.UTF8.GetString(e.ReceivedData),
                        MessageType = AdvancedTcpClient.PacketType.Message
                    });
                    break;

                // 接收到公告
                case AdvancedTcpClient.PacketType.Notice:
                    MessageReceived(null, new MessageReceivedEventArgs
                    {
                        Message = Encoding.UTF8.GetString(e.ReceivedData),
                        MessageType = AdvancedTcpClient.PacketType.Notice
                    });
                    break;
            }
        }

        public void SendMessage(string message)
            => client.SendPacket(AdvancedTcpClient.PacketType.Message, Encoding.UTF8.GetBytes(message));

        public string GetValue(int uid, ValuesType type)
        {
            string result = null;
            EventHandler<AdvancedTcpClient.DataReceivedEventArgs> action = (s, e) =>
            {
                if (e.DataType == AdvancedTcpClient.PacketType.Get_Value_Result)
                {
                    result = Encoding.UTF8.GetString(e.ReceivedData);
                }
            };
            client.DataReceived += action;
            client.SendPacket(AdvancedTcpClient.PacketType.Get_Value, BitConverter.GetBytes(uid));
            while (result == null) ;
            client.DataReceived -= action;
            return result;
        }

        public UserInfo GetAccountInfo(int uid)
        {
            UserInfo userInfo = null;
            EventHandler<AdvancedTcpClient.DataReceivedEventArgs> action = (s, e) =>
            {
                if (e.DataType == AdvancedTcpClient.PacketType.Get_Account_Info_Result)
                {
                    UserInfo info1 = new UserInfo();
                    string[] infos = Encoding.UTF8.GetString(e.ReceivedData).Split('^');
                    userInfo.UID = uid;
                    userInfo.Username = infos[0];
                    userInfo.Email = infos[1];
                    userInfo.QQ = infos[2];
                    userInfo = info1;
                }
            };
            client.DataReceived += action;
            client.SendPacket(AdvancedTcpClient.PacketType.Get_Account_Info, BitConverter.GetBytes(uid));
            while (userInfo == null) ;
            client.DataReceived -= action;
            return userInfo;
        }

        public void ChangeProfile(ValuesType type, string value)
            => client.SendPacket(AdvancedTcpClient.PacketType.Update_Value,
                new byte[1] { (byte)type }.Concat(Encoding.UTF8.GetBytes(value)).ToArray());

        public enum ValuesType
        {
            Username,
            Password,
            FriendList,
            Email,
            QQ
        }

        public void Disconnect()
        {
            client.Dispose();
        }
    }
}
