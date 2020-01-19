using System;
using System.Windows.Media.Media3D;
using System.IO;
using Syroot.BinaryData;
using System.Collections.Generic;

namespace MarioKart
{
    public class ModelOctree
    {
        public uint Key { get; set; }

        public ModelOctree[] Children;

        public bool Selected { get; set; }

        public void Read(BinaryDataReader reader, uint parentPosition)
        {
            Key = reader.ReadUInt32();
            uint offset = parentPosition + Key & 0x3FFFFFFF;
            long pos = reader.Position;
            if (Key >> 31 != 0)
            {
                reader.Seek(2, SeekOrigin.Current);
                ushort lastIndex = 0;
                while (lastIndex != 0xFFFF)
                {
                    lastIndex = reader.ReadUInt16();
                }
            }
            else
            {
                Children = new ModelOctree[8];
                for (int i = 0; i < 8; i++)
                {
                    Children[i] = new ModelOctree();
                    Children[i].Read(reader, offset);
                }
            }

            reader.Seek(pos, SeekOrigin.Begin);
        }
    }

    public abstract class KCLHeader
	{
        public ModelOctree[] ModelOctrees;

        public UInt32 VerticesOffset;
		public UInt32 NormalsOffset;
		public UInt32 PlanesOffset;//-0x10
		public UInt32 OctreeOffset;
		public Single PrismThickness;
        public Single PrismCount;
        public Vector3D OctreeOrigin;
		public Vector3D OctreeMax;
		//public float n_x;
		//public float n_y;
		//public float n_z;
		public UInt32 XMask;
		public UInt32 YMask;
		public UInt32 ZMask;
		public UInt32 CoordShift;
		public UInt32 YShift;
		public UInt32 ZShift;
		public Single SphereRadius;
	}
}
