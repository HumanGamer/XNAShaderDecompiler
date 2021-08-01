namespace XNAShaderDecompiler
{
    public class Effect
    {
        public byte[] EffectCode
        {
            get;
            private set;
        }
        
        public Effect(byte[] effectCode)
        {
            EffectCode = effectCode;
        }
    }
}