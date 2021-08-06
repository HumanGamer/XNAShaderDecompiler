using System;
using System.Text;

namespace XNAShaderDecompiler
{
    public unsafe sealed class BinReader
    {
        private readonly byte[] data;
        private uint index;

        public BinReader(byte[] data, uint index = 0)
        {
            this.data = data;
            this.index = index;
        }

        public T Read<T>() where T:unmanaged
        {
            fixed(byte* data0 = data)
            {
                var ret = *(T*)(data0+index);
                index += (uint)sizeof(T);
                return ret;
            }
        }

        public T Read<T>(uint offset) where T:unmanaged
		{
            fixed(byte* data0 = data)
            {
                return *(T*)(data0+index+offset);
            }
		}
        
        public void Skip<T>() where T:unmanaged
        {
            index += (uint)sizeof(T);
        }

        public void Skip(uint length)
        {
            index += length;
        }

        public byte[] ReadBytes(uint count)
        {
            var ret = ReadBytes(0, count);
            index += count;
            return ret;
        }

        public byte[] ReadBytes(uint offset, uint count)
        {
            var bytes = new byte[count];
            Array.Copy(data, index+offset, bytes, 0, count);
            return bytes;
        }

        public string ReadString(uint offset)
        {
            uint oldIndex = index;
            index += offset;
            var len = Read<uint>();
            var ret = Encoding.ASCII.GetString(data, (int)index, (int)len);
            index = oldIndex;
            return ret;
        }

        public string ReadString(uint offset, uint length)
		{
            if(length == 0){return string.Empty;}
            return Encoding.ASCII.GetString(data, (int)(index+offset), (int)length);
		}

        public BinReader Slice(uint offset) => new BinReader(data, index+offset);
    }
}