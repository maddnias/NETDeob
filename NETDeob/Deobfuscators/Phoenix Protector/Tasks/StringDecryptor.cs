using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Misc.Structs__Enums___Interfaces.Tasks.Base;

namespace NETDeob.Deobfuscators.Phoenix_Protector
{
    public class PhoenixStringWorker : AssemblyDeobfuscationTask, IStringDecryptor
    {
        public class PhoenixEntry : DecryptionContext
        {
            public string OldString;

            public Instruction BadInstruction;
            public MethodDefinition Source;

            public override string ToString()
            {
                return string.Format(@"[Decrypt] ""{0}"" -> ""{1}""", OldString, PlainText);
            }
        }

        public PhoenixStringWorker(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "String decryption";
        }

        [DeobfuscationPhase(1, "Locating decryptor method")]
        public bool Phase1()
        {
            PhaseParam = FindDecryptor();
            MarkMember(PhaseParam.DeclaringType);

            if (PhaseParam == null)
            {
                ThrowPhaseError("No string encryption?", 0, true);
                return true;
            }

            Logger.VSLog("Found string decryptor method at " + PhaseParam.Name + "...");
            return true;
        }

        [DeobfuscationPhase(2, "Construct entries")]
        public bool Phase2()
        {
            var decEntries = ConstructEntries<PhoenixEntry>(PhaseParam as MethodDefinition).ToList();

            PhaseParam = decEntries;
            Logger.VSLog(string.Format("{0} decryption entries constructed...", decEntries.Count));

            return true;
        }

        [DeobfuscationPhase(3, "Decrypt strings")]
        public bool Phase3()
        {
            var decEntries = PhaseParam;

            foreach (var entry in PhaseParam)
            {
                var tmpEntry = entry as DecryptionContext;
                DecryptEntry(ref tmpEntry );

                Logger.VLog(entry.ToString());
            }

            Logger.VSLog(string.Format("{0} strings decrypted...", decEntries.Count));
            return true;
        }

        [DeobfuscationPhase(4, "Process entries")]
        public bool Phase4()
        {
            var decEntries = PhaseParam;

            foreach (var entry in decEntries)
                ProcessEntry(entry);

            Logger.VSLog("All entries processed...");
            return true;
        }

        #region Reversed methods

        private static string DecryptString(string str)
        {
            var chrArr = new char[str.Length];
            var i = 0;

            foreach (char c in str)
                chrArr[i] =
                    char.ConvertFromUtf32((((byte) ((c >> 8) ^ i) << 8) | (byte) (c ^ (chrArr.Length - i++))))[0];

            return string.Intern(new string(chrArr));
        }

        #endregion

        public static MethodDefinition FindDecryptor()
        {
            var signature = new ILSignature
                                {
                                    StartIndex = 1,
                                    StartOpCode = OpCodes.Nop,
                                    Instructions = new List<OpCode>
                                                       {
                                                           OpCodes.Callvirt,
                                                           OpCodes.Stloc_0,
                                                           OpCodes.Ldloc_0,
                                                           OpCodes.Newarr,
                                                           OpCodes.Stloc_1,
                                                           OpCodes.Ldc_I4_0,
                                                           OpCodes.Stloc_2
                                                       }
                                };

            return AsmDef.FindMethod(mdef => SignatureFinder.IsMatch(mdef, signature));
        }

        public void InitializeDecryption(object param)
        {
            // not used
        }
        public void DecryptEntry(ref DecryptionContext entry)
        {
            var pEntry = entry as PhoenixEntry;
            pEntry.PlainText = DecryptString(pEntry.OldString);
        }
        public void ProcessEntry(DecryptionContext entry)
        {
            var pEntry = entry as PhoenixEntry;
            var ilProc = pEntry.Source.Body.GetILProcessor();

            ilProc.InsertBefore(pEntry.BadInstruction, ilProc.Create(OpCodes.Ldstr, pEntry.PlainText));
        }
        public IEnumerable<T> ConstructEntries<T>(object param) where T : DecryptionContext
        {
            foreach (var mDef in AsmDef.FindMethods(method => method.HasBody))
                foreach (var instr in mDef.Body.Instructions)
                {
                    if (instr.OpCode == OpCodes.Ldstr)
                        if (instr.Next.OpCode == OpCodes.Call)
                            if ((instr.Next.Operand as MethodReference).SafeRefCheck(param as MethodDefinition))
                            {
                                var entry = new PhoenixEntry
                                                {
                                                    BadInstruction = instr,
                                                    Source = mDef,
                                                    OldString = instr.Operand as string
                                                };

                                MarkMember(instr, mDef);
                                MarkMember(instr.Next, mDef);

                                yield return (T) Convert.ChangeType(entry, typeof (T));
                            }
                }
        }
    }
}
