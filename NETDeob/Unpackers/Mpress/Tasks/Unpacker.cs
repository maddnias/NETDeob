using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Mono.Cecil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Misc.Structs__Enums___Interfaces.Tasks.Base;

namespace NETDeob.Core.Unpackers.Mpress.Tasks
{
    public class Unpacker : UnpackingTask
    {
        public Unpacker(AssemblyDefinition asmDef) : base(asmDef)
        {
            CurrentInstance = this;
            RoutineDescription = "Unpacking";
        }

        [DeobfuscationPhase(1, "Data retrieval")]
        public static bool Phase1()
        {
            var target = AsmDef.FindMethod(mDef => mDef.Name == "lf");
            _offset = (int)Convert.ChangeType(target.Body.Instructions.First(instr => instr.IsLdcI4WOperand()).Operand, typeof(Int32));

            PhaseParam = lf(DeobfuscatorContext.InPath);
            
            if(PhaseParam.Length == 0)
            {
                PhaseError = new PhaseError
                                 {
                                     Level = PhaseError.ErrorLevel.Critical,
                                     Message = "Failed to retrieve data!"
                                 };

                return false;
            }

            Logger.VSLog("Retrieved compressed data...");

            return true;
        }

        [DeobfuscationPhase(2, "Decompression")]
        public static bool Phase2()
        {
            string filename;
            File.WriteAllBytes(filename = Path.Combine(Application.StartupPath, "tmpFile.exe"), PhaseParam);

            if (LzmatWrapper.Decompress(filename))
                Logger.VSLog(string.Format("Decompressed file, raw size: {0}...", File.ReadAllBytes(filename).Length));
            else
            {
                PhaseError = new PhaseError
                {
                    Level = PhaseError.ErrorLevel.Critical,
                    Message = "Failed to decompress data!"
                };

                return false;
            }

            if (File.Exists(Path.Combine(Application.StartupPath, "tmpFile.exe")))
            {
                File.Delete(Path.Combine(Application.StartupPath, "tmpFile.exe"));
                Logger.VSLog("Cleaned up files...");
            }

            return true;
        }

        #region Reversed Methods

        private static int _offset = 0;

// ReSharper disable InconsistentNaming
        private static byte[] lf(string fn)
// ReSharper restore InconsistentNaming
        {
            FileStream input = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.Read);
            int length = (int)input.Length;
            input.Seek(_offset, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(input);
            int num2 = reader.ReadInt32();
            if ((num2 >= 2) && (num2 <= (length - 0x200)))
            {
                input.Seek(num2, SeekOrigin.Begin);
                if (reader.ReadUInt32() == 0x4550)
                {
                    ushort num4 = reader.ReadUInt16();
                    if (num4 == 0x8664)
                    {
                        num2 += 0x144;
                    }
                    else
                    {
                        num2 += 0x15c;
                    }
                    input.Seek(num2, SeekOrigin.Begin);
                    int num5 = reader.ReadInt32();
                    if (num4 == 0x8664)
                    {
                        num2 -= 12;
                        input.Seek(num2, SeekOrigin.Begin);
                        num5 += reader.ReadInt32();
                    }
                    else
                    {
                        num5 += 0x10;
                    }
                    if ((num5 < length) && (num5 >= 0x300))
                    {
                        length -= num5;
                        byte[] buffer = new byte[length];
                        input.Seek(num5, SeekOrigin.Begin);
                        input.Read(buffer, 0, length);
                        input.Close();

                        return buffer;
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
