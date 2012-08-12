using Mono.Cecil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using de4dot.blocks.cflow;

namespace NETDeob.Core.Deobfuscators.Generic
{
    class MethodCleaner : AssemblyDeobfuscationTask
    {
        public MethodCleaner(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        [DeobfuscationPhase(1, "Clean Control Flow (de4dot)")]
        public static bool Phase1()
        {
            var cflowCleaner = new CflowDeobfuscator();
            var mCounter = 0;

            foreach (var mDef in AsmDef.FindMethods(m => true))
            {
                cflowCleaner.deobfuscate(mDef);
                mCounter++;

                Logger.VLog("Cleaned method: " + mDef.Name);
            }

            Logger.VSLog(string.Format("{0} methods cleaned...", mCounter));
            return true;
        }
    }
}
