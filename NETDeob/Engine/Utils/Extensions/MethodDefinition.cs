using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NETDeob.Core.Engine.Utils.Extensions
{
    public static class MethodDefinitionExt
    {
        public static Instruction HasDelegateReference(this MethodDefinition mDef, FieldDefinition Delegate)
        {
            return mDef.Body.Instructions.Where(instr => instr.OpCode == OpCodes.Newobj).FirstOrDefault(instr => instr.Operand == Delegate);
        }

        public static IEnumerable<Tuple<Instruction, MethodDefinition>> FindAllReferences(this MethodDefinition mDef, ModuleDefinition modDef)
        {
            foreach(var _mDef in modDef.Assembly.FindMethods(m => m.HasBody))
            {
                foreach(var instr in _mDef.Body.Instructions)
                {
                    if (instr.OpCode.OperandType != OperandType.InlineMethod) continue;
                    if ((instr.Operand as MethodReference).Resolve() != mDef)
                        continue;

                    yield return new Tuple<Instruction, MethodDefinition>(instr, _mDef);
                }
            }
        }
    }
}
