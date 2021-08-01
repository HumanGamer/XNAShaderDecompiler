using System;

namespace XNAShaderDecompiler
{
    public class ContentLoadException : Exception
    {
        public ContentLoadException() : base()
        {
            
        }

        public ContentLoadException(string message) : base(message)
        {
            
        }

        public ContentLoadException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}