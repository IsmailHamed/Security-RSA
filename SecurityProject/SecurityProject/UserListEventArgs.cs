using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client
{
    public class UserListEventArgs : EventArgs
    {
        public List<User> lstUsers = new List<User>();

        public UserListEventArgs(List<User> lstUsers)
        {
            this.lstUsers = lstUsers;
        }
    }
}
