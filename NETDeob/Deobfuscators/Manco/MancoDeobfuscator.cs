using Mono.Cecil;
using NETDeob.Core.Deobfuscators.Generic;
using NETDeob.Core.Deobfuscators.Manco.Tasks;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Core.Deobfuscators.Manco
{
    class MancoDeobfuscator : AssemblyWorker
    {
        public MancoDeobfuscator(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override void CreateTaskQueue()
        {
            TaskQueue.Add(new MethodCleaner(AsmDef));
            TaskQueue.Add(new CFlowCleaner(AsmDef));

            if (Globals.DeobContext.DynStringCtx == null)
                TaskQueue.Add(new StringDecryptor(AsmDef));
            else
                TaskQueue.Add(new GenericStringDecryptor(AsmDef));

            TaskQueue.Add(new Renamer(AsmDef, new RenamingScheme(true)));

            Deobfuscate();
        }
    }
}
