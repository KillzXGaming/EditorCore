namespace KCLExt
{
	partial class MaterialSetForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.applyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameSelectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.marioOdysseyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Mk8PresetToolstrip = new System.Windows.Forms.ToolStripMenuItem();
            this.Splatoon2PresetToolstrip = new System.Windows.Forms.ToolStripMenuItem();
            this.SplatoonPresetToolstrip = new System.Windows.Forms.ToolStripMenuItem();
            this.OtherPresetToolstrip = new System.Windows.Forms.ToolStripMenuItem();
            this.radioBtnMats = new System.Windows.Forms.RadioButton();
            this.radioBtnMeshes = new System.Windows.Forms.RadioButton();
            this.chkOdysseyTypeEditor = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applyToolStripMenuItem,
            this.gameSelectToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(531, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // applyToolStripMenuItem
            // 
            this.applyToolStripMenuItem.Name = "applyToolStripMenuItem";
            this.applyToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.applyToolStripMenuItem.Text = "Apply";
            this.applyToolStripMenuItem.Click += new System.EventHandler(this.applyToolStripMenuItem_Click);
            // 
            // gameSelectToolStripMenuItem
            // 
            this.gameSelectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.marioOdysseyToolStripMenuItem,
            this.Mk8PresetToolstrip,
            this.Splatoon2PresetToolstrip,
            this.SplatoonPresetToolstrip,
            this.OtherPresetToolstrip});
            this.gameSelectToolStripMenuItem.Name = "gameSelectToolStripMenuItem";
            this.gameSelectToolStripMenuItem.Size = new System.Drawing.Size(84, 20);
            this.gameSelectToolStripMenuItem.Text = "Game Select";
            // 
            // marioOdysseyToolStripMenuItem
            // 
            this.marioOdysseyToolStripMenuItem.Name = "marioOdysseyToolStripMenuItem";
            this.marioOdysseyToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.marioOdysseyToolStripMenuItem.Text = "Mario Odyssey";
            this.marioOdysseyToolStripMenuItem.Click += new System.EventHandler(this.GamePresetToolStripMenuItem_Click);
            // 
            // Mk8PresetToolstrip
            // 
            this.Mk8PresetToolstrip.Name = "Mk8PresetToolstrip";
            this.Mk8PresetToolstrip.Size = new System.Drawing.Size(216, 22);
            this.Mk8PresetToolstrip.Text = "Mario Kart 8 Wii U / Deluxe";
            this.Mk8PresetToolstrip.Click += new System.EventHandler(this.GamePresetToolStripMenuItem_Click);
            // 
            // Splatoon2PresetToolstrip
            // 
            this.Splatoon2PresetToolstrip.Name = "Splatoon2PresetToolstrip";
            this.Splatoon2PresetToolstrip.Size = new System.Drawing.Size(216, 22);
            this.Splatoon2PresetToolstrip.Text = "Splatoon 2";
            this.Splatoon2PresetToolstrip.Click += new System.EventHandler(this.GamePresetToolStripMenuItem_Click);
            // 
            // SplatoonPresetToolstrip
            // 
            this.SplatoonPresetToolstrip.Name = "SplatoonPresetToolstrip";
            this.SplatoonPresetToolstrip.Size = new System.Drawing.Size(216, 22);
            this.SplatoonPresetToolstrip.Text = "Splatoon";
            this.SplatoonPresetToolstrip.Click += new System.EventHandler(this.GamePresetToolStripMenuItem_Click);
            // 
            // OtherPresetToolstrip
            // 
            this.OtherPresetToolstrip.Name = "OtherPresetToolstrip";
            this.OtherPresetToolstrip.Size = new System.Drawing.Size(216, 22);
            this.OtherPresetToolstrip.Text = "Other";
            this.OtherPresetToolstrip.Click += new System.EventHandler(this.GamePresetToolStripMenuItem_Click);
            // 
            // radioBtnMats
            // 
            this.radioBtnMats.AutoSize = true;
            this.radioBtnMats.Location = new System.Drawing.Point(157, 4);
            this.radioBtnMats.Name = "radioBtnMats";
            this.radioBtnMats.Size = new System.Drawing.Size(120, 17);
            this.radioBtnMats.TabIndex = 2;
            this.radioBtnMats.TabStop = true;
            this.radioBtnMats.Text = "Material by materials";
            this.radioBtnMats.UseVisualStyleBackColor = true;
            this.radioBtnMats.CheckedChanged += new System.EventHandler(this.radioBtnMats_CheckedChanged);
            this.radioBtnMats.Click += new System.EventHandler(this.radioBtnMats_Click);
            // 
            // radioBtnMeshes
            // 
            this.radioBtnMeshes.AutoSize = true;
            this.radioBtnMeshes.Location = new System.Drawing.Point(283, 4);
            this.radioBtnMeshes.Name = "radioBtnMeshes";
            this.radioBtnMeshes.Size = new System.Drawing.Size(115, 17);
            this.radioBtnMeshes.TabIndex = 3;
            this.radioBtnMeshes.TabStop = true;
            this.radioBtnMeshes.Text = "Material by meshes";
            this.radioBtnMeshes.UseVisualStyleBackColor = true;
            this.radioBtnMeshes.CheckedChanged += new System.EventHandler(this.radioBtnMeshes_CheckedChanged);
            this.radioBtnMeshes.Click += new System.EventHandler(this.radioBtnMeshes_Click);
            // 
            // chkOdysseyTypeEditor
            // 
            this.chkOdysseyTypeEditor.AutoSize = true;
            this.chkOdysseyTypeEditor.Checked = true;
            this.chkOdysseyTypeEditor.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOdysseyTypeEditor.Location = new System.Drawing.Point(404, 5);
            this.chkOdysseyTypeEditor.Name = "chkOdysseyTypeEditor";
            this.chkOdysseyTypeEditor.Size = new System.Drawing.Size(126, 17);
            this.chkOdysseyTypeEditor.TabIndex = 4;
            this.chkOdysseyTypeEditor.Text = "Odyssey Type  Editor";
            this.chkOdysseyTypeEditor.UseVisualStyleBackColor = true;
            this.chkOdysseyTypeEditor.CheckedChanged += new System.EventHandler(this.chkOdysseyTypeEditor_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(531, 292);
            this.panel1.TabIndex = 5;
            // 
            // MaterialSetForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(531, 316);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.chkOdysseyTypeEditor);
            this.Controls.Add(this.radioBtnMeshes);
            this.Controls.Add(this.radioBtnMats);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MaterialSetForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Write the material code, close this to continue";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem applyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameSelectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem marioOdysseyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem Mk8PresetToolstrip;
        private System.Windows.Forms.ToolStripMenuItem Splatoon2PresetToolstrip;
        private System.Windows.Forms.ToolStripMenuItem SplatoonPresetToolstrip;
        private System.Windows.Forms.ToolStripMenuItem OtherPresetToolstrip;
        private System.Windows.Forms.RadioButton radioBtnMats;
        private System.Windows.Forms.RadioButton radioBtnMeshes;
        private System.Windows.Forms.CheckBox chkOdysseyTypeEditor;
        private System.Windows.Forms.Panel panel1;
    }
}