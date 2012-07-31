using Mono.Cecil;
using NETDeob.Core.Deobfuscators.Generic;
using NETDeob.Deobfuscators.Generic;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Deobfuscators.Phoenix_Protector
{
    class PhoenixProtectorDeobfuscator : AssemblyWorker
    {
        public PhoenixProtectorDeobfuscator(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override void CreateTaskQueue()
        {
            TaskQueue.Add(new MethodCleaner2(AsmDef));
            TaskQueue.Add(new PhoenixStringWorker(AsmDef));
            TaskQueue.Add(new Renamer(AsmDef, new RenamingScheme(true)));

            Deobfuscate();
        }

        
    }
}
