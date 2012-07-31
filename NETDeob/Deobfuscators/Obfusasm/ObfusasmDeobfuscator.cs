using Mono.Cecil;
using NETDeob.Core.Deobfuscators.Generic;
using NETDeob.Core.Deobfuscators.Obfusasm.Tasks;
using NETDeob.Deobfuscators.Generic;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Core.Deobfuscators.Obfusasm
{
    class ObfusasmDeobfuscator : AssemblyWorker
    {
        public ObfusasmDeobfuscator(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override void CreateTaskQueue()
        {
            TaskQueue.Add(new MetadataFixer(AsmDef));
            TaskQueue.Add(new MethodCleaner2(AsmDef));
            TaskQueue.Add(new Renamer(AsmDef, new RenamingScheme(true) { Resources = false }));
            TaskQueue.Add(new StringDecryptor(AsmDef));

            Deobfuscate();
        }
    }
}
