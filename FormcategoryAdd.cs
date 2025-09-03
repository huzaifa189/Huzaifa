using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RM.Models;
using RM;
namespace RM.Models
{
    public partial class FormcategoryAdd : SampleAdd
    {
        private int _categoryId = 0;
        private bool _isEditMode = false;

        public FormcategoryAdd()
        {
            InitializeComponent();
            _isEditMode = false;
        }

        public FormcategoryAdd(int categoryId) : this()
        {
            _categoryId = categoryId;
            _isEditMode = true;
            LoadCategory(categoryId);
        }

        private void FormcategoryAdd_Load(object sender, EventArgs e)
        {
            try
            {
                if (((SampleAdd)this).label2 != null) ((SampleAdd)this).label2.Visible = false;
                if (((SampleAdd)this).txtSearch1 != null) ((SampleAdd)this).txtSearch1.Visible = false;
            }
            catch { }

            if (_isEditMode)
            {
                this.Text = "Edit Category";
            }
            else
            {
                this.Text = "Add New Category";
            }
        }

        private void LoadCategory(int categoryId)
        {
            try
            {
                var dt = DatabaseHelper.GetCategories();
                var categoryRow = dt.AsEnumerable()
                    .FirstOrDefault(row => Convert.ToInt32(row["CategoryID"]) == categoryId);

                if (categoryRow != null)
                {
                    if (txtNaMe != null)
                        txtNaMe.Text = categoryRow["CategoryName"].ToString();
                    
                    if (txtDescription != null)
                        txtDescription.Text = categoryRow["Description"]?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading category: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public override void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNaMe?.Text))
                {
                    MessageBox.Show("Please enter a category name.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool success = false;
                string message = "";

                if (_isEditMode)
                {
                    success = DatabaseHelper.UpdateCategory(_categoryId, txtNaMe.Text.Trim(), txtDescription?.Text?.Trim());
                    message = success ? "Category updated successfully!" : "Failed to update category.";
                }
                else
                {
                    success = DatabaseHelper.AddCategory(txtNaMe.Text.Trim(), txtDescription?.Text?.Trim());
                    message = success ? "Category added successfully!" : "Failed to add category.";
                }

                if (success)
                {
                    MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving category: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

