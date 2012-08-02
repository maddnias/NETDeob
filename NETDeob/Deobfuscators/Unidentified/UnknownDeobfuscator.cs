using Mono.Cecil;
using NETDeob.Core.Deobfuscators.Generic;
using NETDeob.Core.Misc;
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

            if (DeobfuscatorContext.Debug && DeobfuscatorContext.DynamicResourceResolving)
                TaskQueue.Add(new GenericResourceDumper(AsmDef));

            TaskQueue.Add(new MetadataFixer(AsmDef));

            if (DeobfuscatorContext.Debug && DeobfuscatorContext.DynStringCtx != null)
                TaskQueue.Add(new GenericStringDecryptor(AsmDef));

            TaskQueue.Add(new Renamer(AsmDef, new RenamingScheme(true) { Resources = false }));

            Deobfuscate();
        }
    }
}
