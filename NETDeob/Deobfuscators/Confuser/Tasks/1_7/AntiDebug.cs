using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Deobfuscators;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Misc.Structs__Enums___Interfaces.Tasks.Base;

namespace NETDeob.Deobfuscators.Confuser.Tasks._1_7
{
    public class AntiDebug : AssemblyDeobfuscationTask
    {
        public AntiDebug(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "Remove anti-debug type";
        }

        [DeobfuscationPhase(1, "Locate bad type & mark for removal")]
        public static bool Phase1()
        {
            var ctorBody =
                AsmDef.MainModule.Types.First(t => t.Name == "<Module>").Methods.FirstOrDefault(
                    m => m.IsConstructor && m.Body.Instructions.Count >= 1);

            if(ctorBody == null){
                ThrowPhaseError("No anti-debug?", 0, true);
                return true;
            }

            var badInstr =
                ctorBody.Body.Instructions.FirstOrDefault(
                    i => i.OpCode == OpCodes.Call && IsBadType((i.Operand as MethodReference).Resolve().DeclaringType));

            if(badInstr == null){
                ThrowPhaseError("No anti-debug?", 0, true);
                return true;
            }

            MarkMember(badInstr, ctorBody);
            MarkMember((badInstr.Operand as MethodReference).Resolve().DeclaringType);

            if (ctorBody.Body.Instructions.Count == 2)
                MarkMember(ctorBody);

            return true;
        }

        private static bool IsBadType(TypeDefinition typeDef)
        {
            MethodDefinition target;

            if ((target = typeDef.Methods.FirstOrDefault(m => !m.HasBody)) == null)
                return false;

            if (target.PInvokeInfo == null)
                return false;

            if(target.PInvokeInfo.EntryPoint != "NtQueryInformationProcess")
                return false;

            return true;
        }
    }
}
