﻿using System;

namespace XNAShaderDecompiler
{
    public sealed class BinReader
    {
        private readonly byte[] data;
        private uint index;

        public BinReader(byte[] data, uint index = 0)
        {
            this.data = data;
            this.index = index;
        }

        public unsafe T Read<T>() where T:unmanaged
        {
            fixed(byte* data0 = data)
            {
                var ret = *(T*)(data0+index);
                index += (uint)sizeof(T);
                return ret;
            }
        }
        
        public unsafe void Skip<T>() where T:unmanaged
        {
            index += (uint)sizeof(T);
        }

        public void Skip(uint length)
        {
            index += length;
        }

        public byte[] ReadBytes(uint count)
        {
            var bytes = new byte[count];
            Array.Copy(data, index, bytes, 0, count);
            index += count;
            return bytes;
        }

        public string ReadString(uint offset)
        {
            uint oldIndex = index;
            index += offset;
            var len = Read<uint>();
            var ret = System.Text.Encoding.ASCII.GetString(data, (int)index, (int)len);
            index = oldIndex;
            return ret;
        }

        public BinReader Slice(uint offset) => new BinReader(data, index+offset);
    }
}