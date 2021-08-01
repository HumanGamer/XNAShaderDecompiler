namespace XNAShaderDecompiler
{
    public class EffectReader : ContentTypeReader<Effect>
    {
        private static EffectReader _instance;
        public static EffectReader Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EffectReader();

                return _instance;
            }
        }

        private EffectReader()
        {
            
        }

        protected internal override Effect Read(ContentReader input, Effect existingInstance)
        {
            int length = input.ReadInt32();
            Effect effect = new Effect(input.ReadBytes(length));
            return effect;
        }
    }
}