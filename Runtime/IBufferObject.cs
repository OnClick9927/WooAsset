using System.Collections.Generic;
using System.Text;
using System;
using System.IO;

namespace WooAsset
{
    public interface IBufferObject
    {
        void ReadData(BufferReader reader);
        void WriteData(BufferWriter writer);
    }
    public class BufferReader
    {
        private readonly byte[] _buffer;
        private int _index = 0;

        public BufferReader(byte[] data)
        {
            _buffer = data;
        }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (_buffer == null || _buffer.Length == 0)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// 缓冲区容量
        /// </summary>
        public int Capacity
        {
            get { return _buffer.Length; }
        }

        public byte[] ReadBytes(int count)
        {
            CheckReaderIndex(count);
            var data = new byte[count];
            Buffer.BlockCopy(_buffer, _index, data, 0, count);
            _index += count;
            return data;
        }
        public byte ReadByte()
        {
            CheckReaderIndex(1);
            return _buffer[_index++];
        }
        public char ReadChar()
        {
            CheckReaderIndex(2);
            char c = (char)(((_buffer[_index] & 0xFF) << 8) | (_buffer[_index + 1] & 0xFF));
            _index += 2;
            return c;
        }

        public bool ReadBool()
        {
            CheckReaderIndex(1);
            return _buffer[_index++] == 1;
        }
        public short ReadInt16()
        {
            CheckReaderIndex(2);
            if (BitConverter.IsLittleEndian)
            {
                short value = (short)((_buffer[_index]) | (_buffer[_index + 1] << 8));
                _index += 2;
                return value;
            }
            else
            {
                short value = (short)((_buffer[_index] << 8) | (_buffer[_index + 1]));
                _index += 2;
                return value;
            }
        }
        public ushort ReadUInt16()
        {
            return (ushort)ReadInt16();
        }
        public int ReadInt32()
        {
            CheckReaderIndex(4);
            if (BitConverter.IsLittleEndian)
            {
                int value = (_buffer[_index]) | (_buffer[_index + 1] << 8) | (_buffer[_index + 2] << 16) | (_buffer[_index + 3] << 24);
                _index += 4;
                return value;
            }
            else
            {
                int value = (_buffer[_index] << 24) | (_buffer[_index + 1] << 16) | (_buffer[_index + 2] << 8) | (_buffer[_index + 3]);
                _index += 4;
                return value;
            }
        }
        public uint ReadUInt32()
        {
            return (uint)ReadInt32();
        }
        public long ReadInt64()
        {
            CheckReaderIndex(8);
            if (BitConverter.IsLittleEndian)
            {
                int i1 = (_buffer[_index]) | (_buffer[_index + 1] << 8) | (_buffer[_index + 2] << 16) | (_buffer[_index + 3] << 24);
                int i2 = (_buffer[_index + 4]) | (_buffer[_index + 5] << 8) | (_buffer[_index + 6] << 16) | (_buffer[_index + 7] << 24);
                _index += 8;
                return (uint)i1 | ((long)i2 << 32);
            }
            else
            {
                int i1 = (_buffer[_index] << 24) | (_buffer[_index + 1] << 16) | (_buffer[_index + 2] << 8) | (_buffer[_index + 3]);
                int i2 = (_buffer[_index + 4] << 24) | (_buffer[_index + 5] << 16) | (_buffer[_index + 6] << 8) | (_buffer[_index + 7]);
                _index += 8;
                return (uint)i2 | ((long)i1 << 32);
            }
        }
        public ulong ReadUInt64()
        {
            return (ulong)ReadInt64();
        }

        public string ReadUTF8()
        {
            ushort count = ReadUInt16();
            if (count == 0)
                return string.Empty;

            CheckReaderIndex(count);
            string value = Encoding.UTF8.GetString(_buffer, _index, count);
            _index += count;
            return value;
        }
        public int[] ReadInt32Array()
        {
            ushort count = ReadUInt16();
            int[] values = new int[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = ReadInt32();
            }
            return values;
        }
        public List<int> ReadInt32List()
        {
            ushort count = ReadUInt16();
            List<int> values = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                values.Add(ReadInt32());
            }
            return values;
        }
        public List<char> ReadCharList()
        {
            ushort count = ReadUInt16();
            List<char> values = new List<char>(count);
            for (int i = 0; i < count; i++)
            {
                values.Add(ReadChar());
            }
            return values;
        }
        public List<byte> ReadByteList()
        {
            ushort count = ReadUInt16();
            List<byte> values = new List<byte>(count);
            for (int i = 0; i < count; i++)
            {
                values.Add(ReadByte());
            }
            return values;
        }
        public long[] ReadInt64Array()
        {
            ushort count = ReadUInt16();
            long[] values = new long[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = ReadInt64();
            }
            return values;
        }
        public string[] ReadUTF8Array()
        {
            ushort count = ReadUInt16();
            string[] values = new string[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = ReadUTF8();
            }
            return values;
        }
        public List<string> ReadUTF8List()
        {
            ushort count = ReadUInt16();
            List<string> values = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                values.Add(ReadUTF8());
            }
            return values;
        }
        public T ReadObject<T>() where T : IBufferObject, new()
        {
            T t = new T();
            t.ReadData(this);
            return t;
        }
        public List<T> ReadObjectList<T>() where T : IBufferObject, new()
        {
            ushort count = ReadUInt16();
            List<T> values = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                values.Add(ReadObject<T>());
            }
            return values;
        }
        private void CheckReaderIndex(int length)
        {
            if (_index + length > Capacity)
            {
                AssetsHelper.LogError("IndexOutOfRangeException");
            }
        }
    }
    public class BufferWriter
    {
        private byte[] _buffer;
        private int _index = 0;
        public int length => _index;
        public byte[] buffer => _buffer;
        public BufferWriter(int capacity)
        {
            _buffer = new byte[capacity];
        }

        /// <summary>
        /// 缓冲区容量
        /// </summary>
        public int Capacity
        {
            get { return _buffer.Length; }
        }

        /// <summary>
        /// 清空缓冲区
        /// </summary>
        public void Clear()
        {
            _index = 0;
        }

        /// <summary>
        /// 将有效数据写入文件流
        /// </summary>
        public void WriteToStream(Stream fileStream)
        {
            fileStream.Write(_buffer, 0, _index);
        }


        public void WriteBytes(byte[] data)
        {
            int count = data.Length;
            CheckWriterIndex(count);
            Buffer.BlockCopy(data, 0, _buffer, _index, count);
            _index += count;
        }
        public void WriteByte(byte value)
        {
            CheckWriterIndex(1);
            _buffer[_index++] = value;
        }
        public void WriteChar(char value)
        {
            CheckWriterIndex(2);
            _buffer[_index++] = (byte)((value & 0xFF00) >> 8);
            _buffer[_index++] = (byte)(value & 0xFF);
        }
        public void WriteBool(bool value)
        {
            WriteByte((byte)(value ? 1 : 0));
        }
        public void WriteInt16(short value)
        {
            WriteUInt16((ushort)value);
        }
        public void WriteUInt16(ushort value)
        {
            CheckWriterIndex(2);
            _buffer[_index++] = (byte)value;
            _buffer[_index++] = (byte)(value >> 8);
        }
        public void WriteInt32(int value)
        {
            WriteUInt32((uint)value);
        }
        public void WriteUInt32(uint value)
        {
            CheckWriterIndex(4);
            _buffer[_index++] = (byte)value;
            _buffer[_index++] = (byte)(value >> 8);
            _buffer[_index++] = (byte)(value >> 16);
            _buffer[_index++] = (byte)(value >> 24);
        }
        public void WriteInt64(long value)
        {
            WriteUInt64((ulong)value);
        }
        public void WriteUInt64(ulong value)
        {
            CheckWriterIndex(8);
            _buffer[_index++] = (byte)value;
            _buffer[_index++] = (byte)(value >> 8);
            _buffer[_index++] = (byte)(value >> 16);
            _buffer[_index++] = (byte)(value >> 24);
            _buffer[_index++] = (byte)(value >> 32);
            _buffer[_index++] = (byte)(value >> 40);
            _buffer[_index++] = (byte)(value >> 48);
            _buffer[_index++] = (byte)(value >> 56);
        }

        public void WriteUTF8(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteUInt16(0);
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                int count = bytes.Length;
                if (count > ushort.MaxValue)
                    throw new FormatException($"Write string length cannot be greater than {ushort.MaxValue} !");

                WriteUInt16(Convert.ToUInt16(count));
                WriteBytes(bytes);
            }
        }
        public void WriteInt32Array(int[] values)
        {
            if (values == null)
            {
                WriteUInt16(0);
            }
            else
            {
                int count = values.Length;
                if (count > ushort.MaxValue)
                    throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");

                WriteUInt16(Convert.ToUInt16(count));
                for (int i = 0; i < count; i++)
                {
                    WriteInt32(values[i]);
                }
            }
        }
        public void WriteInt64Array(long[] values)
        {
            if (values == null)
            {
                WriteUInt16(0);
            }
            else
            {
                int count = values.Length;
                if (count > ushort.MaxValue)
                    throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");

                WriteUInt16(Convert.ToUInt16(count));
                for (int i = 0; i < count; i++)
                {
                    WriteInt64(values[i]);
                }
            }
        }
        public void WriteUTF8Array(string[] values)
        {
            if (values == null)
            {
                WriteUInt16(0);
            }
            else
            {
                int count = values.Length;
                if (count > ushort.MaxValue)
                    throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");

                WriteUInt16(Convert.ToUInt16(count));
                for (int i = 0; i < count; i++)
                {
                    WriteUTF8(values[i]);
                }
            }
        }
        public void WriteUTF8List(List<string> values)
        {
            if (values == null)
            {
                WriteUInt16(0);
            }
            else
            {
                int count = values.Count;
                if (count > ushort.MaxValue)
                    throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");

                WriteUInt16(Convert.ToUInt16(count));
                for (int i = 0; i < count; i++)
                {
                    WriteUTF8(values[i]);
                }
            }
        }
        public void WriteInt32List(List<int> values)
        {
            if (values == null)
            {
                WriteUInt16(0);
            }
            else
            {
                int count = values.Count;
                if (count > ushort.MaxValue)
                    throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");

                WriteUInt16(Convert.ToUInt16(count));
                for (int i = 0; i < count; i++)
                {
                    WriteInt32(values[i]);
                }
            }
        }
        public void WriteCharList(List<char> values)
        {
            if (values == null)
            {
                WriteUInt16(0);
            }
            else
            {
                int count = values.Count;
                if (count > ushort.MaxValue)
                    throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");

                WriteUInt16(Convert.ToUInt16(count));
                for (int i = 0; i < count; i++)
                {
                    WriteChar(values[i]);
                }
            }
        }
        public void WriteByteList(List<byte> values)
        {
            if (values == null)
            {
                WriteUInt16(0);
            }
            else
            {
                int count = values.Count;
                if (count > ushort.MaxValue)
                    throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");

                WriteUInt16(Convert.ToUInt16(count));
                for (int i = 0; i < count; i++)
                {
                    WriteByte(values[i]);
                }
            }
        }


        public void WriteObjectList<T>(List<T> values) where T : IBufferObject
        {
            if (values == null)
            {
                WriteUInt16(0);
            }
            else
            {
                int count = values.Count;
                if (count > ushort.MaxValue)
                    throw new FormatException($"Write array length cannot be greater than {ushort.MaxValue} !");

                WriteUInt16(Convert.ToUInt16(count));
                for (int i = 0; i < count; i++)
                {
                    values[i].WriteData(this);
                }
            }
        }
        public void WriteObject<T>(T value) where T : IBufferObject
        {
            value.WriteData(this);
        }

        private void CheckWriterIndex(int length)
        {
            if (_index + length > _buffer.Length)
            {
                byte[] bytes = new byte[_buffer.Length * 2];
                Buffer.BlockCopy(_buffer, 0, bytes, 0, _buffer.Length);
                _buffer = bytes;
            }
        }
    }

}
