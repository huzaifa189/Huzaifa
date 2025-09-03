using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace RM
{
    public partial class MainForm : Form
    {
        private string _username;

        public MainForm(string username)
        {
            InitializeComponent();
            _username = username;
            label2.Text = $"User_{_username}";
            InitializeForm();
            
            btnHome_Click(null, null);
        }
        
        public MainForm()
        {
            InitializeComponent();
            InitializeForm();
        }
        
        private void InitializeForm()
        {
            this.FormClosing += MainForm_FormClosing;
            this.KeyPreview = true;
           
            foreach (Control control in this.Controls)
            {
                control.PreviewKeyDown += (s, e) => {
                    if (e.KeyCode == Keys.F) e.IsInputKey = true;
                };
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            
            if (keyData == Keys.F)
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                }
                else
                {
                    this.WindowState = FormWindowState.Maximized;
                }
                return true; 
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Hide();
            login loginForm = new login();
            loginForm.Show();
        }

        private void btnTestDb_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("\n[TEST] Testing database connection and staff data retrieval...");
                
                Console.WriteLine("[TEST] Testing database connection...");
                bool connectionOk = DatabaseHelper.TestConnection();
                Console.WriteLine($"[TEST] Database connection test: {(connectionOk ? "SUCCESS" : "FAILED")}");

                if (connectionOk)
                {
                    Console.WriteLine("\n[TEST] Retrieving staff data...");
                    DataTable dt = DatabaseHelper.GetStaff();
                    Console.WriteLine($"[TEST] Retrieved {dt.Rows.Count} staff records");

                    if (dt.Rows.Count > 0)
                    {
                        Console.WriteLine("\n[TEST] Sample staff data:");
                        for (int i = 0; i < Math.Min(3, dt.Rows.Count); i++)
                        {
                            Console.WriteLine($"  - ID: {dt.Rows[i]["ID"]}, " +
                                          $"Name: {dt.Rows[i]["Name"]}, " +
                                          $"Username: {dt.Rows[i]["Username"]}, " +
                                          $"Role: {dt.Rows[i]["Role"]}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[TEST] No staff records found in the database");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TEST ERROR] {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                MessageBox.Show($"Error testing database: {ex.Message}", "Test Failed", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Console.WriteLine("\n[TEST] Test completed. Check the Output window for details.");
                MessageBox.Show("Test completed. Check the Output window for details.", 
                    "Test Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                var result = MessageBox.Show("Are you sure you want to exit?", "Exit Application", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    Application.Exit();
                }
            }
        }

        private void btnexit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnmax_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                btnmax.Text = "□";
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                btnmax.Text = "❐";
            }
        }

        private void btnCat_Click(object sender, EventArgs e)
        {
            LoadFormInPanel(new RM.Views.CategoriesForm());
        }

        private void btnTb_Click(object sender, EventArgs e)
        {
            LoadFormInPanel(new RM.Views.TablesForm());
        }

        private void btnStf_Click(object sender, EventArgs e)
        {
            LoadFormInPanel(new RM.Views.frmStaffView());
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
          
            guna2Panel3.Controls.Clear();
            
            
            var welcomePanel = new Guna.UI2.WinForms.Guna2Panel();
            welcomePanel.Dock = DockStyle.Fill;
            welcomePanel.FillColor = Color.White;
            
       
            var welcomeLabel = new Label();
            welcomeLabel.Text = $"Welcome to Restaurant Management System\nUser: {_username}";
            welcomeLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            welcomeLabel.ForeColor = Color.FromArgb(50, 55, 89);
            welcomeLabel.TextAlign = ContentAlignment.MiddleCenter;
            welcomeLabel.Dock = DockStyle.Fill;
            welcomeLabel.AutoSize = false;
            
            welcomePanel.Controls.Add(welcomeLabel);
            guna2Panel3.Controls.Add(welcomePanel);
        }

        private void LoadFormInPanel(Form form)
        {
          
            guna2Panel3.Controls.Clear();
            
          
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            
          
            guna2Panel3.Controls.Add(form);
            
            
            form.Show();
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}

