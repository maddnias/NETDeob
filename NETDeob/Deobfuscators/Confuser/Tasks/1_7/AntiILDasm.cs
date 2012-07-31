using System.Linq;
using Mono.Cecil;
using NETDeob.Deobfuscators;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Misc.Structs__Enums___Interfaces.Tasks.Base;

namespace NETDeob.Deobfuscators.Confuser.Tasks._1_7
{
    class AntiILDasm : AssemblyDeobfuscationTask
    {
        public AntiILDasm(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "Remove SuppressILDasm attribute";
        }

        [DeobfuscationPhase(1, "Remove bad attribute")]
        public static bool Phase1()
        {
            var attrib =
                AsmDef.MainModule.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "SuppressIldasmAttribute");

            if(attrib == null){
                ThrowPhaseError("AntiILDasm not activated?", 0, true);
                return true;
            }

            MarkMember(attrib, AsmDef.MainModule);

            return true;
        }
    }
}
