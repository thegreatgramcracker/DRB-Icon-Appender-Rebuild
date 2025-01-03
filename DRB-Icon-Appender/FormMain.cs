﻿using Octokit;
using Semver;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Windows.Forms;

namespace DRB_Icon_Appender
{
    public partial class FormMain : Form
    {
        private const string UPDATE_URL = "https://github.com/thegreatgramcracker/DRB-Icon-Appender-Rebuild/releases";
        private const string TPF_PATH = @"\menu\menu.tpf";
        private const string DRB_PATH = @"\menu\menu.drb";
        private static Properties.Settings settings = Properties.Settings.Default;

        private DRB.DRBVersion version;
        private DRB drb;
        private List<string> textures;
        private List<SpriteWrapper> sprites;
        private BindingSource iconBindingSource;

        internal List<SpriteWrapper> Sprites
        {
            get { return sprites; }
        }

        public FormMain()
        {
            InitializeComponent();
            dgvIcons.AutoGenerateColumns = false;
            iconBindingSource = new BindingSource();
            dgvIcons.DataSource = iconBindingSource;
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            Text = "DRB Icon Appender Rebuild " + System.Windows.Forms.Application.ProductVersion;
            Location = settings.WindowLocation;
            Size = settings.WindowSize;
            if (settings.WindowMaximized)
                WindowState = FormWindowState.Maximized;

            txtGameDir.Text = settings.GameDir;
            loadFiles(txtGameDir.Text, true);

            GitHubClient gitHubClient = new GitHubClient(new ProductHeaderValue("DRB-Icon-Appender-Rebuild"));
            try
            {
                Release release = await gitHubClient.Repository.Release.GetLatest("thegreatgramcracker", "DRB-Icon-Appender-Rebuild");
                if (SemVersion.Parse(release.TagName) > System.Windows.Forms.Application.ProductVersion)
                {
                    lblUpdate.Visible = false;
                    LinkLabel.Link link = new LinkLabel.Link();
                    link.LinkData = UPDATE_URL;
                    llbUpdate.Links.Add(link);
                    llbUpdate.Visible = true;
                }
                else
                {
                    lblUpdate.Text = "App up to date";
                }
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is ApiException || ex is ArgumentException)
            {
                lblUpdate.Text = "Update status unknown";
            }
        }

        private void llbUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            settings.WindowMaximized = WindowState == FormWindowState.Maximized;
            if (WindowState == FormWindowState.Normal)
            {
                settings.WindowLocation = Location;
                settings.WindowSize = Size;
            }
            else
            {
                settings.WindowLocation = RestoreBounds.Location;
                settings.WindowSize = RestoreBounds.Size;
            }

            settings.GameDir = txtGameDir.Text;
        }

        private void enableControls(bool enable)
        {
            btnBrowse.Enabled = !enable;
            btnOpen.Enabled = !enable;
            btnAddIcon.Enabled = enable;
            btnBatchAddIcons.Enabled = enable;
            btnSave.Enabled = enable;
            btnClose.Enabled = enable;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            ofdExecutable.InitialDirectory = txtGameDir.Text;
            if (ofdExecutable.ShowDialog() == DialogResult.OK)
            {
                txtGameDir.Text = Path.GetDirectoryName(ofdExecutable.FileName);
                loadFiles(txtGameDir.Text);
            }
        }

        private void btnExplore_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(txtGameDir.Text))
                Process.Start(txtGameDir.Text);
            else
                SystemSounds.Hand.Play();
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            closeFiles();
            string drbPath = txtGameDir.Text + DRB_PATH + (version == DRB.DRBVersion.DarkSoulsRemastered ? ".dcx" : "");
            if (File.Exists(drbPath + ".bak"))
            {
                File.Delete(drbPath);
                File.Move(drbPath + ".bak", drbPath);
            }
            loadFiles(txtGameDir.Text);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string drbPath = txtGameDir.Text + DRB_PATH + (version == DRB.DRBVersion.DarkSoulsRemastered ? ".dcx" : "");
            if (!File.Exists(drbPath + ".bak"))
                File.Copy(drbPath, drbPath + ".bak");
            drb.Write(drbPath);
            SystemSounds.Asterisk.Play();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            loadFiles(txtGameDir.Text);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            closeFiles();
        }

        private bool shapePresent(int id)
        {
            foreach (SpriteWrapper sprite in sprites)
                if (sprite.ID == id)
                    return true;
            return false;
        }

        private void btnAddIcon_Click(object sender, EventArgs e)
        {
            int id = (int)nudIconID.Value;

            if (shapePresent(id))
            {
                DialogResult choice = MessageBox.Show("That icon ID is already in use.\nWould you like to use the next available one?",
                    "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (choice == DialogResult.Yes)
                {
                    while (shapePresent(id))
                        id++;
                    if (id > 9999)
                    {
                        ShowError("ID may not exceed 9999.");
                        return;
                    }
                    nudIconID.Value = id;
                }
                else
                    return;
            }

            var shape = new DRB.Shape.Sprite()
            {
                TexLeftEdge = 1,
                TexTopEdge = 1,
                TexRightEdge = (short)(version == DRB.DRBVersion.DarkSoulsRemastered ? 160 : 80),
                TexBottomEdge = (short)(version == DRB.DRBVersion.DarkSoulsRemastered ? 180 : 90),
            };
            var control = new DRB.Control.Static();
            var dlgo = new DRB.Dlgo($"EquIcon_{id:D4}", shape, control);
            DRB.Dlg icons = drb.Dlgs.Find(dlg => dlg.Name == "Icon");
            icons.Dlgos.Add(dlgo);

            var sprite = new SpriteWrapper(dlgo, textures);
            iconBindingSource.Add(sprite);
            sprites.Sort();

            foreach (DataGridViewRow row in dgvIcons.Rows)
                if ((int)row.Cells[0].Value == id)
                    dgvIcons.CurrentCell = row.Cells[0];
        }

        public bool RangeHasDuplicates(int startId, int endId, int totalIcons, out List<int> duplicateIds, out (int Start, int End)? nextAvailableRange)
        {
            duplicateIds = new List<int>();
            nextAvailableRange = null;

            // Check all existing IDs in the sprites list
            HashSet<int> existingIds = sprites.Select(sprite => sprite.ID).ToHashSet();

            // Check the range for duplicates
            for (int id = startId; id <= endId; id++)
            {
                if (existingIds.Contains(id))
                {
                    duplicateIds.Add(id);
                }
            }

            // If duplicates exist, calculate the next available range
            if (duplicateIds.Count > 0)
            {
                int nextStart = startId;
                while (existingIds.Contains(nextStart))
                    nextStart++;

                int nextEnd = nextStart + totalIcons - 1;
                while (existingIds.Contains(nextEnd))
                {
                    nextStart++;
                    nextEnd = nextStart + totalIcons - 1;
                }

                if (nextEnd <= 9999) // Ensure the range doesn't exceed the limit
                    nextAvailableRange = (nextStart, nextEnd);
            }

            return duplicateIds.Count > 0;
        }

        public void BatchAddIcons(int startId, int endId, string selectedTexture, int width, int height, int rows, int columns, int margin, int startRow, int startColumn)
        {
            if (startId > endId)
            {
                MessageBox.Show("The starting ID must not exceed the ending ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate the range for duplicates
            if (RangeHasDuplicates(startId, endId, endId - startId + 1, out List<int> duplicateIds, out var nextAvailableRange))
            {
                string duplicateMessage = $"The following IDs are already in use: {string.Join(", ", duplicateIds)}.";
                if (nextAvailableRange.HasValue)
                {
                    duplicateMessage += $"\n\nSuggested Range: {nextAvailableRange.Value.Start} to {nextAvailableRange.Value.End}.";
                }
                MessageBox.Show(duplicateMessage, "Duplicate IDs Detected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Cancel operation if duplicates exist
            }

            // Correct calculation for tile width and height
            int tileWidth = width + (2 * margin);  // Full tile width, including margins
            int tileHeight = height + (2 * margin); // Full tile height, including margins

            // Initialize starting position
            int leftEdge = 1 + (startColumn * tileWidth);  // Start from the first column
            int topEdge = 1 + (startRow * tileHeight);     // Start from the first row

            // Initialize row and column counters
            int currentColumn = startColumn;
            int currentRow = startRow;

            for (int id = startId; id <= endId; id++)
            {
                // Adjust for non-first icons in the row or column
                int adjustedLeftEdge = leftEdge + (currentColumn > 0 ? 1 : 0); // +1 if not the first column
                int adjustedTopEdge = topEdge + (currentRow > 0 ? 1 : 0);      // +1 if not the first row

                // Create the new sprite shape with accurate edges
                var shape = new DRB.Shape.Sprite()
                {
                    TexLeftEdge = (short)adjustedLeftEdge,
                    TexTopEdge = (short)adjustedTopEdge,
                    TexRightEdge = (short)(adjustedLeftEdge + width),
                    TexBottomEdge = (short)(adjustedTopEdge + height),
                };

                var control = new DRB.Control.Static();
                var dlgo = new DRB.Dlgo($"EquIcon_{id:D4}", shape, control);
                DRB.Dlg icons = drb.Dlgs.Find(dlg => dlg.Name == "Icon");
                icons.Dlgos.Add(dlgo);

                var sprite = new SpriteWrapper(dlgo, textures);
                sprite.Texture = selectedTexture;
                iconBindingSource.Add(sprite);

                // Update Left Edge and Top Edge for the next icon
                currentColumn++;

                if (currentColumn >= columns)
                {
                    // Move to the next row
                    currentColumn = 0;
                    currentRow++;
                    leftEdge = 1;  // Reset Left Edge for the new row
                    topEdge = 1 + (currentRow * tileHeight); // Increment Top Edge
                }
                else
                {
                    // Stay in the same row: Increment Left Edge
                    leftEdge = 1 + (currentColumn * tileWidth);
                }
            }

            sprites.Sort();
            dgvIcons.Refresh();
        }

        private void btnBatchAddIcons_Click(object sender, EventArgs e)
        {
            using (var batchAddForm = new FormBatchAdd(this, this.Textures, this.Sprites))
            {
                if (batchAddForm.ShowDialog() == DialogResult.OK)
                {
                    BatchAddIcons(
                        batchAddForm.RangeStart,
                        batchAddForm.RangeEnd,
                        batchAddForm.SelectedTexture,
                        batchAddForm.Width,
                        batchAddForm.Height,
                        batchAddForm.Rows,
                        batchAddForm.Columns,
                        batchAddForm.PixelMargin,
                        batchAddForm.StartRow,
                        batchAddForm.StartColumn
                    );
                }
            }
        }

        private void loadFiles(string gameDir, bool silent = false)
        {
            if (File.Exists($@"{gameDir}\DARKSOULS.exe") || File.Exists($@"{gameDir}\EBOOT.bin"))
            {
                version = DRB.DRBVersion.DarkSouls;
            }
            else if (File.Exists($@"{gameDir}\DarkSoulsRemastered.exe"))
            {
                version = DRB.DRBVersion.DarkSoulsRemastered;
            }
            else
            {
                ShowError($"Dark Souls executable not found in directory:\n{gameDir}", silent);
                return;
            }

            TPF menuTPF;
            string tpfPath = $"{gameDir}{TPF_PATH}{(version == DRB.DRBVersion.DarkSoulsRemastered ? ".dcx" : "")}";
            try
            {
                menuTPF = TPF.Read(tpfPath);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to read TPF:\n{tpfPath}\n\n{ex}", silent);
                return;
            }

            DRB menuDRB;
            string drbPath = $"{gameDir}{DRB_PATH}{(version == DRB.DRBVersion.DarkSoulsRemastered ? ".dcx" : "")}";
            try
            {
                menuDRB = DRB.Read(drbPath, version);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to read DRB:\n{drbPath}{ex}", silent);
                return;
            }

            fillDataGridView(menuTPF, menuDRB);
            enableControls(true);
        }

        public void fillDataGridView(TPF menuTPF, DRB menuDRB)
        {
            iconBindingSource.Clear();
            textures = new List<string>();
            foreach (TPF.Texture entry in menuTPF.Textures)
                textures.Add(entry.Name);

            List<string> sortedNames = new List<string>(textures);
            sortedNames.Sort();
            dgvIconsTextureCol.DataSource = sortedNames;

            drb = menuDRB;
            sprites = new List<SpriteWrapper>();

            DRB.Dlg icons = menuDRB.Dlgs.Find(dlg => dlg.Name == "Icon");
            foreach (DRB.Dlgo dlgo in icons.Dlgos.Where(dlgo => dlgo.Shape is DRB.Shape.Sprite))
            {
                sprites.Add(new SpriteWrapper(dlgo, textures));
            }
            sprites.Sort();
            iconBindingSource.DataSource = sprites;
        }
        public List<string> Textures
        {
            get { return textures; }
        }

        private void closeFiles()
        {
            iconBindingSource.Clear();
            drb = null;
            textures = null;
            sprites = null;
            enableControls(false);
        }

        private void dgvIcons_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            SystemSounds.Hand.Play();
            e.Cancel = true;
        }

        private void ShowError(string message, bool silent = false)
        {
            if (!silent)
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void dgvIcons_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
