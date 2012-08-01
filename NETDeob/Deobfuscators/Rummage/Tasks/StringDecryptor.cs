using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Misc.Structs__Enums___Interfaces.Tasks.Base;
using NETDeob.Core.Engine.Utils.Extensions;

namespace NETDeob.Core.Deobfuscators.Rummage.Tasks
{
    public class RummageContext : DecryptionContext
    {
        public MethodDefinition SourceMethod;
        public Instruction Source;

        public FieldDefinition TargetField;
        public int Key;

        public override string ToString()
        {
            return string.Format(@"[Decrypt] ""{0}({1})"" -> ""{2}""", TargetField.Name,
                                 Key,
                                 PlainText);
        }
    }

    public class RummageStringDecryptor : AssemblyDeobfuscationTask, IStringDecryptor<RummageContext>
    {
        public RummageStringDecryptor(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "String decryption";
        }

        [DeobfuscationPhase(1, "Locate string decryption method")]
        public bool Phase1()
        {
            // WEAK!
            var target =
                AsmDef.FindMethod(
                    mDef =>
                    mDef.Body.Instructions.GetOpCodeCount(OpCodes.Xor) == 4 && 
                    mDef.Body.Variables.Count == 10 &&
                    mDef.Parameters.Count == 1 &&
                    mDef.Parameters[0].ParameterType.ToString().Contains("Int32"));

            if (target == null)
            {
                ThrowPhaseError("Could not locate decryption method!", 1, false);
                return false;
            }

            PhaseParam = target;
            Logger.VSLog(string.Format("Located decryption method at {0}::{1}...", target.DeclaringType.Name,
                                       target.Name));

            return true;
        }

        [DeobfuscationPhase(2, "Construct decryption entries")]
        public bool Phase2()
        {
            var decMethod = PhaseParam as MethodDefinition;
            var decEntries = ConstructEntries(decMethod).ToList();

            PhaseParam = new object[] {decMethod, decEntries};
            Logger.VSLog(string.Format("Constructed {0} decryption entries...", decEntries.Count));

            return true;
        }

        [DeobfuscationPhase(3, "Decrypt strings")]
        public bool Phase3()
        {
            var decEntries = PhaseParam[1] as List<RummageContext>;
            var decMethod = PhaseParam[0] as MethodDefinition;

            InitializeDecryption(decMethod.Body.Instructions.GetOperandAt<int>(OpCodes.Ldc_I4, 0));

            for (var i = 0; i < decEntries.Count; i++)
            {
                var t = decEntries[i] as DecryptionContext;

                DecryptEntry(ref t);
                Logger.VLog(t.ToString());
            }

            Logger.VSLog(string.Format("{0} strings decrypted to plaintext...", decEntries.Count));

            return true;
        }

        [DeobfuscationPhase(4, "Process decryption entries")]
        public bool Phase4()
        {
            var decEntries = PhaseParam[1];

            foreach (var entry in decEntries)
                ProcessEntry(entry);

            Logger.VSLog("All encrypted strings in assembly was replaced with plaintext...");

            return true;
        }

        [DeobfuscationPhase(5, "Mark bad members")]
        public bool Phase5()
        {
            var decEntries = PhaseParam[1];

            foreach (var entry in decEntries)
                MarkMember(entry.TargetField.DeclaringType);

            foreach (var source in AssemblyUtils.FindMethodReferences(PhaseParam[0] as MethodDefinition))
                MarkMember(source.Item1, source.Item2);

            //MarkMember(PhaseParam[0].DeclaringType);
            Logger.VSLog(string.Format("Marked {0} types, {0} fields and {1} methods for removal...", decEntries.Count,
                                       decEntries.Count + 1));

            return true;
        }

        //public static bool IsBadType(TypeDefinition type, MethodDefinition target)
        //{
        //    MethodDefinition cctor;
        //    Instruction source;

        //    if ((cctor = type.GetStaticConstructor()) == null)
        //        return false;

        //    if (type.Fields.Count != 1)
        //        return false;

        //    if ((source = cctor.Body.Instructions.FirstOrDefault(instr => instr.OpCode == OpCodes.Call)) == null)
        //        return false;

        //    if ((source.Operand as MethodReference).Resolve() != target)
        //        return false;

        //    return true;
        //}

        private static UInt32[] _decMod;
        private static int _offset;

        public bool BaseIsDecryptor(params object[] param)
        {
            MethodDefinition cctor;
            Instruction source;

            var type = param[0] as TypeDefinition;
            var target = param[1] as MethodDefinition;

            if ((cctor = type.GetStaticConstructor()) == null)
                return false;

            if (type.Fields.Count != 1)
                return false;

            if ((source = cctor.Body.Instructions.FirstOrDefault(instr => instr.OpCode == OpCodes.Call)) == null)
                return false;

            if ((source.Operand as MethodReference).Resolve() != target)
                return false;

            return true;
        }

        public void InitializeDecryption(object param)
        {
            _offset = (int) param;

            using (var fs = new FileStream(DeobfuscatorContext.InPath, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(-48L, SeekOrigin.End);

                byte[] buf1 = new byte[16], buf2 = null;
                var i = 0;

                while (i < 16)
                {
                    buf2 = buf1;
                    i += fs.Read(buf1, i, 16 - i);
                }

                Buffer.BlockCopy(buf2, 0, _decMod = new uint[4], 0, 16);
            }
        }

        public void DecryptEntry(ref RummageContext entry)
        {
            throw new NotImplementedException();
        }

        public void ProcessEntry(RummageContext entry)
        {
            var rEntry = entry;

            var ilProc = rEntry.SourceMethod.Body.GetILProcessor();
            ilProc.Replace(rEntry.Source, ilProc.Create(OpCodes.Ldstr, rEntry.PlainText));
        }

        public IEnumerable<RummageContext> ConstructEntries(object param)
        {
            foreach (var mDef in AsmDef.FindMethods(method => method.HasBody))
                foreach (var instr in mDef.Body.Instructions)
                    if (instr.OpCode == OpCodes.Ldsfld)
                    {
                        if (BaseIsDecryptor((instr.Operand as FieldReference).Resolve().DeclaringType,
                                      param as MethodDefinition))
                        {
                            var tmpEntry = new RummageContext
                            {
                                SourceMethod = mDef,
                                Source = instr,
                                TargetField = instr.Operand as FieldDefinition
                            };

                            var target = (instr.Operand as FieldDefinition).DeclaringType.GetStaticConstructor();
                            tmpEntry.Key = target.Body.Instructions.First(iinstr => iinstr.IsLdcI4()).GetLdcI4();

                            yield return tmpEntry;
                        }
                    }
        }

        public void DecryptEntry(ref DecryptionContext entry)
        {
            var rEntry = entry as RummageContext;

            if (rEntry.Key == 0)
                ThrowPhaseError("Failed to decrypt string!", 0, false);

            var str = "";
            using (var fs = new FileStream(DeobfuscatorContext.InPath, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    if (rEntry.Key == 61 + 1)
                        rEntry.Key = 62;

                    uint[] numArray = null;
                    uint[] numArray2 = null;

                    fs.Seek((rEntry.Key*4) - _offset, SeekOrigin.End);

                    byte[] outData = null;
                    var x = 0;

                    while (x < (numArray ?? new uint[1337]).Length)
                    {
                        uint num = 0;

                        try
                        {
                            num = br.ReadUInt32();
                        }
                        catch (EndOfStreamException e)
                        {
                            break;
                        }
                        catch
                        {
                            PhaseError = new PhaseError
                                             {
                                                 Level = PhaseError.ErrorLevel.Minor,
                                                 Message = "Message author!"
                                             };
                            continue;
                        }

                        var num2 = br.ReadUInt32();
                        var num3 = 3337565984;

                        var tmp = numArray2;

                        for (var i = 32; i > 0; i--)
                        {
                            num2 -= (((num << 4) ^ (num >> 5)) + num) ^
                                    (num3 + _decMod[(int) ((IntPtr) ((num3 >> 11) & 3))]);
                            num3 -= 2654435769;
                            num -= (((num2 << 4) ^ (num2 >> 5)) + num2) ^ (num3 + _decMod[(int) ((IntPtr) (num3 & 3))]);
                        }

                        uint[] numArray4;
                        if (tmp == null)
                        {
                            outData = new byte[num];
                            numArray4 = numArray = new uint[num + 11/8*2 - 1];
                        }
                        else
                        {
                            numArray[x - 1] = num;
                            numArray4 = numArray;
                        }

                        numArray4[x] = num2;
                        x += 2;

                        if (x < numArray.Length)
                            numArray2 = numArray4;

                        Buffer.BlockCopy(numArray4, 0, outData, 0, outData.Length);
                        str = Encoding.UTF8.GetString(outData);
                    }
                }
            }

            rEntry.PlainText = str;
        }
        //public void ProcessEntry(DecryptionContext entry)
        //{
        //    var rEntry = entry as RummageContext;

        //    var ilProc = rEntry.SourceMethod.Body.GetILProcessor();
        //    ilProc.Replace(rEntry.Source, ilProc.Create(OpCodes.Ldstr, rEntry.PlainText));
        //}
        //public IEnumerable<T> ConstructEntries<T>(object param) where T : DecryptionContext
        //{
        //    foreach (var mDef in AsmDef.FindMethods(method => method.HasBody))
        //        foreach (var instr in mDef.Body.Instructions)
        //            if (instr.OpCode == OpCodes.Ldsfld)
        //            {
        //                if (BaseIsDecryptor((instr.Operand as FieldReference).Resolve().DeclaringType,
        //                              param as MethodDefinition))
        //                {
        //                    var tmpEntry = new RummageContext
        //                                       {
        //                                           SourceMethod = mDef,
        //                                           Source = instr,
        //                                           TargetField = instr.Operand as FieldDefinition
        //                                       };

        //                    var target = (instr.Operand as FieldDefinition).DeclaringType.GetStaticConstructor();
        //                    tmpEntry.Key = target.Body.Instructions.First(iinstr => iinstr.IsLdcI4()).GetLdcI4();

        //                    yield return (T) Convert.ChangeType(tmpEntry, typeof (T));
        //                }
        //            }
        //}
    }
}
