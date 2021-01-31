using System;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace Client
{
    public partial class MainForm : Form
    {
        public delegate void UserListDelegate(UserListEventArgs e);
        public static event UserListDelegate UserListEvent;
        public static MyRSA rsa;
        static string password = GenerateKey(8);
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtPath.Text = openFileDialog1.FileName;
        }

        public static string GenerateKey(int length)
        {
            StringBuilder sb = new StringBuilder();
            Random r = new Random();
            for (int i = 0; i < length; i++)
            {
                sb.Append((char)r.Next(65, 91));
            }
            return sb.ToString();
        }

        TcpClient client;
        static NetworkStream stream;
        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                string fileContent = string.Empty;

                if (!File.Exists(txtPath.Text))
                {
                    MessageBox.Show("File does not exist!", "File path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (lstUsers.SelectedIndex == -1)
                {
                    MessageBox.Show("Select destination user first", "Destination user", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (FileStream fs = new FileStream(txtPath.Text, FileMode.Open, FileAccess.Read, FileShare.None))
                using (StreamReader sr = new StreamReader(fs))
                {
                    fileContent = sr.ReadToEnd();
                }

                byte[] to = System.Text.Encoding.UTF8.GetBytes((lstUsers.SelectedItem as User).Id + "@");

                // Translate the passed message into UTF8 and store it as a Byte array.
                Byte[] data = System.Text.Encoding.UTF8.GetBytes(fileContent);

                password = GenerateKey(8);

                Byte[] encryptedMsg = Encryption.AES_Encrypt(data, System.Text.Encoding.UTF8.GetBytes(password));


                User selectedUser = (lstUsers.SelectedItem as User);
                MyRSA rsa1 = new MyRSA(rsa.GetRSAParametersRepresentation(selectedUser.PublicKey));
                Byte[] encryptedPassword = rsa1.Encrypt(System.Text.Encoding.UTF8.GetBytes(password));

                byte[] CombinedArr = Combine(to, encryptedPassword, encryptedMsg);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(CombinedArr, 0, CombinedArr.Length);
                // Receive the TcpServer.response.

                MessageBox.Show("File has been sent to the server", "File sending", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show(string.Format("ArgumentNullException: {0}", ex));
            }
            catch (SocketException ex)
            {
                MessageBox.Show(string.Format("SocketException: {0}", ex));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception: {0}", ex));
            }
        }

        private static string GetFileName()
        {
            string newFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\NewFile.txt";

            int counter = 1;
            while (File.Exists(newFilePath))
                newFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\NewFile_" + counter++ + ".txt";

            return newFilePath;
        }

        private static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;

        }

        private void GetUsersList()
        {
            string MSG = "<GUL>";
            Byte[] data = System.Text.Encoding.UTF8.GetBytes(MSG);
            stream = client.GetStream();
            stream.Write(data, 0, data.Length);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtUsername.Text))
                {
                    MessageBox.Show("You must enter a username to connect!", "Connect", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                client = new TcpClient();

                client.Connect(txtIPAddress.Text, int.Parse(txtPort.Text));

                string MSG = string.Format("<USR>@{0}@{1}", txtUsername.Text, rsa.GetStringRepresentation(rsa.GetPublicKey()));

                // Translate the passed message into UTF8 and store it as a Byte array.
                Byte[] data = System.Text.Encoding.UTF8.GetBytes(MSG);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);


                btnConnect.Enabled = false;
                th.Start();
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show(string.Format("ArgumentNullException: {0}", ex));
            }
            catch (SocketException ex)
            {
                MessageBox.Show(string.Format("SocketException: {0}", ex));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception: {0}", ex));
            }
        }

        String ServerPublicKey = String.Empty;
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Close everything.
                client.Close();
                stream.Close();
            }
            catch { }
        }

        Thread th = new Thread(new ThreadStart(ReceivingFiles));

        private static void ReceivingFiles()
        {
            try
            {
                while (true)
                {
                    Byte[] bytes = new Byte[20480];
                    int i;
                    string data1;
                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        byte[] desArr = new byte[i];
                        Buffer.BlockCopy(bytes, 0, desArr, 0, i);
                        //byte[] decryptedData = Encryption.AES_Decrypt(desArr, System.Text.Encoding.UTF8.GetBytes(password));

                        // Translate data bytes to a UTF8 string.
                        data1 = System.Text.Encoding.UTF8.GetString(desArr);

                        string match = "<GUL_Response>";
                        if (data1.StartsWith(match))
                        {
                            string responseData = data1.Substring(data1.IndexOf(match) + match.Length);
                            List<User> lstUsers = new List<User>();

                            foreach (string item in responseData.Split(';'))
                                if (!string.IsNullOrEmpty(item))
                                    lstUsers.Add(new User(int.Parse(item.Split('#')[0].ToString()), item.Split('#')[1].ToString(), item.Split('#')[2].ToString()));

                            UserListEventArgs args = new UserListEventArgs(lstUsers);
                            if (UserListEvent != null)
                                UserListEvent.Invoke(args);
                        }
                        else
                        {
                            byte[] encryptedPassword = new byte[256];
                            Buffer.BlockCopy(desArr, 0, encryptedPassword, 0, 256);
                            byte[] encryptedData = new byte[desArr.Length - encryptedPassword.Length];
                            Buffer.BlockCopy(desArr, 256, encryptedData, 0, desArr.Length - 256);

                            byte[] decryptedPassword = rsa.Decrypt(encryptedPassword);

                            byte[] decryptedData = Encryption.AES_Decrypt(encryptedData, decryptedPassword);
                            data1 = System.Text.Encoding.UTF8.GetString(decryptedData);
                            string filePath = string.Empty;
                            using (FileStream fs = new FileStream(filePath = GetFileName(), FileMode.Create, FileAccess.Write, FileShare.Read))
                            using (StreamWriter sw = new StreamWriter(fs))
                            {
                                sw.WriteLine(data1);
                            }

                            MessageBox.Show(string.Format(@"File saved at ""{0}""", filePath));
                        }
                    }
                }
            }
            catch { }
        }

        private void btnGetUsersList_Click(object sender, EventArgs e)
        {
            GetUsersList();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UserListEvent += MainForm_UserListEvent;
            rsa = new MyRSA();
        }

        private void MainForm_UserListEvent(UserListEventArgs e)
        {
            lstUsers.Invoke(new Action(() =>
            {
                lstUsers.Items.Clear();
                foreach (User user in e.lstUsers)
                {
                    if (user.Name != txtUsername.Text)
                        lstUsers.Items.Add(user);
                }
            }));
        }
    }
}
