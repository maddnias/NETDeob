using Mono.Cecil;
using NETDeob.Core.Deobfuscators.Generic;
using NETDeob.Core.Deobfuscators.Rummage.Tasks;
using NETDeob.Deobfuscators.Generic;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Deobfuscators.Rummage
{
    class RummageDeobfuscator : AssemblyWorker
    {
        public RummageDeobfuscator(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override void CreateTaskQueue()
        {
            TaskQueue.Add(new MethodCleaner2(AsmDef));
            TaskQueue.Add(new RummageStringDecryptor(AsmDef));
            TaskQueue.Add(new Renamer(AsmDef, new RenamingScheme(true)));
            
            Deobfuscate();
        }
    }
}
