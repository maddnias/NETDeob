using Mono.Cecil;
using NETDeob.Core.Deobfuscators.Generic;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Core.Deobfuscators.Unidentified
{
    class UnknownDeobfuscator : AssemblyWorker
    {
        public UnknownDeobfuscator(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override void CreateTaskQueue()
        {
            TaskQueue.Add(new MethodCleaner2(AsmDef));
            TaskQueue.Add(new MetadataFixer(AsmDef));
            TaskQueue.Add(new Renamer(AsmDef, new RenamingScheme(true) { Resources = false }));

            Deobfuscate();
        }
    }
}
