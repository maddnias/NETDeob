using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;

namespace NETDeob.Deobfuscators.Confuser.Tasks._1_7
{
    public class StackUnderflowCleaner : AssemblyDeobfuscationTask
    {
        public StackUnderflowCleaner(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "Fix stackunderflow code";
        }

        [DeobfuscationPhase(1, "Mark bad instructions")]
        public static bool Phase1()
        {

            foreach (var mDef in AsmDef.FindMethods(m => true))
            {
                if (!mDef.HasBody)
                    continue;

                if (mDef.Body.Instructions[0].OpCode != OpCodes.Br_S)
                    continue;

                if ((mDef.Body.Instructions[0].Operand as Instruction) == mDef.Body.Instructions[3])
                    for (var i = 0; i < 3; i++)
                        MarkMember(mDef.Body.Instructions[i], mDef);
            }

            Logger.VSLog(string.Format("{0} bad instructions marked for removal...", MarkedMemberCount));
            return true;
        }
    }
}
