using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Syroot.BinaryData;
using Syroot.Maths;
using System.Diagnostics;
using EditorCore;

namespace ByamlExt.Byaml
{
	public class BymlFileData
	{
        public ByteOrder byteOrder;
		public ushort Version;
		public bool SupportPaths;
		public dynamic RootNode;
        public static Encoding Encoding = Encoding.UTF8;
    }

    /// <summary>
    /// Represents the loading and saving logic of BYAML files and returns the resulting file structure in dynamic
    /// objects.
    /// </summary>
	/// 
    public class ByamlFile
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const ushort _magicBytes = 0x4259; // "BY"
		// ---- MEMBERS ------------------------------------------------------------------------------------------------
		private ushort _version;
		private bool _supportPaths;
        private ByteOrder _byteOrder;

        private List<string> _nameArray;
        private List<string> _stringArray;

        private Dictionary<ulong, List<long>> _pointedUint64s;
        private Dictionary<long, List<long>> _pointedInt64s;
        private Dictionary<double, List<long>> _pointedDoubles;

        private List<List<ByamlPathPoint>> _pathArray;
		private bool _fastLoad = false;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        private ByamlFile(bool supportPaths, ByteOrder byteOrder, ushort _ver , bool fastLoad = false)
        {
			_version = _ver;
            _supportPaths = supportPaths;
            _byteOrder = byteOrder;
			_fastLoad = fastLoad;
			if (fastLoad)
			{
				AlreadyReadNodes = new Dictionary<uint, dynamic>();
			}
		}

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------
        
        /// <summary>
        /// Deserializes and returns the dynamic value of the BYAML node read from the given file.
        /// </summary>
        /// <param name="fileName">The name of the file to read the data from.</param>
        /// <param name="supportPaths">Whether to expect a path array offset. This must be enabled for Mario Kart 8
        /// files.</param>
        /// <param name="byteOrder">The <see cref="ByteOrder"/> to read data in.</param>
        public static BymlFileData LoadN(string fileName, bool supportPaths = false, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return LoadN(stream, supportPaths, byteOrder);
            }
        }
        
        /// <summary>
        /// Deserializes and returns the dynamic value of the BYAML node read from the specified stream.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read the data from.</param>
        /// <param name="supportPaths">Whether to expect a path array offset. This must be enabled for Mario Kart 8
        /// files.</param>
        /// <param name="byteOrder">The <see cref="ByteOrder"/> to read data in.</param>
        public static BymlFileData LoadN(Stream stream, bool supportPaths = false, ByteOrder byteOrder = ByteOrder.LittleEndian, bool leaveOpen = false)
        {
            ByamlFile byamlFile = new ByamlFile(supportPaths, byteOrder,3);
			var r = byamlFile.Read(stream, BymlFileData.Encoding, leaveOpen);
			return new BymlFileData() { byteOrder = byamlFile._byteOrder, RootNode = r, Version = byamlFile._version, SupportPaths = byamlFile._supportPaths };
		}

		/// <summary>
		/// Deserializes and returns the dynamic value of the BYAML node read from the specified stream keeping the references, do not use this if you intend to edit the byml.
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/> to read the data from.</param>
		/// <param name="supportPaths">Whether to expect a path array offset. This must be enabled for Mario Kart 8
		/// files.</param>
		/// <param name="byteOrder">The <see cref="ByteOrder"/> to read data in.</param>
		public static BymlFileData FastLoadN(Stream stream, bool supportPaths = false, ByteOrder byteOrder = ByteOrder.LittleEndian, bool leaveOpen  = false)
		{
			ByamlFile byamlFile = new ByamlFile(supportPaths, byteOrder, 3, true);
			var r = byamlFile.Read(stream, BymlFileData.Encoding, leaveOpen);
			return new BymlFileData() { byteOrder = byamlFile._byteOrder, RootNode = r, Version = byamlFile._version, SupportPaths = byamlFile._supportPaths };
		}
		
		/// <summary>
		/// Serializes the given dynamic value which requires to be an array or dictionary of BYAML compatible values
		/// and stores it in the given file.
		/// </summary>
		public static void SaveN(string fileName, BymlFileData file)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                SaveN(stream, file);
            }
        }

        public static byte[] SaveN(BymlFileData file)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                SaveN(stream, file);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Serializes the given dynamic value which requires to be an array or dictionary of BYAML compatible values
        /// and stores it in the specified stream.
        /// </summary>
        public static void SaveN(Stream stream, BymlFileData file)
        {
			ByamlFile byamlFile = new ByamlFile(file.SupportPaths, file.byteOrder, file.Version);
            byamlFile.Write(stream, file.RootNode, BymlFileData.Encoding);
        }

        // ---- Helper methods ----

        /// <summary>
        /// Tries to retrieve the value of the element with the specified <paramref name="key"/> stored in the given
        /// dictionary <paramref name="node"/>. If the key does not exist, <c>null</c> is returned.
        /// </summary>
        /// <param name="node">The dictionary BYAML node to retrieve the value from.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The value stored under the given key or <c>null</c> if the key is not present.</returns>
        public static dynamic GetValue(IDictionary<string, dynamic> node, string key)
        {
            dynamic value;
            return node.TryGetValue(key, out value) ? value : null;
        }

        /// <summary>
        /// Sets the given <paramref name="value"/> in the provided dictionary <paramref name="node"/> under the
        /// specified <paramref name="key"/>. If the value is <c>null</c>, the key is removed from the dictionary node.
        /// </summary>
        /// <param name="node">The dictionary node to store the value under.</param>
        /// <param name="key">The key under which the value will be stored or which will be removed.</param>
        /// <param name="value">The value to store under the key or <c>null</c> to remove the key.</param>
        public static void SetValue(IDictionary<string, dynamic> node, string key, dynamic value)
        {
            if (value == null)
            {
                node.Remove(key);
            }
            else
            {
                node[key] = value;
            }
        }

        /// <summary>
        /// Casts all elements of the given array <paramref name="node"/> into the provided type
        /// <typeparamref name="T"/>. If the node is <c>null</c>, <c>null</c> is returned.
        /// </summary>
        /// <typeparam name="T">The type to cast each element to.</typeparam>
        /// <param name="node">The array node which elements will be casted.</param>
        /// <returns>The list of type <typeparamref name="T"/> or <c>null</c> if the node is <c>null</c>.</returns>
        public static List<T> GetList<T>(IEnumerable<dynamic> node)
        {
            return node?.Cast<T>().ToList();
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        // ---- Loading ----
        private dynamic Read(Stream stream, Encoding encoding, bool leaveOpen)
        {
            // Open a reader on the given stream.
            using (BinaryDataReader reader = new BinaryDataReader(stream, encoding, true))
            {
                reader.ByteOrder = _byteOrder;

                // Load the header, specifying magic bytes ("BY"), version and main node offsets.
                if (reader.ReadUInt16() != _magicBytes)
                {
                    _byteOrder = _byteOrder == ByteOrder.LittleEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian;
                    reader.ByteOrder = _byteOrder;
                    reader.BaseStream.Position = 0;
                    if (reader.ReadUInt16() != _magicBytes) throw new Exception("Header mismatch");
                }
				_version = reader.ReadUInt16();
	
                // Read the name array, holding strings referenced by index for the names of other nodes.
                _nameArray = ReadEnumerableNode(reader,reader.ReadUInt32(), true);
                _stringArray = ReadEnumerableNode(reader, reader.ReadUInt32(), true);

                if (reader.BaseStream.Length <= 16)
                    return new List<dynamic>();

                using (reader.TemporarySeek())
                {
                    //Thanks to Syroot https://gitlab.com/Syroot/NintenTools.Byaml/blob/master/src/Syroot.NintenTools.Byaml/DynamicLoader.cs
                    // Paths are supported if the third offset is a path array (or null) and the fourth a root.
                    ByamlNodeType thirdNodeType = PeekNodeType(reader);
                    reader.Seek(sizeof(uint));
                    ByamlNodeType fourthNodeType = PeekNodeType(reader);

                    _supportPaths = (thirdNodeType == ByamlNodeType.None || thirdNodeType == ByamlNodeType.PathArray)
                         && (fourthNodeType == ByamlNodeType.Array || fourthNodeType == ByamlNodeType.Dictionary);

                }

                if (_supportPaths)
                    _pathArray = ReadEnumerableNode(reader, reader.ReadUInt32(), true);

                uint rootNodeOffset = reader.ReadUInt32();
                if (rootNodeOffset == 0) //empty byml
                    return new List<dynamic>();
                else
                    return ReadEnumerableNode(reader, rootNodeOffset, true);
            }
        }

        private static ByamlNodeType PeekNodeType(BinaryDataReader reader)
        {
            using (reader.TemporarySeek())
            {
                // If the offset is invalid, the type cannot be determined.
                uint offset = reader.ReadUInt32();
                if (offset > 0 && offset < reader.BaseStream.Length)
                {
                    // Seek to the offset and try to read a valid type.
                    reader.Position = offset;
                    ByamlNodeType nodeType = (ByamlNodeType)reader.ReadByte();
                    if (Enum.IsDefined(typeof(ByamlNodeType), nodeType))
                        return nodeType;
                }
            }
            return ByamlNodeType.None;

        }

        //Node references are disabled unless fastLoad is active since it leads to multiple objects sharing the same values for different fields (eg. a position node can be shared between multiple objects)
        private Dictionary<uint, dynamic> AlreadyReadNodes = new Dictionary<uint, dynamic>(); //Offset in the file, reference to node

		private dynamic ReadNode(BinaryDataReader reader, ByamlNodeType nodeType)
        {
            // Read the following UInt32 which is representing the value directly.
            switch (nodeType)
            {
                case ByamlNodeType.Array:
                case ByamlNodeType.Dictionary:
                case ByamlNodeType.StringArray:
                case ByamlNodeType.PathArray:
                    return reader.ReadUInt32(); // offset
                case ByamlNodeType.StringIndex:
                    return _stringArray[reader.ReadInt32()];
                case ByamlNodeType.PathIndex:
                    {
                        int index = reader.ReadInt32();
                        if (_pathArray != null && _pathArray.Count > index)
                            return _pathArray[index];
                        else
                            return new ByamlPathIndex() { Index = index };
                    }
                case ByamlNodeType.Boolean:
                    return reader.ReadInt32() != 0;
                case ByamlNodeType.Integer:
                    return reader.ReadInt32();
                case ByamlNodeType.Float:
                    return reader.ReadSingle();
                case ByamlNodeType.Uinteger:
                    return reader.ReadUInt32();
                case ByamlNodeType.Long:
                case ByamlNodeType.ULong:
                case ByamlNodeType.Double:
                    var pos = reader.Position;
                    reader.Position = reader.ReadUInt32();
                    dynamic value = readLongValFromOffset(nodeType);
                    reader.Position = pos + 4;
                    return value;
                case ByamlNodeType.Null:
                    reader.Seek(0x4);
                    return null;
                default:
                    throw new ByamlException($"Unknown node type '{nodeType.ToString("X")}'.");
            }

            dynamic readLongValFromOffset(ByamlNodeType type)
			{
				switch (type)
				{
					case ByamlNodeType.Long:
						return reader.ReadInt64();
					case ByamlNodeType.ULong:
						return reader.ReadUInt64();
					case ByamlNodeType.Double:
						return reader.ReadDouble();
				}
				throw new ByamlException($"Unknown node type '{nodeType}'.");
			}
        }

        private dynamic ReadEnumerableNode(BinaryDataReader reader, uint offset, bool isRoot = false)
        {
            if (AlreadyReadNodes.ContainsKey(offset))
                return AlreadyReadNodes[offset];

            object node = null;
            if (offset > 0 && !AlreadyReadNodes.TryGetValue(offset, out node))
            {
                using (reader.TemporarySeek(offset, SeekOrigin.Begin))
                {
                    ByamlNodeType type = (ByamlNodeType)reader.ReadByte();
                    reader.Seek(-1);
                    int length = (int)Get3LsbBytes(reader.ReadUInt32());
                    switch (type)
                    {
                        case ByamlNodeType.Array:
                            node = ReadArrayNode(reader, length, offset);
                            break;
                        case ByamlNodeType.Dictionary:
                            node = ReadDictionaryNode(reader, length, offset);
                            break;
                        case ByamlNodeType.StringArray:
                            node = ReadStringArrayNode(reader, length);
                            break;
                        case ByamlNodeType.PathArray:
                            node = ReadPathArrayNode(reader, length);
                            break;
                        default: throw new ByamlException($"Invalid enumerable node type {type}.");
                    }
                }
            }
            return node;
        }

        private List<dynamic> ReadArrayNode(BinaryDataReader reader, int length, uint offset = 0)
        {
            List<dynamic> array = new List<dynamic>(length);
            if (offset != 0) AlreadyReadNodes.Add(offset, array);

            IEnumerable<ByamlNodeType> types = reader.ReadBytes(length).Select(x => (ByamlNodeType)x);
            reader.Align(4);

            foreach (ByamlNodeType type in types)
            {
                dynamic value = ReadNode(reader, type);
                if (type.IsEnumerable())
                    array.Add(ReadEnumerableNode(reader, value));
                else
                    array.Add(value);
            }

            return array;
        }

        private Dictionary<string, dynamic> ReadDictionaryNode(BinaryDataReader reader, int length, uint offset = 0)
        {
            Dictionary<string, dynamic> dictionary = new Dictionary<string, dynamic>(length);
            SortedList<uint, string> enumerables = new SortedList<uint, string>(new DuplicateKeyComparer<uint>());
            if (offset != 0)  AlreadyReadNodes.Add(offset, dictionary);

            // Read the elements of the dictionary.
            for (int i = 0; i < length; i++)
            {
                uint indexAndType = reader.ReadUInt32();
                int nodeNameIndex = (int)Get3MsbBytes(indexAndType);
                ByamlNodeType type = (ByamlNodeType)Get1MsbByte(indexAndType);
                string name = _nameArray[nodeNameIndex];

                dynamic value = ReadNode(reader, type);

                if (type.IsEnumerable())
                    dictionary.Add(name, ReadEnumerableNode(reader, value));
                else
                    dictionary.Add(name, value);

                    // Add primitive values directly, queue offset enumerable nodes to read them afterwards.
                    /*      if (type.IsEnumerable())
                          enumerables.Add(value, name);
                      else
                          dictionary.Add(name, value);*/
            }

            // Read the offset enumerable nodes in the order of how they appear in the file.
            foreach (KeyValuePair<uint, string> enumerable in enumerables)
                dictionary.Add(enumerable.Value, ReadEnumerableNode(reader, enumerable.Key));

            return dictionary;
        }

        private List<string> ReadStringArrayNode(BinaryDataReader reader, int length)
        {
            long nodeOffset = reader.Position - sizeof(uint); // Element offsets are relative to the start of node.
            List<string> stringArray = new List<string>(length);

            // Read the element offsets.
            uint[] offsets = reader.ReadUInt32s(length);

            // Read the elements.
            foreach (uint offset in offsets)
            {
                reader.Position = nodeOffset + offset;
                stringArray.Add(reader.ReadString(BinaryStringFormat.ZeroTerminated));
            }

            return stringArray;
        }

        private List<List<ByamlPathPoint>> ReadPathArrayNode(BinaryDataReader reader, int length)
        {
            long nodeOffset = reader.Position - sizeof(uint); // Element offsets are relative to the start of node.
            List<List<ByamlPathPoint>> pathArray = new List<List<ByamlPathPoint>>(length);

            // Read the element offsets.
            uint[] offsets = reader.ReadUInt32s(length + 1);

            // Read the elements.
            for (int i = 0; i < length; i++)
            {
                reader.Position = nodeOffset + offsets[i];
                int pointCount = (int)((offsets[i + 1] - offsets[i]) / ByamlPathPoint.SizeInBytes);
                pathArray.Add(ReadPath(reader, pointCount));
            }

            return pathArray;
        }

        private List<ByamlPathPoint> ReadPath(BinaryDataReader reader, int length)
        {
            List<ByamlPathPoint> byamlPath = new List<ByamlPathPoint>();
            for (int j = 0; j < length; j++)
            {
                byamlPath.Add(ReadPathPoint(reader));
            }
            return byamlPath;
        }

        private ByamlPathPoint ReadPathPoint(BinaryDataReader reader)
        {
            ByamlPathPoint point = new ByamlPathPoint();
            point.Position = new Vector3F(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            point.Normal = new Vector3F(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            point.Unknown = reader.ReadUInt32();
            return point;
        }

        // ---- Saving ----

        private void Write(Stream stream, object root, Encoding encoding)
        {
            // Check if the root is of the correct type.
            if (root == null)
            {
                throw new ByamlException("Root node must not be null.");
            }
            else if (!(root is IDictionary<string, dynamic> || root is IEnumerable))
            {
                throw new ByamlException($"Type '{root.GetType()}' is not supported as a BYAML root node.");
            }

            // Generate the name, string and path array nodes.
            _nameArray = new List<string>();
            _stringArray = new List<string>();
            _pathArray = new List<List<ByamlPathPoint>>();
            _pointedUint64s = new Dictionary<ulong, List<long>>();
            _pointedInt64s = new Dictionary<long, List<long>>();
            _pointedDoubles = new Dictionary<double, List<long>>();

            CollectNodeArrayContents(root);
            readNodes.Clear();
            _nameArray.Sort(StringComparer.Ordinal);
            _stringArray.Sort(StringComparer.Ordinal);
            alreadyWrittenNodes = new Dictionary<dynamic, uint>();

            // Open a writer on the given stream.
            using (BinaryDataWriter writer = new BinaryDataWriter(stream, encoding, true))
            {
                writer.ByteOrder = _byteOrder;

                // Write the header, specifying magic bytes, version and main node offsets.
                writer.Write(_magicBytes);
                writer.Write(_version);
                Offset nameArrayOffset = writer.ReserveOffset();
                Offset stringArrayOffset = writer.ReserveOffset();
                Offset pathArrayOffset = _supportPaths ? writer.ReserveOffset() : null;
                Offset rootOffset = writer.ReserveOffset();

                // Write the main nodes.
                WriteEnumerableNode(writer, nameArrayOffset.Position, ByamlNodeType.StringArray, _nameArray);
                if (_stringArray.Count == 0)
                    writer.Write(0);
                else
                    WriteEnumerableNode(writer, stringArrayOffset.Position, ByamlNodeType.StringArray, _stringArray);

                // Include a path array offset if requested.
                if (_supportPaths) {
                    if (_pathArray.Count == 0)
                        writer.Write(0);
                    else
                        WriteEnumerableNode(writer, pathArrayOffset.Position, ByamlNodeType.PathArray, _pathArray);
                }

                // Write the root node.
                WriteEnumerableNode(writer, rootOffset.Position, GetNodeType(root), (IEnumerable)root);


                foreach (var val in _pointedUint64s)
                {
                    foreach (var pos in val.Value)
                    {
                        writer.Align(4);

                        long p = writer.Position;
                        using (writer.TemporarySeek(pos, SeekOrigin.Begin))
                        {
                            writer.Write((uint)p);
                        }
                        writer.Write(val.Key);
                    }
                }
                foreach (var val in _pointedInt64s)
                {
                    foreach (var pos in val.Value)
                    {
                        writer.Align(4);

                        long p = writer.Position;
                        using (writer.TemporarySeek(pos, SeekOrigin.Begin))
                        {
                            writer.Write((uint)p);
                        }
                        writer.Write(val.Key);
                    }
                }
                foreach (var val in _pointedDoubles)
                {
                    foreach (var pos in val.Value)
                    {
                        writer.Align(4);

                        long p = writer.Position;
                        using (writer.TemporarySeek(pos, SeekOrigin.Begin))
                        {
                            writer.Write((uint)p);
                        }
                        writer.Write(val.Key);
                    }
                }
            }
        }

        List<dynamic> readNodes = new List<dynamic>();
        private void CollectNodeArrayContents(dynamic node)
        {
            if (node == null) return;

            if (readNodes.Contains(node))
                return;

            readNodes.Add(node);
            switch (node)
            {
                case string stringNode:
                    if (!_stringArray.Contains(stringNode))
                        _stringArray.Add(stringNode);
                    break;
                case List<ByamlPathPoint> pathNode:
                    _pathArray.Add(pathNode);
                    break;
                case IDictionary<string, object> dictionaryNode:
                    foreach (KeyValuePair<string, object> entry in dictionaryNode)
                    {
                        if (!_nameArray.Contains(entry.Key))
                            _nameArray.Add(entry.Key);
                        CollectNodeArrayContents(entry.Value);
                    }
                    break;
                case IEnumerable arrayNode:
                    foreach (object childNode in arrayNode)
                        CollectNodeArrayContents(childNode);
                    break;
            }
        }
		
        Dictionary<dynamic, uint> alreadyWrittenNodes = new Dictionary<dynamic, uint>();
        private uint WriteValue(BinaryDataWriter writer, dynamic value)
        {
            // Only reserve and return an offset for the complex value contents, write simple values directly.
            ByamlNodeType type = GetNodeType(value);
            switch (type)
            {
                case ByamlNodeType.StringIndex:
                    WriteStringIndexNode(writer, value);
                    return 0;
                case ByamlNodeType.PathIndex:
                    if (value is ByamlPathIndex)
                        writer.Write(((ByamlPathIndex)value).Index);
                    else
                        WritePathIndexNode(writer, value);
                    return 0;
                case ByamlNodeType.Dictionary:
                case ByamlNodeType.Array:
                    return writer.ReserveOffset().Position;
                case ByamlNodeType.Boolean:
                    writer.Write(value ? 1 : 0);
                    return 0;
                case ByamlNodeType.Integer:
                    writer.Write((int)value);
                    return 0;
                case ByamlNodeType.Float:
                    writer.Write((float)value);
                    return 0;
                case ByamlNodeType.Uinteger:
                    writer.Write((uint)value);
                    return 0;
                case ByamlNodeType.Double:
                case ByamlNodeType.ULong:
                case ByamlNodeType.Long:
                    long pos = writer.Position;
                    writer.Write(0);

                    if (type == ByamlNodeType.ULong)
                    {
                        if (!_pointedUint64s.ContainsKey(value))
                            _pointedUint64s.Add(value, new List<long>());
                        _pointedUint64s[value].Add(pos);
                    }
                    if (type == ByamlNodeType.Long)
                    {
                        if (!_pointedInt64s.ContainsKey(value))
                            _pointedInt64s.Add(value, new List<long>());
                        _pointedInt64s[value].Add(pos);
                    }
                    if (type == ByamlNodeType.Double)
                    {
                        if (!_pointedDoubles.ContainsKey(value))
                            _pointedDoubles.Add(value, new List<long>());
                        _pointedDoubles[value].Add(pos);
                    }
                    return 0;
                case ByamlNodeType.Null:
                    writer.Write(0x0);
                    return 0;
                default:
                    throw new ByamlException($"{type} not supported as value node.");
            }
        }

        private void WriteEnumerableNode(BinaryDataWriter writer, uint offset, ByamlNodeType type, IEnumerable node)
        {
            if (alreadyWrittenNodes.TryGetValue(node, out uint position))
            {
                writer.SatisfyOffset(offset, position);
                return;
            }
            else
            {
                // Satisfy the offset to the complex node value which must be 4-byte aligned.
                position = (uint)writer.Position;
                writer.SatisfyOffset(offset, position);
                WriteTypeAndLength(writer, type, node);

                alreadyWrittenNodes.Add(node, position);

                // Write the value contents.
                switch (type)
                {
                    case ByamlNodeType.Array:
                        WriteArrayNode(writer, node);
                        break;
                    case ByamlNodeType.Dictionary:
                        WriteDictionaryNode(writer, (IDictionary<string, object>)node);
                        break;
                    case ByamlNodeType.StringArray:
                        WriteStringArrayNode(writer, (List<string>)node);
                        break;
                    case ByamlNodeType.PathArray:
                        WritePathArrayNode(writer, (List<List<ByamlPathPoint>>)node); 
                        break;
                    default:
                        throw new ByamlException($"{type} not supported as complex node.");
                }
            }
        }

        private void WriteTypeAndLength(BinaryDataWriter writer, ByamlNodeType type, dynamic node)
        {
            uint value;
            if (_byteOrder == ByteOrder.BigEndian)
            {
                value = (uint)type << 24 | (uint)Enumerable.Count(node);
            }
            else
            {
                value = (uint)type | (uint)Enumerable.Count(node) << 8;
            }
            writer.Write(value);
        }

        private void WriteStringIndexNode(BinaryDataWriter writer, string node)
        {
            uint index = (uint)_stringArray.IndexOf(node);
            writer.Write(index);
        }

        private void WritePathIndexNode(BinaryDataWriter writer, List<ByamlPathPoint> node)
        {
            writer.Write(_pathArray.IndexOf(node));
        }
        
        private void WriteArrayNode(BinaryDataWriter writer, IEnumerable node)
        {
            // Write the element types.
            foreach (dynamic element in node)
                writer.Write((byte)GetNodeType(element));

            // Write the elements, which begin after a padding to the next 4 bytes.
            writer.Align(4);

            List<IEnumerable> enumerables = new List<IEnumerable>();
            List<uint> offsets = new List<uint>();
            foreach (dynamic element in node)
            {
				var off = WriteValue(writer, element);
				if (off > 0) {
                    offsets.Add(off);
                    enumerables.Add((IEnumerable)element);
                }
            }

            // Write the contents of complex nodes and satisfy the offsets.
            for (int i = 0; i < enumerables.Count; i++)
            {
                IEnumerable enumerable = enumerables[i];
                WriteEnumerableNode(writer, offsets[i], GetNodeType(enumerable), enumerable);
            }
        }

        private void WriteDictionaryNode(BinaryDataWriter writer, IDictionary<string, dynamic> node)
        {
            if (node?.Count <= 0)
                return;

            List<EnumerableNode> enumerables = node
               .Where(x => (GetNodeType(x.Value) >= ByamlNodeType.Array && GetNodeType(x.Value) <= ByamlNodeType.PathArray))
               .Select(x => new EnumerableNode { Node = (IEnumerable)x.Value })
               .ToList();

            // Write the key-value pairs.
            foreach (KeyValuePair<string, object> element in node.OrderBy(x => x.Key, StringComparer.Ordinal))
            {
                // Get the index of the key string in the file's name array and write it together with the type.
                uint keyIndex = (uint)_nameArray.IndexOf(element.Key);
                if (_byteOrder == ByteOrder.BigEndian)
                {
                    writer.Write(keyIndex << 8 | (uint)GetNodeType(element.Value));
                }
                else
                {
                    writer.Write(keyIndex | (uint)GetNodeType(element.Value) << 24);
                }

				// Write the elements.
				var offset = WriteValue(writer, element.Value);
				if (offset > 0)
                    enumerables.Where(x => x.Node == element.Value && x.Offset == 0).First().Offset = offset;
            }

            // Write the value contents.
            foreach (EnumerableNode enumerable in enumerables)
                WriteEnumerableNode(writer, enumerable.Offset, GetNodeType(enumerable.Node), enumerable.Node);
        }

        private void WriteStringArrayNode(BinaryDataWriter writer, List<string> node)
        {
            // Write the offsets to the strings, where the last one points to the end of the last string.
            long offset = sizeof(uint) + sizeof(uint) * (node.Count + 1); // Relative to node start + all uint32 offsets.
            foreach (string str in node)
            {
                writer.Write((uint)offset);
                offset += writer.Encoding.GetByteCount(str) + 1;
            }
            writer.Write((uint)offset);

            foreach (string str in node)
                writer.Write(str, BinaryStringFormat.ZeroTerminated);
            writer.Align(4);
        }


        private void WritePathArrayNode(BinaryDataWriter writer, IEnumerable<List<ByamlPathPoint>> node)
        {
            // Write the offsets to the paths, where the last one points to the end of the last path.
            long offset = 4 + 4 * (node.Count() + 1); // Relative to node start + all uint32 offsets.
            foreach (List<ByamlPathPoint> path in node)
            {
                writer.Write((uint)offset);
                offset += path.Count * 28; // 28 bytes are required for a single point.
            }
            writer.Write((uint)offset);

            // Write the paths.
            foreach (List<ByamlPathPoint> path in node)
            {
                WritePathNode(writer, path);
            }
        }

        private class EnumerableNode
        {
            internal IEnumerable Node;
            internal uint Offset;
        }

        private void WritePathNode(BinaryDataWriter writer, List<ByamlPathPoint> node)
        {
            foreach (ByamlPathPoint point in node)
            {
                WritePathPoint(writer, point);
            }
        }

        private void WritePathPoint(BinaryDataWriter writer, ByamlPathPoint point)
        {
            writer.Write(point.Position.X);
            writer.Write(point.Position.Y);
            writer.Write(point.Position.Z);
            writer.Write(point.Normal.X);
            writer.Write(point.Normal.Y);
            writer.Write(point.Normal.Z);
            writer.Write(point.Unknown);
        }

        // ---- Helper methods ----

		static internal ByamlNodeType GetNodeType(dynamic node, bool isInternalNode = false)
        {
            if (isInternalNode)
            {
                if (node is IEnumerable<string>) return ByamlNodeType.StringArray;
                else if (node is IEnumerable<List<ByamlPathPoint>>) return ByamlNodeType.PathArray;
                else throw new ByamlException($"Type '{node.GetType()}' is not supported as a main BYAML node.");
            }
            else
            {
                if (node is string)
                    return ByamlNodeType.StringIndex;
                else if (node is List<ByamlPathPoint>) return ByamlNodeType.PathIndex;
                else if (node is IDictionary<string, dynamic>) return ByamlNodeType.Dictionary;
                else if (node is IEnumerable) return ByamlNodeType.Array;
                else if (node is bool) return ByamlNodeType.Boolean;
                else if (node is int) return ByamlNodeType.Integer;
                else if (node is float) return ByamlNodeType.Float; /*TODO decimal is float or double ? */
				else if (node is uint)	return ByamlNodeType.Uinteger;
				else if (node is Int64) return ByamlNodeType.Long;
				else if (node is UInt64) return ByamlNodeType.ULong;
				else if (node is double) return ByamlNodeType.Double;
				else if (node == null) return ByamlNodeType.Null;
                else throw new ByamlException($"Type '{node.GetType()}' is not supported as a BYAML node.");
            }
        }

        private uint Get1MsbByte(uint value)
        {
            if (_byteOrder == ByteOrder.BigEndian)
            {
                return value & 0x000000FF;
            }
            else
            {
                return value >> 24;
            }
        }

        private uint Get3LsbBytes(uint value)
        {
            if (_byteOrder == ByteOrder.BigEndian)
            {
                return value & 0x00FFFFFF;
            }
            else
            {
                return value >> 8;
            }
        }

        private uint Get3MsbBytes(uint value)
        {
            if (_byteOrder == ByteOrder.BigEndian)
            {
                return value >> 8;
            }
            else
            {
                return value & 0x00FFFFFF;
            }
        }
    }

	public static class IEnumerableCompare
	{
		private static bool IDictionaryIsEqual(IDictionary<string, dynamic> a, IDictionary<string, dynamic> b)
		{
			if (a.Count != b.Count) return false;
			foreach (string key in a.Keys)
			{
				if (!b.ContainsKey(key)) return false;
				if ((a[key] == null && b[key] != null) || (a[key] != null && b[key] == null)) return false;
				else if (a[key] == null && b[key] == null) continue;

				if (TypeNotEqual(a[key].GetType(), b[key].GetType())) return false;

				if (a[key] is IList<dynamic> && IListIsEqual(a[key], b[key])) continue;
				else if (a[key] is IDictionary<string, dynamic> && IDictionaryIsEqual(a[key], b[key])) continue;
				else if (a[key] == b[key]) continue;

				return false;
			}
			return true;
		}

		private static bool IListIsEqual(IList<dynamic> a, IList<dynamic> b)
		{
			if (a.Count != b.Count) return false;
			for (int i = 0; i < a.Count; i++)
			{
				if ((a[i] == null && b[i] != null) || (a[i] != null && b[i] == null)) return false;
				else if (a[i] == null && b[i] == null) continue;

				if (TypeNotEqual(a[i].GetType(), b[i].GetType())) return false;

				if (a[i] is IList<dynamic> && IListIsEqual(a[i], b[i])) continue;
				else if (a[i] is IDictionary<string, dynamic> && IDictionaryIsEqual(a[i], b[i])) continue;
				else if (a[i] == b[i]) continue;

				return false;
			}
			return true;
		}

		public static bool TypeNotEqual(Type a, Type b)
		{
			return !(a.IsAssignableFrom(b) || b.IsAssignableFrom(a)); // without this LinksNode wouldn't be equal to IDictionary<string,dynamic>
		}

		public static bool IsEqual(IEnumerable a, IEnumerable b)
		{
			if (TypeNotEqual(a.GetType(), b.GetType())) return false;
			if (a is IDictionary) return IDictionaryIsEqual((IDictionary<string, dynamic>)a, (IDictionary<string, dynamic>)b);
			else if (a is IList<ByamlPathPoint>) return ((IList<ByamlPathPoint>)a).SequenceEqual((IList<ByamlPathPoint>)b);
			else return IListIsEqual((IList<dynamic>)a, (IList<dynamic>)b);
		}
	}
}
