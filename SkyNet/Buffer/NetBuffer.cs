using System;
using System.IO;

namespace SkyNet
{
    public class NetBuffer
    {
        readonly BinaryReader reader;
        readonly BinaryWriter writer;
        readonly Stream stream;
        
        public NetBuffer(byte[] buffer)
        {
            reader = new BinaryReader(new MemoryStream(buffer));
            stream = reader.BaseStream;
        }

        public NetBuffer()
        {
            writer = new BinaryWriter(new MemoryStream());
            stream = writer.BaseStream;
        }

        public int Position { get { return (int)stream.Position; } set { stream.Position = value; } }
        public int Length { get { return (int)stream.Length; } }

        #region Reader
        public byte ReadByte() { return reader.ReadByte(); }
        public sbyte ReadSByte() { return reader.ReadSByte(); }
        public char ReadChar() { return reader.ReadChar(); }
        public bool ReadBoolean() { return reader.ReadBoolean(); }
        public short ReadInt16() { return reader.ReadInt16(); }
        public ushort ReadUInt16() { return reader.ReadUInt16(); }
        public int ReadInt32() { return reader.ReadInt32(); }
        public uint ReadUInt32() { return reader.ReadUInt32(); }
        public long ReadInt64() { return reader.ReadInt64(); }
        public ulong ReadUInt64() { return reader.ReadUInt64(); }
        public decimal ReadDecimal() { return reader.ReadDecimal(); }
        public float ReadSingle() { return reader.ReadSingle(); }
        public double ReadDouble() { return reader.ReadDouble(); }

        public string ReadString()
        {
            return reader.ReadBoolean() ? reader.ReadString() : null; // null support, see NetworkWriter
        }

        public byte[] ReadBytes(int count)
        {
            return reader.ReadBytes(count);
        }

        public byte[] ReadBytesAndSize()
        {
            // notNull? (see NetworkWriter)
            bool notNull = reader.ReadBoolean();
            if (notNull)
            {
                uint size = ReadPackedUInt32();
                return reader.ReadBytes((int)size);
            }
            return null;
        }

        // http://sqlite.org/src4/doc/trunk/www/varint.wiki
        // NOTE: big endian.
        public UInt32 ReadPackedUInt32()
        {
            UInt64 value = ReadPackedUInt64();
            if (value > UInt32.MaxValue)
            {
                throw new IndexOutOfRangeException("ReadPackedUInt32() failure, value too large");
            }
            return (UInt32)value;
        }

        public UInt64 ReadPackedUInt64()
        {
            byte a0 = ReadByte();
            if (a0 < 241)
            {
                return a0;
            }

            byte a1 = ReadByte();
            if (a0 >= 241 && a0 <= 248)
            {
                return 240 + 256 * (a0 - ((UInt64)241)) + a1;
            }

            byte a2 = ReadByte();
            if (a0 == 249)
            {
                return 2288 + (((UInt64)256) * a1) + a2;
            }

            byte a3 = ReadByte();
            if (a0 == 250)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16);
            }

            byte a4 = ReadByte();
            if (a0 == 251)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24);
            }

            byte a5 = ReadByte();
            if (a0 == 252)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32);
            }

            byte a6 = ReadByte();
            if (a0 == 253)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40);
            }

            byte a7 = ReadByte();
            if (a0 == 254)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40) + (((UInt64)a7) << 48);
            }

            byte a8 = ReadByte();
            if (a0 == 255)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40) + (((UInt64)a7) << 48) + (((UInt64)a8) << 56);
            }

            throw new IndexOutOfRangeException("ReadPackedUInt64() failure: " + a0);
        }


        //public Vector2 ReadVector2()
        //{
        //    return new Vector2(ReadSingle(), ReadSingle());
        //}

        //public Vector3 ReadVector3()
        //{
        //    return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        //}

        //public Vector4 ReadVector4()
        //{
        //    return new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        //}

        //public Color ReadColor()
        //{
        //    return new Color(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        //}

        //public Color32 ReadColor32()
        //{
        //    return new Color32(ReadByte(), ReadByte(), ReadByte(), ReadByte());
        //}

        //public Quaternion ReadQuaternion()
        //{
        //    return new Quaternion(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        //}

        public Guid ReadGuid()
        {
            byte[] bytes = reader.ReadBytes(16);
            return new Guid(bytes);
        }

        /// <summary>
        /// Reads an PrefabId value from the bit packer.
        /// </summary>
        public PrefabId ReadPrefabId()
        {
            return new PrefabId(ReadInt32());
        }

        /// <summary>
        /// Reads an PrefabId value from the bit packer.
        /// </summary>
        public NetworkId ReadNetworkId()
        {
            return new NetworkId(ReadUInt32());
        }

        /// <summary>
        /// Reads an PrefabId value from the bit packer.
        /// </summary>
        public TypeId ReadTypeId()
        {
            return new TypeId(ReadUInt32());
        }

        #endregion


        #region Writer
        public void Flush()
        {
            writer.Flush();
        }

        public byte[] ToArray()
        {
            writer.Flush();
            byte[] slice = new byte[Position];
            Array.Copy(((MemoryStream)stream).ToArray(), slice, Position);
            return slice;
        }

        public void Write(byte value) { writer.Write(value); }
        public void Write(sbyte value) { writer.Write(value); }
        public void Write(char value) { writer.Write(value); }
        public void Write(bool value) { writer.Write(value); }
        public void Write(short value) { writer.Write(value); }
        public void Write(ushort value) { writer.Write(value); }
        public void Write(int value) { writer.Write(value); }
        public void Write(uint value) { writer.Write(value); }
        public void Write(long value) { writer.Write(value); }
        public void Write(ulong value) { writer.Write(value); }
        public void Write(float value) { writer.Write(value); }
        public void Write(double value) { writer.Write(value); }
        public void Write(decimal value) { writer.Write(value); }

        public void Write(string value)
        {
            // BinaryWriter doesn't support null strings, so let's write an extra boolean for that
            // (note: original HLAPI would write "" for null strings, but if a string is null on the server then it
            //        should also be null on the client)
            writer.Write(value != null);
            if (value != null) writer.Write(value);
        }

        public void Write(byte[] buffer)
        {
            // no null check because we would need to write size info for that too (hence WriteBytesAndSize)
            writer.Write(buffer);
        }

        // for byte arrays with consistent size, where the reader knows how many to read
        // (like a packet opcode that's always the same)
        public void Write(byte[] buffer, int offset, int count)
        {
            // no null check because we would need to write size info for that too (hence WriteBytesAndSize)
            writer.Write(buffer, offset, count);
        }

        // for byte arrays with dynamic size, where the reader doesn't know how many will come
        // (like an inventory with different items etc.)
        public void WriteBytesAndSize(byte[] buffer, int offset, int count)
        {
            // null is supported because [SyncVar]s might be structs with null byte[] arrays
            // (writing a size=0 empty array is not the same, the server and client would be out of sync)
            // (using size=-1 for null would limit max size to 32kb instead of 64kb)
            if (buffer == null)
            {
                SkyLog.Error("WriteBytesAndSize: buffer cannot be null");
                writer.Write(false); // notNull?
                return;
            }
            if (count < 0)
            {
                SkyLog.Error("WriteBytesAndSize: size " + count + " cannot be negative");
                return;
            }

            writer.Write(true); // notNull?
            WritePackedUInt32((uint)count);
            writer.Write(buffer, offset, count);
        }

        // Weaver needs a write function with just one byte[] parameter
        // (we don't name it .Write(byte[]) because it's really a WriteBytesAndSize since we write size / null info too)
        public void WriteBytesAndSize(byte[] buffer)
        {
            // buffer might be null, so we can't use .Length in that case
            WriteBytesAndSize(buffer, 0, buffer != null ? buffer.Length : 0);
        }

        // http://sqlite.org/src4/doc/trunk/www/varint.wiki
        public void WritePackedUInt32(UInt32 value)
        {
            // for 32 bit values WritePackedUInt64 writes the
            // same exact thing bit by bit
            WritePackedUInt64(value);
        }

        public void WritePackedUInt64(UInt64 value)
        {
            if (value <= 240)
            {
                Write((byte)value);
                return;
            }
            if (value <= 2287)
            {
                Write((byte)((value - 240) / 256 + 241));
                Write((byte)((value - 240) % 256));
                return;
            }
            if (value <= 67823)
            {
                Write((byte)249);
                Write((byte)((value - 2288) / 256));
                Write((byte)((value - 2288) % 256));
                return;
            }
            if (value <= 16777215)
            {
                Write((byte)250);
                Write((byte)(value & 0xFF));
                Write((byte)((value >> 8) & 0xFF));
                Write((byte)((value >> 16) & 0xFF));
                return;
            }
            if (value <= 4294967295)
            {
                Write((byte)251);
                Write((byte)(value & 0xFF));
                Write((byte)((value >> 8) & 0xFF));
                Write((byte)((value >> 16) & 0xFF));
                Write((byte)((value >> 24) & 0xFF));
                return;
            }
            if (value <= 1099511627775)
            {
                Write((byte)252);
                Write((byte)(value & 0xFF));
                Write((byte)((value >> 8) & 0xFF));
                Write((byte)((value >> 16) & 0xFF));
                Write((byte)((value >> 24) & 0xFF));
                Write((byte)((value >> 32) & 0xFF));
                return;
            }
            if (value <= 281474976710655)
            {
                Write((byte)253);
                Write((byte)(value & 0xFF));
                Write((byte)((value >> 8) & 0xFF));
                Write((byte)((value >> 16) & 0xFF));
                Write((byte)((value >> 24) & 0xFF));
                Write((byte)((value >> 32) & 0xFF));
                Write((byte)((value >> 40) & 0xFF));
                return;
            }
            if (value <= 72057594037927935)
            {
                Write((byte)254);
                Write((byte)(value & 0xFF));
                Write((byte)((value >> 8) & 0xFF));
                Write((byte)((value >> 16) & 0xFF));
                Write((byte)((value >> 24) & 0xFF));
                Write((byte)((value >> 32) & 0xFF));
                Write((byte)((value >> 40) & 0xFF));
                Write((byte)((value >> 48) & 0xFF));
                return;
            }

            // all others
            {
                Write((byte)255);
                Write((byte)(value & 0xFF));
                Write((byte)((value >> 8) & 0xFF));
                Write((byte)((value >> 16) & 0xFF));
                Write((byte)((value >> 24) & 0xFF));
                Write((byte)((value >> 32) & 0xFF));
                Write((byte)((value >> 40) & 0xFF));
                Write((byte)((value >> 48) & 0xFF));
                Write((byte)((value >> 56) & 0xFF));
            }
        }

        //public void Write(Vector2 value)
        //{
        //    Write(value.x);
        //    Write(value.y);
        //}

        //public void Write(Vector3 value)
        //{
        //    Write(value.x);
        //    Write(value.y);
        //    Write(value.z);
        //}

        //public void Write(Vector4 value)
        //{
        //    Write(value.x);
        //    Write(value.y);
        //    Write(value.z);
        //    Write(value.w);
        //}

        //public void Write(Color value)
        //{
        //    Write(value.r);
        //    Write(value.g);
        //    Write(value.b);
        //    Write(value.a);
        //}

        //public void Write(Color32 value)
        //{
        //    Write(value.r);
        //    Write(value.g);
        //    Write(value.b);
        //    Write(value.a);
        //}

        //public void Write(Quaternion value)
        //{
        //    Write(value.x);
        //    Write(value.y);
        //    Write(value.z);
        //    Write(value.w);
        //}

        //public void Write(Rect value)
        //{
        //    Write(value.xMin);
        //    Write(value.yMin);
        //    Write(value.width);
        //    Write(value.height);
        //}

        //public void Write(Plane value)
        //{
        //    Write(value.normal);
        //    Write(value.distance);
        //}

        //public void Write(Ray value)
        //{
        //    Write(value.direction);
        //    Write(value.origin);
        //}

        //public void Write(Matrix4x4 value)
        //{
        //    Write(value.m00);
        //    Write(value.m01);
        //    Write(value.m02);
        //    Write(value.m03);
        //    Write(value.m10);
        //    Write(value.m11);
        //    Write(value.m12);
        //    Write(value.m13);
        //    Write(value.m20);
        //    Write(value.m21);
        //    Write(value.m22);
        //    Write(value.m23);
        //    Write(value.m30);
        //    Write(value.m31);
        //    Write(value.m32);
        //    Write(value.m33);
        //}

        public void Write(Guid value)
        {
            writer.Write(value.ToByteArray());
        }        

        /// <summary>
        /// Writes an PrefabId value into the bit packer.
        /// </summary>
        public void WritePrefabId(PrefabId value)
        {
            Write(value.Value);
        }

        /// <summary>
        /// Writes an PrefabId value into the bit packer.
        /// </summary>
        public void WriteNetworkId(NetworkId value)
        {
            Write(value.Value);
        }

        /// <summary>
        /// Writes an PrefabId value into the bit packer.
        /// </summary>
        public void WriteTypeId(TypeId value)
        {
            Write(value.Value);
        }
        #endregion

        #region Event
        public TEvnt ReadEvent<TEvnt>() where TEvnt : Event, new()
        {
            var evnt = new TEvnt();
            evnt.Unpack(this);
            return evnt;
        }
        #endregion

    }
}
