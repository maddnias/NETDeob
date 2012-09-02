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
            TaskQueue.Add(new MethodCleaner(AsmDef));
            //TaskQueue.Add(new Renamer(AsmDef, new RenamingScheme(true) {Resources = false}));
            TaskQueue.Add(new Renamer(AsmDef, new RenamingScheme(false) { Methods = true, Fields = true, Properties = true, Parameters = true, Events = true, Delegates = true}));

            if (Globals.DeobContext.DynStringCtx == null)
                TaskQueue.Add(new StringDecryptor(AsmDef));
            else
                TaskQueue.Add(new GenericStringDecryptor(AsmDef));

            Deobfuscate();
        }
    }
}
