using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace ByamlExt.Byaml
{
    public static class BinaryStreamExtension
    {
        internal static void SatisfyOffset(this BinaryDataWriter self, uint offset, uint value)
        {
            using (self.TemporarySeek(offset, SeekOrigin.Begin))
                self.Write(value);
        }

        /* internal static void Write3ByteInt32(this BinaryDataWriter self, Int32 value)
         {
             byte[] bytes = new byte[sizeof(Int32)];
             GetBytes(value, bytes);
             self.Write(bytes, self.ByteOrder == ByteOrder.LittleEndian ? 0 : 1, 3);
         }

         public override void GetBytes(Int32 value, Span<byte> buffer)
         {
             buffer[0] = (byte)(value >> 24);
             buffer[1] = (byte)(value >> 16);
             buffer[2] = (byte)(value >> 8);
             buffer[3] = (byte)value;
         }*/
    }
}
