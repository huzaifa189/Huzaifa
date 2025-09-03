using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using RM;
using RM.Models;

namespace RM.Views
{
    public partial class CategoriesForm : SampleView
    {
          public CategoriesForm()
        {
            InitializeComponent();
            LoadCategories();
            
            // Set form properties for embedding
            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;
            
            // Add cell click event for edit and delete buttons
            guna2DataGridView1.CellClick += Guna2DataGridView1_CellClick;
        }
        private void CategoriesForm_Load(object sender, EventArgs e)
        {
            // You can put any initialization code here if needed.
            // For now, just ensure categories are loaded.
            LoadCategories();
        }
        private void LoadCategories()
        {
            LoadCategoriesData(DatabaseHelper.GetCategories());
        }

        private void LoadCategoriesData(DataTable dt)
        {
            try
            {
                // Create a new DataTable with the expected column structure
                var displayTable = new DataTable();
                displayTable.Columns.Add("Sno", typeof(int));
                displayTable.Columns.Add("ID", typeof(int));
                displayTable.Columns.Add("Name", typeof(string));
                displayTable.Columns.Add("Description", typeof(string));
                
                int sno = 1;
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    var newRow = displayTable.NewRow();
                    newRow["Sno"] = sno++;
                    newRow["ID"] = row["CategoryID"];
                    newRow["Name"] = row["CategoryName"];
                    newRow["Description"] = row["Description"] ?? "";
                    displayTable.Rows.Add(newRow);
                }
                
                // Clear existing columns and recreate them with proper DataPropertyName
                guna2DataGridView1.Columns.Clear();
                
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
                    Name = "dgvName",
                    HeaderText = "Name",
                    DataPropertyName = "Name",
                    ReadOnly = true
                });
                
                guna2DataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "dgvDescription",
                    HeaderText = "Description",
                    DataPropertyName = "Description",
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
                
                // Re-attach the cell click event handler after setting the data source
                guna2DataGridView1.CellClick -= Guna2DataGridView1_CellClick; // Remove any existing handlers
                guna2DataGridView1.CellClick += Guna2DataGridView1_CellClick; // Add the handler
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }





        public override void btnSearch_Click(object sender, EventArgs e)
        {
            // Show add form when search button is clicked
            using (var form = new FormcategoryAdd())
            {
                form.ShowDialog(this.FindForm());
                LoadCategories();
            }
        }

        public override void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Filter categories based on search text
            LoadCategoriesData(DatabaseHelper.GetCategories(txtSearch.Text));
        }

        private void Guna2DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Get the clicked column name
                string columnName = guna2DataGridView1.Columns[e.ColumnIndex].Name;
                
                // Get the category ID from the hidden ID column
                int categoryId = Convert.ToInt32(guna2DataGridView1.Rows[e.RowIndex].Cells["dgvid"].Value);

                if (columnName == "dgvEdit")
                {
                    // Edit button clicked
                    using (var form = new FormcategoryAdd(categoryId))
                    {
                        form.ShowDialog(this.FindForm());
                        LoadCategories();
                    }
                }
                else if (columnName == "dgvDelete")
                {
                    // Delete button clicked
                    if (MessageBox.Show("Are you sure you want to delete this category?", "Confirm Delete", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        if (DatabaseHelper.DeleteCategory(categoryId))
                        {
                            MessageBox.Show("Category deleted successfully!", "Success", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadCategories();
                        }
                    }
                }
            }
        }
    }
}
