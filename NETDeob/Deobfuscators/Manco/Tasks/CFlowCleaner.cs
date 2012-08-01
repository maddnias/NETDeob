using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Deobfuscators;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;

namespace NETDeob.Core.Deobfuscators.Manco.Tasks
{
    class CFlowCleaner : AssemblyDeobfuscationTask
    {
        private bool? _value;

        public CFlowCleaner(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "Remove extra control flow obfuscation";
        }

        [DeobfuscationPhase(1, "Locate bool initializer")]
        public bool Phase1()
        {
            var targetType = AsmDef.MainModule.GetAllTypes().FirstOrDefault(t => IsBoolInitializer(t, out _value));

            if(targetType == null || _value == null){
                ThrowPhaseError("No extra control flow obfuscation?", 0, true);
                return false;
            }

            MarkMember(targetType);

            PhaseParam = targetType;
            return true;
        }

        [DeobfuscationPhase(2, "Remove obfuscation")]
        public bool Phase2()
        {
            var targetType = PhaseParam as TypeDefinition;

            foreach (var mDef in AsmDef.FindMethods(m => m.HasBody))
            {
                if (mDef.Body.Instructions.GetOpCodeCount(OpCodes.Ldsfld) <= 0)
                    continue;

                var badBranches = YieldBadBranches(targetType).ToList();

                for (var i = 0; i < badBranches.Count; i++)
                    RemoveBranch(badBranches[i].Item1, badBranches[i].Item2);
            }

            return true;
        }

        public IEnumerable<Tuple<MethodDefinition, Instruction>> YieldBadBranches(TypeDefinition targetType)
        {
            foreach (var mDef in AsmDef.FindMethods(m => m.HasBody))
                foreach (var fLoad in mDef.Body.Instructions.Where(i => i.OpCode == OpCodes.Ldsfld && (i.Operand as FieldReference).Resolve().DeclaringType == targetType))
                    yield return new Tuple<MethodDefinition, Instruction>(mDef, fLoad);
        }
        public void RemoveBranch(MethodDefinition mDef, Instruction instr)
        {
            if (!instr.Next.IsConditionalBranch())
                return;

            mDef.Body.GetILProcessor().RemoveBlock(instr, instr.Next.Operand as Instruction); // load bool (ldsfld) -> branch target
        }
        public bool IsBoolInitializer(TypeDefinition tDef, out bool? value)
        {
            var cctor = tDef.GetStaticConstructor();
            value = null;

            if (cctor == null || !cctor.HasBody)
                return false;

            if (cctor.Body.Instructions.Count >= 4)
                return false;

            if (cctor.Body.Instructions[1].OpCode != OpCodes.Stsfld)
                return false;

            value = (cctor.Body.Instructions.First(i => i.IsLdcI4()).OpCode != OpCodes.Ldc_I4_0);
            return true;
        }
    }
}
