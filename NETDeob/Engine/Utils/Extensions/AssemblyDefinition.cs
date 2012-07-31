using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NETDeob.Core.Engine.Utils.Extensions
{
    public static class AssemblyDefinitionExt
    {
        public static EmbeddedResource FindResource(this AssemblyDefinition asmDef, Predicate<Resource> pred)
        {
            return (from modDef in asmDef.Modules from res in modDef.Resources where pred(res) select res as EmbeddedResource).FirstOrDefault();
        }

        public static MethodDefinition FindMethod(this AssemblyDefinition asmDef, Predicate<MethodDefinition> pred)
        {
            return (from modDef in asmDef.Modules from typeDef in modDef.Types from mDef in typeDef.Methods where mDef.HasBody select mDef).FirstOrDefault(mDef => pred(mDef));
        }

        public static IEnumerable<MethodDefinition> FindMethods(this AssemblyDefinition asmDef, Predicate<MethodDefinition> pred)
        {
            foreach (var tDef in asmDef.MainModule.Types)
            {
                foreach (var mDef in from nt in tDef.NestedTypes from mDef in nt.Methods where pred(mDef) select mDef)
                    yield return mDef;

                foreach (var mDef in tDef.Methods.Where(mDef => pred(mDef)))
                    yield return mDef;
            }
        }

        public static void ReplaceString(this AssemblyDefinition asmDef, string oldStr, string newStr)
        {
            foreach (var mDef in asmDef.FindMethods(m => m.HasBody))
                foreach (var instr in mDef.Body.Instructions.Where(i => i.OpCode == OpCodes.Ldstr && i.Operand as string == oldStr))
                    instr.Operand = newStr;
        }

        public static IEnumerable<TypeDefinition> FindTypes(this AssemblyDefinition asmDef, Predicate<TypeDefinition> pred)
        {
            return asmDef.MainModule.Types.Where(t => pred(t));
        }
    }
}
