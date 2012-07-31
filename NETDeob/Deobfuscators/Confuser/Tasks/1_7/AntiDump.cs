using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Deobfuscators;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Misc.Structs__Enums___Interfaces.Tasks.Base;

namespace NETDeob.Deobfuscators.Confuser.Tasks._1_7
{
    public class AntiDump : AssemblyDeobfuscationTask
    {
        public AntiDump(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        [DeobfuscationPhase(1, "Locate anti-dump method and mark for removal")]
        public static bool Phase1()
        {
            var ctor =
                AsmDef.MainModule.Types.First(t => t.Name == "<Module>").Methods.FirstOrDefault(
                    m => m.IsConstructor && m.Name == ".cctor" && m.Body.Instructions.Count >= 1);

            if(ctor == null){
                ThrowPhaseError("No anti-dump?", 0, true);
                return true;
            }

            var badInstr =
                ctor.Body.Instructions.FirstOrDefault(i => i.OpCode == OpCodes.Call && IsBadMethod((i.Operand as MethodReference).Resolve()));

            if (badInstr == null){
                ThrowPhaseError("No anti-dump?", 0, true);
                return true;
            }

            MarkMember(badInstr, ctor);
            MarkMember((badInstr.Operand as MethodReference).Resolve().DeclaringType);

            return true;
        }

        public static bool IsBadMethod(MethodDefinition mDef)
        {
            if (!mDef.HasBody)
                return false;

            if (mDef.Body.Variables.Count != 44)
                return false;

            if (mDef.Body.Instructions[0].OpCode != OpCodes.Ldtoken && (mDef.Body.Instructions[0].Operand as TypeReference).Resolve() == mDef.DeclaringType)
                return false;

            return true;
        }
    }
}
