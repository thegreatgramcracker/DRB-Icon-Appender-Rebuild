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

            chkAutoSet.CheckedChanged += ChkAutoSet_CheckedChanged;

            // Update Effective Tile Size on load
            UpdateEffectiveTileSize();
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

        private const int MaxPanelWidth = 322;  // Maximum width of the grid panel
        private const int MaxPanelHeight = 322; // Maximum height of the grid panel

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

            int startRow = selectedCell.X;
            int startColumn = selectedCell.Y;
            int remainingRows = rows - startRow;
            int remainingColumns = columns - startColumn;

            int availableIcons = (remainingRows - 1) * columns + remainingColumns;

            int startId = (int)nudBatchAddRangeStart.Value;
            int minEnd = startId;
            int maxEnd = Math.Min(9999, startId + availableIcons - 1); // Enforce hard limit at 9999

            nudBatchAddRangeEnd.Minimum = minEnd;
            nudBatchAddRangeEnd.Maximum = maxEnd;

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
            nudBatchAddRangeStart.ValueChanged -= nudBatchAddRangeStart_ValueChanged;
            nudBatchAddRangeEnd.ValueChanged -= nudBatchAddRangeEnd_ValueChanged;

            if (newStart.HasValue)
            {
                nudBatchAddRangeStart.Value = newStart.Value;
            }

            UpdateRangeEndConstraints();

            int currentStart = (int)nudBatchAddRangeStart.Value;
            int calculatedEnd = rangeDifference == 0 ? currentStart : currentStart + rangeDifference;
            calculatedEnd = Math.Min(9999, Math.Max((int)nudBatchAddRangeEnd.Minimum, Math.Min((int)nudBatchAddRangeEnd.Maximum, calculatedEnd)));

            if (newEnd.HasValue)
            {
                nudBatchAddRangeEnd.Value = newEnd.Value;
            }
            else
            {
                nudBatchAddRangeEnd.Value = calculatedEnd;
            }

            rangeDifference = (int)nudBatchAddRangeEnd.Value - (int)nudBatchAddRangeStart.Value;

            nudBatchAddRangeStart.ValueChanged += nudBatchAddRangeStart_ValueChanged;
            nudBatchAddRangeEnd.ValueChanged += nudBatchAddRangeEnd_ValueChanged;

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
            var matchingSprites = sprites
                .Where(sprite => sprite.Texture == selectedTexture)
                .ToList();

            if (!matchingSprites.Any())
            {
                return;
            }

            var mostCommonDimensions = matchingSprites
                .GroupBy(sprite => new { sprite.Width, sprite.Height })
                .OrderByDescending(group => group.Count())
                .First()
                .Key;

            txtBoxWidth.Text = mostCommonDimensions.Width.ToString();
            txtBoxHeight.Text = mostCommonDimensions.Height.ToString();

            if (!int.TryParse(txtBoxMargins.Text, out int margin) || margin < 0)
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

            int totalIcons = CalculateIconCount();
            HighlightGrid(totalIcons);

            UpdateRangeEndConstraints();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtBoxWidth.Text, out int width) || width <= 0)
            {
                MessageBox.Show("Please enter a valid positive integer for Width.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtBoxHeight.Text, out int height) || height <= 0)
            {
                MessageBox.Show("Please enter a valid positive integer for Height.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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

            int totalIcons = RangeEnd - RangeStart + 1;
            if (mainForm.RangeHasDuplicates(RangeStart, RangeEnd, totalIcons, out List<int> duplicateIds, out var nextAvailableRange))
            {
                string duplicateMessage = $"The following IDs are already in use: {string.Join(", ", duplicateIds)}.";
                if (nextAvailableRange.HasValue)
                {
                    duplicateMessage += $"\n\nSuggested Range: {nextAvailableRange.Value.Start} to {nextAvailableRange.Value.End}.";
                }
                MessageBox.Show(duplicateMessage, "Duplicate IDs Detected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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

            if (!int.TryParse(txtBoxWidth.Text, out int width) || width <= 0 ||
                !int.TryParse(txtBoxHeight.Text, out int height) || height <= 0 ||
                !int.TryParse(txtBoxMargins.Text, out int margin) || margin < 0)
            {
                gridButtons = null;
                return; // Exit on invalid input
            }

            int effectiveWidth = width + (margin * 2);
            int effectiveHeight = height + (margin * 2);

            float cellAspectRatio = (float)effectiveWidth / effectiveHeight;

            int maxCellWidth = MaxPanelWidth / columns;
            int maxCellHeight = MaxPanelHeight / rows;

            int cellWidth, cellHeight;

            if (cellAspectRatio > 1)
            {
                cellWidth = Math.Min(maxCellWidth, maxCellHeight * effectiveWidth / effectiveHeight);
                cellHeight = cellWidth * effectiveHeight / effectiveWidth;
            }
            else
            {
                cellHeight = Math.Min(maxCellHeight, maxCellWidth * effectiveHeight / effectiveWidth);
                cellWidth = cellHeight * effectiveWidth / effectiveHeight;
            }

            int gridWidth = columns * cellWidth;
            int gridHeight = rows * cellHeight;

            pnlGridPreview.Width = gridWidth;
            pnlGridPreview.Height = gridHeight;

            gridButtons = new Button[rows, columns];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    Button btn = new Button
                    {
                        Width = cellWidth,
                        Height = cellHeight,
                        Location = new Point(col * cellWidth, row * cellHeight),
                        Tag = new Point(row, col),
                        BackColor = Color.White
                    };

                    btn.MouseDown += GridCell_MouseDown;
                    gridButtons[row, col] = btn;
                    pnlGridPreview.Controls.Add(btn);
                }
            }

            pnlGridPreview.ResumeLayout();
            HighlightExistingIcons();
            UpdateTooltips();
        }

        private void UpdateTooltips()
        {
            if (gridButtons == null || sprites == null) return;

            if (!int.TryParse(txtBoxWidth.Text, out int width) || width <= 0 ||
                !int.TryParse(txtBoxHeight.Text, out int height) || height <= 0 ||
                !int.TryParse(txtBoxMargins.Text, out int margin) || margin < 0)
            {
                return;
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
                    btn.BackColor = Color.Gray;
                }
                else
                {
                    btn.BackColor = Color.White;
                }
            }

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
                    currentButton.BackColor = i == 0 ? Color.DarkRed : Color.LightCoral;
                }
                else
                {
                    currentButton.BackColor = i == 0 ? Color.DarkBlue : Color.LightBlue;
                }

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
                    int effectiveWidth = Width + (PixelMargin * 2);
                    int effectiveHeight = Height + (PixelMargin * 2);

                    int row = (sprite.TopEdge - 1) / effectiveHeight;
                    int col = (sprite.LeftEdge - 1) / effectiveWidth;

                    if (row >= 0 && row < gridButtons.GetLength(0) &&
                        col >= 0 && col < gridButtons.GetLength(1))
                    {
                        gridButtons[row, col].BackColor = Color.DarkGray;
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
                    selectedCell = clickedCell;
                    UpdateRangeEndConstraints();
                    HighlightGrid(CalculateIconCount());
                }
                else if (e.Button == MouseButtons.Right)
                {
                    AdjustEndValueToIncludeCell(clickedCell);
                }
            }
        }

        private void AdjustEndValueToIncludeCell(Point targetCell)
        {
            int targetRow = targetCell.X;
            int targetCol = targetCell.Y;

            int startRow = selectedCell.X;
            int startCol = selectedCell.Y;

            int rowDifference = targetRow - startRow;
            int colDifference = targetCol - startCol;

            int totalIcons = rowDifference * gridButtons.GetLength(1) + colDifference + 1;

            int newEndValue = (int)nudBatchAddRangeStart.Value + totalIcons - 1;
            newEndValue = Math.Min(9999, Math.Min((int)nudBatchAddRangeEnd.Maximum, Math.Max((int)nudBatchAddRangeEnd.Minimum, newEndValue)));

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
            if (!chkAutoSet.Checked) return;

            string selectedTexture = comboBoxTexture.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTexture))
            {
                return;
            }

            if (!int.TryParse(txtBoxWidth.Text, out int width) || width <= 0 ||
                !int.TryParse(txtBoxHeight.Text, out int height) || height <= 0)
            {
                return;
            }

            int rows = 11;
            int columns = 12;

            if (width == 160 && height == 180)
            {
                if ((selectedTexture == "Icon40" || selectedTexture == "Icon12" || selectedTexture == "Icon21") &&
                    !HasOccupiedCellsBeyondRow(5))
                {
                    rows = 5;
                }
            }
            else if (width == 80 && height == 80)
            {
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

            txtBoxRows.Text = rows.ToString();
            txtBoxColumns.Text = columns.ToString();
        }

        private bool HasOccupiedCellsBeyondRow(int maxRow)
        {
            foreach (var cell in occupiedCells.Keys)
            {
                if (cell.X > maxRow)
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasOccupiedCellsBeyondColumn(int maxColumn)
        {
            foreach (var cell in occupiedCells.Keys)
            {
                if (cell.Y > maxColumn)
                {
                    return true;
                }
            }
            return false;
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

                if (int.TryParse(txtBoxRows.Text, out int rows) && rows > 0 &&
                    int.TryParse(txtBoxColumns.Text, out int columns) && columns > 0)
                {
                    GenerateGrid(rows, columns);
                    UpdateOccupiedCells();
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

            txtBoxRows.Enabled = !isAutoSetEnabled;
            txtBoxColumns.Enabled = !isAutoSetEnabled;
            txtBoxWidth.Enabled = !isAutoSetEnabled;
            txtBoxHeight.Enabled = !isAutoSetEnabled;
            txtBoxMargins.Enabled = !isAutoSetEnabled;

            if (isAutoSetEnabled)
            {
                AutoSetGridSize();
            }
        }

        private void btnAutoSetRange_Click(object sender, EventArgs e)
        {
            int totalIcons = RangeEnd - RangeStart + 1;
            if (mainForm.RangeHasDuplicates((int)nudBatchAddRangeStart.Value, (int)nudBatchAddRangeEnd.Value, totalIcons, out _, out var nextAvailableRange))
            {
                if (nextAvailableRange.HasValue)
                {
                    nudBatchAddRangeStart.Value = nextAvailableRange.Value.Start;
                    nudBatchAddRangeEnd.Value = nextAvailableRange.Value.End;
                }
                else
                {
                    MessageBox.Show("No available range found within the valid ID limits.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("The current range is already valid.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
