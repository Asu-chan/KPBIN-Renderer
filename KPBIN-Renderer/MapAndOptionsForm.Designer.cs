namespace KPBIN_Renderer
{
    partial class MapAndOptionsForm
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
            this.mapListBox = new System.Windows.Forms.ListBox();
            this.tilesetsCheckBox = new System.Windows.Forms.CheckBox();
            this.doodadsCheckBox = new System.Windows.Forms.CheckBox();
            this.layersCheckBox = new System.Windows.Forms.CheckBox();
            this.worldmapCheckBox = new System.Windows.Forms.CheckBox();
            this.formatComboBox = new System.Windows.Forms.ComboBox();
            this.formatLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mapListBox
            // 
            this.mapListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mapListBox.FormattingEnabled = true;
            this.mapListBox.Location = new System.Drawing.Point(12, 12);
            this.mapListBox.Name = "mapListBox";
            this.mapListBox.Size = new System.Drawing.Size(165, 277);
            this.mapListBox.TabIndex = 0;
            this.mapListBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.mapListBox_MouseDoubleClick);
            // 
            // tilesetsCheckBox
            // 
            this.tilesetsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tilesetsCheckBox.AutoSize = true;
            this.tilesetsCheckBox.Location = new System.Drawing.Point(184, 13);
            this.tilesetsCheckBox.Name = "tilesetsCheckBox";
            this.tilesetsCheckBox.Size = new System.Drawing.Size(95, 17);
            this.tilesetsCheckBox.TabIndex = 1;
            this.tilesetsCheckBox.Text = "Export Tilesets";
            this.tilesetsCheckBox.UseVisualStyleBackColor = true;
            // 
            // doodadsCheckBox
            // 
            this.doodadsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.doodadsCheckBox.AutoSize = true;
            this.doodadsCheckBox.Location = new System.Drawing.Point(184, 36);
            this.doodadsCheckBox.Name = "doodadsCheckBox";
            this.doodadsCheckBox.Size = new System.Drawing.Size(102, 17);
            this.doodadsCheckBox.TabIndex = 2;
            this.doodadsCheckBox.Text = "Export Doodads";
            this.doodadsCheckBox.UseVisualStyleBackColor = true;
            // 
            // layersCheckBox
            // 
            this.layersCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.layersCheckBox.AutoSize = true;
            this.layersCheckBox.Location = new System.Drawing.Point(184, 59);
            this.layersCheckBox.Name = "layersCheckBox";
            this.layersCheckBox.Size = new System.Drawing.Size(90, 17);
            this.layersCheckBox.TabIndex = 3;
            this.layersCheckBox.Text = "Export Layers";
            this.layersCheckBox.UseVisualStyleBackColor = true;
            // 
            // worldmapCheckBox
            // 
            this.worldmapCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.worldmapCheckBox.AutoSize = true;
            this.worldmapCheckBox.Location = new System.Drawing.Point(184, 82);
            this.worldmapCheckBox.Name = "worldmapCheckBox";
            this.worldmapCheckBox.Size = new System.Drawing.Size(145, 17);
            this.worldmapCheckBox.TabIndex = 4;
            this.worldmapCheckBox.Text = "Export Worldmap Render";
            this.worldmapCheckBox.UseVisualStyleBackColor = true;
            // 
            // formatComboBox
            // 
            this.formatComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.formatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.formatComboBox.FormattingEnabled = true;
            this.formatComboBox.Items.AddRange(new object[] {
            ".png",
            ".jpg",
            ".gif",
            ".bmp",
            ".tiff"});
            this.formatComboBox.Location = new System.Drawing.Point(229, 105);
            this.formatComboBox.Name = "formatComboBox";
            this.formatComboBox.Size = new System.Drawing.Size(100, 21);
            this.formatComboBox.TabIndex = 5;
            // 
            // formatLabel
            // 
            this.formatLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.formatLabel.AutoSize = true;
            this.formatLabel.Location = new System.Drawing.Point(181, 108);
            this.formatLabel.Name = "formatLabel";
            this.formatLabel.Size = new System.Drawing.Size(42, 13);
            this.formatLabel.TabIndex = 6;
            this.formatLabel.Text = "Format:";
            // 
            // MapAndOptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(342, 301);
            this.Controls.Add(this.formatLabel);
            this.Controls.Add(this.formatComboBox);
            this.Controls.Add(this.worldmapCheckBox);
            this.Controls.Add(this.layersCheckBox);
            this.Controls.Add(this.doodadsCheckBox);
            this.Controls.Add(this.tilesetsCheckBox);
            this.Controls.Add(this.mapListBox);
            this.Name = "MapAndOptionsForm";
            this.ShowIcon = false;
            this.Text = "KPBIN Reverser - Map & Options";
            this.Load += new System.EventHandler(this.MapAndOptionsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox mapListBox;
        private System.Windows.Forms.CheckBox tilesetsCheckBox;
        private System.Windows.Forms.CheckBox doodadsCheckBox;
        private System.Windows.Forms.CheckBox layersCheckBox;
        private System.Windows.Forms.CheckBox worldmapCheckBox;
        private System.Windows.Forms.ComboBox formatComboBox;
        private System.Windows.Forms.Label formatLabel;
    }
}