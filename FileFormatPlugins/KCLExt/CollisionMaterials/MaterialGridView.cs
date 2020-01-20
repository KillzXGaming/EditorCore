﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KCLExt
{
    public partial class MaterialGridView : UserControl
    {
        private MaterialSetForm ParentEditor;

        public string[] Materials => ParentEditor.Materials;
        public string[] Meshes => ParentEditor.Meshes;

        public bool UseObjectMaterials => ParentEditor.UseObjectMaterials;

        public List<string> MaterialPreset = new List<string>();
        private DataGridViewComboBoxColumn presetsCB;

        public Dictionary<string, ushort> Result
        {
            get
            {
                var result = new Dictionary<string, ushort>();
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    var v = dataGridView1[1, i].Value.ToString();
                    result.Add(dataGridView1[0, i].Value.ToString(), v == "-1" ? ushort.MaxValue : ushort.Parse(v));
                }
                return result;
            }
        }

        public MaterialGridView(MaterialSetForm parentForm)
        {
            InitializeComponent();

            ParentEditor = parentForm;
            presetsCB = new DataGridViewComboBoxColumn();
            presetsCB.Width = 140;
            presetsCB.HeaderText = "Presets";
            presetsCB.DataPropertyName = "Type";
            dataGridView1.Columns.Add(presetsCB);

            dataGridView1.CellValueChanged +=
       new DataGridViewCellEventHandler(dataGridView1_CellValueChanged);
            dataGridView1.CurrentCellDirtyStateChanged +=
                 new EventHandler(dataGridView1_CurrentCellDirtyStateChanged);

            if (this.dataGridView1.IsCurrentCellDirty)
            {
                // This fires the cell value changed handler below
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        public void ReloadDataList()
        {
            presetsCB.DataSource = MaterialPreset;

            dataGridView1.Rows.Clear();

            if (UseObjectMaterials)
            {
                for (int i = 0; i < Materials.Length; i++)
                    dataGridView1.Rows.Add(Materials[i], 0);
            }
            else
            {
                for (int i = 0; i < Meshes.Length; i++)
                    dataGridView1.Rows.Add(Meshes[i], 0);
            }

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[2] is DataGridViewComboBoxCell)
                {
                    var cb = ((DataGridViewComboBoxCell)dataGridView1.Rows[i].Cells[2]);
                    if (cb.Items.Count > 0)
                    {
                        cb.Value = cb.Items[0];
                    }
                }
            }
        }

        private void dataGridView1_CurrentCellDirtyStateChanged(object sender,
        EventArgs e)
        {

        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1) return;

            DataGridViewComboBoxCell cb = (DataGridViewComboBoxCell)dataGridView1.Rows[e.RowIndex].Cells[2];
            if (cb.Value != null)
            {
                dataGridView1.Invalidate();
                string key = (string)cb.Value;
                if (MaterialSetForm.ActiveGamePreset == MaterialSetForm.ActivePreset.MK8)
                {
                    if (CollisionMk8.ContainsKey(key))
                    {
                        dataGridView1.Rows[e.RowIndex].Cells[1].Value = CollisionMk8[key];
                    }
                }

                dataGridView1.RefreshEdit();
            }
        }

        public void EndEdit() {
            dataGridView1.EndEdit();
        }

        public void LoadMk8TypePresets()
        {
            MaterialPreset.Clear();
            foreach (var item in CollisionMk8.Keys)
                MaterialPreset.Add(item);
        }

        private Dictionary<string, int> CollisionMk8 = new Dictionary<string, int>()
        {
            { "Road", 0 },
            { "Road (Bumpy)", 2 },
            { "Road (Slippery)", 4 },
            { "Road (Offroad Sand)", 6 },
            { "Road (Slippery Effect Only)", 9 },
            { "Road (Booster)", 10 },
            { "Latiku" ,16 },
            { "Glider", 31 },
            { "Road (Foamy Sound)", 32 },
            { "Road (Offroad, clicking Sound)", 40 },
            { "Unsolid",56 },
            { "Water (Drown reset)", 60 },
            { "Road (Rocky Sound)", 64 },
            { "Wall", 81 },
            { "Road (3DS MP Piano)", 129 },
            { "Road (RoyalR Offroad Grass)", 134 },
            { "Road (3DS MP Xylophone)", 161 },
            { "Road (3DS MP Vibraphone)", 193 },
            { "Road (SNES RR road)", 227 },
            { "Road (MKS Offroad Grass)", 297 },
            { "Road (Water Wall)", 500 },
            { "Road (Stunt)", 4096 },
            { "Road (Booster + Stunt)", 4106 },
            { "Road (Stunt + Glider)", 4108 },
        };
    }
}