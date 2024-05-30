using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WooAsset
{
    public class BufferWriter
    {
        private byte[] _buffer;
        private int _index = 0;

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
        public byte[] GetRealBytes()
        {
            byte[] bytes = new byte[_index];
            Buffer.BlockCopy(_buffer, 0, bytes, 0, _index);
            return bytes;
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
