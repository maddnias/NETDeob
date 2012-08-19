using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NETDeob.Core.Engine.Utils.Extensions
{
    public static class TypeDefinitionExt
    {
        public static IEnumerable<MethodDefinition> GetConstructors(this TypeDefinition typeDef)
        {
            return typeDef.Methods.Where(m => m.IsConstructor);
        }

        public static MethodDefinition GetStaticConstructor(this TypeDefinition typeDef)
        {
            return typeDef.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);
        }

        public static IEnumerable<Tuple<Instruction, MethodDefinition>> GetAllReferences(this TypeDefinition typeDef, ModuleDefinition modDef)
        {
            foreach (var mDef in modDef.Assembly.FindMethods(m => m.HasBody))
                foreach (var instr in mDef.Body.Instructions.Where(instr => instr.OpCode.OperandType == OperandType.InlineField ||
                                                                            instr.OpCode.OperandType == OperandType.InlineType ||
                                                                            instr.OpCode.OperandType == OperandType.InlineTok ||
                                                                            instr.OpCode.OperandType == OperandType.InlineMethod))
                {
                    switch (instr.OpCode.Code)
                    {
                        case Code.Ldsfld:
                        case Code.Ldsflda:
                            if ((instr.Operand as FieldReference).Resolve().DeclaringType == typeDef)
                                if ((TopParentType<MethodDefinition>(mDef) != typeDef))
                                    yield return new Tuple<Instruction, MethodDefinition>(instr, mDef);

                            break;

                        case Code.Stobj:
                        case Code.Ldobj:
                        case Code.Castclass:
                        case Code.Isinst:
                        case Code.Unbox:
                        case Code.Box:
                        case Code.Newarr:
                        case Code.Ldelem_I1:
                        case Code.Ldelem_Any:
                        case Code.Stelem_Any:
                        case Code.Unbox_Any:
                        case Code.Refanyval:
                        case Code.Mkrefany:
                        case Code.Initobj:
                        case Code.Constrained:
                        case Code.Sizeof:
                            if ((instr.Operand as TypeReference).Resolve() == typeDef)
                                if ((TopParentType(mDef) != typeDef))
                                    yield return new Tuple<Instruction, MethodDefinition>(instr, mDef);

                            break;

                        case Code.Ldtoken:
                            if ((instr.Operand is TypeReference))
                            {
                                if ((instr.Operand as TypeReference).Resolve() == typeDef)
                                    if ((TopParentType(mDef) != typeDef))
                                        yield return new Tuple<Instruction, MethodDefinition>(instr, mDef);
                            }
                            else if ((instr.Operand is FieldReference) || (instr.Operand is MethodReference))
                                if ((instr.Operand as dynamic).Resolve().DeclaringType == typeDef)
                                    if ((TopParentType(mDef) != typeDef))
                                        yield return new Tuple<Instruction, MethodDefinition>(instr, mDef);

                            break;

                        case Code.Call:
                        case Code.Callvirt:
                        case Code.Jmp:
                        case Code.Newobj:
                        case Code.Ldftn:
                        case Code.Ldvirtftn:
                            if (((instr.Operand as MethodReference).Resolve()).DeclaringType == typeDef)
                                if ((TopParentType(mDef) != typeDef))
                                    yield return new Tuple<Instruction, MethodDefinition>(instr, mDef);

                            break;

                    }
                }
        }

        public static TypeDefinition TopParentType<T>(this T member) where T : IMemberDefinition
        {
            TypeDefinition topParent;

            if ((topParent = member.DeclaringType) == null)
                return member as TypeDefinition;

            while (topParent.DeclaringType != null)
                topParent = (topParent).DeclaringType;

            return topParent;
        }
    }
}
