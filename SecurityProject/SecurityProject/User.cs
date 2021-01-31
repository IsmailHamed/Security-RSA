using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PublicKey { get; set; }
        public User() { }
        public User(int Id, string Name, string PublicKey)
        {
            this.Id = Id;
            this.Name = Name;
            this.PublicKey = PublicKey;
        }
        public override string ToString()
        {
            return this.Name;
        }
    }

}
