namespace KPBIN_Renderer
{
    partial class UnlockDataForm
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
            this.nameLabel = new System.Windows.Forms.Label();
            this.normalCheckbox = new System.Windows.Forms.CheckBox();
            this.secretCheckbox = new System.Windows.Forms.CheckBox();
            this.comparisonNumUpDown = new System.Windows.Forms.NumericUpDown();
            this.okButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.comparisonNumUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // nameLabel
            // 
            this.nameLabel.Location = new System.Drawing.Point(12, 9);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(153, 16);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "1-1 (01-01)";
            this.nameLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // normalCheckbox
            // 
            this.normalCheckbox.AutoSize = true;
            this.normalCheckbox.Location = new System.Drawing.Point(171, 8);
            this.normalCheckbox.Name = "normalCheckbox";
            this.normalCheckbox.Size = new System.Drawing.Size(59, 17);
            this.normalCheckbox.TabIndex = 1;
            this.normalCheckbox.Text = "Normal";
            this.normalCheckbox.UseVisualStyleBackColor = true;
            this.normalCheckbox.CheckedChanged += new System.EventHandler(this.normalCheckbox_CheckedChanged);
            // 
            // secretCheckbox
            // 
            this.secretCheckbox.AutoSize = true;
            this.secretCheckbox.Location = new System.Drawing.Point(236, 8);
            this.secretCheckbox.Name = "secretCheckbox";
            this.secretCheckbox.Size = new System.Drawing.Size(57, 17);
            this.secretCheckbox.TabIndex = 2;
            this.secretCheckbox.Text = "Secret";
            this.secretCheckbox.UseVisualStyleBackColor = true;
            this.secretCheckbox.CheckedChanged += new System.EventHandler(this.secretCheckbox_CheckedChanged);
            // 
            // comparisonNumUpDown
            // 
            this.comparisonNumUpDown.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.comparisonNumUpDown.Location = new System.Drawing.Point(171, 31);
            this.comparisonNumUpDown.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.comparisonNumUpDown.Name = "comparisonNumUpDown";
            this.comparisonNumUpDown.Size = new System.Drawing.Size(59, 20);
            this.comparisonNumUpDown.TabIndex = 3;
            this.comparisonNumUpDown.ValueChanged += new System.EventHandler(this.comparisonNumUpDown_ValueChanged);
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(171, 165);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // UnlockDataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(385, 200);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.comparisonNumUpDown);
            this.Controls.Add(this.secretCheckbox);
            this.Controls.Add(this.normalCheckbox);
            this.Controls.Add(this.nameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "UnlockDataForm";
            this.ShowIcon = false;
            this.Text = "Change Unlock Data";
            this.Load += new System.EventHandler(this.UnlockDataForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.comparisonNumUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.CheckBox normalCheckbox;
        private System.Windows.Forms.CheckBox secretCheckbox;
        private System.Windows.Forms.NumericUpDown comparisonNumUpDown;
        private System.Windows.Forms.Button okButton;
    }
}