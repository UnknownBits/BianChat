using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BianChat
{
    public class AccountProfile
    {
        public static AccountProfile Current { get; set; }
        public static bool Connected { get; private set; } = false;
        public static bool IsLogin { get; private set; } = false;
        public UserInfo UserInfo { get; private set; } = new UserInfo();
        public ChatClient Client { get; private set; }

        public AccountProfile()
        {
            Client = new ChatClient();
            Client.Disconnected += delegate
            {
                IsLogin = false;
                Connected = false;
            };
            Client.ProfileChanged += delegate
            {
                UserInfo.Username = Client.GetValue(ChatClient.ValuesType.Username);
                string friends = Client.GetValue(ChatClient.ValuesType.FriendList);
                List<UserInfo> friendInfos = new List<UserInfo>();
                foreach (string friend in friends.Split('^'))
                {
                    int uid = int.Parse(friend);
                    friendInfos.Add(GetUserInfo(uid));
                }
                UserInfo.FriendList = friendInfos.ToArray();
                UserInfo.Email = Client.GetValue(ChatClient.ValuesType.Email);
            };
            Client.LoginCompleted += (s, e) =>
            {
                if (e.LoginState == ChatClient.LoginCompletedEventArgs.State.Success)
                {
                    UserInfo = e.UserInfo;
                    IsLogin = true;
                }
            };
        }

        public void Connect(string username, string password)
        {
            if (!Connected)
            {
                Connected = true;
                Client.Connect(username, password);
            }
        }

        public void Disconnect()
        {
            if (Connected)
            {
                Client.Disconnect();
            }
        }

        public UserInfo GetUserInfo(int uid)
        {
            if (Connected)
            {
                return Client.GetAccountInfo(uid);
            }
            throw new ArgumentException("未连接到服务器");
        }

        public void AddFriend(int uid)
        {
            if (Connected)
            {
                Client.ChangeProfile(ChatClient.ValuesType.FriendList, sb.ToString().Trim('^'));
            }
        }
    }
}
