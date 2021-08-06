using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			public SymbolStructMember[] Members;
		}
		
		public struct EffectValue
		{
			public string Name;
			public string Semantic;
			public SymbolTypeInfo Type;
			
			public IList Values;

			public int[] ValuesI => (int[])Values;
			public float[] ValuesF => (float[])Values;

			public EffectSamplerState[] ValuesSS => (EffectSamplerState[])Values;
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

		public struct EffectSamplerMap
		{
			public SymbolType Type;
			public string Name;
		}

		public struct EffectObject
		{
			public SymbolType Type;

			public EffectShader Shader;
			public EffectSamplerMap Mapping;
			public string String;//EffectString
			//public EffectTexture Texture;
		}

		public struct EffectSamplerState
		{
			public SamplerStateType Type;
			public EffectValue Value;
		}

		/* Defined later in the state change types... */
		public struct SamplerStateRegister
		{
			public string SamplerName;
			public uint SamplerRegister;
			public EffectSamplerState[] SamplerStates;
		}

		public sealed class Preshader
		{
			public double[] Literals;
			public uint TempCount;
			public Symbol[] Symbols;
			//public PreshaderInstruction[] Instructions;
		}

		public struct EffectShader
		{
			public SymbolType Type;
			public int Technique;
			public int Pass;
			public bool IsPreshader;
			public uint[] PreshaderParams;
			public uint[] Params;
			public SamplerStateRegister[] Samplers;
			
			//union
			//{
			public object Shader; /* glShader, mtlShader, etc. */
			public Preshader Preshader;
			//};
		}

		public enum SymbolRegisterSet
		{
			Bool,
			Int4,
			Float4,
			Sampler,
		}
		public struct Symbol
		{
			public string Name;
			public SymbolRegisterSet RegisterSet;
			public uint RegisterIndex;
			public uint RegisterCount;
			public SymbolTypeInfo Info;
		}

		public class GlShader{}

		public List<EffectParam> EffectParams;
		public List<EffectTechnique> EffectTechniques;
		public EffectObject[] EffectObjects;
		
		public EffectParser(Effect effect)
		{
			_effect = effect;
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

			var @base = br.Slice(0);
			br.Skip(offset);
			//length -= offset; // ???

			//if (length < 16)
			//    throw new EndOfStreamException();

			uint numParams = br.Read<uint>();
			uint numTechniques = br.Read<uint>();
			uint unknown = br.Read<uint>();
			uint numObjects = br.Read<uint>();

			EffectObjects = new EffectObject[numObjects];

			EffectParams = ReadParameters(numParams, br, @base);
			EffectTechniques = ReadTechniques(numTechniques, br, @base);

			//if (length < 8)
			//    throw new EndOfStreamException();

			uint numSmallObjects = br.Read<uint>();
			uint numLargeObjects = br.Read<uint>();

			ReadSmallObjects(numSmallObjects, br);
			ReadLargeObjects(numLargeObjects, numSmallObjects, br);
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

		private EffectValue ReadValue(BinReader @base, uint typeOffset, uint valOffset)
		{
			EffectValue value = new EffectValue();

			var typePtr = @base.Slice(typeOffset);
			var valPtr = @base.Slice(valOffset);

			SymbolType type = typePtr.Read<SymbolType>();
			SymbolClass valClass = typePtr.Read<SymbolClass>();
			uint nameOffset = typePtr.Read<uint>();
			uint semanticOffset = typePtr.Read<uint>();
			uint numElements = typePtr.Read<uint>();

			value.Type.ParameterType = type;
			value.Type.ParameterClass = valClass;
			value.Name = @base.ReadString(nameOffset);
			value.Semantic = @base.ReadString(semanticOffset);
			value.Type.Elements = numElements;

			/* Class sanity check */
			if(valClass < SymbolClass.Scalar || valClass > SymbolClass.Struct)
			{
				throw new Exception();
			}
			
			if (valClass == SymbolClass.Scalar
			 || valClass == SymbolClass.Vector
			 || valClass == SymbolClass.MatrixRows
			 || valClass == SymbolClass.MatrixColumns)
			{
				/* These classes only ever contain scalar values */
				if(type < SymbolType.Bool || type > SymbolType.Float)
				{
					throw new Exception();
				}

				var columnCount = typePtr.Read<uint>();
				var rowCount = typePtr.Read<uint>();

				value.Type.Columns = columnCount;
				value.Type.Rows = rowCount;

				uint size = 4 * rowCount;
				if(numElements > 0)
				{
					size *= numElements;
				}
				var values = new float[size];

				for(uint i = 0; i<size; i+=4)
				{
					for(uint c = 0; c<columnCount; c++)
					{
						values[i+c] = valPtr.Read<float>(columnCount*i + c);
					}
				}
				value.Values = values;
			}
			else if(valClass == SymbolClass.Object)
			{
				/* This class contains either samplers or "objects" */
				if(type < SymbolType.String || type > SymbolType.VertexShader)
				{
					throw new Exception();
				}

				if (type == SymbolType.Sampler
				 || type == SymbolType.Sampler1D
				 || type == SymbolType.Sampler2D
				 || type == SymbolType.Sampler3D
				 || type == SymbolType.SamplerCube)
				{
					var numStates = valPtr.Read<uint>();

					var values = new EffectSamplerState[numStates];

					for(int i=0; i<numStates; i++)
					{
						var state = new EffectSamplerState();
						
						var stype = (SamplerStateType)(valPtr.Read<uint>() & ~0xA0);
						valPtr.Skip<uint>();//FIXME
						var stateTypeOffset = valPtr.Read<uint>();
						var stateValOffset = valPtr.Read<uint>();

						state.Type = stype;
						state.Value = ReadValue(@base, stateTypeOffset, stateValOffset);

						if(stype == SamplerStateType.Texture)
						{
							EffectObjects[state.Value.ValuesI[0]].Type = type;
						}

						values[i] = state;
					}

					value.Values = values;
				}
				else
				{
					uint numObjects = 1;
					if(numElements > 0)
					{
						numObjects = numElements;
					}

					var values = new int[numObjects];

					for(int i=0; i<values.Length; i++)
					{
						var val = valPtr.Read<uint>();
						values[i] = (int)val;

						EffectObjects[val].Type = type;
					}

					value.Values = values;
				}
			}
			else if(valClass == SymbolClass.Struct)
			{
				value.Type.Members = new SymbolStructMember[typePtr.Read<uint>()];

				uint structSize = 0;

				for(int i=0; i<value.Type.Members.Length; i++)
				{
					ref var mem = ref value.Type.Members[i];

					mem.Info.ParameterType = typePtr.Read<SymbolType>();
					mem.Info.ParameterClass = typePtr.Read<SymbolClass>();

					var memNameOffset = typePtr.Read<uint>();
					var memSemantic = typePtr.Read<uint>();//Unused
					mem.Name = @base.ReadString(memNameOffset);

					mem.Info.Elements = typePtr.Read<uint>();
					mem.Info.Columns = typePtr.Read<uint>();
					mem.Info.Rows = typePtr.Read<uint>();

					if(mem.Info.ParameterClass < SymbolClass.Scalar || mem.Info.ParameterClass > SymbolClass.MatrixColumns)
					{
						throw new Exception();
					}
					if(mem.Info.ParameterType < SymbolType.Bool || mem.Info.ParameterType > SymbolType.Float)
					{
						throw new Exception();
					}

					mem.Info.Members = null;

					uint memSize = 4 * mem.Info.Rows;
					if(mem.Info.Elements > 0)
					{
						memSize *= mem.Info.Elements;
					}
					structSize += memSize;
				}

				value.Type.Columns = structSize;
				value.Type.Rows = 1;
				var valueCount = structSize;
				if(numElements > 0)
				{
					valueCount *= numElements;
				}

				var values = new float[valueCount];

				uint dstOffset = 0;
				uint srcOffset = 0;

				int i2=0;
				do
				{
					for(int j=0; j<value.Type.Members.Length; j++)
					{
						var size = value.Type.Members[j].Info.Rows * value.Type.Members[j].Info.Elements;
						for(int k=0; k<size; k++)
						{
							for(int f=0; f<value.Type.Members[j].Info.Columns; f++)
							{
								values[dstOffset + f] = typePtr.Read<float>(srcOffset*sizeof(float));/* Yes, typeptr. -flibit */
							}
							dstOffset += 1;
							srcOffset += value.Type.Members[j].Info.Columns;
						}
					}
				}
				while(++i2 < numElements);

				value.Values = values;
			}

			return value;
		}

		private void ReadSmallObjects(uint numSmallObjects, BinReader br)
		{
			if(numSmallObjects == 0){return;}

			for(int i = 1; i < numSmallObjects + 1; i++)
			{
				var index = br.Read<uint>();
				var length = br.Read<uint>();

				ref var obj = ref EffectObjects[index];
				if(obj.Type == SymbolType.String)
				{
					if(length>0)
					{
						obj.String = br.ReadString(0, length);
					}
				}
				else if (obj.Type == SymbolType.Texture
					  || obj.Type == SymbolType.Texture1D
					  || obj.Type == SymbolType.Texture2D
					  || obj.Type == SymbolType.Texture3D
					  || obj.Type == SymbolType.TextureCube
					  || obj.Type == SymbolType.Sampler
					  || obj.Type == SymbolType.Sampler1D
					  || obj.Type == SymbolType.Sampler2D
					  || obj.Type == SymbolType.Sampler3D
					  || obj.Type == SymbolType.SamplerCube)
				{
					if(length>0)
					{
						obj.Mapping.Name = br.ReadString(0, length);
					}
				}
				else if (obj.Type == SymbolType.PixelShader
					  || obj.Type == SymbolType.VertexShader)
				{
					var mainfn = $"ShaderFunction{index}";
					obj.Shader.Technique = -1;
					obj.Shader.Pass = -1;
					obj.Shader.Shader = CompileShader(mainfn, br, length);

					if(obj.Shader.Shader == null)
					{
						throw new Exception();
					}

					GetParseData(obj.Shader.Shader, out var symbols, out var preshader);

					int samplerCount = 0;
					for(int j=0; j<symbols.Length; j++)
					{
						if(symbols[j].RegisterSet == SymbolRegisterSet.Sampler)
						{
							samplerCount++;
						}
					}
					obj.Shader.Params = new uint[samplerCount];
					obj.Shader.Samplers = new SamplerStateRegister[samplerCount];
					uint curSampler = 0;
					for(int j=0; j<symbols.Length; j++)
					{
						uint par = FindParameter(symbols[j].Name);
						obj.Shader.Params[j] = par;
						if(symbols[j].RegisterSet == SymbolRegisterSet.Sampler)
						{
							obj.Shader.Samplers[curSampler] = new SamplerStateRegister
							{
								SamplerName = EffectParams[(int)par].Value.Name,
								SamplerRegister = symbols[j].RegisterIndex,
								SamplerStates = EffectParams[(int)par].Value.ValuesSS
							};
							curSampler++;
						}
					}

					if(preshader != null)
					{
						obj.Shader.PreshaderParams = new uint[preshader.Symbols.Length];
						for(int j=0; j<preshader.Symbols.Length; j++)
						{
							obj.Shader.PreshaderParams[j] = FindParameter(preshader.Symbols[j].Name);
						}
					}
				}
				else
				{
					throw new Exception("Small object type unknown!");
				}

				//Object block is always a multiple of four
				uint blocklen = (length + 3) - ((length - 1) % 4);
				br.Skip(blocklen);
			}
		}

		//https://github.com/icculus/mojoshader/blob/main/mojoshader_effects.c#L749
		private void ReadLargeObjects(uint numLargeObjects, uint numSmallObjects, BinReader br)
		{
			if(numLargeObjects == 0){return;}

			uint numObjects = numSmallObjects + numLargeObjects + 1;
			for(uint i=numSmallObjects+1; i<numObjects; i++)
			{
				int technique = br.Read<int>();
				int index = br.Read<int>();
				uint FIXME = br.Read<uint>();
				int state = br.Read<int>();
				uint type = br.Read<uint>();
				uint length = br.Read<uint>();

				int objectIndex;
				if(technique == -1)
				{
					objectIndex = EffectParams[index].Value.ValuesSS[state].Value.ValuesI[0];
				}
				else
				{
					objectIndex = EffectTechniques[technique].Passes[index].States[state].Value.ValuesI[0];
				}

				ref var obj = ref EffectObjects[objectIndex];
				if (obj.Type == SymbolType.PixelShader
				 || obj.Type == SymbolType.VertexShader)
				{
					obj.Shader.Technique = technique;
					obj.Shader.Pass = index;

					if(type == 2)
					{
						// This is a standalone preshader!
						// It exists solely for effect passes that do not use a single
						// vertex/fragment shader.
						obj.Shader.IsPreshader = true;

						var array = br.ReadString(0);
						var start = (uint)array.Length+4;//Gets the total length of the string and length field
						obj.Shader.Params = new uint[1];
						obj.Shader.Params[0] = FindParameter(array);

						obj.Shader.Preshader = ParsePreshader(br.Slice(start), length);

						// !!! FIXME: check for errors.
						obj.Shader.PreshaderParams = new uint[obj.Shader.Preshader.Symbols.Length];
						for(int j=0; j<obj.Shader.Preshader.Symbols.Length; j++)
						{
							obj.Shader.PreshaderParams[j] = FindParameter(obj.Shader.Preshader.Symbols[j].Name);
						}
					}
					else
					{

						string mainfn = $"ShaderFuntion{objectIndex}";
						obj.Shader.Shader = CompileShader(mainfn, br, length);
						File.WriteAllBytes($"Shader_{i}.bin", (byte[])obj.Shader.Shader);

						//if(obj.Shader.Shader == null)
						//{
						//	throw new Exception();
						//}
						//GetParseData(obj.Shader.Shader, out var symbols, out var preshader);

						//int samplerCount = 0;
						//for(int j=0; j<symbols.Length; j++)
						//{
						//	if(symbols[j].RegisterSet == SymbolRegisterSet.Sampler)
						//	{
						//		samplerCount++;
						//	}
						//}
						//obj.Shader.Params = new uint[samplerCount];
						//obj.Shader.Samplers = new SamplerStateRegister[samplerCount];
						//uint curSampler = 0;
						//for(int j=0; j<symbols.Length; j++)
						//{
						//	uint par = FindParameter(symbols[j].Name);
						//	obj.Shader.Params[j] = par;
						//	if(symbols[j].RegisterSet == SymbolRegisterSet.Sampler)
						//	{
						//		obj.Shader.Samplers[curSampler] = new SamplerStateRegister
						//		{
						//			SamplerName = EffectParams[(int)par].Value.Name,
						//			SamplerRegister = symbols[j].RegisterIndex,
						//			SamplerStates = EffectParams[(int)par].Value.ValuesSS
						//		};
						//		curSampler++;
						//	}
						//}

						//if(preshader != null)
						//{
						//	obj.Shader.PreshaderParams = new uint[preshader.Symbols.Length];
						//	for(int j=0; j<preshader.Symbols.Length; j++)
						//	{
						//		obj.Shader.PreshaderParams[j] = FindParameter(preshader.Symbols[j].Name);
						//	}
						//}
					}
				}
				else if(obj.Type == SymbolType.Texture
					 || obj.Type == SymbolType.Texture1D
					 || obj.Type == SymbolType.Texture2D
					 || obj.Type == SymbolType.Texture3D
					 || obj.Type == SymbolType.TextureCube
					 || obj.Type == SymbolType.Sampler
					 || obj.Type == SymbolType.Sampler1D
					 || obj.Type == SymbolType.Sampler2D
					 || obj.Type == SymbolType.Sampler3D
					 || obj.Type == SymbolType.SamplerCube)
				{
					obj.Mapping.Name = br.ReadString(0, length);
				}
				else if (obj.Type != SymbolType.Void) // FIXME: Why? -flibit
				{
					throw new Exception("Large object type unknown!");
				}
				
				// Object block is always a multiple of four
				uint blocklen = (length + 3) - ((length - 1) % 4);
				br.Skip(blocklen);
			}
		}

		private uint FindParameter(string name)
		{
			for(int i=0; i<EffectParams.Count; i++)
			{
				if(name == EffectParams[i].Value.Name)
				{
					return (uint)i;
				}
			}
			throw new Exception("Parameter not found!");
		}

		private void GetParseData(object shader, out Symbol[] symbols, out Preshader preshader)
		{
			throw new NotImplementedException();
		}

		private Preshader ParsePreshader(BinReader br, uint length)
		{
			throw new NotImplementedException();
		}

		private object CompileShader(string tokenBuf, BinReader br, uint length)
		{
			//throw new NotImplementedException();
			return br.ReadBytes(0, length);
		}

		//https://github.com/icculus/mojoshader/blob/main/mojoshader_effects.c
		//https://github.com/icculus/mojoshader/blob/main/mojoshader_effects.h

		//MOJOSHADER_parseData
		//https://github.com/icculus/mojoshader/blob/main/mojoshader.h
	}
}