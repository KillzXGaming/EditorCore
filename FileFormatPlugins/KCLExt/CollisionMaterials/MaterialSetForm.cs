using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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

        public bool UseObjectMaterials => radioBtnMats.Checked;

        private MaterialGridView DataGridView;
        private OdysseyCollisionPicker OdysseyCollisionPicker;
        private MaterialSetForm(string[] mats, string[] meshes)
		{
            Meshes = meshes;
            Materials = mats;
            InitializeComponent();

            chkOdysseyTypeEditor.Visible = false;
            radioBtnMats.Checked = true;
            SetMaterialEditor();
            ReloadDataList();

            isLoaded = true;
        }

        private void SetMaterialEditor()
        {
            panel1.Controls.Clear();
            if (chkOdysseyTypeEditor.Checked && ActiveGamePreset == ActivePreset.SMO)
            {
                DataGridView = null;
                OdysseyCollisionPicker = new OdysseyCollisionPicker(this);
                OdysseyCollisionPicker.Dock = DockStyle.Fill;
                panel1.Controls.Add(OdysseyCollisionPicker);
            }
            else
            {
                OdysseyCollisionPicker = null;
                DataGridView = new MaterialGridView(this);
                DataGridView.Dock = DockStyle.Fill;
                panel1.Controls.Add(DataGridView);
            }
        }

        public static ActivePreset ActiveGamePreset = ActivePreset.None;

        public enum ActivePreset
        {
            None,
            SMO,
            MK8,
            Splatoon2,
            Splatoon,
            Other,
        }

        public void ReloadDataList()
        {
            if (DataGridView != null) {
                DataGridView.ReloadDataList();
            }
            else
            {
                OdysseyCollisionPicker.ReloadDataList();
            }
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
            else
            {
                Result = OdysseyCollisionPicker.Result;
                AttributeByml = OdysseyCollisionPicker.GenerateByaml();
            }
		}

		private void applyToolStripMenuItem_Click(object sender, EventArgs e)
		{
            if (ActiveGamePreset == ActivePreset.None)
            {
                MessageBox.Show("Make sure to choose a game preset first!");
                return;
            }

            if (DataGridView != null)
                DataGridView.EndEdit();
            this.Close();
		}

        private void GamePresetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DataGridView != null)
                DataGridView.MaterialPreset.Clear();

            marioOdysseyToolStripMenuItem.Checked = false;
            Mk8PresetToolstrip.Checked = false;
            Splatoon2PresetToolstrip.Checked = false;
            SplatoonPresetToolstrip.Checked = false;
            OtherPresetToolstrip.Checked = false;

            if (sender is ToolStripMenuItem) {
                ((ToolStripMenuItem)sender).Checked = true;

                string name = ((ToolStripMenuItem)sender).Text;
                if (name == "Mario Odyssey")
                {
                    ActiveGamePreset = ActivePreset.SMO;
                    chkOdysseyTypeEditor.Visible = true;
                }
                else if (name == "Mario Kart 8 Wii U / Deluxe")
                    ActiveGamePreset = ActivePreset.MK8;
                else if (name == "Splatoon 2")
                    ActiveGamePreset = ActivePreset.Splatoon2;
                else if (name == "Splatoon")
                    ActiveGamePreset = ActivePreset.Splatoon;
                else
                    ActiveGamePreset = ActivePreset.Other;

                if (ActiveGamePreset != ActivePreset.SMO)
                    chkOdysseyTypeEditor.Visible = false;

                SetMaterialEditor();

                if (ActiveGamePreset == ActivePreset.MK8)
                    DataGridView.LoadMk8TypePresets();

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
