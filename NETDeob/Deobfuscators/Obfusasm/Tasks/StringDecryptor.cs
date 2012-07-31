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
    class StringDecryptor : AssemblyDeobfuscationTask, IStringDecryptor
    {
        private class ObfusasmEntry : DecryptionContext
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
            var decEntries = ConstructEntries<ObfusasmEntry>(targetType);

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
                var entry = t as DecryptionContext;
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

            return true;
        }
        private static bool IsStringProxy(MethodDefinition mDef)
        {
            if (mDef.Body.HasVariables || mDef.HasParameters)
                return false;

            if (mDef.ReturnType.Name != "String")
                return false;

            return true;
        }

        public void InitializeDecryption(object param)
        {
        }
        public void DecryptEntry(ref DecryptionContext entry)
        {
            var _entry = entry as ObfusasmEntry;

            for (int i = _entry.Key; i < (_entry.Key + _entry.Key1); i++)
                _staticKey[i] ^= (byte)_entry.Key2;

            entry.PlainText = Encoding.UTF8.GetString(_staticKey, _entry.Key, _entry.Key1);
        }
        public void ProcessEntry(DecryptionContext entry)
        {
            var _entry = entry as ObfusasmEntry;

            foreach (var target in _entry.InsertTargets)
            {
                var ilProc = target.Item2.Body.GetILProcessor();
                ilProc.InsertBefore(target.Item1,
                                   ilProc.Create(OpCodes.Ldstr, entry.PlainText));
            }
        }

        public IEnumerable<T> ConstructEntries<T>(object param) where T : DecryptionContext
        {
            foreach (var mDef in AsmDef.FindMethods(m => true))
            {
                if (!mDef.HasBody)
                    continue;

                for (var i = 0; i < mDef.Body.Instructions.Count; i++)
                {
                    var instr = mDef.Body.Instructions[i];
                    MethodDefinition tmpTarget = null;

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

                        yield return decEntry as T;
                    }
                }
            }
        }
    }
}
