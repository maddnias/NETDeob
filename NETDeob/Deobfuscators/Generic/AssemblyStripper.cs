using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;

namespace NETDeob.Core.Deobfuscators.Generic
{
    public class AssemblyStripper : AssemblyDeobfuscationTask
    {
        public AssemblyStripper(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "Remove bad members";
        }

        [DeobfuscationPhase(1, "Strip assembly from bad members")]
        public static bool Phase1()
        {
            return RemoveMembers();
        }

        private static bool RemoveMembers()
        {
            var tmp = new MarkedMember();

            try
            {
                foreach (var member in Globals.DeobContext.MarkedMembers)
                {
                    switch (member.Type)
                    {
                        case MemberType.Type:
                            Logger.VLog("[Remove(Type)] " + (member.Member as TypeDefinition).Name.Truncate(10));
                            AsmDef.MainModule.Types.Remove(member.Member as TypeDefinition);
                            break;

                        case MemberType.Method:
                            Logger.VLog("[Remove(Method)] " + (member.Member as MethodDefinition).Name);
                            (member.Member as MethodDefinition).DeclaringType.Methods.Remove(
                                member.Member as MethodDefinition);
                            break;

                        case MemberType.Field:
                            Logger.VLog("[Remove(Field)] " + (member.Member as FieldDefinition).Name);
                            (member.Member as FieldDefinition).DeclaringType.Fields.Remove(
                                member.Member as FieldDefinition);
                            break;

                        case MemberType.Property:
                            Logger.VLog("[Remove(Property)] " + (member.Member as PropertyDefinition).Name);
                            (member.Member as PropertyDefinition).DeclaringType.Properties.Remove(
                                member.Member as PropertyDefinition);
                            break;

                        case MemberType.Resource:
                            Logger.VLog("[Remove(Resource)] " +
                                        ((member.Member as EmbeddedResource).Name.Truncate(10)));
                            AsmDef.MainModule.Resources.Remove(member.Member as EmbeddedResource);
                            break;

                        case MemberType.Attribute:
                            Logger.VLog("[Remove(Attribute)] " + (member.Member as CustomAttribute).AttributeType.Name);
                            (member.ParentMember as ModuleDefinition).CustomAttributes.Remove(
                                member.Member as CustomAttribute);
                            break;

                        case MemberType.Instruction:
                            Logger.VLog("[Remove(Instruction)] " + (member.Member as Instruction).OpCode);
                            (member.ParentMember as MethodDefinition).Body.Instructions.Remove(
                                member.Member as Instruction);
                            break;

                        case MemberType.AssemblyReference:
                            Logger.VLog("[Remove(AssemblyReference)] " + (member.Member as AssemblyNameReference).MetadataToken.ToInt32());
                            AsmDef.MainModule.AssemblyReferences.Remove(member.Member as AssemblyNameReference);
                            break;
                    }

                    tmp = member;
                }
            }
            catch (Exception e)
            {
                if (tmp == null)
                    return true;

                ThrowPhaseError("Failed to clean up member!", 0, false);
            }

            var totalNum = Globals.DeobContext.MarkedMembers.Sum(member => member.Member.CalcChildMembers());

            Logger.VSLog(string.Format("{0} members cleaned from assembly...", totalNum));
            return true;
        }
    }
}
