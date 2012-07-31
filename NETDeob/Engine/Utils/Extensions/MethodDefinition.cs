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
    }
}
