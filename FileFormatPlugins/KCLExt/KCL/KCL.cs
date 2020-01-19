using System;
using System.Collections.Generic;
using System.IO;
using Syroot.BinaryData;
using Syroot.NintenTools.MarioKart8.IO;
using EditorCore.Common;
using LibEveryFileExplorer._3D;
using System.Windows.Media.Media3D;
using ExtensionMethods;
using KCLExt;

namespace MarioKart.MK7
{
    public class KCL
    {
        public KCLModel.KCLModelHeader GlobalHeader;
        public List<KCLModel> Models = new List<KCLModel>();

        internal static Vector3D MinOffset
        {
            get
            {
                if (EFESettings)
                    return new Vector3D(50f, 80f, 50f);
                else
                    return new Vector3D(60, 60, 60);
            }
        }

        internal static Vector3D MaxOffset
        {
            get
            {
                if (EFESettings)
                    return new Vector3D(50f, 50f, 50f);
                else
                    return new Vector3D(60, 60, 60);
            }
        }

        public KCL() { }


        public KCL(byte[] Data, ByteOrder bo = ByteOrder.LittleEndian)
        {
            BinaryDataReader er = new BinaryDataReader(new MemoryStream(Data));
            er.ByteOrder = ByteOrder.BigEndian;
            if (er.ReadUInt32() != 0x02020000) throw new Exception("Wrong KCL Header");
            er.ByteOrder = bo;

            uint OctreeOffset = er.ReadUInt32();
            uint ModelListOff = er.ReadUInt32();
            uint ModelCount = er.ReadUInt32();
            GlobalHeader = new KCLModel.KCLModelHeader();
            GlobalHeader.OctreeOrigin = er.ReadVector3D();
            GlobalHeader.OctreeMax = er.ReadVector3D();
            GlobalHeader.CoordShift = er.ReadUInt32();
            GlobalHeader.YShift = er.ReadUInt32();
            GlobalHeader.ZShift = er.ReadUInt32();
            GlobalHeader.PrismCount = er.ReadUInt32();

            er.BaseStream.Position = OctreeOffset;
            GlobalHeader.ModelOctrees = new ModelOctree[8];
            for (int g = 0; g < 8; g++)
            {
                GlobalHeader.ModelOctrees[g] = new ModelOctree();
                GlobalHeader.ModelOctrees[g].Read(er, OctreeOffset);
            }

            for (int ModelIndex = 0; ModelIndex < ModelCount; ModelIndex++)
            {
                KCLModel mod = new KCLModel();

                er.BaseStream.Position = ModelListOff + ModelIndex * 4;
                uint CurModelOffset = er.ReadUInt32();
                er.BaseStream.Position = CurModelOffset;

                mod.Header = new KCLModel.KCLModelHeader(er);
                er.BaseStream.Position = mod.Header.VerticesOffset + CurModelOffset;
                uint nr = (mod.Header.NormalsOffset - mod.Header.VerticesOffset) / 0xC;
                mod.Vertices = new Vector3D[nr];
                for (int i = 0; i < nr; i++) mod.Vertices[i] = er.ReadVector3D();

                er.BaseStream.Position = mod.Header.NormalsOffset + CurModelOffset;
                nr = (mod.Header.PlanesOffset - mod.Header.NormalsOffset) / 0xC;
                mod.Normals = new Vector3D[nr];
                for (int i = 0; i < nr; i++) mod.Normals[i] = er.ReadVector3D();

                er.BaseStream.Position = mod.Header.PlanesOffset + CurModelOffset;
                nr = (mod.Header.OctreeOffset - mod.Header.PlanesOffset) / 0x14;
                mod.Planes = new KCLModel.KCLPlane[nr];
                for (int i = 0; i < nr; i++) mod.Planes[i] = new KCLModel.KCLPlane(er);

                er.BaseStream.Position = mod.Header.OctreeOffset + CurModelOffset;
                int nodes = (int)(
                    ((~mod.Header.XMask >> (int)mod.Header.CoordShift) + 1) *
                    ((~mod.Header.YMask >> (int)mod.Header.CoordShift) + 1) *
                    ((~mod.Header.ZMask >> (int)mod.Header.CoordShift) + 1));
                mod.Octree = new KCLOctree(er, nodes);

                //Generate a mesh from the octree
                //List<OBJ.Face> OctreeNodeToMesh(KCLOctree.KCLOctreeNode node)
                //{
                //	List<OBJ.Face> res = new List<OBJ.Face>();
                //	if (node.IsLeaf)
                //	{
                //		foreach (var tr in node.Triangles)
                //		{
                //			var t = mod.GetTriangle(mod.Planes[tr]);
                //			res.Add(new OBJ.Face()
                //			{
                //				VA = new OBJ.Vertex(t.PointA, t.Normal),
                //				VB = new OBJ.Vertex(t.PointB, t.Normal),
                //				VC = new OBJ.Vertex(t.PointC, t.Normal),
                //			});
                //		}
                //	}
                //	else
                //	{
                //		for (int i = 0; i < node.SubNodes.Length; i++)
                //			res.AddRange(OctreeNodeToMesh(node.SubNodes[i]));
                //	}
                //	return res;
                //}

                //var o = new OBJ();
                //for (int i = 0; i < mod.Octree.RootNodes.Length; i++)
                //{
                //	var planes = OctreeNodeToMesh(mod.Octree.RootNodes[i]);
                //	if (planes.Count == 0) continue;
                //	o.Faces.AddRange(planes);
                //}

                //o.toWritableObj().WriteObj($"F:\\oct{ModelIndex}.obj", null);

                Models.Add(mod);
            }
        }

        public byte[] Write(ByteOrder byteOrder)
        {
            if (Models.Count == 0)
                throw new Exception("The root octree is not complete");

            var size = GlobalHeader.OctreeMax - GlobalHeader.OctreeOrigin;

            using (MemoryStream m = new MemoryStream())
            {
                BinaryDataWriter er = new BinaryDataWriter(m);
                //Write KCL Header
                er.ByteOrder = ByteOrder.BigEndian; //The signature is always big endian
                er.Write(0x02020000);
                er.ByteOrder = byteOrder;
                er.Write((UInt32)0x38);
                er.Write((UInt32)0x58);
                er.Write((UInt32)Models.Count);
                er.Write(GlobalHeader.OctreeOrigin);
                er.Write(GlobalHeader.OctreeMax);
                er.Write((UInt32)GlobalHeader.CoordShift);
                er.Write((UInt32)GlobalHeader.YShift);
                er.Write((UInt32)GlobalHeader.ZShift);
                er.Write((UInt32)GlobalHeader.PrismCount);
                List<KCLModel> WriteModels = new List<KCLModel>();
                uint modelCount = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (Models.Count == 1)
                    {
                        er.Write((UInt32)(0x80000000));
                        if (i == 0)
                        {
                            modelCount = 1;
                            WriteModels.Add(Models[i]);
                        }
                    }
                    else if (Models.Count > i && Models[i] != null)
                    {
                        er.Write((UInt32)(0x80000000 | modelCount));
                        modelCount++;
                        WriteModels.Add(Models[i]);
                    }
                    else
                        er.Write((UInt32)(0xC0000000));
                }
                if (modelCount == 0)
                    throw new Exception("No models in the global octree");

                uint ModelListOff = (uint)er.BaseStream.Position;

                er.BaseStream.Position = 0xC;
                er.Write((UInt32)modelCount);
                er.BaseStream.Position = ModelListOff;

                for (int i = 0; i < modelCount; i++) //Update offsets later
                    er.Write((UInt32)0);

                for (int i = 0; i < modelCount; i++)
                {
                    er.Align(4);
                    uint pos = (uint)er.BaseStream.Position;
                    er.BaseStream.Position = ModelListOff + i * 4;
                    er.Write((UInt32)pos);
                    er.BaseStream.Position = pos;
                    WriteModel(er, WriteModels[i]);
                }

                return m.ToArray();
            }
        }

        static void WriteModel(BinaryDataWriter er, KCLModel mod)
        {
            long HeaderPos = er.BaseStream.Position;
            mod.Header.Write(er);
            long curpos = er.BaseStream.Position;
            //Write vertices array position
            er.BaseStream.Position = HeaderPos;
            er.Write((uint)(curpos - HeaderPos));
            er.BaseStream.Position = curpos;
            foreach (Vector3D v in mod.Vertices) er.Write(v);
            er.Align(4);
            curpos = er.BaseStream.Position;
            //Write normal array position
            er.BaseStream.Position = HeaderPos + 4;
            er.Write((uint)(curpos - HeaderPos));
            er.BaseStream.Position = curpos;
            foreach (Vector3D v in mod.Normals) er.Write(v);
            er.Align(4);
            curpos = er.BaseStream.Position;
            //Write Triangles offset
            er.BaseStream.Position = HeaderPos + 8;
            er.Write((uint)(curpos - HeaderPos));
            er.BaseStream.Position = curpos;

            foreach (KCLModel.KCLPlane p in mod.Planes) p.Write(er);
            curpos = er.BaseStream.Position;
            //Write Spatial index offset
            er.BaseStream.Position = HeaderPos + 12;
            er.Write((uint)(curpos - HeaderPos));
            er.BaseStream.Position = curpos;
            mod.Octree.Write(er);
        }

        public static Vector3D NormalAvg(OBJ.Face face) => (face.VA.normal + face.VB.normal + face.VC.normal) / 3;

        public static bool SplitModels = false;
        public static bool EFESettings = false;

        public static KCL FromOBJ(OBJ o)
        {
            KCL res = new KCL();
            res.GlobalHeader = new KCLModel.KCLModelHeader();

            Vector3D min = new Vector3D(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3D max = new Vector3D(float.MinValue, float.MinValue, float.MinValue);

            var form = MaterialSetForm.ShowForm(o.MaterialNames, o.MeshNames.ToArray());
            var MapMeshes = !form.UseObjectMaterials;
            var MatDictionary = form.Result;

            STProgressBar progressBar = new STProgressBar();
            progressBar.Value = 0;
            progressBar.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            progressBar.Show();
            progressBar.Refresh();

            progressBar.Task = "Reading Object...";
            progressBar.Value = 10;
            progressBar.Refresh();

            List<Triangle> Triangles = new List<Triangle>();
            foreach (var v in o.Faces)
            {
                ushort coll = 0;
                if (MapMeshes)
                {
                    if (MatDictionary.ContainsKey(v.MeshObject))
                        coll = MatDictionary[v.MeshObject];
                    if (MatDictionary.ContainsKey(v.MeshGroup))
                        coll = MatDictionary[v.MeshGroup];
                }
                else
                    coll = MatDictionary.ContainsKey(v.Mat) ? MatDictionary[v.Mat] : (ushort)0;
                if (coll == ushort.MaxValue)
                    continue;

                Triangle t = new Triangle(v.VA.pos, v.VB.pos, v.VC.pos);
                t.Collision = coll;

                #region FindMaxMin
                if (t.PointA.X < min.X) min.X = t.PointA.X;
                if (t.PointA.Y < min.Y) min.Y = t.PointA.Y;
                if (t.PointA.Z < min.Z) min.Z = t.PointA.Z;
                if (t.PointA.X > max.X) max.X = t.PointA.X;
                if (t.PointA.Y > max.Y) max.Y = t.PointA.Y;
                if (t.PointA.Z > max.Z) max.Z = t.PointA.Z;

                if (t.PointB.X < min.X) min.X = t.PointB.X;
                if (t.PointB.Y < min.Y) min.Y = t.PointB.Y;
                if (t.PointB.Z < min.Z) min.Z = t.PointB.Z;
                if (t.PointB.X > max.X) max.X = t.PointB.X;
                if (t.PointB.Y > max.Y) max.Y = t.PointB.Y;
                if (t.PointB.Z > max.Z) max.Z = t.PointB.Z;

                if (t.PointC.X < min.X) min.X = t.PointC.X;
                if (t.PointC.Y < min.Y) min.Y = t.PointC.Y;
                if (t.PointC.Z < min.Z) min.Z = t.PointC.Z;
                if (t.PointC.X > max.X) max.X = t.PointC.X;
                if (t.PointC.Y > max.Y) max.Y = t.PointC.Y;
                if (t.PointC.Z > max.Z) max.Z = t.PointC.Z;
                #endregion

                Triangles.Add(t);
            }

            max += KCL.MaxOffset;
            min -= KCL.MinOffset;
            res.GlobalHeader.OctreeOrigin = min;
            res.GlobalHeader.OctreeMax = max;
            var size = max - min;

            res.GlobalHeader.CoordShift = (uint)KCLOctree.next_exponent(size.X);
            res.GlobalHeader.YShift = (uint)KCLOctree.next_exponent(size.Y);
            res.GlobalHeader.ZShift = (uint)KCLOctree.next_exponent(size.Z);

            int MaxPolygonPerModelOctree = (65535 / 4);

            var BoxSize = size / 2f;
            uint baseTriCount = 0;
            int octreeIndex = 0;

            if (Triangles.Count > MaxPolygonPerModelOctree)
                EFESettings = false;

            if (Triangles.Count < MaxPolygonPerModelOctree)
            {
                progressBar.Task = $"Generating Octree {0}";
                progressBar.Value = ((4 * 100) / 8);
                progressBar.Refresh();

                var Boxmin = min + new Vector3D(BoxSize.X * 0, BoxSize.Y * 0, BoxSize.Z * 0);
                var mod = KCLModel.FromTriangles(Triangles, baseTriCount, Boxmin, BoxSize / 2, false);
                res.Models.Add(mod);
                if (mod != null) baseTriCount += (uint)mod.Planes.Length;
            }
            else
            {
                for (int k = 0; k < 2; k++)
                    for (int l = 0; l < 2; l++)
                        for (int m = 0; m < 2; m++)
                        {
                            progressBar.Task = $"Generating Octree {octreeIndex}";
                            progressBar.Value = ((octreeIndex * 100) / 8);
                            progressBar.Refresh();

                            var Boxmin = min + new Vector3D(BoxSize.X * m, BoxSize.Y * l, BoxSize.Z * k);
                            var mod = KCLModel.FromTriangles(Triangles, baseTriCount, Boxmin, BoxSize / 2, true);
                            res.Models.Add(mod);
                            if (mod != null) baseTriCount += (uint)mod.Planes.Length;

                            octreeIndex++;
                        }
            }
                

            res.GlobalHeader.PrismCount = baseTriCount;

            progressBar.Value = 100;
            progressBar.Close();
            progressBar.Dispose();

            return res;
        }

        private static ushort AddIfNotContainsVector3D(Vector3D a, List<Vector3D> b)
        {
            var i = ContainsVector3D(a, b);
            if (i == -1)
            {
                i = b.Count;
                b.Add(a);
            }
            return (ushort)i;
        }

        private static int ContainsVector3D(Vector3D a, List<Vector3D> b)
        {
            for (int i = 0; i < b.Count; i++)
            {
                if (b[i].X == a.X && b[i].Y == a.Y && b[i].Z == a.Z)
                {
                    return i;
                }
            }
            return -1;
        }

        public OBJ ToOBJ()
        {
            //Debug only: export the models in different files
            //int count = 0;
            //foreach (var model in Models)
            //{
            //	using (StreamWriter f = new System.IO.StreamWriter("F:\\m1" + (count++).ToString() + ".obj"))
            //		model.ToOBJ().toWritableObj().WriteObj(f, null);
            //}
            //return null;
            var res = new OBJ();
            foreach (var model in Models) res.Merge(model.ToOBJ());
            return res;
        }

        public class KCLModel
        {
            public KCLModelHeader Header;
            public class KCLModelHeader : KCLHeader
            {
                public KCLModelHeader()
                {
                    PrismThickness = 40f;
                    SphereRadius = 0f;
                }

                public KCLModelHeader(BinaryDataReader er)
                {
                    VerticesOffset = er.ReadUInt32();
                    NormalsOffset = er.ReadUInt32();
                    PlanesOffset = er.ReadUInt32();
                    OctreeOffset = er.ReadUInt32();
                    PrismThickness = er.ReadSingle();
                    OctreeOrigin = er.ReadVector3D();
                    XMask = er.ReadUInt32();
                    YMask = er.ReadUInt32();
                    ZMask = er.ReadUInt32();
                    CoordShift = er.ReadUInt32();
                    YShift = er.ReadUInt32();
                    ZShift = er.ReadUInt32();
                    SphereRadius = er.ReadSingle();
                }
                public void Write(BinaryDataWriter er)
                {
                    er.Write(VerticesOffset);
                    er.Write(NormalsOffset);
                    er.Write((uint)(PlanesOffset));
                    er.Write(OctreeOffset);
                    er.Write(PrismThickness);
                    er.Write(OctreeOrigin);
                    er.Write(XMask);
                    er.Write(YMask);
                    er.Write(ZMask);
                    er.Write(CoordShift);
                    er.Write(YShift);
                    er.Write(ZShift);
                    er.Write(SphereRadius);
                }
            }

            public Vector3D[] Vertices;
            public Vector3D[] Normals;

            public KCLPlane[] Planes;
            public class KCLPlane
            {
                public KCLPlane() { }
                public KCLPlane(BinaryDataReader er)
                {
                    Length = er.ReadSingle();
                    VertexIndex = er.ReadUInt16();
                    DirectionIndex = er.ReadUInt16();
                    NormalAIndex = er.ReadUInt16();
                    NormalBIndex = er.ReadUInt16();
                    NormalCIndex = er.ReadUInt16();
                    CollisionType = er.ReadUInt16();
                    TriangleIndex = er.ReadUInt32(); //Global plane index
                }
                public void Write(BinaryDataWriter er)
                {
                    er.Write(Length);
                    er.Write(VertexIndex);
                    er.Write(DirectionIndex);
                    er.Write(NormalAIndex);
                    er.Write(NormalBIndex);
                    er.Write(NormalCIndex);
                    er.Write(CollisionType);
                    er.Write((UInt32)TriangleIndex);
                }
                public Single Length;
                public UInt16 VertexIndex;
                public UInt16 DirectionIndex;
                public UInt16 NormalAIndex;
                public UInt16 NormalBIndex;
                public UInt16 NormalCIndex;
                public UInt16 CollisionType;
                public UInt32 TriangleIndex;
            }

            public KCLOctree Octree;

            Vector3D NormalAvg(KCLPlane Plane) => (Normals[Plane.NormalAIndex] + Normals[Plane.NormalBIndex] + Normals[Plane.NormalCIndex]) / 3;

            public Triangle GetTriangle(KCLPlane Plane)
            {
                Vector3D A = Vertices[Plane.VertexIndex];
                Vector3D CrossA = Normals[Plane.NormalAIndex].Cross(Normals[Plane.DirectionIndex]);
                Vector3D CrossB = Normals[Plane.NormalBIndex].Cross(Normals[Plane.DirectionIndex]);
                Vector3D B = A + CrossB * (Plane.Length / CrossB.Dot(Normals[Plane.NormalCIndex]));
                Vector3D C = A + CrossA * (Plane.Length / CrossA.Dot(Normals[Plane.NormalCIndex]));
                return new Triangle(A, B, C);
            }

            public OBJ ToOBJ()
            {
                OBJ o = new OBJ();
                foreach (var vv in Planes)
                {
                    Triangle t = GetTriangle(vv);
                    var mat = o.GetOrAddMaterial("COL_" + vv.CollisionType.ToString("X"));
                    var col = KCLColors.GetMaterialColor(vv.CollisionType);
                    mat.Colors = new OBJ.Vertex(new Vector3D(), new Vector3D(col.R / 255f, col.G / 255f, col.B / 255f));
                    o.Faces.Add(new OBJ.Face()
                    {
                        VA = new OBJ.Vertex(t.PointA, t.Normal),
                        VB = new OBJ.Vertex(t.PointB, t.Normal),
                        VC = new OBJ.Vertex(t.PointC, t.Normal),
                        Mat = mat.Name
                    });
                }
                return o;
            }

            internal static KCLModel FromTriangles(List<Triangle> triangles, uint baseTriCount, Vector3D BoxMin, Vector3D QuarterSize, bool splitModels = false)
            {
                if (SplitModels)
                    splitModels = true;

                List<Triangle> modelTri = new List<Triangle>();
                List<Vector3D> Vertices = new List<Vector3D>();
                List<Vector3D> Normals = new List<Vector3D>();
                List<KCLPlane> Planes = new List<KCLPlane>();

                int MaxPolygonPerModelOctree = (65535 / 4);

                for (int i = 0; i < triangles.Count; i++)
                {
                    if (!KCLExt.KCL.TriangleBoxIntersect.triBoxOverlap(BoxMin + QuarterSize, QuarterSize, triangles[i]) && splitModels)
                        continue;

                    modelTri.Add(triangles[i]);

                    var tri = triangles[i];

                    KCLModel.KCLPlane p = new KCLModel.KCLPlane();
                    p.CollisionType = triangles[i].Collision;

                    if (EFESettings)
                    {
                        Vector3D qq = (tri.PointB - tri.PointA).Cross(tri.PointC - tri.PointA);
                        if ((qq.X * qq.X + qq.Y * qq.Y + qq.Z * qq.Z) < 0.01) continue;
                        Vector3D a = (tri.PointC - tri.PointA).Cross(tri.Normal);
                        a.Normalize();
                        a = -a;
                        Vector3D b = (tri.PointB - tri.PointA).Cross(tri.Normal);
                        b.Normalize();
                        Vector3D c = (tri.PointC - tri.PointB).Cross(tri.Normal);
                        c.Normalize();
                        p.Length = (tri.PointC - tri.PointA).Dot(c);
                        int q = ContainsVector3(tri.PointA, Vertices);
                        if (q == -1) { p.VertexIndex = (ushort)Vertices.Count; Vertices.Add(tri.PointA); }
                        else p.VertexIndex = (ushort)q;
                        q = ContainsVector3(tri.Normal, Normals);
                        if (q == -1) { p.DirectionIndex = (ushort)Normals.Count; Normals.Add(tri.Normal); }
                        else p.DirectionIndex = (ushort)q;
                        q = ContainsVector3(a, Normals);
                        if (q == -1) { p.NormalAIndex = (ushort)Normals.Count; Normals.Add(a); }
                        else p.NormalAIndex = (ushort)q;
                        q = ContainsVector3(b, Normals);
                        if (q == -1) { p.NormalBIndex = (ushort)Normals.Count; Normals.Add(b); }
                        else p.NormalBIndex = (ushort)q;
                        q = ContainsVector3(c, Normals);
                        if (q == -1) { p.NormalCIndex = (ushort)Normals.Count; Normals.Add(c); }
                        else p.NormalCIndex = (ushort)q;
                    }
                    else
                    {
                        p.VertexIndex = AddIfNotContainsVector3D(tri.PointA, Vertices);
                        var direction = (tri.PointB - tri.PointA).Cross(tri.PointC - tri.PointA).GetNormalize();
                        p.DirectionIndex = AddIfNotContainsVector3D(direction, Normals);
                        p.NormalAIndex = AddIfNotContainsVector3D(direction.Cross(tri.PointC - tri.PointA).GetNormalize(), Normals);
                        p.NormalBIndex = AddIfNotContainsVector3D((-(direction.Cross(tri.PointB - tri.PointA))).GetNormalize(), Normals);
                        p.NormalCIndex = AddIfNotContainsVector3D(direction.Cross(tri.PointB - tri.PointC).GetNormalize(), Normals);
                        p.Length = (tri.PointB - tri.PointA).Dot(Normals[p.NormalCIndex]);
                    }



                    p.TriangleIndex = baseTriCount + (uint)Planes.Count;
                    Planes.Add(p);
                }
                if (modelTri.Count == 0) return null;
                if (modelTri.Count > 65535) throw new Exception("Too many triangles");

                KCLModel resMod = new KCLModel();
                resMod.Vertices = Vertices.ToArray();
                resMod.Normals = Normals.ToArray();
                if (Planes.Count != modelTri.Count)
                    throw new Exception();
                resMod.Planes = Planes.ToArray();
                resMod.Header = new KCLModelHeader();

                if (EFESettings)
                    resMod.Octree = KCLOctree.FromTriangles(modelTri.ToArray(), resMod.Header, 2048, 128, 128, 50);
                else
                    resMod.Octree = KCLOctree.FromTriangles(modelTri.ToArray(), resMod.Header);

                if (MaterialSetForm.ActiveGamePreset == MaterialSetForm.ActivePreset.SMO)
                {
                    resMod.Header.PrismThickness = 40f; //odyssey values
                    resMod.Header.SphereRadius = 0f;
                }
                else
                {
                    resMod.Header.PrismThickness = 30f; //mk8 values
                    resMod.Header.SphereRadius = 25f;
                }

                return resMod;
            }
        }

        private static int ContainsVector3(Vector3D a, List<Vector3D> b)
        {
            for (int i = 0; i < b.Count; i++)
            {
                if (b[i].X == a.X && b[i].Y == a.Y && b[i].Z == a.Z)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}