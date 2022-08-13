using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace SARCExt
{
    public class SAHT
    {
        public SAHT() { }

        public SAHT(byte[] data) {
            Read(new BinaryDataReader(new System.IO.MemoryStream(data)));
        }

        public SAHT(string filePath)
        {
            Read(new BinaryDataReader(System.IO.File.OpenRead(filePath)));
        }

        public Dictionary<uint, string> HashEntries = new Dictionary<uint, string>();

        private void Read(BinaryDataReader reader)
        {
            if (reader.ReadString(4) != "SAHT")
                throw new Exception("Wrong magic");
            uint FileSize = reader.ReadUInt32();
            uint Offset = reader.ReadUInt32();
            uint EntryCount = reader.ReadUInt32();

            reader.Seek(Offset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < EntryCount; i++)
            {
                HashEntry entry = new HashEntry();
                entry.Read(reader);
                reader.Align(16);
                HashEntries.Add(entry.Hash, entry.Name);
            }
        }

        public class HashEntry
        {
            public uint Hash { get; set; }
            public string Name { get; set; }

            public void Read(BinaryDataReader reader)
            {
                Hash = reader.ReadUInt32();
                Name = reader.ReadString(BinaryStringFormat.ZeroTerminated);
            }
        }
    }
}
