using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Core.Engine.Utils.Extensions;

namespace NETDeob.Core.Deobfuscators.HurpFuscator.Tasks._1_1
{
    internal class StringDecryptor : AssemblyDeobfuscationTask, IStringDecryptor
    {
        public string Modifier1;
        public string Modifier2;

        public class HurpFuscatorEntry : DecryptionContext
        {
            public MethodDefinition Source;
            public List<Instruction> BadInstructions;
            public string Arg;

            public override string ToString()
            {
                return string.Format(@"[Decrypt] ""{0}"" -> ""{1}""", Arg, PlainText);
            }
        }

        public StringDecryptor(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        [DeobfuscationPhase(1, "Locate decryptor method")]
        public bool Phase1()
        {
            var decryptor = AsmDef.FindMethod(IsDecryptor);

            if (decryptor == null)
            {
                ThrowPhaseError("Could not find decryptor module!", 1, false);
                return false;
            }

            MarkMember(decryptor);

            Logger.VSLog("Found decryptor module at: " + decryptor.Name);
            PhaseParam = decryptor;

            return true;
        }

        [DeobfuscationPhase(2, "Construct decryption entries")]
        public bool Phase2()
        {
            InitializeDecryption(PhaseParam);

            Logger.VSLog(string.Format("\nLocated dynamic decryption modifiers; \n{0}\n{1}\n", Modifier1, Modifier2));

            var entryList =
                ConstructEntries<HurpFuscatorEntry>(PhaseParam as MethodDefinition).ToList();

            Logger.VSLog(string.Format("Constructed {0} entries...", entryList.Count));

            PhaseParam = entryList;
            return true;
        }

        [DeobfuscationPhase(3, "Decrypt entries")]
        public bool Phase3()
        {
            for (int i = 0; i < (PhaseParam as List<HurpFuscatorEntry>).Count; i++)
            {
                var entry = PhaseParam[i] as DecryptionContext;
                DecryptEntry(ref entry);

                Logger.VLog(entry.ToString());
            }

            return true;
        }

        [DeobfuscationPhase(3, "Process entries")]
        public bool Phase4()
        {
            foreach (var entry in PhaseParam)
                ProcessEntry(entry);

            Logger.VSLog("Decrypted all strings and replaced them in assembly...");

            return true;
        }

        public bool IsDecryptor(MethodDefinition mDef)
        {
            return (mDef.Parameters.Count == 1 &&
                    mDef.Body.Variables.Count == 7 &&
                    mDef.ReturnType.ToString().ToLower().Contains("string") &&
                    mDef.Body.Instructions.GetOpCodeCount(OpCodes.Callvirt) == 7 &&
                    mDef.Body.Instructions.GetOpCodeCount(OpCodes.Ldstr) == 2);
        }

        public void InitializeDecryption(object param)
        {
            var decryptor = param as MethodDefinition;

            Modifier1 = decryptor.Body.Instructions.GetOperandAt<string>(OpCodes.Ldstr, 0);
            Modifier2 = decryptor.Body.Instructions.GetOperandAt<string>(OpCodes.Ldstr, 1);
        }

        public void DecryptEntry(ref DecryptionContext entry)
        {
            var inputBuffer = Convert.FromBase64String((entry as HurpFuscatorEntry).Arg);
            var bytes = Encoding.UTF8.GetBytes(Modifier1);
            var rgbIV = Encoding.UTF8.GetBytes(Modifier2);
            
            var managed2 = new RijndaelManaged
            {
                BlockSize = 256,
                Padding = PaddingMode.PKCS7
            };

            (entry as HurpFuscatorEntry).PlainText =
                Encoding.Default.GetString(managed2.CreateDecryptor(bytes, rgbIV).TransformFinalBlock(inputBuffer, 0,
                                                                                                      inputBuffer.Length));
        }

        public void ProcessEntry(DecryptionContext entry)
        {
            var _entry = entry as HurpFuscatorEntry;

            var ilProc = _entry.Source.Body.GetILProcessor();

            ilProc.InsertBefore(_entry.BadInstructions[0], ilProc.Create(OpCodes.Ldstr, _entry.PlainText));
            MarkMember(_entry.BadInstructions[0], _entry.Source);
            MarkMember(_entry.BadInstructions[1], _entry.Source);
        }

        public IEnumerable<T> ConstructEntries<T>(object param) where T : DecryptionContext
        {
            foreach (var mDef in AsmDef.FindMethods(m => m.HasBody))
            {
                foreach (var instr in mDef.Body.Instructions.Where(i => i.OpCode == OpCodes.Call))
                    if ((instr.Operand as MethodReference).Resolve() == param as MethodDefinition)
                    {
                        yield return new HurpFuscatorEntry
                                         {
                                             Arg = instr.Previous.Operand as string,
                                             Source = mDef,
                                             BadInstructions = new List<Instruction>
                                                                   {
                                                                       instr.Previous,
                                                                       instr
                                                                   }
                                         } as T;
                    }
            }
        }
    }
}
