using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Engine;
using NETDeob.Misc;

namespace NETDeob.Deobfuscators.Netz.Tasks
{
    public class Unpacker : IDeobfuscationTask
    {
        private AssemblyDefinition _asmDef;

        public Unpacker(AssemblyDefinition asmDef)
        {
            _asmDef = asmDef;
        }

        public unsafe override void PerformTask()
        {
            var target = _asmDef.EntryPoint;
            target = target.Body.Instructions.FirstOfOpCode(OpCodes.Call, 3).Operand as MethodDefinition;
      
            var resName = target.Body.Instructions.FirstOfOpCode(OpCodes.Ldstr).Operand as string;

            Logger.VSLog(string.Format("Found compressed resource: {0}...", resName));

            var resReader = new ResourceReader(_asmDef.FindResource(res => res.Name == "app.resources").GetResourceStream());
            var en = resReader.GetEnumerator();
            byte[] resData = null;

            while(en.MoveNext())
            {
                if (en.Key.ToString() == resName)
                    resData = en.Value as byte[];
            }

            File.WriteAllBytes(DeobfuscatorContext.OutPath, GetAssemblyData(resData));
            Logger.VSLog("Writing decompressed payload to disk...");
        }

        #region Reversed methods

        private byte[] GetAssemblyData(byte[] data)
        {
            MemoryStream stream = null;
            Assembly assembly = null;

            Logger.VSLog("Decompressing payload (" + data.Length + " bytes)...");
            stream = UnZip(data);
            stream.Seek(0L, SeekOrigin.Begin);

            return stream.ToArray();
        }

        private static MemoryStream UnZip(byte[] data)
        {
            if (data == null)
            {
                return null;
            }
            MemoryStream baseInputStream = null;
            MemoryStream stream2 = null;
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
                baseInputStream = null;
                stream3 = null;
            }
            return stream2;
        }

        #endregion

        public override void CleanUp()
        {
            //throw new NotImplementedException();
        }
    }
}
