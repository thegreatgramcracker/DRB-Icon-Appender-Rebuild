using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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

        private List<SpriteWrapper> sprites; // Store existing icons
        private ToolTip gridToolTip = new ToolTip();

        private Dictionary<Point, bool> occupiedCells; // Maps grid cells to their occupied status

        internal FormBatchAdd(FormMain mainForm, List<string> textures, List<SpriteWrapper> sprites)
        {
            InitializeComponent();

            this.mainForm = mainForm;
            this.textures = textures;
            this.sprites = sprites; // Save sprites for validation

            // Populate the grid and dropdowns
            GenerateGrid(int.Parse(txtBoxRows.Text), int.Parse(txtBoxColumns.Text)); // Ensure gridButtons is initialized
            PopulateTextureDropdown();

            // Initialize the range difference
            rangeDifference = (int)nudBatchAddRangeEnd.Value - (int)nudBatchAddRangeStart.Value;

            // Initialize constraints
            UpdateRangeEndConstraints();

            // Highlight initial grid preview
            HighlightGrid(CalculateIconCount());
            HighlightExistingIcons(); // Initial shading for existing icons

            // Attach event handlers for dynamic updates
            comboBoxTexture.SelectedIndexChanged += comboBoxTexture_SelectedIndexChanged;
            txtBoxWidth.TextChanged += txtBoxWidth_TextChanged;
            txtBoxHeight.TextChanged += txtBoxHeight_TextChanged;
            txtBoxMargins.TextChanged += txtBoxMargins_TextChanged;

            // Ensure "Auto Set" logic applies on load
            if (chkAutoSet.Checked)
            {
                ChkAutoSet_CheckedChanged(chkAutoSet, EventArgs.Empty);
            }

            // Attach event handlers
            comboBoxTexture.SelectedIndexChanged += comboBoxTexture_SelectedIndexChanged;
            chkAutoSet.CheckedChanged += ChkAutoSet_CheckedChanged;

            // Update Effective Tile Size on load
            UpdateEffectiveTileSize();

            // Ensure the first texture is fully processed
            if (comboBoxTexture.Items.Count > 0)
            {
                comboBoxTexture.SelectedIndex = 0; // Select the first texture
                comboBoxTexture_SelectedIndexChanged(comboBoxTexture, EventArgs.Empty); // Simulate selection
            }
        }

        public int RangeStart => (int)nudBatchAddRangeStart.Value;
        public int RangeEnd => (int)nudBatchAddRangeEnd.Value;
        public string SelectedTexture => comboBoxTexture.SelectedItem?.ToString();
        public int Width => int.Parse(txtBoxWidth.Text);
        public int Height => int.Parse(txtBoxHeight.Text);
        public int PixelMargin => int.Parse(txtBoxMargins.Text);
        public int Rows => int.Parse(txtBoxRows.Text);
        public int Columns => int.Parse(txtBoxColumns.Text);
        public int StartRow => selectedCell.X; // Row of selected cell
        public int StartColumn => selectedCell.Y; // Column of selected cell

        private const int MaxPanelWidth = 292;  // Maximum width of the grid panel
        private const int MaxPanelHeight = 292; // Maximum height of the grid panel

        private void FormBatchAdd_Load(object sender, EventArgs e)
        {

        }

        private void PopulateTextureDropdown()
        {
            List<string> filteredTextures = textures
                .Where(texture => texture.StartsWith("Icon"))
                .OrderBy(texture => texture)
                .ToList();

            comboBoxTexture.DataSource = filteredTextures;

            if (filteredTextures.Any())
            {
                comboBoxTexture.SelectedIndex = 0; // Ensure the first item is selected
                comboBoxTexture_SelectedIndexChanged(comboBoxTexture, EventArgs.Empty); // Trigger logic for the first item
            }
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

        private void txtBoxWidth_TextChanged(object sender, EventArgs e)
        {
            UpdateEffectiveTileSize();
            UpdateOccupiedCells();
            HighlightGrid(CalculateIconCount());
            UpdateTooltips();
        }

        private void txtBoxHeight_TextChanged(object sender, EventArgs e)
        {
            UpdateEffectiveTileSize();
            UpdateOccupiedCells();
            HighlightGrid(CalculateIconCount());
            UpdateTooltips();
        }

        private void txtBoxMargins_TextChanged(object sender, EventArgs e)
        {
            UpdateEffectiveTileSize();
            UpdateOccupiedCells();
            HighlightGrid(CalculateIconCount());
            UpdateTooltips();
        }

        private void comboBoxTexture_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedTexture = comboBoxTexture.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTexture)) return;

            UpdateInputsForTexture(selectedTexture);
            GenerateGrid(int.Parse(txtBoxRows.Text), int.Parse(txtBoxColumns.Text));
            UpdateOccupiedCells();
            AutoSetGridSize(); // Trigger Auto Set
            HighlightGrid(CalculateIconCount());
            UpdateTooltips();
        }

        private void UpdateInputsForTexture(string selectedTexture)
        {
            // Find the most common dimensions for this texture
            var matchingSprites = sprites
                .Where(sprite => sprite.Texture == selectedTexture)
                .ToList();

            if (!matchingSprites.Any())
            {
                return;
            }

            // Determine the most common dimensions
            var mostCommonDimensions = matchingSprites
                .GroupBy(sprite => new { sprite.Width, sprite.Height })
                .OrderByDescending(group => group.Count())
                .First()
                .Key;

            txtBoxWidth.Text = mostCommonDimensions.Width.ToString();
            txtBoxHeight.Text = mostCommonDimensions.Height.ToString();

            // Optionally set a default margin if it's not part of the sprite
            if (int.TryParse(txtBoxMargins.Text, out int margin) && margin >= 0)
            {
            }
            else
            {
                txtBoxMargins.Text = "2"; // Default margin value
            }
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
            pnlGridPreview.Controls.Clear();
            pnlGridPreview.SuspendLayout();

            if (rows <= 0 || columns <= 0)
            {
                gridButtons = null;
                return; // Exit if invalid dimensions
            }

            // Calculate effective tile size
            if (!int.TryParse(txtBoxWidth.Text, out int width) || width <= 0 ||
                !int.TryParse(txtBoxHeight.Text, out int height) || height <= 0 ||
                !int.TryParse(txtBoxMargins.Text, out int margin) || margin < 0)
            {
                gridButtons = null;
                return; // Exit on invalid input
            }

            int effectiveWidth = width + (margin * 2);
            int effectiveHeight = height + (margin * 2);

            // Calculate aspect ratio
            float cellAspectRatio = (float)effectiveWidth / effectiveHeight;

            // Calculate the maximum cell size to fit within the panel
            int maxCellWidth = MaxPanelWidth / columns;
            int maxCellHeight = MaxPanelHeight / rows;

            // Adjust cell dimensions to maintain aspect ratio
            int cellWidth, cellHeight;

            if (cellAspectRatio > 1) // Wider than tall
            {
                cellWidth = Math.Min(maxCellWidth, maxCellHeight * effectiveWidth / effectiveHeight);
                cellHeight = cellWidth * effectiveHeight / effectiveWidth;
            }
            else // Taller than wide
            {
                cellHeight = Math.Min(maxCellHeight, maxCellWidth * effectiveHeight / effectiveWidth);
                cellWidth = cellHeight * effectiveWidth / effectiveHeight;
            }

            // Adjust the grid panel size to center the grid
            int gridWidth = columns * cellWidth;
            int gridHeight = rows * cellHeight;

            pnlGridPreview.Width = gridWidth;
            pnlGridPreview.Height = gridHeight;

            gridButtons = new Button[rows, columns];

            // Initialize grid cells
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    Button btn = new Button
                    {
                        Width = cellWidth,
                        Height = cellHeight,
                        Location = new Point(col * cellWidth, row * cellHeight),
                        Tag = new Point(row, col), // Store row and column
                        BackColor = Color.White // Default color
                    };

                    btn.MouseDown += GridCell_MouseDown; // Attach event
                    gridButtons[row, col] = btn;
                    pnlGridPreview.Controls.Add(btn);
                }
            }

            pnlGridPreview.ResumeLayout();
            HighlightExistingIcons(); // Apply initial shading
            UpdateTooltips(); // Initialize tooltips
        }

        private void UpdateTooltips()
        {
            if (gridButtons == null || sprites == null) return;

            if (!int.TryParse(txtBoxWidth.Text, out int width) || width <= 0 ||
                !int.TryParse(txtBoxHeight.Text, out int height) || height <= 0 ||
                !int.TryParse(txtBoxMargins.Text, out int margin) || margin < 0)
            {
                return; // Exit on invalid dimensions
            }

            string selectedTexture = comboBoxTexture.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTexture)) return;

            int effectiveWidth = width + (margin * 2);
            int effectiveHeight = height + (margin * 2);

            foreach (var sprite in sprites)
            {
                if (sprite.Texture == selectedTexture &&
                    sprite.Width == width &&
                    sprite.Height == height)
                {
                    int row = (sprite.TopEdge - 1) / effectiveHeight;
                    int col = (sprite.LeftEdge - 1) / effectiveWidth;

                    if (row >= 0 && row < gridButtons.GetLength(0) &&
                        col >= 0 && col < gridButtons.GetLength(1))
                    {
                        Button cellButton = gridButtons[row, col];

                        if (cellButton != null)
                        {
                            string tooltipText = $"ID: {sprite.ID}\nResolution: {sprite.Width}x{sprite.Height}";
                            gridToolTip.SetToolTip(cellButton, tooltipText);
                        }
                    }
                }
            }
        }

        private void HighlightGrid(int totalIcons)
        {
            if (gridButtons == null) return;

            foreach (var btn in gridButtons)
            {
                if (btn == null) continue;

                Point cell = (Point)btn.Tag;

                if (occupiedCells != null && occupiedCells.ContainsKey(cell))
                {
                    btn.BackColor = Color.Gray; // Occupied cells
                }
                else
                {
                    btn.BackColor = Color.White; // Default unoccupied
                }
            }

            // Highlight selected range
            int currentRow = selectedCell.X;
            int currentCol = selectedCell.Y;

            for (int i = 0; i < totalIcons; i++)
            {
                if (currentRow >= gridButtons.GetLength(0)) break;

                Button currentButton = gridButtons[currentRow, currentCol];
                if (currentButton == null) continue;

                Point currentCell = new Point(currentRow, currentCol);

                if (occupiedCells != null && occupiedCells.ContainsKey(currentCell))
                {
                    currentButton.BackColor = i == 0 ? Color.DarkRed : Color.LightCoral; // Overlapping
                }
                else
                {
                    currentButton.BackColor = i == 0 ? Color.DarkBlue : Color.LightBlue; // Selected
                }

                // Move to next cell
                currentCol++;
                if (currentCol >= gridButtons.GetLength(1))
                {
                    currentCol = 0;
                    currentRow++;
                }
            }
        }

        private void HighlightExistingIcons()
        {
            foreach (var sprite in sprites)
            {
                if (sprite.Texture == SelectedTexture &&
                    sprite.Width == Width &&
                    sprite.Height == Height)
                {
                    // Calculate grid position based on effective tile size
                    int effectiveWidth = Width + (PixelMargin * 2);
                    int effectiveHeight = Height + (PixelMargin * 2);

                    int row = (sprite.TopEdge - 1) / effectiveHeight;
                    int col = (sprite.LeftEdge - 1) / effectiveWidth;

                    if (row >= 0 && row < gridButtons.GetLength(0) &&
                        col >= 0 && col < gridButtons.GetLength(1))
                    {
                        gridButtons[row, col].BackColor = Color.DarkGray; // Indicate occupied
                    }
                }
            }
        }

        private void GridCell_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                Point clickedCell = (Point)clickedButton.Tag;

                if (e.Button == MouseButtons.Left)
                {
                    selectedCell = clickedCell; // Update the starting cell
                    UpdateRangeEndConstraints();
                    HighlightGrid(CalculateIconCount()); // Refresh the grid
                }
                else if (e.Button == MouseButtons.Right)
                {
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

        private void AutoSetGridSize()
        {
            if (!chkAutoSet.Checked) return; // Ensure auto set is enabled

            string selectedTexture = comboBoxTexture.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTexture))
            {
                return; // Exit if no texture is selected
            }

            int width, height;
            if (!int.TryParse(txtBoxWidth.Text, out width) || width <= 0 ||
                !int.TryParse(txtBoxHeight.Text, out height) || height <= 0)
            {
                return; // Exit on invalid dimensions
            }

            // Default grid size to 11x12
            int rows = 11;
            int columns = 12;

            // Adjust grid size based on texture and conditions
            if (width == 160 && height == 180)
            {
                // Special case for Icon40, Icon12, or Icon21
                if ((selectedTexture == "Icon40" || selectedTexture == "Icon12" || selectedTexture == "Icon21") &&
                    !HasOccupiedCellsBeyondRow(5))
                {
                    rows = 5;
                }
            }
            else if (width == 80 && height == 80)
            {
                // Special case for Icon50
                if (selectedTexture == "Icon50" &&
                    !HasOccupiedCellsBeyondRow(6) &&
                    !HasOccupiedCellsBeyondColumn(6))
                {
                    rows = 6;
                    columns = 6;
                }
                else
                {
                    rows = 12;
                    columns = 12;
                }
            }

            // Apply the calculated grid size
            txtBoxRows.Text = rows.ToString();
            txtBoxColumns.Text = columns.ToString();
        }

        private bool HasOccupiedCellsBeyondRow(int maxRow)
        {
            foreach (var cell in occupiedCells.Keys)
            {
                if (cell.X > maxRow)
                {
                    return true; // Found an occupied cell beyond the row limit
                }
            }
            return false; // No occupied cells beyond the row limit
        }

        private bool HasOccupiedCellsBeyondColumn(int maxColumn)
        {
            foreach (var cell in occupiedCells.Keys)
            {
                if (cell.Y > maxColumn)
                {
                    return true; // Found an occupied cell beyond the column limit
                }
            }
            return false; // No occupied cells beyond the column limit
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

                // Regenerate the grid with the updated tile size
                if (int.TryParse(txtBoxRows.Text, out int rows) && rows > 0 &&
                    int.TryParse(txtBoxColumns.Text, out int columns) && columns > 0)
                {
                    GenerateGrid(rows, columns); // Refresh the grid preview
                    UpdateOccupiedCells();       // Ensure occupied cells are recalculated
                    HighlightGrid(CalculateIconCount());
                    UpdateTooltips();
                }
            }
            else
            {
                lblEffectiveTileSize.Text = "Effective Tile Size: Invalid Input";
            }
        }

        private void UpdateOccupiedCells()
        {
            occupiedCells = new Dictionary<Point, bool>();

            if (!int.TryParse(txtBoxWidth.Text, out int width) || width <= 0 ||
                !int.TryParse(txtBoxHeight.Text, out int height) || height <= 0 ||
                !int.TryParse(txtBoxMargins.Text, out int margin) || margin < 0)
            {
                return;
            }

            string selectedTexture = SelectedTexture;
            if (string.IsNullOrEmpty(selectedTexture))
            {
                return;
            }

            int effectiveWidth = width + (margin * 2);
            int effectiveHeight = height + (margin * 2);

            foreach (var sprite in sprites)
            {
                if (sprite.Texture == selectedTexture && sprite.Width == width && sprite.Height == height)
                {
                    int row = (sprite.TopEdge - 1) / effectiveHeight;
                    int col = (sprite.LeftEdge - 1) / effectiveWidth;

                    Point cell = new Point(row, col);
                    occupiedCells[cell] = true;
                }
            }
        }

        private void ChkAutoSet_CheckedChanged(object sender, EventArgs e)
        {
            bool isAutoSetEnabled = chkAutoSet.Checked;

            // Enable/disable the input fields based on the checkbox state
            txtBoxRows.Enabled = !isAutoSetEnabled;
            txtBoxColumns.Enabled = !isAutoSetEnabled;
            txtBoxWidth.Enabled = !isAutoSetEnabled;
            txtBoxHeight.Enabled = !isAutoSetEnabled;
            txtBoxMargins.Enabled = !isAutoSetEnabled;

            if (isAutoSetEnabled)
            {
                AutoSetGridSize(); // Trigger Auto Set logic when enabled
            }
        }
    }
}
