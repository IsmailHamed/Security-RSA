using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Client
{
    [Serializable]
    public class User
    {

        public int Id { get; set; }
        public string Name { get; set; }

        public bool status { get; set; }

        public RSAParameters pubKey { get; set; }

        public RSAParameters privKey { get; set; }

        public User() { }
        public User(int Id, string Name, RSAParameters pubKey, RSAParameters privKey)
        {
            this.Id = Id;
            this.Name = Name;
            this.privKey = privKey;
            this.pubKey = pubKey;
        }
        public User(int Id, string Name, RSAParameters pubKey)
        {
            this.Id = Id;
            this.Name = Name;
            this.pubKey = pubKey;
        }
        public User(int Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
            this.pubKey = pubKey;
        }
        public override string ToString()
        {
            return this.Name;
        }

        
    }
    

}
