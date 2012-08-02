using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;

namespace NETDeob.Core.Deobfuscators.HurpFuscator.Tasks._1_0
{
    internal class StringDecryptor : AssemblyDeobfuscationTask, IStringDecryptor
    {
        public class HurpFuscatorEntry : DecryptionContext
        {
            public MethodDefinition Source;
            public List<Instruction> BadInstructions;
            public string Arg1, Arg2;

            public override string ToString()
            {
                return string.Format(@"[Decrypt] ""({0}, {1})"" -> ""{2}""", Arg1, Arg2, PlainText);
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
            var entryList =
                ConstructEntries<HurpFuscatorEntry>(PhaseParam as MethodDefinition).ToList();

            Logger.VSLog(string.Format("Constructed {0} entries...", entryList.Count));

            PhaseParam = entryList;
            return true;
        }

        [DeobfuscationPhase(3, "Decrypt entries")]
        public bool Phase3()
        {
            for (var i = 0; i < (PhaseParam as List<HurpFuscatorEntry>).Count; i++)
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
            return (mDef.Parameters.Count == 2 &&
                    mDef.Body.Variables.Count == 9 &&
                    mDef.ReturnType.ToString().ToLower().Contains("string") &&
                    mDef.Body.Instructions.GetOpCodeCount(OpCodes.Callvirt) == 7);
        }

        public void InitializeDecryption(object param)
        {
            throw new NotImplementedException();
        }

        public void DecryptEntry(ref DecryptionContext entry)
        {
            var destinationArray = new byte[8];
            var provider2 = new MD5CryptoServiceProvider();

            Array.Copy(provider2.ComputeHash(Encoding.ASCII.GetBytes((entry as HurpFuscatorEntry).Arg2)), 0,
                       destinationArray, 0, 8);


            var provider = new DESCryptoServiceProvider
                               {
                                   Key = destinationArray,
                                   Mode = CipherMode.ECB
                               };

            var inputBuffer = Convert.FromBase64String((entry as HurpFuscatorEntry).Arg1);

            (entry as HurpFuscatorEntry).PlainText =
                Encoding.ASCII.GetString(provider.CreateDecryptor().TransformFinalBlock(inputBuffer, 0, inputBuffer.Length));
        }

        public void ProcessEntry(DecryptionContext entry)
        {
            var _entry = entry as HurpFuscatorEntry;

            var ilProc = _entry.Source.Body.GetILProcessor();

            ilProc.Replace(_entry.BadInstructions[0], ilProc.Create(OpCodes.Ldstr, _entry.PlainText));
            MarkMember(_entry.BadInstructions[1], _entry.Source);
            MarkMember(_entry.BadInstructions[2], _entry.Source);
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
                                             Arg1 = instr.Previous.Previous.Operand as string,
                                             Arg2 = instr.Previous.Operand as string,
                                             Source = mDef,
                                             BadInstructions = new List<Instruction>
                                                                   {
                                                                       instr.Previous.Previous,
                                                                       instr.Previous,
                                                                       instr
                                                                   }
                                         } as T;
                    }
            }
        }
    }
}
