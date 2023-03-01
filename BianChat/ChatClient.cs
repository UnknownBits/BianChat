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
        public event EventHandler<bool> ProfileChanged = delegate { };
        public event EventHandler<UserInfo> GotAccountInfo = delegate { };
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
                    string[] account_info = Encoding.UTF8.GetString(e.ReceivedData).Split('|');
                    int uid = int.Parse(account_info[0]);
                    string username = account_info[1];
                    string email = account_info[2];
                    string qq = account_info[3];
                    string[] friendsUID = account_info[4].Split('^');
                    List<UserInfo> friends = new List<UserInfo>();

                    // 获取好友信息
                    foreach (string friend in friendsUID)
                    {
                        if (friend == "") continue;
                        int friendUid = int.Parse(friend);
                        friends.Add(GetAccountInfo(friendUid));
                    }
                    
                    LoginCompleted(null, new LoginCompletedEventArgs
                    {
                        LoginState = LoginCompletedEventArgs.State.Success,
                        UserInfo = new UserInfo
                        {
                            UID = uid,
                            Username = username,
                            Email = email,
                            QQ = qq,
                            FriendList = friends.ToArray()
                        }
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
                // 用户档案信息更改
                case AdvancedTcpClient.PacketType.Update_Value_Result:
                    ProfileChanged(null, e.ReceivedData[0] == 0 ? false : true);
                    break;
                // 获取用户档案
                case AdvancedTcpClient.PacketType.Get_Account_Info_Result:
                    string[] infos = Encoding.UTF8.GetString(e.ReceivedData).Split('^');
                    UserInfo info1 = new UserInfo();
                    info1.UID = int.Parse(infos[0]);
                    if (e.ReceivedData.Length == 1) throw new ArgumentException($"找不到 UID 为 {info1.UID} 用户");
                    info1.Username = infos[1];
                    info1.Email = infos[2];
                    info1.QQ = infos[3];
                    GotAccountInfo(null, info1);
                    break;
            }
        }

        public void SendMessage(string message)
            => client.SendPacket(AdvancedTcpClient.PacketType.Message, Encoding.UTF8.GetBytes(message));

        public string GetValue(int uid, ValuesType type)
        {
            string result = null;
            client.DataReceived += (s, e) =>
            {
                if (e.DataType == AdvancedTcpClient.PacketType.Get_Value_Result)
                {
                    if (e.ReceivedData.Length == 1) throw new ArgumentException($"找不到 UID 为 {uid} 的用户");
                    result = Encoding.UTF8.GetString(e.ReceivedData);
                }
            };
            client.SendPacket(AdvancedTcpClient.PacketType.Get_Value, new byte[1] { (byte)type }
            .Concat(BitConverter.GetBytes(uid)).ToArray());
            while (result == null) Task.Delay(10).Wait();
            return result;
        }

        public UserInfo GetAccountInfo(int uid)
        {
            UserInfo userInfo = null;
            GotAccountInfo += (s, e) =>
            {
                userInfo = e;
            };
            bool result = client.SendPacket(AdvancedTcpClient.PacketType.Get_Account_Info, BitConverter.GetBytes(uid));
            while (userInfo == null) Task.Delay(10).Wait();
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
