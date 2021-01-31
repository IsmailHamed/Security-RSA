using System;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Client
{
    public partial class MainForm : Form
    {
        string password = "0000";
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtPath.Text = openFileDialog1.FileName;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                TcpClient client = new TcpClient();

                client.Connect(txtIPAddress.Text, int.Parse(txtPort.Text));

                string fileContent = string.Empty;

                if (!File.Exists(txtPath.Text))
                {
                    MessageBox.Show("File does not exist!", "File path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (FileStream fs = new FileStream(txtPath.Text, FileMode.Open, FileAccess.Read,FileShare.None))
                using (StreamReader sr = new StreamReader(fs))
                {
                    fileContent = sr.ReadToEnd();
                }

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(fileContent);

                Byte[] encryptedMsg = Encryption.AES_Encrypt(data, System.Text.Encoding.ASCII.GetBytes(password)); 

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(encryptedMsg, 0, encryptedMsg.Length);
                // Receive the TcpServer.response.

                // Close everything.
                client.Close();
                stream.Close();
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine("ArgumentNullException: {0}", ex);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("SocketException: {0}", ex);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }
    }
}
