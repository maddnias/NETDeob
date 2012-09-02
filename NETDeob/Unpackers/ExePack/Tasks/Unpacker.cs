using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Mono.Cecil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Misc.Structs__Enums___Interfaces.Tasks.Base;

namespace NETDeob.Core.Unpackers.ExePack.Tasks
{
    public class Unpacker : UnpackingTask
    {
        public Unpacker(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "Unpack ExePack assembly";
        }

        [DeobfuscationPhase(1, "Locate and load compressed resource")]
        public static bool Phase1()
        {
            EmbeddedResource targetRes = null;

            foreach (var res in AsmDef.MainModule.Resources)
            {
                try
                {
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
                    Convert.FromBase64String(res.Name);
// ReSharper restore ReturnValueOfPureMethodIsNotUsed
                    targetRes = res as EmbeddedResource;
                }
                catch
                {
                    PhaseError = new PhaseError
                                     {
                                         Level = PhaseError.ErrorLevel.Critical,
                                         Message = "Could not locate compressed resource!"
                                     };
                    return false;
                }
            }

            PhaseParam = targetRes.Name;
            Logger.VSLog(string.Format("Located compressed resource at {0}...", PhaseParam));

            return true;
        }

        [DeobfuscationPhase(2, "Decompress stream and write output")]
        public static bool Phase2()
        {
            var finalData = Decompress(PhaseParam);

            if(finalData.Length == 0)
            {
                PhaseError = new PhaseError
                                 {
                                     Level = PhaseError.ErrorLevel.Critical,
                                     Message = "Could not decompress buffer!"
                                 };
                return false;
            }

            Logger.VSLog(string.Format("Decompressed data, raw size: {0} bytes...", finalData.Length));
            File.WriteAllBytes(Globals.DeobContext.OutPath, finalData);

            return true;
        }

        #region Reversed methods

        public static byte[] Decompress(string resName)
        {
            using (Stream stream = Assembly.LoadFile(Globals.DeobContext.InPath).GetManifestResourceStream(resName))
            {
                int num;

                num = new BinaryReader(stream).ReadInt32();

                using (var stream2 = new DeflateStream(stream, CompressionMode.Decompress))
                    return new BinaryReader(stream2).ReadBytes(num);
            }
        }

        #endregion
    }
}