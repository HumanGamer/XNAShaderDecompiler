using System;
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

            //using MemoryStream stream = new MemoryStream(effectCode);
            //using BinaryReader br = new BinaryReader(stream);

            BinReader br = new BinReader(effectCode);

            //long length = stream.Length;

            var header = br.Read<uint>();
            if (header == 0xBCF00BCF)
            {
                var skip = br.Read<uint>() - 8;
                br.Skip(skip);
                // length += skip; // ???
                header = br.Read<uint>();
            }

            if (header != 0xFEFF0901)
                throw new ContentLoadException("Invalid Effect!");

            uint offset = br.Read<uint>();
            //if (offset > length)
            //    throw new EndOfStreamException();

            var @base = br.Slice(offset);
            //length -= offset; // ???

            //if (length < 16)
            //    throw new EndOfStreamException();

            uint numParams = br.Read<uint>();
            uint numTechniques = br.Read<uint>();
            uint unknown = br.Read<uint>();
            uint numObjects = br.Read<uint>();

            EffectObjects = new List<EffectObject>();

            EffectParams = ReadParameters(numParams, br, @base);
            EffectTechniques = ReadTechniques(numTechniques, br, @base);

            //if (length < 8)
            //    throw new EndOfStreamException();

            uint numSmallObjects = br.Read<uint>();
            uint numLargeObjects = br.Read<uint>();

            ReadSmallObjects(numSmallObjects, br);
            ReadLargeObjects(numLargeObjects, br);
        }

        private List<EffectParam> ReadParameters(uint numParams, BinReader br, BinReader @base)
        {
            List<EffectParam> effectParams = new List<EffectParam>();
            if (numParams == 0)
                return effectParams;

            for (int i = 0; i < numParams; i++)
            {
                EffectParam param = new EffectParam();
                
                uint typeOffset = br.Read<uint>();
                uint valOffset = br.Read<uint>();
                uint flags = br.Read<uint>();
                uint numAnnotations = br.Read<uint>();

                param.Annotations = ReadAnnotations(numAnnotations, br, @base);
                param.Value = ReadValue(@base, typeOffset, valOffset);

                effectParams.Add(param);
            }

            return effectParams;
        }

        private List<EffectAnnotation> ReadAnnotations(uint numAnnotations, BinReader br, BinReader @base)
        {
            List<EffectAnnotation> annotations = new List<EffectAnnotation>();
            
            if (numAnnotations == 0)
                return annotations;

            for (int i = 0; i < numAnnotations; i++)
            {
                EffectAnnotation annotation = new EffectAnnotation();

                uint typeOffset = br.Read<uint>();
                uint valOffset = br.Read<uint>();

                annotation.Value = ReadValue(@base, typeOffset, valOffset);
            }

            return annotations;
        }
        
        private List<EffectTechnique> ReadTechniques(uint numTechniques, BinReader br, BinReader @base)
        {
            List<EffectTechnique> effectTechniques = new List<EffectTechnique>();
            if (numTechniques == 0)
                return effectTechniques;

            for (int i = 0; i < numTechniques; i++)
            {
                EffectTechnique technique = new EffectTechnique();
                
                uint nameOffset = br.Read<uint>();
                uint numAnnotations = br.Read<uint>();
                uint numPasses = br.Read<uint>();

                technique.Name = @base.ReadString(nameOffset);

                technique.Annotations = ReadAnnotations(numAnnotations, br, @base);
                technique.Passes = ReadPasses(numPasses, br, @base);
                
                effectTechniques.Add(technique);
            }

            return effectTechniques;
        }

        private List<EffectPass> ReadPasses(uint numPasses, BinReader br, BinReader @base)
        {
            List<EffectPass> passes = new List<EffectPass>();

            if (numPasses == 0)
                return passes;

            for (int i = 0; i < numPasses; i++)
            {
                EffectPass pass = new EffectPass();

                uint passNameOffset = br.Read<uint>();
                uint numAnnotations = br.Read<uint>();
                uint numStates = br.Read<uint>();

                pass.Name = @base.ReadString(passNameOffset);
                pass.Annotations = ReadAnnotations(numAnnotations, br, @base);
                pass.States = ReadStates(numStates, br, @base);
                
                passes.Add(pass);
            }
            
            return passes;
        }

        private List<EffectState> ReadStates(uint numStates, BinReader br, BinReader @base)
        {
            List<EffectState> states = new List<EffectState>();

            if (numStates == 0)
                return states;

            for (int i = 0; i < numStates; i++)
            {
                EffectState state = new EffectState();

                uint type = br.Read<uint>();
                uint unknown = br.Read<uint>();
                uint typeOffset = br.Read<uint>();
                uint valOffset = br.Read<uint>();

                state.Type = (RenderStateType) type;
                state.Value = ReadValue(@base, typeOffset, valOffset);

                states.Add(state);
            }

            return states;
        }

        private EffectValue ReadValue(BinReader br, uint typeOffset, uint valOffset)
        {
            EffectValue value = new EffectValue();

            // long pos = br.BaseStream.Position;
            //
            // br.BaseStream.Position = _startPos + typeOffset;
            // SymbolType type = (SymbolType) br.ReadUInt32();
            // SymbolClass valClass = (SymbolClass) br.ReadUInt32();
            // uint nameOffset = br.ReadUInt32();
            // uint semanticOffset = br.ReadUInt32();
            // uint numElements = br.ReadUInt32();
            //
            // value.Type.ParameterType = type;
            // value.Type.ParameterClass = valClass;
            // value.Name = ReadString(br, nameOffset);
            // value.Semantic = ReadString(br, semanticOffset);
            // value.Type.Elements = numElements;
            
            // TODO: Implement ReadValue
            
            return value;
        }

        // private string ReadString(BinaryReader br, uint offset)
        // {
        //     long start = br.BaseStream.Position;
        //     br.BaseStream.Position = _startPos + offset;
        //     int len = br.ReadInt32();
        //     //Console.WriteLine("Length: " + len);
        //     string result = Encoding.ASCII.GetString(br.ReadBytes(len));
        //     br.BaseStream.Position = start;
        //     return result;
        // }

        private void ReadSmallObjects(uint numSmallObjects, BinReader br)
        {
            // TODO: Implement ReadSmallObjects
        }

        private void ReadLargeObjects(uint numLargeObjects, BinReader br)
        {
            // TODO: Implement ReadLargeObjects
        }
    }
}