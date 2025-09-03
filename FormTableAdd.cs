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
    public partial class FormTableAdd : SampleAdd
    {
        private int _tableId = 0;
        private bool _isEditMode = false;

        public FormTableAdd()
        {
            InitializeComponent();
            _isEditMode = false;
        }

        public FormTableAdd(int tableId) : this()
        {
            _tableId = tableId;
            _isEditMode = true;
            LoadTable(tableId);
        }

        private void FormTableAdd_Load(object sender, EventArgs e)
        {
            try
            {
                if (((SampleAdd)this).label2 != null) ((SampleAdd)this).label2.Visible = false;
                if (((SampleAdd)this).txtSearch1 != null) ((SampleAdd)this).txtSearch1.Visible = false;
            }
            catch { }

            if (_isEditMode)
            {
                this.Text = "Edit Table";
            }
            else
            {
                this.Text = "Add New Table";
                try
                {
                    if (cmbStatus != null && cmbStatus.Items != null && cmbStatus.Items.Count > 0)
                    {
                        int idx = cmbStatus.Items.IndexOf("Available");
                        if (idx >= 0) cmbStatus.SelectedIndex = idx;
                    }
                }
                catch { }
            }
        }

        private void LoadTable(int tableId)
        {
            try
            {
                var dt = DatabaseHelper.GetTables();
                var tableRow = dt.AsEnumerable()
                    .FirstOrDefault((System.Data.DataRow row) => Convert.ToInt32(row["TableID"]) == tableId);

                if (tableRow != null)
                {
                    if (txtTableNumber != null)
                        txtTableNumber.Text = tableRow["TableNumber"].ToString();
                    
                    if (txtCapacity != null)
                        txtCapacity.Text = tableRow["Capacity"].ToString();
                    
                    if (cmbStatus != null)
                    {
                        var raw = tableRow["Status"] != DBNull.Value ? tableRow["Status"].ToString() : string.Empty;
                        var normalized = NormalizeStatus(raw);
                        int idx = cmbStatus.Items.IndexOf(normalized);
                        if (idx >= 0) cmbStatus.SelectedIndex = idx;
                        else cmbStatus.Text = normalized;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading table: " + ex.Message, "Error", 
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
            try { return char.ToUpper(status[0]) + status.Substring(1).ToLower(); } catch { return status; }
        }

        public override void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTableNumber?.Text))
                {
                    MessageBox.Show("Please enter a table number.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtCapacity?.Text))
                {
                    MessageBox.Show("Please enter table capacity.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(txtCapacity.Text, out int capacity) || capacity <= 0)
                {
                    MessageBox.Show("Please enter a valid capacity (positive number).", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string status = NormalizeStatus(cmbStatus?.Text ?? "Available");
                if (string.IsNullOrWhiteSpace(status))
                {
                    status = "Available";
                }

                bool success = false;
                string message = "";

                if (_isEditMode)
                {
                    success = DatabaseHelper.UpdateTable(_tableId, txtTableNumber.Text.Trim(), capacity, status);
                    message = success ? "Table updated successfully!" : "Failed to update table.";
                }
                else
                {
                    success = DatabaseHelper.AddTable(txtTableNumber.Text.Trim(), capacity, status);
                    message = success ? "Table added successfully!" : "Failed to add table.";
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
                MessageBox.Show("Error saving table: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
