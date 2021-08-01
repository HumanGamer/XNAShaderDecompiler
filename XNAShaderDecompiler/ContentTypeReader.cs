using System;

namespace XNAShaderDecompiler
{
    public abstract class ContentTypeReader
    {
        private Type targetType;
        
        protected ContentTypeReader(Type targetType)
        {
            this.targetType = targetType;
        }
        
        protected internal abstract object Read(ContentReader input, object existingInstance);
    }

    public abstract class ContentTypeReader<T> : ContentTypeReader
    {
        protected ContentTypeReader() : base(typeof(T))
        {
            
        }

        protected internal override object Read(ContentReader input, object existingInstance)
        {
            if (existingInstance == null)
                return Read(input, default(T));
            else
                return Read(input, (T) existingInstance);
        }

        protected internal abstract T Read(ContentReader input, T existingInstance);
    }
}