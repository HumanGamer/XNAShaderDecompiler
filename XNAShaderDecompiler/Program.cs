using System;
using System.IO;

namespace XNAShaderDecompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("XNA Shader Decompiler");
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: <shader.xnb>");
                return;
            }

            string inputFile = args[0];
            string outputFile = Path.ChangeExtension(inputFile, ".fxb");
            
            Console.WriteLine("Reading XNB...");
            Effect effect = ContentManager.ReadAsset<Effect>(inputFile);
            
            Console.WriteLine("Writing FXB...");
            File.WriteAllBytes(outputFile, effect.EffectCode);
            
            Console.WriteLine("Done!");
        }
    }
}