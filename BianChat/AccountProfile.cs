using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BianChat
{
    public class AccountProfile
    {
        public static AccountProfile Current { get; private set; }

        public string Username { get; init; }
        public string ProfilePhotoUrl { get; init; }
        public AccountProfile[] FriendList { get; init; }
        public ChatClient Client { get; } = new ChatClient();

        public AccountProfile() { }

        public static void Connect(string username, string password)
        {
            Current = new AccountProfile();
            Current.Client.Connect(username, password);
        }
    }
}
