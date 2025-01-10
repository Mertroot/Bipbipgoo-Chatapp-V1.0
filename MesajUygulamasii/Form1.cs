using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MesajUygulamasii
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Color btn = Color.SpringGreen;
        Color btr = Color.FromArgb(137, 140, 142);
        Color bb = Color.FromArgb(7, 94, 84);

        string constring = "Data Source=LAPTOP-D09NHKMN;Initial Catalog=Bipbipgo;Integrated Security=True;";


        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            panel1.BringToFront();
            buttonRegister.FillColor = btn;
            buttonLogin.FillColor = btr;
            panel3.BackColor = btn;
            panel4.BackColor = bb;


        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void guna2TextBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            buttonLogin.PerformClick();

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            panel2.BringToFront();
            buttonLogin.FillColor = btn;
            buttonRegister.FillColor = btr;
            panel3.BackColor = bb;
            panel4.BackColor = btn;

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void guna2CirclePictureBox1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Bir resim seçin"
            };

            // Kullanıcı bir dosya seçtiyse
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    guna2CirclePictureBox1.Image = Image.FromFile(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Resim yükleme sırasında bir hata oluştu: " + ex.Message);
                }
            }
        }

        public void guna2Button3_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(constring))
                {
                    con.Open();

                    string query = "SELECT COUNT(*) FROM Login WHERE email = @email AND password = @password";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@email", guna2TextBox1.Text.Trim());
                        cmd.Parameters.AddWithValue("@password", guna2TextBox2.Text.Trim());

                        int userExists = Convert.ToInt32(cmd.ExecuteScalar());

                        if (userExists > 0)
                        {
                            // Kullanıcıyı giriş yaptı olarak işaretle
                            string updateQuery = "UPDATE Login SET isLoggedIn = 1 WHERE email = @email";

                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                            {
                                updateCmd.Parameters.AddWithValue("@email", guna2TextBox1.Text.Trim());
                                updateCmd.ExecuteNonQuery();  // Güncelleme işlemini uygula
                            }

                            // Kullanıcının username'ini almak için sorgu
                            string getUsernameQuery = "SELECT username FROM Login WHERE email = @email";
                            string username = "";

                            using (SqlCommand getUsernameCmd = new SqlCommand(getUsernameQuery, con))
                            {
                                getUsernameCmd.Parameters.AddWithValue("@email", guna2TextBox1.Text.Trim());
                                SqlDataReader reader = getUsernameCmd.ExecuteReader();

                                if (reader.Read())
                                {
                                    username = reader["username"].ToString();  // Kullanıcı adını al
                                }
                            }

                            Form2 form2 = new Form2
                            {
                                Username = username
                            };
                           
                            form2.Show();
                            this.Hide();

                            MessageBox.Show("Giriş başarılı");
                        }
                        else
                        {
                            MessageBox.Show("Geçersiz e-posta veya şifre.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private int GenerateRandomPort()
        {
            Random rand = new Random();
            return rand.Next(10000, 65535);
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {

            // Rastgele portlar
            int ListenerPort = GenerateRandomPort();
            int SenderPort = GenerateRandomPort();

            // Fotoğraf kontrolü
            if (guna2CirclePictureBox1.Image == null)
            {
                MessageBox.Show("Fotoğraf seçilmelidir!");
                return;
            }

            // Kullanıcı adı kontrolü
            if (string.IsNullOrEmpty(guna2TextBox3.Text.Trim()))
            {
                errorProvider1.SetError(guna2TextBox3, "Kullanıcı adı gerekli.");
                return;
            }
            else { errorProvider1.SetError(guna2TextBox3, string.Empty); }

            // Email kontrolü
            if (string.IsNullOrEmpty(guna2TextBox4.Text.Trim()))
            {
                errorProvider1.SetError(guna2TextBox4, "Email gerekli.");
                return;
            }
            else { errorProvider1.SetError(guna2TextBox4, string.Empty); }

            // Şifre kontrolü
            if (string.IsNullOrEmpty(guna2TextBox5.Text.Trim()))
            {
                errorProvider1.SetError(guna2TextBox5, "Şifre gerekli.");
                return;
            }
            else { errorProvider1.SetError(guna2TextBox5, string.Empty); }

            // Şifre tekrar kontrolü
            if (string.IsNullOrEmpty(guna2TextBox6.Text.Trim()))
            {
                errorProvider1.SetError(guna2TextBox6, "Şifre tekrarını girin.");
                return;
            }
            else { errorProvider1.SetError(guna2TextBox6, string.Empty); }

            // Şifre eşleşmesi kontrolü
            if (guna2TextBox5.Text != guna2TextBox6.Text)
            {
                MessageBox.Show("Girilen iki şifre birbiriyle uyumsuzdur.");
                return;
            }

            // Veritabanı bağlantısı
            SqlConnection con = new SqlConnection(constring);
            string query = "INSERT INTO Login (username, email, password, confirmPassword, ListenerPort, SenderPort, images) " +
                           "VALUES (@username, @email, @password, @confirmPassword, @ListenerPort, @SenderPort, @images)";

            SqlCommand cmd = new SqlCommand(query, con);

            // Fotoğrafı byte dizisine dönüştürme
            MemoryStream ms = new MemoryStream();
            guna2CirclePictureBox1.Image.Save(ms, guna2CirclePictureBox1.Image.RawFormat);
            byte[] imageBytes = ms.ToArray();

            // Parametreleri ekleme
            cmd.Parameters.AddWithValue("@username", guna2TextBox3.Text);
            cmd.Parameters.AddWithValue("@email", guna2TextBox4.Text);
            cmd.Parameters.AddWithValue("@password", guna2TextBox5.Text);
            cmd.Parameters.AddWithValue("@confirmPassword", guna2TextBox6.Text);
            cmd.Parameters.AddWithValue("@ListenerPort", ListenerPort);
            cmd.Parameters.AddWithValue("@SenderPort", SenderPort);
            cmd.Parameters.AddWithValue("@images", imageBytes);  // Fotoğrafı veritabanına kaydetme

            try
            {
                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();  // Kayıt işlemi

                // Başarıyla kayıt olduktan sonra işlem yapılacak
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Başarıyla kayıt oldunuz.");
                    // Kayıttan sonra formu sıfırlama
                    guna2TextBox3.Clear();
                    guna2TextBox4.Clear();
                    guna2TextBox5.Clear();
                    guna2TextBox6.Clear();
                    guna2CirclePictureBox1.Image = null; // Fotoğrafı temizle
                }
                else
                {
                    MessageBox.Show("Kayıt sırasında hata oluştu.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
            finally
            {
                con.Close();
            }

        }
    }
}
