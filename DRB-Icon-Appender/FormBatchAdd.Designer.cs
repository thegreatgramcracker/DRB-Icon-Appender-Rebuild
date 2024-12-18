namespace DRB_Icon_Appender
{
    partial class FormBatchAdd
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBatchAdd));
            this.nudBatchAddRangeStart = new System.Windows.Forms.NumericUpDown();
            this.nudBatchAddRangeEnd = new System.Windows.Forms.NumericUpDown();
            this.grpBoxIconRange = new System.Windows.Forms.GroupBox();
            this.lblDivider = new System.Windows.Forms.Label();
            this.grpBoxTexture = new System.Windows.Forms.GroupBox();
            this.comboBoxTexture = new System.Windows.Forms.ComboBox();
            this.txtBoxWidth = new System.Windows.Forms.TextBox();
            this.txtBoxHeight = new System.Windows.Forms.TextBox();
            this.lblWidth = new System.Windows.Forms.Label();
            this.lblHeight = new System.Windows.Forms.Label();
            this.grpBoxIconResolution = new System.Windows.Forms.GroupBox();
            this.txtBoxRows = new System.Windows.Forms.TextBox();
            this.lblRows = new System.Windows.Forms.Label();
            this.txtBoxColumns = new System.Windows.Forms.TextBox();
            this.lblColumns = new System.Windows.Forms.Label();
            this.grpBoxGridSize = new System.Windows.Forms.GroupBox();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudBatchAddRangeStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBatchAddRangeEnd)).BeginInit();
            this.grpBoxIconRange.SuspendLayout();
            this.grpBoxTexture.SuspendLayout();
            this.grpBoxIconResolution.SuspendLayout();
            this.grpBoxGridSize.SuspendLayout();
            this.SuspendLayout();
            // 
            // nudBatchAddRangeStart
            // 
            this.nudBatchAddRangeStart.Location = new System.Drawing.Point(6, 19);
            this.nudBatchAddRangeStart.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudBatchAddRangeStart.Name = "nudBatchAddRangeStart";
            this.nudBatchAddRangeStart.Size = new System.Drawing.Size(62, 20);
            this.nudBatchAddRangeStart.TabIndex = 0;
            this.nudBatchAddRangeStart.Value = new decimal(new int[] {
            5500,
            0,
            0,
            0});
            this.nudBatchAddRangeStart.ValueChanged += new System.EventHandler(this.nudBatchAddRangeStart_ValueChanged);
            // 
            // nudBatchAddRangeEnd
            // 
            this.nudBatchAddRangeEnd.Location = new System.Drawing.Point(89, 19);
            this.nudBatchAddRangeEnd.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudBatchAddRangeEnd.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBatchAddRangeEnd.Name = "nudBatchAddRangeEnd";
            this.nudBatchAddRangeEnd.Size = new System.Drawing.Size(62, 20);
            this.nudBatchAddRangeEnd.TabIndex = 1;
            this.nudBatchAddRangeEnd.Value = new decimal(new int[] {
            5600,
            0,
            0,
            0});
            this.nudBatchAddRangeEnd.ValueChanged += new System.EventHandler(this.nudBatchAddRangeEnd_ValueChanged);
            // 
            // grpBoxIconRange
            // 
            this.grpBoxIconRange.Controls.Add(this.nudBatchAddRangeEnd);
            this.grpBoxIconRange.Controls.Add(this.nudBatchAddRangeStart);
            this.grpBoxIconRange.Controls.Add(this.lblDivider);
            this.grpBoxIconRange.Location = new System.Drawing.Point(12, 12);
            this.grpBoxIconRange.Name = "grpBoxIconRange";
            this.grpBoxIconRange.Size = new System.Drawing.Size(158, 51);
            this.grpBoxIconRange.TabIndex = 2;
            this.grpBoxIconRange.TabStop = false;
            this.grpBoxIconRange.Text = "Icon Range";
            this.grpBoxIconRange.Enter += new System.EventHandler(this.grpBoxIconRange_Enter);
            // 
            // lblDivider
            // 
            this.lblDivider.Location = new System.Drawing.Point(6, 21);
            this.lblDivider.Name = "lblDivider";
            this.lblDivider.Size = new System.Drawing.Size(149, 13);
            this.lblDivider.TabIndex = 0;
            this.lblDivider.Text = "to";
            this.lblDivider.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDivider.Click += new System.EventHandler(this.lblDivider_Click);
            // 
            // grpBoxTexture
            // 
            this.grpBoxTexture.Controls.Add(this.comboBoxTexture);
            this.grpBoxTexture.Location = new System.Drawing.Point(176, 12);
            this.grpBoxTexture.Name = "grpBoxTexture";
            this.grpBoxTexture.Size = new System.Drawing.Size(158, 51);
            this.grpBoxTexture.TabIndex = 3;
            this.grpBoxTexture.TabStop = false;
            this.grpBoxTexture.Text = "Texture";
            this.grpBoxTexture.Enter += new System.EventHandler(this.grpBoxTexture_Enter);
            // 
            // comboBoxTexture
            // 
            this.comboBoxTexture.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTexture.FormattingEnabled = true;
            this.comboBoxTexture.Location = new System.Drawing.Point(6, 18);
            this.comboBoxTexture.Name = "comboBoxTexture";
            this.comboBoxTexture.Size = new System.Drawing.Size(146, 21);
            this.comboBoxTexture.TabIndex = 4;
            // 
            // txtBoxWidth
            // 
            this.txtBoxWidth.Location = new System.Drawing.Point(71, 19);
            this.txtBoxWidth.Name = "txtBoxWidth";
            this.txtBoxWidth.Size = new System.Drawing.Size(62, 20);
            this.txtBoxWidth.TabIndex = 4;
            this.txtBoxWidth.Text = "80";
            // 
            // txtBoxHeight
            // 
            this.txtBoxHeight.Location = new System.Drawing.Point(71, 46);
            this.txtBoxHeight.Name = "txtBoxHeight";
            this.txtBoxHeight.Size = new System.Drawing.Size(62, 20);
            this.txtBoxHeight.TabIndex = 5;
            this.txtBoxHeight.Text = "80";
            this.txtBoxHeight.TextChanged += new System.EventHandler(this.txtBoxHeight_TextChanged);
            // 
            // lblWidth
            // 
            this.lblWidth.AutoSize = true;
            this.lblWidth.Location = new System.Drawing.Point(33, 22);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(35, 13);
            this.lblWidth.TabIndex = 6;
            this.lblWidth.Text = "Width";
            this.lblWidth.Click += new System.EventHandler(this.lblWidth_Click);
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.lblHeight.Location = new System.Drawing.Point(30, 49);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(38, 13);
            this.lblHeight.TabIndex = 7;
            this.lblHeight.Text = "Height";
            this.lblHeight.Click += new System.EventHandler(this.lblHeight_Click);
            // 
            // grpBoxIconResolution
            // 
            this.grpBoxIconResolution.Controls.Add(this.lblHeight);
            this.grpBoxIconResolution.Controls.Add(this.txtBoxWidth);
            this.grpBoxIconResolution.Controls.Add(this.lblWidth);
            this.grpBoxIconResolution.Controls.Add(this.txtBoxHeight);
            this.grpBoxIconResolution.Location = new System.Drawing.Point(12, 69);
            this.grpBoxIconResolution.Name = "grpBoxIconResolution";
            this.grpBoxIconResolution.Size = new System.Drawing.Size(158, 78);
            this.grpBoxIconResolution.TabIndex = 8;
            this.grpBoxIconResolution.TabStop = false;
            this.grpBoxIconResolution.Text = "Icon Resolution";
            this.grpBoxIconResolution.Enter += new System.EventHandler(this.grpBoxIconResolution_Enter);
            // 
            // txtBoxRows
            // 
            this.txtBoxRows.Location = new System.Drawing.Point(71, 19);
            this.txtBoxRows.Name = "txtBoxRows";
            this.txtBoxRows.Size = new System.Drawing.Size(62, 20);
            this.txtBoxRows.TabIndex = 8;
            this.txtBoxRows.Text = "12";
            this.txtBoxRows.TextChanged += new System.EventHandler(this.txtBoxRows_TextChanged);
            // 
            // lblRows
            // 
            this.lblRows.AutoSize = true;
            this.lblRows.Location = new System.Drawing.Point(31, 22);
            this.lblRows.Name = "lblRows";
            this.lblRows.Size = new System.Drawing.Size(34, 13);
            this.lblRows.TabIndex = 9;
            this.lblRows.Text = "Rows";
            // 
            // txtBoxColumns
            // 
            this.txtBoxColumns.Location = new System.Drawing.Point(71, 46);
            this.txtBoxColumns.Name = "txtBoxColumns";
            this.txtBoxColumns.Size = new System.Drawing.Size(62, 20);
            this.txtBoxColumns.TabIndex = 10;
            this.txtBoxColumns.Text = "12";
            this.txtBoxColumns.TextChanged += new System.EventHandler(this.txtBoxColumns_TextChanged);
            // 
            // lblColumns
            // 
            this.lblColumns.AutoSize = true;
            this.lblColumns.Location = new System.Drawing.Point(18, 49);
            this.lblColumns.Name = "lblColumns";
            this.lblColumns.Size = new System.Drawing.Size(47, 13);
            this.lblColumns.TabIndex = 11;
            this.lblColumns.Text = "Columns";
            // 
            // grpBoxGridSize
            // 
            this.grpBoxGridSize.Controls.Add(this.txtBoxRows);
            this.grpBoxGridSize.Controls.Add(this.lblRows);
            this.grpBoxGridSize.Controls.Add(this.txtBoxColumns);
            this.grpBoxGridSize.Controls.Add(this.lblColumns);
            this.grpBoxGridSize.Location = new System.Drawing.Point(176, 69);
            this.grpBoxGridSize.Name = "grpBoxGridSize";
            this.grpBoxGridSize.Size = new System.Drawing.Size(158, 78);
            this.grpBoxGridSize.TabIndex = 12;
            this.grpBoxGridSize.TabStop = false;
            this.grpBoxGridSize.Text = "Grid Size";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(12, 161);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(158, 23);
            this.btnConfirm.TabIndex = 13;
            this.btnConfirm.Text = "Confirm";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(176, 161);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(158, 23);
            this.btnCancel.TabIndex = 14;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // FormBatchAdd
            // 
            this.AcceptButton = this.btnConfirm;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(346, 198);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.grpBoxIconResolution);
            this.Controls.Add(this.grpBoxTexture);
            this.Controls.Add(this.grpBoxIconRange);
            this.Controls.Add(this.grpBoxGridSize);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormBatchAdd";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "  Add Icons";
            ((System.ComponentModel.ISupportInitialize)(this.nudBatchAddRangeStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBatchAddRangeEnd)).EndInit();
            this.grpBoxIconRange.ResumeLayout(false);
            this.grpBoxTexture.ResumeLayout(false);
            this.grpBoxIconResolution.ResumeLayout(false);
            this.grpBoxIconResolution.PerformLayout();
            this.grpBoxGridSize.ResumeLayout(false);
            this.grpBoxGridSize.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NumericUpDown nudBatchAddRangeStart;
        private System.Windows.Forms.NumericUpDown nudBatchAddRangeEnd;
        private System.Windows.Forms.GroupBox grpBoxIconRange;
        private System.Windows.Forms.Label lblDivider;
        private System.Windows.Forms.GroupBox grpBoxTexture;
        private System.Windows.Forms.ComboBox comboBoxTexture;
        private System.Windows.Forms.TextBox txtBoxWidth;
        private System.Windows.Forms.TextBox txtBoxHeight;
        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.GroupBox grpBoxIconResolution;
        private System.Windows.Forms.TextBox txtBoxRows;
        private System.Windows.Forms.Label lblRows;
        private System.Windows.Forms.TextBox txtBoxColumns;
        private System.Windows.Forms.Label lblColumns;
        private System.Windows.Forms.GroupBox grpBoxGridSize;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnConfirm;
    }
}