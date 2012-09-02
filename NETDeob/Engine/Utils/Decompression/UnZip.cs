using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace NETDeob.Core.Engine.Utils.Decompression
{
    public static class UnZip
    {
        public static MemoryStream ProcessData(byte[] data)
        {
            if (data == null)
            {
                return null;
            }
            MemoryStream baseInputStream = null;
            MemoryStream stream2;
            InflaterInputStream stream3 = null;
            try
            {
                baseInputStream = new MemoryStream(data);
                stream2 = new MemoryStream();
                stream3 = new InflaterInputStream(baseInputStream);
                byte[] buffer = new byte[data.Length];
                while (true)
                {
                    int count = stream3.Read(buffer, 0, buffer.Length);
                    if (count <= 0)
                    {
                        break;
                    }
                    stream2.Write(buffer, 0, count);
                }
                stream2.Flush();
                stream2.Seek(0L, SeekOrigin.Begin);
            }
            finally
            {
                if (baseInputStream != null)
                {
                    baseInputStream.Close();
                }
                if (stream3 != null)
                {
                    stream3.Close();
                }
            }
            return stream2;
        }
    }
}
