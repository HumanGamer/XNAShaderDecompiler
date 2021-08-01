using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XNAShaderDecompiler
{
    public class EffectParser
    {
        private Effect _effect;

        public struct SymbolStructMember
        {
            public string Name;
            public SymbolTypeInfo Info;
        }

        public struct SymbolTypeInfo
        {
            public SymbolClass ParameterClass;
            public SymbolType ParameterType;
            public uint Rows;
            public uint Columns;
            public uint Elements;
            public uint MemberCount;
            public List<SymbolStructMember> Members;
        }
        
        public struct EffectValue
        {
            public string Name;
            public string Semantic;
            public SymbolTypeInfo Type;
            public List<object> Values;
        }

        public struct EffectState
        {
            public RenderStateType Type;
            public EffectValue Value;
        }

        public struct EffectPass
        {
            public string Name;
            public List<EffectAnnotation> Annotations;
            public List<EffectState> States;
        }

        public struct EffectTechnique
        {
            public string Name;
            public List<EffectAnnotation> Annotations;
            public List<EffectPass> Passes;
        }

        public struct EffectAnnotation
        {
            public EffectValue Value;
        }

        public struct EffectParam
        {
            public List<EffectAnnotation> Annotations;
            public EffectValue Value;
        }

        public struct EffectObject
        {
            
        }

        public List<EffectParam> EffectParams;
        public List<EffectTechnique> EffectTechniques;
        public List<EffectObject> EffectObjects;
        
        private long _startPos;
        
        public EffectParser(Effect effect)
        {
            _effect = effect;
            _startPos = 0;
        }

        public void Parse()
        {
            byte[] effectCode = _effect.EffectCode;

            using MemoryStream stream = new MemoryStream(effectCode);
            using BinaryReader br = new BinaryReader(stream);

            long length = stream.Length;

            uint header = br.ReadUInt32();
            if (header == 0xBCF00BCF)
            {
                uint skip = br.ReadUInt32() - 8;
                stream.Position += skip;
                // length += skip; // ???
                header = br.ReadUInt32();
            }

            if (header != 0xFEFF0901)
                throw new ContentLoadException("Invalid Effect!");

            uint offset = br.ReadUInt32();
            _startPos = stream.Position;
            if (offset > length)
                throw new EndOfStreamException();

            stream.Position += offset;
            //length -= offset; // ???

            //if (length < 16)
            //    throw new EndOfStreamException();

            uint numParams = br.ReadUInt32();
            uint numTechniques = br.ReadUInt32();
            uint unknown = br.ReadUInt32();
            uint numObjects = br.ReadUInt32();

            EffectObjects = new List<EffectObject>();

            EffectParams = ReadParameters(numParams, br);
            EffectTechniques = ReadTechniques(numTechniques, br);

            //if (length < 8)
            //    throw new EndOfStreamException();

            uint numSmallObjects = br.ReadUInt32();
            uint numLargeObjects = br.ReadUInt32();

            ReadSmallObjects(numSmallObjects, br);
            ReadLargeObjects(numLargeObjects, br);
        }

        private List<EffectParam> ReadParameters(uint numParams, BinaryReader br)
        {
            List<EffectParam> effectParams = new List<EffectParam>();
            if (numParams == 0)
                return effectParams;

            for (int i = 0; i < numParams; i++)
            {
                EffectParam param = new EffectParam();
                
                uint typeOffset = br.ReadUInt32();
                uint valOffset = br.ReadUInt32();
                uint flags = br.ReadUInt32();
                uint numAnnotations = br.ReadUInt32();

                param.Annotations = ReadAnnotations(numAnnotations, br);
                param.Value = ReadValue(br, typeOffset, valOffset);

                effectParams.Add(param);
            }

            return effectParams;
        }

        private List<EffectAnnotation> ReadAnnotations(uint numAnnotations, BinaryReader br)
        {
            List<EffectAnnotation> annotations = new List<EffectAnnotation>();
            
            if (numAnnotations == 0)
                return annotations;

            for (int i = 0; i < numAnnotations; i++)
            {
                EffectAnnotation annotation = new EffectAnnotation();

                uint typeOffset = br.ReadUInt32();
                uint valOffset = br.ReadUInt32();

                annotation.Value = ReadValue(br, typeOffset, valOffset);
            }

            return annotations;
        }
        
        private List<EffectTechnique> ReadTechniques(uint numTechniques, BinaryReader br)
        {
            List<EffectTechnique> effectTechniques = new List<EffectTechnique>();
            if (numTechniques == 0)
                return effectTechniques;

            for (int i = 0; i < numTechniques; i++)
            {
                EffectTechnique technique = new EffectTechnique();
                
                uint nameOffset = br.ReadUInt32();
                uint numAnnotations = br.ReadUInt32();
                uint numPasses = br.ReadUInt32();

                technique.Name = ReadString(br, nameOffset);

                technique.Annotations = ReadAnnotations(numAnnotations, br);
                technique.Passes = ReadPasses(numPasses, br);
                
                effectTechniques.Add(technique);
            }

            return effectTechniques;
        }

        private List<EffectPass> ReadPasses(uint numPasses, BinaryReader br)
        {
            List<EffectPass> passes = new List<EffectPass>();

            if (numPasses == 0)
                return passes;

            for (int i = 0; i < numPasses; i++)
            {
                EffectPass pass = new EffectPass();

                uint passNameOffset = br.ReadUInt32();
                uint numAnnotations = br.ReadUInt32();
                uint numStates = br.ReadUInt32();

                pass.Name = ReadString(br, passNameOffset);
                pass.Annotations = ReadAnnotations(numAnnotations, br);
                pass.States = ReadStates(numStates, br);
                
                passes.Add(pass);
            }
            
            return passes;
        }

        private List<EffectState> ReadStates(uint numStates, BinaryReader br)
        {
            List<EffectState> states = new List<EffectState>();

            if (numStates == 0)
                return states;

            for (int i = 0; i < numStates; i++)
            {
                EffectState state = new EffectState();

                uint type = br.ReadUInt32();
                uint unknown = br.ReadUInt32();
                uint typeOffset = br.ReadUInt32();
                uint valOffset = br.ReadUInt32();

                state.Type = (RenderStateType) type;
                state.Value = ReadValue(br, typeOffset, valOffset);

                states.Add(state);
            }

            return states;
        }

        private EffectValue ReadValue(BinaryReader br, uint typeOffset, uint valOffset)
        {
            EffectValue value = new EffectValue();
            
            // TODO: Implement ReadValue
            
            return value;
        }

        private string ReadString(BinaryReader br, uint offset)
        {
            long start = br.BaseStream.Position;
            br.BaseStream.Position = _startPos + offset;
            int len = br.ReadInt32();
            string result = Encoding.ASCII.GetString(br.ReadBytes(len));
            br.BaseStream.Position = start;
            return result;
        }

        private void ReadSmallObjects(uint numSmallObjects, BinaryReader br)
        {
            // TODO: Implement ReadSmallObjects
        }

        private void ReadLargeObjects(uint numLargeObjects, BinaryReader br)
        {
            // TODO: Implement ReadLargeObjects
        }
    }
}