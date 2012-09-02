using System;
using System.IO;
using System.IO.Packaging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Core.Engine.Utils.Extensions;

namespace NETDeob.Core.Unpackers.Rpx.Tasks
{
    class Unpacker : UnpackingTask
    {
        public Unpacker(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "Unpacking";
        }

        #region Phases

        [DeobfuscationPhase(1, "Resource retrieval")]
        public static bool Phase1()
        {
            var resStream = GetResourceData();

            if (resStream == null){
                Logger.VSLog("Could not find resource!");
                return false;
            }

            PhaseParam = resStream;
            return true;
        }

        [DeobfuscationPhase(2, "Decompression & creating output")]
        public static bool Phase2()
        {
            var resStream = PhaseParam;

            using (var package = Package.Open(resStream, FileMode.Open, FileAccess.Read))
            {
                //File.WriteAllBytes(Globals.DeobContext.OutPath,
                //                   DecompressData(package,
                //                                  string.Concat("/",
                //                                                Globals.DeobContext.InPath.Substring(
                //                                                    Globals.DeobContext.InPath.LastIndexOf("\\", StringComparison.Ordinal) + 1))));
                File.WriteAllBytes(Globals.DeobContext.OutPath,
                                   DecompressData(package,
                                                  AsmDef.EntryPoint.Body.Instructions.GetOperandAt<string>(
                                                      OpCodes.Ldstr, 1)));
            }

            return true;
        }

        #endregion

        private static Stream GetResourceData()
        {
            foreach (var res in AsmDef.MainModule.Resources)
                if (res.Name == AsmDef.EntryPoint.Body.Instructions.FirstOfOpCode(OpCodes.Ldstr).Operand as string)
                {
                    Logger.VSLog(string.Concat("Found compressed assembly; ", res.Name, " (", (res as EmbeddedResource).GetResourceData().Length,") bytes..."));
                    return (res as EmbeddedResource).GetResourceStream();
                }

            return null;
        }

        #region Reversed methods

        private static byte[] DecompressData(Package p, string u)
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

    }
}
