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

        private int rangeDifference; // Tracks the difference between Start and End
        private int maxIcons;        // Maximum icons based on grid dimensions

        public FormBatchAdd(FormMain mainForm, List<string> textures)
        {
            InitializeComponent();

            this.mainForm = mainForm;
            this.textures = textures;

            // Populate the grid and dropdowns
            GenerateGrid(int.Parse(txtBoxRows.Text), int.Parse(txtBoxColumns.Text));
            PopulateTextureDropdown();

            // Initialize the range difference
            rangeDifference = (int)nudBatchAddRangeEnd.Value - (int)nudBatchAddRangeStart.Value;

            // Initialize constraints
            UpdateRangeEndConstraints();

            // Highlight initial grid preview
            HighlightGrid(CalculateIconCount());

            // Attach event handlers for dynamic updates
            txtBoxWidth.TextChanged += txtBoxWidth_TextChanged;
            txtBoxHeight.TextChanged += txtBoxHeight_TextChanged;
            txtBoxMargins.TextChanged += txtBoxMargins_TextChanged;

            // Update Effective Tile Size on load
            UpdateEffectiveTileSize();
        }

        public int RangeStart => (int)nudBatchAddRangeStart.Value;
        public int RangeEnd => (int)nudBatchAddRangeEnd.Value;
        public string SelectedTexture => comboBoxTexture.SelectedItem.ToString();
        public int Width => int.Parse(txtBoxWidth.Text);
        public int Height => int.Parse(txtBoxHeight.Text);
        public int PixelMargin => int.Parse(txtBoxMargins.Text);
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
            UpdateRange(); // Recalculate `End` while preserving the range difference
        }

        private void nudBatchAddRangeEnd_ValueChanged(object sender, EventArgs e)
        {
            // Recalculate the range difference
            rangeDifference = (int)nudBatchAddRangeEnd.Value - (int)nudBatchAddRangeStart.Value;

            // Update the grid preview
            HighlightGrid(CalculateIconCount());
        }

        private void UpdateRangeEndConstraints()
        {
            if (!int.TryParse(txtBoxRows.Text, out int rows) || rows <= 0 ||
                !int.TryParse(txtBoxColumns.Text, out int columns) || columns <= 0)
            {
                return; // Exit if grid dimensions are invalid
            }

            maxIcons = rows * columns; // Total cells in the grid

            // Calculate the available space based on the starting position
            int startRow = selectedCell.X; // Row of the starting cell
            int startColumn = selectedCell.Y; // Column of the starting cell
            int remainingRows = rows - startRow;
            int remainingColumns = columns - startColumn;

            // Calculate the available icons based on grid space
            int availableIcons = (remainingRows - 1) * columns + remainingColumns;

            int startId = (int)nudBatchAddRangeStart.Value;

            // Calculate valid range for nudBatchAddRangeEnd
            int minEnd = startId;
            int maxEnd = Math.Min(9999, startId + availableIcons - 1); // Enforce hard limit at 9999

            // Update constraints for nudBatchAddRangeEnd
            nudBatchAddRangeEnd.Minimum = minEnd;
            nudBatchAddRangeEnd.Maximum = maxEnd;

            // Ensure the current end value respects the constraints
            if (nudBatchAddRangeEnd.Value < minEnd)
            {
                nudBatchAddRangeEnd.Value = minEnd;
            }
            else if (nudBatchAddRangeEnd.Value > maxEnd)
            {
                nudBatchAddRangeEnd.Value = maxEnd;
            }
        }

        private void UpdateRange(int? newStart = null, int? newEnd = null)
        {
            // Temporarily disable events to prevent recursion
            nudBatchAddRangeStart.ValueChanged -= nudBatchAddRangeStart_ValueChanged;
            nudBatchAddRangeEnd.ValueChanged -= nudBatchAddRangeEnd_ValueChanged;

            // Update start value if provided
            if (newStart.HasValue)
            {
                nudBatchAddRangeStart.Value = newStart.Value;
            }

            // Update constraints before calculating the new end value
            UpdateRangeEndConstraints();

            // Calculate the intended new End value based on the preserved rangeDifference
            int currentStart = (int)nudBatchAddRangeStart.Value;
            int calculatedEnd = rangeDifference == 0 ? currentStart : currentStart + rangeDifference;

            // Ensure calculatedEnd respects the updated constraints and the hard limit of 9999
            calculatedEnd = Math.Min(9999, Math.Max((int)nudBatchAddRangeEnd.Minimum, Math.Min((int)nudBatchAddRangeEnd.Maximum, calculatedEnd)));

            // Update end value if a new one is provided or use the calculated value
            if (newEnd.HasValue)
            {
                nudBatchAddRangeEnd.Value = newEnd.Value;
            }
            else
            {
                nudBatchAddRangeEnd.Value = calculatedEnd;
            }

            // Recalculate and preserve the rangeDifference
            rangeDifference = (int)nudBatchAddRangeEnd.Value - (int)nudBatchAddRangeStart.Value;

            // Re-enable events
            nudBatchAddRangeStart.ValueChanged += nudBatchAddRangeStart_ValueChanged;
            nudBatchAddRangeEnd.ValueChanged += nudBatchAddRangeEnd_ValueChanged;

            // Update the grid preview
            HighlightGrid(CalculateIconCount());
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

        private void txtBoxWidth_TextChanged(object sender, EventArgs e)
        {
            UpdateEffectiveTileSize();
        }

        private void txtBoxHeight_TextChanged(object sender, EventArgs e)
        {
            UpdateEffectiveTileSize();
        }

        private void txtBoxMargins_TextChanged(object sender, EventArgs e)
        {
            UpdateEffectiveTileSize();
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

            UpdateRangeEndConstraints();
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

            UpdateRangeEndConstraints();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            // Validate width
            if (!int.TryParse(txtBoxWidth.Text, out int width) || width <= 0)
            {
                MessageBox.Show("Please enter a valid positive integer for Width.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate height
            if (!int.TryParse(txtBoxHeight.Text, out int height) || height <= 0)
            {
                MessageBox.Show("Please enter a valid positive integer for Height.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate rows and columns
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

            // Validate for duplicate IDs
            if (mainForm.RangeHasDuplicates(RangeStart, RangeEnd, out List<int> duplicateIds))
            {
                string duplicateMessage = $"The following IDs are already in use: {string.Join(", ", duplicateIds)}.";
                MessageBox.Show(duplicateMessage, "Duplicate IDs Detected", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Prevent the form from closing
                return;
            }

            // Set DialogResult to OK to signal that the operation can proceed
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void FormBatchAdd_Load(object sender, EventArgs e)
        {

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

                    btn.MouseDown += GridCell_MouseDown; // Attach MouseDown event
                    gridButtons[row, col] = btn;
                    pnlGridPreview.Controls.Add(btn);
                }
            }

            pnlGridPreview.ResumeLayout();
        }

        private void HighlightGrid(int totalIcons)
        {
            foreach (var btn in gridButtons)
            {
                btn.BackColor = Color.White;
            }

            gridButtons[selectedCell.X, selectedCell.Y].BackColor = ColorTranslator.FromHtml("#3287c1");

            int currentRow = selectedCell.X;
            int currentCol = selectedCell.Y;

            for (int i = 1; i < totalIcons; i++)
            {
                currentCol++;

                if (currentCol >= gridButtons.GetLength(1))
                {
                    currentCol = 0;
                    currentRow++;

                    if (currentRow >= gridButtons.GetLength(0))
                    {
                        break;
                    }
                }

                gridButtons[currentRow, currentCol].BackColor = Color.LightBlue;
            }
        }
        private void GridCell_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                // Get the clicked cell's position
                Point clickedCell = (Point)clickedButton.Tag;

                if (e.Button == MouseButtons.Left)
                {
                    // Handle left-click: update the starting position
                    selectedCell = clickedCell;

                    // Recalculate constraints and range
                    UpdateRangeEndConstraints();

                    // Recalculate the grid preview
                    HighlightGrid(CalculateIconCount());
                }
                else if (e.Button == MouseButtons.Right)
                {
                    // Handle right-click: update the end value to include the clicked cell
                    AdjustEndValueToIncludeCell(clickedCell);
                }
            }
        }

        private void AdjustEndValueToIncludeCell(Point targetCell)
        {
            // Calculate the number of icons between the selected starting cell and the target cell
            int targetRow = targetCell.X;
            int targetCol = targetCell.Y;

            int startRow = selectedCell.X;
            int startCol = selectedCell.Y;

            // Determine the row and column difference
            int rowDifference = targetRow - startRow;
            int colDifference = targetCol - startCol;

            // Calculate the total number of icons
            int totalIcons = rowDifference * gridButtons.GetLength(1) + colDifference + 1;

            // Update the end value based on the calculated totalIcons
            int newEndValue = (int)nudBatchAddRangeStart.Value + totalIcons - 1;

            // Ensure the new end value respects constraints and the hard limit of 9999
            newEndValue = Math.Min(9999, Math.Min((int)nudBatchAddRangeEnd.Maximum, Math.Max((int)nudBatchAddRangeEnd.Minimum, newEndValue)));

            // Update the end value
            UpdateRange(null, newEndValue);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void lblEffectiveTileSize_Click(object sender, EventArgs e)
        {

        }

        private void UpdateEffectiveTileSize()
        {
            if (int.TryParse(txtBoxWidth.Text, out int width) &&
                int.TryParse(txtBoxHeight.Text, out int height) &&
                int.TryParse(txtBoxMargins.Text, out int margin))
            {
                int effectiveWidth = width + (margin * 2);
                int effectiveHeight = height + (margin * 2);

                lblEffectiveTileSize.Text = $"Effective Tile Size: {effectiveWidth}x{effectiveHeight}";
            }
            else
            {
                lblEffectiveTileSize.Text = "Effective Tile Size: Invalid Input";
            }
        }

        private void txtBoxMargins_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void lblPixels_Click(object sender, EventArgs e)
        {

        }

        private void lblGridExplanation_Click(object sender, EventArgs e)
        {

        }
    }
}
