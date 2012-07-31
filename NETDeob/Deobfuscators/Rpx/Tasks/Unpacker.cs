using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Engine;
using NETDeob.Misc.Structs__Enums___Interfaces;
using NETDeob.Misc;

namespace NETDeob.Deobfuscators.Rpx.Tasks
{
    class Unpacker : IDeobfuscationTask
    {
        private AssemblyDefinition _asmDef;

        public Unpacker(AssemblyDefinition asmDef)
        {
            _asmDef = asmDef;
        }

        public override void PerformTask()
        {
            var resStream = GetResourceData();

            if(resStream == null){
                Logger.VSLog("Could not find resource!");
                return;
            }

            using(var package = Package.Open(resStream, FileMode.Open, FileAccess.Read))
            {
                File.WriteAllBytes(DeobfuscatorContext.OutPath,
                                   DecompressData(package,
                                                  string.Concat("/",
                                                                DeobfuscatorContext.InPath.Substring(
                                                                    DeobfuscatorContext.InPath.LastIndexOf("\\") + 1))));
            }

        }

        private Stream GetResourceData()
        {
            foreach (var res in _asmDef.MainModule.Resources)
                if (res.Name == _asmDef.EntryPoint.Body.Instructions.FirstOfOpCode(OpCodes.Ldstr).Operand as string)
                {
                    Logger.VSLog(string.Concat("Found compressed assembly; ", res.Name, " (", (res as EmbeddedResource).GetResourceData().Length,") bytes..."));
                    return (res as EmbeddedResource).GetResourceStream();
                }

            return null;
        }

        #region Reversed methods

        private byte[] DecompressData(Package p, string u)
        {
            Logger.VSLog("Decompressing data...");

            byte[] buffer;
            Uri partUri = new Uri(u, UriKind.Relative);
            using (Stream stream = p.GetPart(partUri).GetStream())
            {
                buffer = new byte[(int)stream.Length];
                stream.Read(buffer, 0, buffer.Length);
            }

            return buffer;
        }

        #endregion

        public override void CleanUp()
        {
            
        }
    }
}
