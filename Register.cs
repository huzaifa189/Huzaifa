using System;
using System.Windows.Forms;

namespace RM
{
    public partial class Register : Form
    {
        public Register()
        {
            InitializeComponent();
            this.ActiveControl = txtUser1;
        }

        private void btnLogin1_Click(object sender, EventArgs e)
        {
            this.Close();
            login loginForm = new login();
            loginForm.Show();
        }

        private void btnRegister1_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("Register button clicked");
                string username = txtUser1.Text.Trim();
                string password = txtPass1.Text;
                string confirmPassword = txtPass1.Text; // Using the same field for now since there's no separate confirm password field

                Console.WriteLine($"Username: {username}, Password: {password}");

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
                {
                    MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (password != confirmPassword)
                {
                    MessageBox.Show("Passwords do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Console.WriteLine("Calling RegisterUser...");
                bool result = DatabaseHelper.RegisterUser(username, password);
                Console.WriteLine($"RegisterUser result: {result}");

                if (result)
                {
                    MessageBox.Show("Registration successful! Please login with your new credentials.", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                    login loginForm = new login();
                    loginForm.Show();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in btnRegister1_Click: {ex}");
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CheckView_CheckedChanged(object sender, EventArgs e)
        {
            // Toggle password visibility based on checkbox state
            txtPass1.UseSystemPasswordChar = !CheckView.Checked;
        }

        private void Register_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !this.Modal)
            {
                login loginForm = new login();
                loginForm.Show();
            }
        }
    }
}
