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

            // Populate default grid
            GenerateGrid(int.Parse(txtBoxRows.Text), int.Parse(txtBoxColumns.Text));
            // Highlight default range
            int totalIcons = CalculateIconCount();
            HighlightGrid(totalIcons);

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
        public int StartRow => selectedCell.X; // Row of selected cell
        public int StartColumn => selectedCell.Y; // Column of selected cell

        private const int MaxPanelWidth = 292;  // Maximum width of the grid panel
        private const int MaxPanelHeight = 292; // Maximum height of the grid panel


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
            int totalIcons = CalculateIconCount();
            HighlightGrid(totalIcons);
        }

        private void nudBatchAddRangeEnd_ValueChanged(object sender, EventArgs e)
        {
            int totalIcons = CalculateIconCount();
            HighlightGrid(totalIcons);
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

        private void txtBoxRows_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtBoxRows.Text, out int rows) && int.TryParse(txtBoxColumns.Text, out int columns) && rows > 0 && columns > 0)
            {
                GenerateGrid(rows, columns);
            }

            // Recalculate and highlight the range
            int totalIcons = CalculateIconCount();
            HighlightGrid(totalIcons);
        }

        private void txtBoxColumns_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtBoxRows.Text, out int rows) && int.TryParse(txtBoxColumns.Text, out int columns) && rows > 0 && columns > 0)
            {
                GenerateGrid(rows, columns);
            }

            // Recalculate and highlight the range
            int totalIcons = CalculateIconCount();
            HighlightGrid(totalIcons);
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

            if (!int.TryParse(txtBoxMargins.Text, out int margin) || margin < 0)
            {
                MessageBox.Show("Please enter a valid non-negative integer for the pixel margin.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Set DialogResult to OK to signal that the operation can proceed
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void FormBatchAdd_Load(object sender, EventArgs e)
        {

        }
        public int PixelMargin
        {
            get
            {
                if (int.TryParse(txtBoxMargins.Text, out int margin) && margin >= 0)
                {
                    return margin;
                }
                else
                {
                    MessageBox.Show("Please enter a valid non-negative integer for the pixel margin.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return 2; // Default margin if input is invalid
                }
            }
        }

        private void lblMargins_Click(object sender, EventArgs e)
        {

        }

        private int CalculateIconCount()
        {
            int startId = (int)nudBatchAddRangeStart.Value;
            int endId = (int)nudBatchAddRangeEnd.Value;

            return endId - startId + 1; // Inclusive range
        }

        private Button[,] gridButtons; // To keep track of the grid buttons
        private Point selectedCell = new Point(0, 0); // Default starting point

        private void GenerateGrid(int rows, int columns)
        {
            pnlGridPreview.Controls.Clear(); // Clear previous grid
            pnlGridPreview.SuspendLayout();

            // Calculate cell size based on maximum dimensions
            int cellWidth = Math.Min(MaxPanelWidth / columns, 30); // Default size is 30x30
            int cellHeight = Math.Min(MaxPanelHeight / rows, 30);
            int cellSize = Math.Min(cellWidth, cellHeight); // Ensure square cells

            // Adjust panel size to fit the grid
            pnlGridPreview.Width = Math.Min(columns * cellSize, MaxPanelWidth);
            pnlGridPreview.Height = Math.Min(rows * cellSize, MaxPanelHeight);

            gridButtons = new Button[rows, columns];
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    Button btn = new Button
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Location = new Point(col * cellSize, row * cellSize),
                        Tag = new Point(row, col), // Store row and column as tag
                        BackColor = (row == 0 && col == 0) ? Color.LightBlue : Color.White // Default selection
                    };

                    btn.Click += GridCell_Click; // Attach click event
                    gridButtons[row, col] = btn;
                    pnlGridPreview.Controls.Add(btn);
                }
            }

            pnlGridPreview.ResumeLayout();
        }

        private void HighlightGrid(int totalIcons)
        {
            // Reset all cells to default color
            foreach (var btn in gridButtons)
            {
                btn.BackColor = Color.White;
            }

            // Highlight the starting cell in darker blue
            gridButtons[selectedCell.X, selectedCell.Y].BackColor = ColorTranslator.FromHtml("#3287c1");

            // Highlight the subsequent cells
            int currentRow = selectedCell.X;
            int currentCol = selectedCell.Y;

            for (int i = 1; i < totalIcons; i++) // Start from the second icon
            {
                currentCol++;

                // Move to the next row if we exceed the current column count
                if (currentCol >= gridButtons.GetLength(1))
                {
                    currentCol = 0;
                    currentRow++;

                    // Stop if we exceed the grid dimensions
                    if (currentRow >= gridButtons.GetLength(0))
                    {
                        break;
                    }
                }

                // Highlight the cell
                gridButtons[currentRow, currentCol].BackColor = Color.LightBlue;
            }
        }

        private void GridCell_Click(object sender, EventArgs e)
        {
            if (sender is Button clickedButton)
            {
                // Deselect previously selected range
                HighlightGrid(0);

                // Update the selected cell
                selectedCell = (Point)clickedButton.Tag;

                // Recalculate and highlight the range
                int totalIcons = CalculateIconCount();
                HighlightGrid(totalIcons);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
