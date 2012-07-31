using System.Linq;
using Mono.Cecil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;

namespace NETDeob.Deobfuscators.Confuser.Tasks.Common
{
    public class WatermarkRemover : AssemblyDeobfuscationTask
    {
        public WatermarkRemover(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        [DeobfuscationPhase(1, "Remove watermark")]
        public static bool Phase1()
        {
            MarkMember(AsmDef.MainModule.CustomAttributes.FirstOrDefault(attrib => attrib.AttributeType.Name == "ConfusedByAttribute"), AsmDef.MainModule);
            MarkMember(AsmDef.MainModule.Types.FirstOrDefault(t => t.Name == "ConfusedByAttribute"));

            Logger.VSLog("Marked ConfusedByAttribute attribute for removal...");
            return true;
        }
    }
}
