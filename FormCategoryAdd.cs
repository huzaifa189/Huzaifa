using System;
using System.Data;
using System.Windows.Forms;
using RM;

namespace RM.Views
{
    public partial class FormCategoryAdd : Form
    {
        private int _categoryId = 0;
        private bool _isEditMode = false;

        public FormCategoryAdd()
        {
            InitializeComponent();
            this.Text = "Add New Category";
        }

        public FormCategoryAdd(int categoryId) : this()
        {
            _categoryId = categoryId;
            _isEditMode = true;
            this.Text = "Edit Category";
            LoadCategoryData();
        }

        private void LoadCategoryData()
        {
            try
            {
                var dt = DatabaseHelper.GetCategories();
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    if (Convert.ToInt32(row["CategoryID"]) == _categoryId)
                    {
                        txtName.Text = row["CategoryName"].ToString();
                        txtDescription.Text = row["Description"] != DBNull.Value ? row["Description"].ToString() : "";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading category: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a category name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool result;
                if (_isEditMode)
                {
                    result = DatabaseHelper.UpdateCategory(_categoryId, txtName.Text.Trim(), txtDescription.Text.Trim());
                }
                else
                {
                    result = DatabaseHelper.AddCategory(txtName.Text.Trim(), txtDescription.Text.Trim());
                }

                if (result)
                {
                    MessageBox.Show("Category " + (_isEditMode ? "updated" : "added") + " successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving category: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
