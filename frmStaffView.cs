using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using RM;
using RM.Models;
using Guna.UI2.WinForms;

namespace RM.Views
{
    public partial class frmStaffView : SampleView
    {
        public frmStaffView()
        {
            InitializeComponent();
            LoadStaff();
            
            // Set form properties for embedding
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;
            
            // Set the header text
            this.label2.Text = "Staff List";
            
            // Add cell click event for edit and delete buttons
            guna2DataGridView1.CellClick += Guna2DataGridView1_CellClick;
        }

        private void frmStaffView_Load(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("[DEBUG] frmStaffView_Load: Starting to load staff data");
                
                // Load staff data
                LoadStaff();
                
                // Set up the search button click event
                btnSearch.Click += BtnSearch_Click;
                
                // Set up the search text changed event with delay
                txtSearch.TextChanged += TxtSearch_TextChanged;
                
                Console.WriteLine("[DEBUG] frmStaffView_Load: Successfully loaded staff data");
            }
            catch (Exception ex)
            {
                string errorDetails = $"Error in frmStaffView_Load: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                Console.WriteLine($"[ERROR] {errorDetails}");
                MessageBox.Show(errorDetails, "Error Loading Staff View", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // No need for InitializeDataGridView as we're setting up columns in LoadStaff

        private void LoadStaff(string searchText = "")
        {
            try
            {
                Console.WriteLine($"[DEBUG] LoadStaff: Starting to load staff data. Search text: '{searchText}'");

                // Clear existing data, but DO NOT clear columns!
                guna2DataGridView1.DataSource = null;
                guna2DataGridView1.Rows.Clear();
                // Remove this line: guna2DataGridView1.Columns.Clear();

                Console.WriteLine("[DEBUG] LoadStaff: Cleared existing grid data");


                // Get staff data from database
                Console.WriteLine("[DEBUG] LoadStaff: Calling DatabaseHelper.GetStaff");
                DataTable dt = DatabaseHelper.GetStaff(searchText);



                if (dt.Rows.Count > 0)
                {
                    // Create a new DataTable with the expected column structure
                    var displayTable = new DataTable();
                    displayTable.Columns.Add("Sno", typeof(int));
                    displayTable.Columns.Add("ID", typeof(int));
                    displayTable.Columns.Add("Name", typeof(string));
                    displayTable.Columns.Add("Username", typeof(string));
                    displayTable.Columns.Add("Role", typeof(string));
                    
                    // Make sure the DataGridView has the correct columns
                    if (guna2DataGridView1.Columns.Count == 0)
                    {
                        guna2DataGridView1.AutoGenerateColumns = false;
                        
                        // Add columns if they don't exist
                        if (!guna2DataGridView1.Columns.Contains("Sno"))
                            guna2DataGridView1.Columns.Add("Sno", "S.No");
                        if (!guna2DataGridView1.Columns.Contains("ID"))
                            guna2DataGridView1.Columns.Add("ID", "ID");
                        if (!guna2DataGridView1.Columns.Contains("Name"))
                            guna2DataGridView1.Columns.Add("Name", "Name");
                        if (!guna2DataGridView1.Columns.Contains("Username"))
                            guna2DataGridView1.Columns.Add("Username", "Username");
                        if (!guna2DataGridView1.Columns.Contains("Role"))
                            guna2DataGridView1.Columns.Add("Role", "Role");
                            
                        // Hide ID column
                        guna2DataGridView1.Columns["ID"].Visible = false;
                    }
                    
                    // Add data to the display table
                    int sno = 1;
                    foreach (DataRow row in dt.Rows)
                    {
                        var newRow = displayTable.NewRow();
                        newRow["Sno"] = sno++;
                        newRow["ID"] = row["ID"];
                        newRow["Name"] = row["Name"];
                        newRow["Username"] = row["Username"];
                        newRow["Role"] = row["Role"];
                        displayTable.Rows.Add(newRow);
                    }
                    
                    // Set the data source
                    Console.WriteLine("[DEBUG] LoadStaff: Setting data source for grid");
                    guna2DataGridView1.DataSource = displayTable;
                    Console.WriteLine("[DEBUG] LoadStaff: Data source set successfully");
                    
                    // Configure column display
                    foreach (DataGridViewColumn column in guna2DataGridView1.Columns)
                    {
                        column.DataPropertyName = column.Name;
                        column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    
                    // Configure appearance
                    if (guna2DataGridView1.Columns.Contains("Sno")) 
                        guna2DataGridView1.Columns["Sno"].Width = 60;
                    if (guna2DataGridView1.Columns.Contains("Name"))
                        guna2DataGridView1.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    if (guna2DataGridView1.Columns.Contains("Username"))
                        guna2DataGridView1.Columns["Username"].Width = 150;
                    if (guna2DataGridView1.Columns.Contains("Role"))
                        guna2DataGridView1.Columns["Role"].Width = 150;
                    
                    // Set cell padding
                    foreach (DataGridViewColumn col in guna2DataGridView1.Columns)
                    {
                        if (col is DataGridViewTextBoxColumn)
                        {
                            col.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
                        }
                    }
                }
                else
                {
                    // No data found
                    guna2DataGridView1.Rows.Clear();
                    guna2DataGridView1.Columns.Clear();
                    guna2DataGridView1.Rows.Add("No staff members found");
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error loading staff: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                Console.WriteLine($"[ERROR] {errorMsg}");
                MessageBox.Show(errorMsg, "Error Loading Staff", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Check if the click is on a valid row and column
                if (e.RowIndex < 0 || e.RowIndex >= guna2DataGridView1.Rows.Count || e.ColumnIndex < 0)
                    return;

                // Get the clicked column
                var column = guna2DataGridView1.Columns[e.ColumnIndex];
                
                // Get the staff ID from the hidden column
                var idCell = guna2DataGridView1.Rows[e.RowIndex].Cells["dgvid"];
                
                if (idCell?.Value == null || !int.TryParse(idCell.Value.ToString(), out int staffId))
                    return;
                    
                // Handle edit button click
                if (column.Name == "dgvEdit")
                {
                    EditStaff(staffId);
                }
                // Handle delete button click
                else if (column.Name == "dgvDelete")
                {
                    DeleteStaff(staffId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error handling cell click: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Timer searchTimer;

        private void EditStaff(int staffId)
        {
            try
            {
                using (var form = new FormStaffAdd(staffId))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        // Refresh the grid after editing
                        LoadStaff(txtSearch.Text.Trim());
                        
                        // Show success message
                        MessageBox.Show("Staff member updated successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error editing staff: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteStaff(int staffId)
        {
            try
            {
                // Confirm before deleting
                var result = MessageBox.Show("Are you sure you want to delete this staff member?", 
                    "Confirm Delete", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (DatabaseHelper.DeleteStaff(staffId))
                    {
                        // Refresh the grid after deleting
                        LoadStaff(txtSearch.Text.Trim());
                        
                        // Show success message
                        MessageBox.Show("Staff member deleted successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete staff member. Please try again.", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting staff: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                // If search box is empty, open add staff form
                using (var form = new FormStaffAdd())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        // Refresh the grid after adding
                        LoadStaff();
                        
                        // Show success message
                        MessageBox.Show("Staff member added successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
            {
                // Perform search
                LoadStaff(txtSearch.Text.Trim());
            }
        }
        
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            // Reset the timer on each keystroke
            if (searchTimer != null)
            {
                searchTimer.Stop();
                searchTimer.Dispose();
            }
            
            // Create a new timer with 500ms delay
            searchTimer = new Timer();
            searchTimer.Interval = 500;
            searchTimer.Tick += (s, args) =>
            {
                searchTimer.Stop();
                LoadStaff(txtSearch.Text.Trim());
            };
            searchTimer.Start();
        }
    }
}
