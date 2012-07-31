using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Misc.Structs__Enums___Interfaces.Tasks.Base;
using SevenZip.Sdk.Compression.Lzma;

namespace NETDeob.Core.Unpackers.Sixxpack.Tasks
{
    public class Unpacker : UnpackingTask
    {
        public Unpacker(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "Unpack Sixxpack assembly";
        }

        [DeobfuscationPhase(1, "Locate and load compressed data")]
        public static bool Phase1()
        {
            var orig = AsmDef.EntryPoint.DeclaringType.GetStaticConstructor();

            if (orig == null)
            {
                PhaseError = new PhaseError
                                 {
                                     Level = PhaseError.ErrorLevel.Critical,
                                     Message = "Could not find orig field!"
                                 };
                return false;
            }

            PhaseParam = (int) orig.Body.Instructions.First(instr => instr.OpCode == OpCodes.Ldc_I4).Operand;

            Logger.VSLog(string.Format("Found orig field initializing value; {0}...", PhaseParam));
            return true;
        }

        [DeobfuscationPhase(2, "Create data stream")]
        public static bool Phase2()
        {
            var orig = PhaseParam;
            var inStream = new MemoryStream();

            var stream2 = new FileStream(DeobfuscatorContext.InPath, FileMode.Open, FileAccess.Read)
                              {
                                  Position = orig
                              };

            byte[] buffer = new byte[stream2.Length - orig];
            stream2.Read(buffer, 0, Convert.ToInt32(buffer.Length));
            inStream.Write(buffer, 0, buffer.Length);
            inStream.Seek(0L, SeekOrigin.Begin);

            if (buffer.Length == 0)
            {
                PhaseError = new PhaseError
                                 {
                                     Level = PhaseError.ErrorLevel.Critical,
                                     Message = "Data buffer is empty!"
                                 };
                return false;
            }

            Logger.VSLog(string.Format("Created memorystream ({0} bytes)...", buffer.Length));
            PhaseParam = inStream;

            return true;
        }

        [DeobfuscationPhase(3, "Decompress stream and write to file")]
        public static bool Phase3()
        {
            var dataStream = PhaseParam;
            var finalData = Decompress(dataStream);

            Logger.VSLog(string.Format("Decompressed stream, raw size: {0}...", finalData.Length));

            dataStream.Dispose();
            File.WriteAllBytes(DeobfuscatorContext.OutPath, finalData);

            return true;
        }

        #region Reversed methods

        public static byte[] Decompress(Stream inStream)
        {
            MemoryStream outStream = new MemoryStream();
            byte[] buffer = new byte[5];
            if (inStream.Read(buffer, 0, 5) != 5)
            {
                throw new Exception("Err");
            }
            Decoder decoder = new Decoder();
            decoder.SetDecoderProperties(buffer);
            long outSize = 0L;
            for (int i = 0; i < 8; i++)
            {
                int num3 = inStream.ReadByte();
                if (num3 < 0)
                {
                    throw new Exception("Err");
                }
                outSize |= ((byte)num3) << (8 * i);
            }
            long inSize = inStream.Length - inStream.Position;
            decoder.Code(inStream, outStream, inSize, outSize, null);
            return outStream.ToArray();
        }

        #endregion
    }
}
