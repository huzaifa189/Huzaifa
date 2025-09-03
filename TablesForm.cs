using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using RM;
using RM.Models;

namespace RM.Views
{
    public partial class TablesForm : SampleView
    {
        public TablesForm()
        {
            InitializeComponent();
            LoadTables();
            
            // Set form properties for embedding
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;
            
            // Add cell click event for edit and delete buttons
            guna2DataGridView1.CellClick += Guna2DataGridView1_CellClick;
        }
        
        private void TablesForm_Load(object sender, EventArgs e)
        {
            // You can put any initialization code here if needed.
            // For now, just ensure tables are loaded.
            LoadTables();
        }
        
        private void LoadTables()
        {
            LoadTablesData(DatabaseHelper.GetTables());
        }

        private void LoadTablesData(DataTable dt)
        {
            try
            {
                // Create a new DataTable with the expected column structure
                var displayTable = new DataTable();
                displayTable.Columns.Add("Sno", typeof(int));
                displayTable.Columns.Add("ID", typeof(int));
                displayTable.Columns.Add("TableNumber", typeof(string));
                displayTable.Columns.Add("Capacity", typeof(int));
                displayTable.Columns.Add("Status", typeof(string));
                
                int sno = 1;
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    var newRow = displayTable.NewRow();
                    newRow["Sno"] = sno++;
                    newRow["ID"] = row["TableID"];
                    newRow["TableNumber"] = row["TableNumber"];
                    newRow["Capacity"] = row["Capacity"];
                    var rawStatus = row["Status"] != DBNull.Value ? row["Status"].ToString() : string.Empty;
                    newRow["Status"] = NormalizeStatus(rawStatus);
                    displayTable.Rows.Add(newRow);
                }
                
                // Clear existing columns and recreate them with proper DataPropertyName
                guna2DataGridView1.Columns.Clear();
                guna2DataGridView1.AutoGenerateColumns = false;
                
                // Add columns with proper DataPropertyName
                guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "dgvSno",
                    HeaderText = "Sr#",
                    DataPropertyName = "Sno",
                    Width = 70,
                    ReadOnly = true
                });
                
                guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "dgvid",
                    HeaderText = "Id",
                    DataPropertyName = "ID",
                    Visible = false,
                    ReadOnly = true
                });
                
                guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "dgvTableNumber",
                    HeaderText = "Table Number",
                    DataPropertyName = "TableNumber",
                    ReadOnly = true
                });
                
                guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "dgvCapacity",
                    HeaderText = "Capacity",
                    DataPropertyName = "Capacity",
                    ReadOnly = true
                });
                
                guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "dgvStatus",
                    HeaderText = "Status",
                    DataPropertyName = "Status",
                    ReadOnly = true
                });
                
                guna2DataGridView1.Columns.Add(new DataGridViewImageColumn
                {
                    Name = "dgvEdit",
                    HeaderText = "Edit",
                    Image = Properties.Resources.pen_tool,
                    ImageLayout = DataGridViewImageCellLayout.Zoom,
                    Width = 50,
                    ReadOnly = false
                });
                
                guna2DataGridView1.Columns.Add(new DataGridViewImageColumn
                {
                    Name = "dgvDelete",
                    HeaderText = "Delete",
                    Image = Properties.Resources.recycle_bin,
                    ImageLayout = DataGridViewImageCellLayout.Zoom,
                    Width = 50,
                    ReadOnly = false
                });
                
                guna2DataGridView1.DataSource = displayTable;
                guna2DataGridView1.ReadOnly = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading tables: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string NormalizeStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "Available";
            var s = status.Trim().ToLowerInvariant();
            if (s == "booked") return "Reserved";
            if (s == "reserved") return "Reserved";
            if (s == "occupied" || s == "busy" || s == "inuse" || s == "in use") return "Occupied";
            if (s == "available" || s == "free" || s == "vacant") return "Available";
            // Default to Title Case-like formatting
            try { return char.ToUpper(status[0]) + status.Substring(1).ToLower(); } catch { return status; }
        }

        public override void btnSearch_Click(object sender, EventArgs e)
        {
            // Show add form when search button is clicked
            using (var form = new FormTableAdd())
            {
                form.ShowDialog(this.FindForm());
                LoadTables();
            }
        }

        public override void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Filter tables based on search text
            LoadTablesData(DatabaseHelper.GetTables(txtSearch.Text));
        }

        private void Guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Get the clicked column name
                string columnName = guna2DataGridView1.Columns[e.ColumnIndex].Name;
                
                // Get the table ID from the hidden ID column
                int tableId = 0;
                var idCell = guna2DataGridView1.Rows[e.RowIndex].Cells["dgvid"]; 
                if (idCell != null && idCell.Value != null && idCell.Value != DBNull.Value)
                {
                    int.TryParse(idCell.Value.ToString(), out tableId);
                }
                if (tableId <= 0)
                {
                    var drv = guna2DataGridView1.Rows[e.RowIndex].DataBoundItem as DataRowView;
                    if (drv != null && drv.Row.Table.Columns.Contains("ID"))
                    {
                        tableId = Convert.ToInt32(drv["ID"]);
                    }
                }
                if (tableId <= 0)
                {
                    MessageBox.Show("Invalid table id.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (columnName == "dgvEdit")
                {
                    // Edit button clicked
                    using (var form = new FormTableAdd(tableId))
                    {
                        form.ShowDialog(this.FindForm());
                        LoadTables();
                    }
                }
                else if (columnName == "dgvDelete")
                {
                    // Delete button clicked
                    if (MessageBox.Show("Are you sure you want to delete this table?", "Confirm Delete", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        if (DatabaseHelper.DeleteTable(tableId))
                        {
                            MessageBox.Show("Table deleted successfully!", "Success", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadTables();
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete table.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}
