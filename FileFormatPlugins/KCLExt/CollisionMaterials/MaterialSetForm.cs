using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KCLExt
{
	public partial class MaterialSetForm : Form
	{
        bool isLoaded = false;

        public Dictionary<string, ushort> Result;
        public ByamlExt.Byaml.BymlFileData AttributeByml;

        public string[] Materials;
        public string[] Meshes;

        public List<CollisionEntry> MatCollisionList = new List<CollisionEntry>();
        public List<CollisionEntry> MeshCollisionList = new List<CollisionEntry>();

        public bool UseObjectMaterials => radioBtnMats.Checked;

        private MaterialGridView DataGridView;
        private OdysseyCollisionPicker OdysseyCollisionPicker;
        private MaterialCollisionPicker MaterialCollisionPicker;

        private MaterialSetForm(string[] mats, string[] meshes)
		{
            Meshes = meshes;
            Materials = mats;
            InitializeComponent();

            for (int i = 0; i < Materials.Length; i++)
                MatCollisionList.Add(new CollisionEntry(Materials[i]));
            for (int i = 0; i < Meshes.Length; i++)
                MeshCollisionList.Add(new CollisionEntry(Meshes[i]));

            foreach (var preset in MarioKart.MK7.KCL.CollisionPresets)
                gameSelectToolStripMenuItem.DropDownItems.Add(new ToolStripMenuItem(preset.GameTitle,
                    null, GamePresetToolStripMenuItem_Click));

            radioBtnMats.Checked = true;
            SetMaterialEditor();
            ReloadDataList();

            isLoaded = true;
        }

        private void SetMaterialEditor()
        {
            panel1.Controls.Clear();
            if (ActiveGamePreset != null && chkPresetTypeEditor.Checked && ActiveGamePreset.GameTitle == "Mario Odyssey")
            {
                DataGridView = null;
                MaterialCollisionPicker = null;
                OdysseyCollisionPicker = new OdysseyCollisionPicker(this);
                OdysseyCollisionPicker.Dock = DockStyle.Fill;
                panel1.Controls.Add(OdysseyCollisionPicker);
            }
            else if (ActiveGamePreset != null && ActiveGamePreset.MaterialPresets?.Count > 0 && chkPresetTypeEditor.Checked)
            {
                DataGridView = null;
                OdysseyCollisionPicker = null;
                MaterialCollisionPicker = new MaterialCollisionPicker(this);
                MaterialCollisionPicker.Dock = DockStyle.Fill;
                panel1.Controls.Add(MaterialCollisionPicker);
            }
            else
            {
                MaterialCollisionPicker = null;
                OdysseyCollisionPicker = null;
                DataGridView = new MaterialGridView(this);
                DataGridView.Dock = DockStyle.Fill;
                panel1.Controls.Add(DataGridView);
            }
        }

        public static CollisionPresetData ActiveGamePreset = null;

        public void ReloadDataList()
        {
            var colList = UseObjectMaterials ? MatCollisionList : MeshCollisionList;

            if (DataGridView != null) 
                DataGridView.ReloadDataList(colList);
            else if (MaterialCollisionPicker != null)
                MaterialCollisionPicker.ReloadDataList(colList);
            else if (OdysseyCollisionPicker != null)
                OdysseyCollisionPicker.ReloadDataList();
        }

        public static MaterialSetForm ShowForm(string[] materials, string[] meshes)
		{
			MaterialSetForm f = new MaterialSetForm(materials, meshes);
			f.ShowDialog();
			return f;
		}

		private void FClosing(object sender, FormClosingEventArgs e)
		{
            if (DataGridView != null)
                Result = DataGridView.Result;
            else if (MaterialCollisionPicker != null)
                Result = MaterialCollisionPicker.Result;
            else if (OdysseyCollisionPicker != null)
            {
                Result = OdysseyCollisionPicker.Result;
                AttributeByml = OdysseyCollisionPicker.GenerateByaml();
            }

            if (DataGridView != null)
                DataGridView.Dispose();
            if (MaterialCollisionPicker != null)
                MaterialCollisionPicker.Dispose();
            if (OdysseyCollisionPicker != null)
                OdysseyCollisionPicker.Dispose();
        }

		private void applyToolStripMenuItem_Click(object sender, EventArgs e)
		{
            if (ActiveGamePreset == null)
            {
                MessageBox.Show("Make sure to choose a game preset first!");
                return;
            }

            if (DataGridView != null) {
                DataGridView.EndEdit();
            }
            this.Close();
        }

        private void GamePresetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem menu in gameSelectToolStripMenuItem.DropDownItems)
                menu.Checked = false;

            if (sender is ToolStripMenuItem) {
                ((ToolStripMenuItem)sender).Checked = true;

                string name = ((ToolStripMenuItem)sender).Text;
                foreach (var preset in MarioKart.MK7.KCL.CollisionPresets) {
                    if (name == preset.GameTitle) {
                        ActiveGamePreset = preset;
                    }
                }

                SetMaterialEditor();
                ReloadDataList();
            }
        }

        private void radioBtnMats_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void radioBtnMeshes_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void radioBtnMats_Click(object sender, EventArgs e) {
            radioBtnMeshes.Checked = !radioBtnMats.Checked;
            ReloadDataList();
        }

        private void radioBtnMeshes_Click(object sender, EventArgs e) {
            radioBtnMats.Checked = !radioBtnMeshes.Checked;
            ReloadDataList();
        }

        private void chkOdysseyTypeEditor_CheckedChanged(object sender, EventArgs e) {
            if (isLoaded) {
                SetMaterialEditor();
                ReloadDataList();
            }
        }
    }
}
