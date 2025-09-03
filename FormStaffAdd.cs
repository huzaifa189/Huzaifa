using System;
using System.Data;
using System.Windows.Forms;
using RM;
using System.Text.RegularExpressions;

namespace RM.Views
{
    public partial class FormStaffAdd : SampleAdd
    {
        private int _staffId = 0;
        private bool _isEditMode = false;

        public FormStaffAdd()
        {
            InitializeComponent();
            this.Text = "Add New Staff";
            InitializeForm();
        }

        public FormStaffAdd(int staffId) : this()
        {
            _staffId = staffId;
            _isEditMode = true;
            this.Text = "Edit Staff";
            LoadStaffData();
        }

        private void InitializeForm()
        {
            // Set up form properties
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
            
            // Configure password field
            txtPassword.PasswordChar = '•';
            
            // Set up role dropdown
            cmbRole.Items.AddRange(new string[] { "Admin", "Manager", "Staff" });
            cmbRole.SelectedIndex = 2; // Default to Staff
            
            // Set focus to name field
            txtName.Focus();
        }

        private void LoadStaffData()
        {
            try
            {
                DataTable dt = DatabaseHelper.GetStaff();
                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToInt32(row["StaffID"]) == _staffId)
                    {
                        txtName.Text = row["Name"].ToString();
                        txtUsername.Text = row["Username"].ToString();
                        txtPassword.Text = ""; // Don't load password for security
                        txtPassword.PlaceholderText = "Leave blank to keep current password";
                        cmbRole.Text = row["Role"].ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading staff data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter staff name", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Please enter username", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return false;
            }
            
            // Only validate password for new staff or when changing password
            if (!_isEditMode || !string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                if (txtPassword.Text.Length < 6)
                {
                    MessageBox.Show("Password must be at least 6 characters long", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return false;
                }
            }
            
            if (string.IsNullOrWhiteSpace(cmbRole.Text))
            {
                MessageBox.Show("Please select a role", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbRole.Focus();
                return false;
            }
            
            return true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                bool result;
                
                if (_isEditMode)
                {
                    // Update existing staff
                    result = DatabaseHelper.UpdateStaff(
                        _staffId,
                        txtName.Text.Trim(),
                        txtUsername.Text.Trim(),
                        string.IsNullOrWhiteSpace(txtPassword.Text) ? null : txtPassword.Text.Trim(),
                        cmbRole.Text
                    );
                }
                else
                {
                    // Add new staff
                    result = DatabaseHelper.AddStaff(
                        txtName.Text.Trim(),
                        txtUsername.Text.Trim(),
                        txtPassword.Text.Trim(),
                        cmbRole.Text
                    );
                }

                if (result)
                {
                    string message = _isEditMode ? "updated" : "added";
                    MessageBox.Show($"Staff {message} successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save staff. Please try again.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Error saving staff: {errorMessage}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
