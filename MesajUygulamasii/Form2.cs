using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.IO;

namespace MesajUygulamasii
{
    public partial class Form2 : Form
    {
        private string _username;
        public string Username { get; set; }
        public Form2()
        {
            InitializeComponent();
            ProfilResim(kullaniciad);
        }
        public delegate void SendCount(int data_count);
        Socket serverSocket;
        string kullaniciad = "";
        IPEndPoint client;
        byte[] dataStream = new byte[1024];
        string constring = "Data Source=LAPTOP-D09NHKMN;Initial Catalog=Bipbipgo;Integrated Security=True;";

        private void ProfilResim(string username)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(constring))
                {
                    connection.Open();
                    string query = "SELECT images FROM Login WHERE username = @username";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);

                        object imageData = command.ExecuteScalar();
                        if (imageData != null && imageData != DBNull.Value)
                        {
                            byte[] imageBytes = (byte[])imageData;
                            using (MemoryStream ms = new MemoryStream(imageBytes))
                            {
                                guna2CirclePictureBox1.Image = Image.FromStream(ms);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadUsersToComboBox()
        {
            try
            {
                comboBox1.Items.Clear();  // Önceki öğeleri temizleyelim.
                using (SqlConnection con = new SqlConnection(constring))
                {
                    con.Open();
                    string query = "SELECT username FROM Login WHERE isLoggedIn = 1";  // Sadece aktif kullanıcılar
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            comboBox1.Items.Add(reader["username"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kullanıcılar yüklenirken hata oluştu: " + ex.Message);
            }
        }

        private (int ListenerPort, int SenderPort) GetPortsFromDatabase(string username)
        {
            int listenerPort = 0;
            int senderPort = 0;

            try
            {
                using (SqlConnection con = new SqlConnection(constring))
                {
                    con.Open();
                    string query = "SELECT ListenerPort, SenderPort FROM Login WHERE username = @username";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", username);

                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            listenerPort = reader.GetInt32(0);  // ListenerPort
                            senderPort = reader.GetInt32(1);    // SenderPort
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Port bilgileri alınırken hata oluştu: " + ex.Message);
            }

            return (listenerPort, senderPort);
        }

        void Server_Start(int listenerPort, int senderPort)
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                string localIP = IpAddressAl();  // Yerel IP'yi alıyoruz

                // **ListenerPort** üzerinden dinleme başlatıyoruz
                IPEndPoint server = new IPEndPoint(IPAddress.Parse(localIP), listenerPort);  // Dinleme portu
                serverSocket.Bind(server);

                // Mesaj gönderecek olan port
                client = new IPEndPoint(IPAddress.Parse(localIP), senderPort);  // Mesaj gönderecek port
                EndPoint epSender = (EndPoint)client;

                // Veri almayı başlatıyoruz
                serverSocket.BeginReceiveFrom(dataStream, 0, dataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(ReceiveData), epSender);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Server başlatılırken hata oluştu: " + ex.Message);
            }

        }

        void ReceiveData(IAsyncResult asyncResult)
        {
            try

            {

                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);

                EndPoint epSender = (EndPoint)clients;


                int bytecount = serverSocket.EndReceiveFrom(asyncResult, ref epSender);

                this.BeginInvoke(new SendCount(InsertValue), new object[] { bytecount });


                serverSocket.BeginReceiveFrom(dataStream, 0, dataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(this.ReceiveData), epSender);

            }

            catch (Exception ex)

            {

                MessageBox.Show("Veri alırken hata oluştu: " + ex.Message);

            }
        }

        public void InsertValue(int data_count)
        {
            byte[] bytedata = new byte[data_count];

            for (int i = 0; i < data_count; i++)

            {

                bytedata[i] = dataStream[i];

            }


            txt_recieve.Text += Encoding.UTF8.GetString(bytedata) + "\n";
        }



        private void guna2Button1_Click(object sender, EventArgs e)
        {
        }

        void SendData(IAsyncResult asyncResult)
        {
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            LoadUsersToComboBox();
            label2.Text = $"Merhaba {Username}";
            ProfilResim(Username);

        }

        private void btn_send_Click(object sender, EventArgs e)
        {

            string message = $"{Username}: {txt_sendData.Text}";
            txt_recieve.Text += "You: " + txt_sendData.Text + "\n";

            byte[] Packet = Encoding.UTF8.GetBytes(message);

            // Mesajı doğru portu kullanarak gönderiyoruz
            serverSocket.BeginSendTo(Packet, 0, Packet.Length, SocketFlags.None, (EndPoint)client, new AsyncCallback(SendData), (EndPoint)client);
            txt_sendData.Text = "";
        }
        private string IpAddressAl()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Yerel IP adresi bulunamadı!");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedUser = comboBox1.SelectedItem.ToString();

                if (string.IsNullOrEmpty(selectedUser))
                {
                    MessageBox.Show("Lütfen geçerli bir kullanıcı seçin.");
                    return;
                }

                // Mevcut kullanıcının ve seçilen kullanıcının port bilgilerini alıyoruz

                var (selectedListenerPort, selectedSenderPort) = GetPortsFromDatabase(selectedUser);
                var (currentListenerPort, currentSenderPort) = GetPortsFromDatabase(Username);

                // Port bilgilerini kontrol ediyoruz

                if (selectedListenerPort == 0 || selectedSenderPort == 0 ||
                    currentListenerPort == 0 || currentSenderPort == 0)
                {
                    MessageBox.Show("Port bilgileri alınamadı.");
                    return;
                }

                // Çaprazlama işlemi: 
                // Mevcut kullanıcının SenderPort'u, seçilen kullanıcının ListenerPort'una bağlanır
                // Seçilen kullanıcının SenderPort'u, mevcut kullanıcının ListenerPort'una bağlanır

                int newCurrentSenderPort = selectedListenerPort; // Mevcut kullanıcının yeni SenderPort'u

                int newSelectedListenerPort = currentSenderPort; // Seçilen kullanıcının yeni ListenerPort'u


                // Port bilgilerini güncelleyerek kullanıyoruz

                // Server başlatılır: Mevcut kullanıcı ListenerPort'unda dinler ve SenderPort'undan gönderir

                Server_Start(currentListenerPort, newCurrentSenderPort);
                MessageBox.Show($"{Username} ve {selectedUser} başarıyla bağlandı!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
      
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}

