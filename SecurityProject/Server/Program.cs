using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Server
{
    class Program
    {
        private static string password = "0000";
        private static TcpListener listener;
        public static void Start()
        {
            listener = new TcpListener(IPAddress.Any, 1922);
            listener.Start();
            Console.WriteLine("Listening...");
            StartAccept();

        }
        private static void StartAccept()
        {
            listener.BeginAcceptTcpClient(HandleAsyncConnection, listener);
        }

        static int LastUserId = 0;
        static List<User> lstUsers = new List<User>();
        private static void HandleAsyncConnection(IAsyncResult res)
        {
            StartAccept(); //listen for new connections again
            TcpClient client = listener.EndAcceptTcpClient(res);

            try
            {
                // Buffer for reading data
                Byte[] bytes = new Byte[20480];
                String data = null;



                data = null;

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    byte[] desArr = new byte[i];
                    Buffer.BlockCopy(bytes, 0, desArr, 0, i);
                    //byte[] decryptedData = Encryption.AES_Decrypt(desArr, System.Text.Encoding.UTF8.GetBytes(password));

                    // Translate data bytes to a UTF8 string.
                    data = System.Text.Encoding.UTF8.GetString(desArr/*decryptedData*/);

                    if (data.StartsWith("<USR>"))
                    {
                        Console.WriteLine(data.Split('@')[1] + " Connected!");
                        lstUsers.Add(new User(++LastUserId, data.Split('@')[1], client, data.Split('@')[2]));
                    }
                    else if (data.StartsWith("<GUL>"))
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("<GUL_Response>");
                        foreach (User user in lstUsers)
                        {
                            if (user.Client.Connected)
                                sb.Append(user.Id + "#" + user.Name + "#" + user.PublicKey + ";");
                        }

                        byte[] usersArray = System.Text.Encoding.UTF8.GetBytes(sb.ToString());

                        stream.Write(usersArray, 0, usersArray.Length);
                    }
                    else
                    {
                        int toId = int.Parse(data.Split('@')[0]);
                        Console.WriteLine("Sending to User: " + lstUsers.Where(x => x.Id == toId).FirstOrDefault());
                        byte[] fileContent = new byte[desArr.Length - (toId.ToString().Length + 1)];
                        Buffer.BlockCopy(desArr, toId.ToString().Length + 1, fileContent, 0, fileContent.Length);

                        User ToUser = lstUsers.Where(x => x.Id == toId).FirstOrDefault();

                        if (ToUser != null)
                        {
                            stream = ToUser.Client.GetStream();

                            // Send the message to the connected TcpServer. 
                            stream.Write(fileContent, 0, fileContent.Length);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Shutdown and end connection
                client.Close();
                Console.WriteLine("User was closed remotly.");
            }
        }

        static MyRSA rsa;
        static string ServerPublicKey = string.Empty;
        static void Main(string[] args)
        {
            try
            {
                rsa = new MyRSA();
                ServerPublicKey = rsa.GetStringRepresentation(rsa.GetPublicKey());

                Start();

                Console.ReadLine();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                listener.Stop();

                foreach (User user in lstUsers)
                {
                    // Shutdown and end connection
                    if (user.Client.Connected)
                        user.Client.Close();
                }
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }

    class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TcpClient Client { get; set; }
        public string PublicKey { get; set; }
        public User() { }
        public User(int Id, string Name, TcpClient Client, string PublicKey)
        {
            this.Id = Id;
            this.Name = Name;
            this.Client = Client;
            this.PublicKey = PublicKey;
        }
        public override string ToString()
        {
            return string.Format("[Id={0}, Name={1}]", this.Id, this.Name);
        }
    }
}
