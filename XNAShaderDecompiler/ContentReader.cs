using System;
using System.IO;

namespace XNAShaderDecompiler
{
    public sealed class ContentReader : BinaryReader
    {
        //private ContentManager contentManager;
        //private ContentTypeReaderManager typeReaderManager;
        //private ContentTypeReader[] typeReaders;
        private string assetName;
        
        internal int version;
        internal char platform;
        
        internal ContentReader(//ContentManager manager,
            Stream stream,
            string assetName,
            int version,
            char platform) : base(stream)
        {
            //this.contentManager = manager;
            this.assetName = assetName;
            this.version = version;
            this.platform = platform;
        }

        public T ReadObject<T>()
        {
            return ReadObject(default(T));
        }

        public T ReadObject<T>(T existingInstance)
        {
            return InnerReadObject(existingInstance);
        }

        private T InnerReadObject<T>(T existingInstance)
        {
            int typeReaderIndex = Read7BitEncodedInt();
            if (typeReaderIndex == 0)
                return existingInstance;
            
            //if (typeReaderIndex != ?)
            //    throw new ContentLoadException("Incorrect type reader index found!");

            EffectReader typeReader = EffectReader.Instance;
            T result = (T) typeReader.Read(this, default(T));
            return result;
        }
        
        internal object ReadAsset<T>()
        {
            int numberOfReaders = Read7BitEncodedInt();
            for (int i = 0; i < numberOfReaders; i++)
            {
                string originalReaderTypeString = ReadString();
                ReadInt32();
            }
            int sharedResourceCount = Read7BitEncodedInt();
            object result = ReadObject<T>();
            return result;
        }
    }
}