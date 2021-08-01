using System.Collections.Generic;
using System.IO;

namespace XNAShaderDecompiler
{
    public static class ContentManager
    {
        // From FNA
        private static List<char> targetPlatformIdentifiers = new List<char>()
        {
            'w', // Windows (DirectX)
            'x', // Xbox360
            'm', // WindowsPhone
            'i', // iOS
            'a', // Android
            'd', // DesktopGL
            'X', // MacOSX
            'W', // WindowsStoreApp
            'n', // NativeClient
            'u', // Ouya
            'p', // PlayStationMobile
            'M', // WindowsPhone8
            'r', // RaspberryPi
            'P', // Playstation 4
            'g', // WindowsGL (deprecated for DesktopGL)
            'l', // Linux (deprecated for DesktopGL)
        };

        public static T ReadAsset<T>(string file)
        {
            byte[] xnbHeader = new byte[4];
            using Stream stream = File.OpenRead(file);

            object result = null;
            
            stream.Read(xnbHeader, 0, xnbHeader.Length);
            if (xnbHeader[0] == 'X' &&
                xnbHeader[1] == 'N' &&
                xnbHeader[2] == 'B' &&
                targetPlatformIdentifiers.Contains((char) xnbHeader[3]))
            {
                using (BinaryReader br = new BinaryReader(stream))
                using (ContentReader reader = GetContentReaderFromXNB(file, stream, br, (char) xnbHeader[3]))
                {
                    result = reader.ReadAsset<T>();
                }
            }

            return (T) result;
        }

        private static ContentReader GetContentReaderFromXNB(string name, Stream stream, BinaryReader br, char platform)
        {
            byte version = br.ReadByte();
            byte flags = br.ReadByte();
            bool compressed = (flags & 0x80) != 0;
            if (version != 5 && version != 4)
            {
                throw new ContentLoadException("Invalid XNB version");
            }

            int len = br.ReadInt32();
            ContentReader reader;
            if (compressed)
            {
                int compressedSize = len - 14;
                int decompressedSize = br.ReadInt32();

                MemoryStream decompressedStream = new MemoryStream(
                    new byte[decompressedSize],
                    0,
                    decompressedSize,
                    true,
                    true
                );

                MemoryStream compressedStream = new MemoryStream(
                    new byte[compressedSize],
                    0,
                    compressedSize,
                    true,
                    true
                );
                stream.Read(compressedStream.GetBuffer(), 0, compressedSize);

                LzxDecoder dec = new LzxDecoder(16);
                int decodedBytes = 0;
                long pos = 0;

                while (pos < compressedSize)
                {
                    int hi = compressedStream.ReadByte();
                    int lo = compressedStream.ReadByte();
                    int block_size = (hi << 8) | lo;
                    int frame_size = 0x8000;
                    
                    if (hi == 0xFF)
                    {
                        hi = lo;
                        lo = (byte) compressedStream.ReadByte();
                        frame_size = (hi << 8) | lo;
                        hi = (byte) compressedStream.ReadByte();
                        lo = (byte) compressedStream.ReadByte();
                        block_size = (hi << 8) | lo;
                        pos += 5;
                    }
                    else
                    {
                        pos += 2;
                    }
                    
                    if (block_size == 0 || frame_size == 0)
                    {
                        break;
                    }
                    dec.Decompress(compressedStream, block_size, decompressedStream, frame_size);
                    pos += block_size;
                    decodedBytes += frame_size;
                    
                    compressedStream.Seek(pos, SeekOrigin.Begin);
                }
                
                if (decompressedStream.Position != decompressedSize)
                {
                    throw new ContentLoadException(
                        "Decompression of " + name + " failed. "
                    );
                }
                decompressedStream.Seek(0, SeekOrigin.Begin);
                reader = new ContentReader(
                    //this,
                    decompressedStream,
                    name,
                    version,
                    platform
                );
            }
            else
            {
                reader = new ContentReader(
                    //this,
                    stream,
                    name,
                    version,
                    platform
                );
            }

            return reader;
        }
    }
}