using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCLExt
{
    public class CollisionPresetData
    {
        public string GameTitle;

        public string Comments;

        public float PrismThickness = 30f;
        public float SphereRadius = 25f;

        public Dictionary<ushort, string> MaterialPresets = new Dictionary<ushort, string>();

        public ushort GetMaterialID(string type) {
            return MaterialPresets.FirstOrDefault(x => x.Value == type).Key;
        }
    }

    public class CollisionEntry
    {
        public string Name { get; set; }
        public string Type
        {
            get
            {
                if (MaterialSetForm.ActiveGamePreset != null &&
                    MaterialSetForm.ActiveGamePreset.MaterialPresets?.Count > 0)
                {
                    if (MaterialSetForm.ActiveGamePreset.MaterialPresets.ContainsKey(TypeID))
                        return MaterialSetForm.ActiveGamePreset.MaterialPresets[TypeID];
                }
                return TypeID.ToString();
            }
        }

        public ushort TypeID { get; set; }

        public CollisionEntry(string name)
        {
            Name = name;
        }
    }

}
