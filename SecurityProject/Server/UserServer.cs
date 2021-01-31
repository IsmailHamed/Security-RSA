using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class UserServer
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public TcpClient Client { get; set; }
        public UserServer() { }
        public UserServer(int Id, string Name, TcpClient Client)
        {
            this.Id = Id;
            this.Name = Name;
            this.Client = Client;
        }
        public override string ToString()
        {
            return string.Format("[Id={0}, Name={1}]", this.Id, this.Name);
        }
    }

}
