using System;
using System.Data;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using System.Data.SqlClient;

namespace RM
{
    public partial class login : Form
    {
        public login()
        {
            InitializeComponent();
            this.ActiveControl = txtUser;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.KeyPreview = true; // Enable form to receive key events
            this.KeyDown += Login_KeyDown; // Add key down event handler
        }

        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle Enter key press
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin.PerformClick();
                e.Handled = true;
            }
            // Handle Ctrl+N for new registration
            else if (e.Control && e.KeyCode == Keys.N)
            {
                btnRegister.PerformClick();
                e.Handled = true;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                string username = txtUser.Text.Trim();
                string password = BtnPass.Text;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Please enter both username and password.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (DatabaseHelper.ValidateUser(username, password))
                {
                    this.Hide();
                    using (MainForm mainForm = new MainForm(username))
                    {
                        mainForm.ShowDialog();
                    }
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password. Please try again or register.", 
                        "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            this.Hide();
            using (Register registerForm = new Register())
            {
                registerForm.ShowDialog();
            }
            this.Show();
        }

        private void login_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Application.Exit();
            }
        }        

        
    }
}
