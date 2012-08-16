using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Misc.Structs__Enums___Interfaces.Tasks.Base;

namespace NETDeob.Core.Deobfuscators.Obfusasm.Tasks
{
    class ObfusasmEntry : DecryptionContext
    {
        public int Key;
        public int Key1;
        public int Key2;

        public readonly List<Tuple<Instruction, MethodDefinition>> InsertTargets =
            new List<Tuple<Instruction, MethodDefinition>>();

        public override string ToString()
        {
            return string.Format(@"[Decrypt]({4}) Key({0}, {1}, {2}) -> ""{3}""", Key, Key1, Key2, PlainText, InsertTargets.Count);
        }
    }

    class StringDecryptor : AssemblyDeobfuscationTask, IStringDecryptor<ObfusasmEntry>
    {
        public StringDecryptor(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "String decryption";
        }

        private static byte[] _staticKey;

        [DeobfuscationPhase(1, "Locate bad type")]
        public static bool Phase1()
        {
            MethodDefinition cctor = null;
            var targetType = AsmDef.MainModule.Types.FirstOrDefault(t => IsBadType(t, out cctor));

            if(targetType == null || cctor == null)
            {
                PhaseError = new PhaseError
                                 {
                                     Level = PhaseError.ErrorLevel.Minor,
                                     Message = "No string encryption?"
                                 };
                EmergencyCancel = true;
                return true;
            }

            MarkMember(targetType);
            Logger.VSLog("Located bad type at " + targetType.Name);

            _staticKey = (cctor.Body.Instructions.FirstOrDefault(op => op.OpCode == OpCodes.Ldtoken).Operand as FieldReference).Resolve().InitialValue;

            if (_staticKey == null || _staticKey.Length == 0){
                ThrowPhaseError("Could not find static key!", 0, true);
                return true;
            }

            PhaseParam = targetType; 

            return true;
        }

        /// <summary>
        /// PhaseParam = targetType
        /// </summary>
        [DeobfuscationPhase(2, "Construct decryption entries")]
        public bool Phase2()
        {
            var targetType = PhaseParam;
            var decEntries = ConstructEntries(targetType);

            PhaseParam = decEntries;

            return true;
        }

        /// <summary>
        /// PhaseParam = decEntries
        /// </summary>
        [DeobfuscationPhase(3, "Decrypt strings")]
        public bool Phase3()
        {
            var decEntries = new List<ObfusasmEntry>();

            foreach (var t in PhaseParam)
                decEntries.Add(t);

            foreach (var t in decEntries)
            {
                var entry = t;
                DecryptEntry(ref entry);

                Logger.VLog(entry.ToString());
            }

            PhaseParam = decEntries;

            Logger.VSLog(string.Format("{0} strings decrypted...", decEntries.Count));
            return true;
        }

        /// <summary>
        /// PhaseParam = decEntries
        /// </summary>
        [DeobfuscationPhase(4, "Replace strings")]
        public bool Phase4()
        {
            var decEntries = PhaseParam;

            foreach (var entry in decEntries)
                ProcessEntry(entry);

            Logger.VSLog(string.Format("{0} strings replaced in assembly with plaintext...", decEntries.Count));

            return true;
        }
   
        private static bool IsBadType(TypeDefinition typeDef, out MethodDefinition cctor)
        {
            if ((cctor = typeDef.GetStaticConstructor()) == null)
                return false;

            if (!cctor.HasBody)
                return false;

            if (cctor.Body.HasVariables || cctor.Body.Instructions.FirstOrDefault(i => i.OpCode == OpCodes.Ldtoken) == null)
                return false;

            if (cctor.Body.Instructions.GetOpCodeCount(OpCodes.Ldtoken) != 1)
                return false;

            if (cctor.Body.Instructions.GetOpCodeCount(OpCodes.Newarr) != 2)
                return false;

            if (!typeDef.IsSealed || typeDef.Fields.Count != 3)
                return false;

            return true;
        }
        private static bool IsStringProxy(MethodDefinition mDef)
        {
            if (mDef.Body.HasVariables || mDef.HasParameters || !mDef.HasBody)
                return false;

            if (mDef.ReturnType.Name != "String")
                return false;

            var body = mDef.Body;

            if (body.Instructions.GetOpCodeCount(OpCodes.Call) != 1)
                return false;

            if (body.Instructions.GetOpCodeCount(OpCodes.Ldsfld) != 1)
                return false;

            return true;
        }

        public bool BaseIsDecryptor(params object[] param)
        {
            throw new NotImplementedException();
        }

        public void InitializeDecryption(object param)
        {
        }

        public void DecryptEntry(ref ObfusasmEntry entry)
        {
            byte[] decryptedString = new byte[entry.Key1];
            for (int i = 0; i < entry.Key1; i++)
            {
                decryptedString[i] = (byte)(_staticKey[entry.Key + i] ^ entry.Key2);
            }

            entry.PlainText = Encoding.UTF8.GetString(decryptedString);
        }

        public void ProcessEntry(ObfusasmEntry entry)
        {
            foreach (var target in entry.InsertTargets)
            {
                var ilProc = target.Item2.Body.GetILProcessor();
                ilProc.InsertBefore(target.Item1,
                                   ilProc.Create(OpCodes.Ldstr, entry.PlainText));
            }
        }

        public IEnumerable<ObfusasmEntry> ConstructEntries(object param)
        {
            foreach (var mDef in AsmDef.FindMethods(m => m.HasBody))
            {
                for (var i = 0; i < mDef.Body.Instructions.Count; i++)
                {
                    var instr = mDef.Body.Instructions[i];
                    MethodDefinition tmpTarget;

                    if (instr.OpCode == OpCodes.Call &&
                        (tmpTarget = (instr.Operand as MethodReference).Resolve()).DeclaringType == param as TypeDefinition)
                    {
                        if (!IsStringProxy(tmpTarget))
                            continue;

                        var decEntry = new ObfusasmEntry
                        {
                            Key =
                                tmpTarget.Body.Instructions.FindInstruction(
                                    iinstr => iinstr.IsLdcI4(), 2).GetLdcI4(),
                            Key1 =
                                tmpTarget.Body.Instructions.FindInstruction(
                                    iinstr => iinstr.IsLdcI4(), 3).GetLdcI4(),
                            Key2 =
                                tmpTarget.Body.Instructions.FindInstruction(
                                    iinstr => iinstr.IsLdcI4(), 4).GetLdcI4(),
                           // TargetProxy = tmpTarget
                        };

                        decEntry.InsertTargets.Add(new Tuple<Instruction, MethodDefinition>(instr.Next, mDef));
                        MarkMember(instr, mDef);

                        yield return decEntry;
                    }
                }
            }
        }
    }
}
