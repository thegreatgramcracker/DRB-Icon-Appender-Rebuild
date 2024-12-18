using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DRB_Icon_Appender
{
    public partial class FormBatchAdd : Form
    {
        private FormMain mainForm;
        private List<string> textures;

        public FormBatchAdd(FormMain mainForm, List<string> textures)
        {
            InitializeComponent();
            this.mainForm = mainForm; // Store the FormMain instance
            this.textures = textures; // Store the textures list

            // Populate the ComboBox with sorted textures
            PopulateTextureDropdown();
        }

        public int RangeStart => (int)nudBatchAddRangeStart.Value;
        public int RangeEnd => (int)nudBatchAddRangeEnd.Value;
        public string SelectedTexture => comboBoxTexture.SelectedItem.ToString();
        public int Width => int.Parse(txtBoxWidth.Text);
        public int Height => int.Parse(txtBoxHeight.Text);
        public int Rows => int.Parse(txtBoxRows.Text);
        public int Columns => int.Parse(txtBoxColumns.Text);

        private void PopulateTextureDropdown()
        {
            List<string> sortedTextures = new List<string>(textures);
            sortedTextures.Sort(); // Ensure the textures are sorted
            comboBoxTexture.DataSource = sortedTextures; // Bind the sorted list to the ComboBox
        }

        private void grpBoxIconRange_Enter(object sender, EventArgs e)
        {

        }

        private void nudBatchAddRangeStart_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nudBatchAddRangeEnd_ValueChanged(object sender, EventArgs e)
        {

        }

        private void lblDivider_Click(object sender, EventArgs e)
        {

        }

        private void grpBoxTexture_Enter(object sender, EventArgs e)
        {

        }

        private void lblWidth_Click(object sender, EventArgs e)
        {

        }

        private void lblHeight_Click(object sender, EventArgs e)
        {

        }

        private void txtBoxHeight_TextChanged(object sender, EventArgs e)
        {

        }

        private void grpBoxIconResolution_Enter(object sender, EventArgs e)
        {

        }

        private void txtBoxColumns_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtBoxRows_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtBoxRows.Text, out int rows) || rows <= 0)
            {
                MessageBox.Show("Please enter a valid positive integer for Rows.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtBoxColumns.Text, out int columns) || columns <= 0)
            {
                MessageBox.Show("Please enter a valid positive integer for Columns.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Set DialogResult to OK to signal that the operation can proceed
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
