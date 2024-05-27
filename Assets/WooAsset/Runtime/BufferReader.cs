using System;
using System.Collections.Generic;
using System.Text;

namespace WooAsset
{
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
            char c = (char)(((_buffer[_index] & 0xFF) << 8) | (_buffer[_index+1] & 0xFF));
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
}
