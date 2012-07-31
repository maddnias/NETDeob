using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Misc;

namespace NETDeob.Core.Misc
{
    public static class AssemblyUtils
    {
        public static IEnumerable<Tuple<Instruction, MethodDefinition>> FindMethodReferences(MethodDefinition target)
        {
            foreach(var mDef in DeobfuscatorContext.AsmDef.FindMethods(m => true))
            {
                if (!mDef.HasBody)
                    continue;

                foreach (var instr in mDef.Body.Instructions.Where(i => i.IsCall()).Where(instr => (instr.Operand as MethodReference).Resolve() == target))
                    yield return new Tuple<Instruction, MethodDefinition>(instr, mDef);
            }
        }
    }
}
