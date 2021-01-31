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
        static void Main(string[] args)
        {
            string password = "0000";
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 1922;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        byte[] desArr = new byte[i];
                        Buffer.BlockCopy(bytes, 0, desArr, 0, i);
                        byte[] decryptedData = Encryption.AES_Decrypt(desArr, System.Text.Encoding.ASCII.GetBytes(password)); 

                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(decryptedData);

                        string filePath = string.Empty;
                        using (FileStream fs = new FileStream(filePath = GetFileName(), FileMode.Create, FileAccess.Write, FileShare.Read))
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine(data);
                        }

                        Console.WriteLine(string.Format(@"File saved at ""{0}""", filePath));
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        private static string GetFileName()
        {
            string newFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\NewFile.txt";

            int counter = 1;
            while (File.Exists(newFilePath))
                newFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\NewFile_" + counter++ + ".txt";

            return newFilePath;
        }
    }
}
