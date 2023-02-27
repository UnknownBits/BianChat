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
        public UserInfo UserInfo { get; private set; } = new UserInfo();
        public UserInfo[] FriendList { get; private set; }
        public ChatClient Client { get; private set; }

        public AccountProfile()
        {
            Client = new ChatClient();
            Client.Disconnected += delegate
            {
                Connected = false;
            };
            Client.LoginCompleted += (s, e) =>
            {
                if (e.LoginState == ChatClient.LoginCompletedEventArgs.State.Success)
                {
                    UserInfo.Username = e.UserInfo.Username;
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
                Connected = false;
            }
        }

        public UserInfo GetUserInfo(int uid)
        {
            if (Connected)
            {
                Client.GetValue();
            }
        }

        public void EditFriendList(List<UserInfo> users)
        {
            if (Connected)
            {
                StringBuilder sb = new StringBuilder();
                foreach (UserInfo user in users)
                {
                    sb.Append(user.UID);
                    sb.Append('^');
                }
                Client.ChangeProfile(ChatClient.ValuesType.FriendList, sb.ToString().Trim('^'));
            }
        }
    }
}
