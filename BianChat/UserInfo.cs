using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BianChat
{
    public class UserInfo
    {
        public int UID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public Uri ProfilePhotoUri { get; set; }
        public UserInfo[] FriendList { get; set; }
    }
}
