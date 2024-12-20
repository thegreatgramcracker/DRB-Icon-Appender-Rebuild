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
            string selectedTexture = comboBoxTexture.SelectedItem.ToString();

            // Filter all sprites with the selected texture
            var matchingSprites = mainForm.Sprites.Where(sprite => sprite.Texture == selectedTexture).ToList();
            if (!matchingSprites.Any())
            {
                txtBoxWidth.Text = "80"; // Default to 80x80 if no entries found
                txtBoxHeight.Text = "80";
                txtBoxRows.Text = "6";
                txtBoxColumns.Text = "6";
                UpdateTooltips(); // Clear any old tooltips
                return;
            }

            // Group by resolution to find the most common width and height
            var resolutionGroups = matchingSprites
                .GroupBy(sprite => new { sprite.Width, sprite.Height })
                .OrderByDescending(group => group.Count())
                .First();

            int mostCommonWidth = resolutionGroups.Key.Width;
            int mostCommonHeight = resolutionGroups.Key.Height;

            // Set the most common resolution
            txtBoxWidth.Text = mostCommonWidth.ToString();
            txtBoxHeight.Text = mostCommonHeight.ToString();

            int tileSize = mostCommonWidth + (PixelMargin * 2); // Effective tile size

            // Calculate occupied rows and columns based on most common resolution
            var occupiedPositions = resolutionGroups
                .Select(sprite => new
                {
                    Row = (sprite.TopEdge - 1) / tileSize,
                    Column = (sprite.LeftEdge - 1) / tileSize
                })
                .Distinct()
                .ToList();

            // Determine the max occupied row and column
            int maxOccupiedRow = occupiedPositions.Max(pos => pos.Row) + 1; // +1 to include zero-based index
            int maxOccupiedColumn = occupiedPositions.Max(pos => pos.Column) + 1;

            // Determine the min occupied row and column to trim unoccupied leading cells
            int minOccupiedRow = occupiedPositions.Min(pos => pos.Row);
            int minOccupiedColumn = occupiedPositions.Min(pos => pos.Column);

            // Adjust rows and columns by trimming unoccupied areas
            int effectiveRows = maxOccupiedRow - minOccupiedRow;
            int effectiveColumns = maxOccupiedColumn - minOccupiedColumn;

            // Adjust odd dimensions if near-square
            if (Math.Abs(effectiveRows - effectiveColumns) <= 3)
            {
                int maxDimension = Math.Max(effectiveRows, effectiveColumns);
                effectiveRows = maxDimension;
                effectiveColumns = maxDimension;
            }

            // Update textboxes with adjusted dimensions
            txtBoxRows.Text = effectiveRows.ToString();
            txtBoxColumns.Text = effectiveColumns.ToString();

            // Update occupied cells and grid
            UpdateOccupiedCells();
            HighlightGrid(CalculateIconCount());
            UpdateTooltips(); // Ensure tooltips are updated
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

            if (rows <= 0 || columns <= 0)
            {
                gridButtons = null; // Reset gridButtons if invalid dimensions
                return;
            }

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
                        BackColor = Color.White // Default color
                    };

                    btn.MouseDown += GridCell_MouseDown; // Attach MouseDown event
                    gridButtons[row, col] = btn;
                    pnlGridPreview.Controls.Add(btn);
                }
            }

            pnlGridPreview.ResumeLayout();

            HighlightExistingIcons(); // Apply highlights after regenerating grid
        }

        private void UpdateTooltips()
        {
            if (!int.TryParse(txtBoxWidth.Text, out int width) || width <= 0 ||
                !int.TryParse(txtBoxHeight.Text, out int height) || height <= 0 ||
                !int.TryParse(txtBoxMargins.Text, out int margin) || margin < 0)
            {
                return; // Exit if dimensions or margin are invalid
            }

            string selectedTexture = comboBoxTexture.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTexture))
            {
                return; // Exit if no texture is selected
            }

            int tileSize = width + (margin * 2);

            foreach (var sprite in sprites)
            {
                if (sprite.Texture == selectedTexture &&
                    sprite.Width == width &&
                    sprite.Height == height)
                {
                    int row = (sprite.TopEdge - 1) / tileSize;
                    int col = (sprite.LeftEdge - 1) / tileSize;

                    if (row >= 0 && row < gridButtons.GetLength(0) &&
                        col >= 0 && col < gridButtons.GetLength(1))
                    {
                        Button cellButton = gridButtons[row, col];

                        // Add or update the tooltip for this cell
                        string tooltipText = $"ID: {sprite.ID}\nResolution: {sprite.Width}x{sprite.Height}";
                        gridToolTip.SetToolTip(cellButton, tooltipText);
                    }
                }
            }
        }

        private void HighlightGrid(int totalIcons)
        {
            if (gridButtons == null)
                return; // Exit if gridButtons is null

            foreach (var btn in gridButtons)
            {
                Point cell = (Point)btn.Tag;

                // Determine the base color of the cell
                if (occupiedCells != null && occupiedCells.ContainsKey(cell))
                {
                    btn.BackColor = Color.Gray; // Persistent gray for occupied cells
                }
                else
                {
                    btn.BackColor = Color.White; // Default for unoccupied cells
                }
            }

            int currentRow = selectedCell.X;
            int currentCol = selectedCell.Y;

            // Highlight the selected and subsequent boxes in the range
            for (int i = 0; i < totalIcons; i++)
            {
                if (currentRow >= gridButtons.GetLength(0)) break;

                Button currentButton = gridButtons[currentRow, currentCol];

                // Check for overlap with occupied cells
                Point currentCell = new Point(currentRow, currentCol);
                if (occupiedCells != null && occupiedCells.ContainsKey(currentCell))
                {
                    currentButton.BackColor = i == 0
                        ? Color.DarkRed  // Overlapping starting box
                        : Color.LightCoral; // Overlapping subsequent box
                }
                else
                {
                    currentButton.BackColor = i == 0
                        ? Color.DarkBlue  // Selected starting box
                        : Color.LightBlue; // Subsequent boxes
                }

                // Move to the next cell
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
            if (!int.TryParse(txtBoxWidth.Text, out int width) || width <= 0 ||
                !int.TryParse(txtBoxHeight.Text, out int height) || height <= 0 ||
                !int.TryParse(txtBoxMargins.Text, out int margin) || margin < 0)
            {
                return; // Exit if dimensions or margin are invalid
            }

            string selectedTexture = comboBoxTexture.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTexture))
            {
                return; // Exit if no texture is selected
            }

            int tileSize = width + (margin * 2);

            foreach (var sprite in sprites)
            {
                if (sprite.Texture == selectedTexture &&
                    sprite.Width == width &&
                    sprite.Height == height)
                {
                    // Calculate the grid position
                    int row = (sprite.TopEdge - 1) / tileSize;
                    int col = (sprite.LeftEdge - 1) / tileSize;

                    if (row >= 0 && row < gridButtons.GetLength(0) &&
                        col >= 0 && col < gridButtons.GetLength(1))
                    {
                        Button cell = gridButtons[row, col];
                        cell.BackColor = Color.DarkGray; // Mark as occupied
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

        private void UpdateOccupiedCells()
        {
            if (mainForm == null || mainForm.Sprites == null)
            {
                occupiedCells = new Dictionary<Point, bool>();
                return; // Exit if mainForm or Sprites is null
            }

            int tileSize = Width + (PixelMargin * 2); // Effective tile size
            occupiedCells = new Dictionary<Point, bool>();

            foreach (var sprite in mainForm.Sprites)
            {
                if (sprite.Texture == SelectedTexture && sprite.Width == Width && sprite.Height == Height)
                {
                    // Calculate the grid cell from the sprite's coordinates
                    int column = (sprite.LeftEdge - 1) / tileSize;
                    int row = (sprite.TopEdge - 1) / tileSize;

                    Point cell = new Point(row, column);
                    occupiedCells[cell] = true;
                }
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
