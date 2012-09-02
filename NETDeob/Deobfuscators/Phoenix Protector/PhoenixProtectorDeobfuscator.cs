using Mono.Cecil;
using NETDeob.Core;
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
            TaskQueue.Add(new MethodCleaner(AsmDef));

            if (Globals.DeobContext.DynStringCtx == null)
                TaskQueue.Add(new StringDecryptor(AsmDef));
            else
                TaskQueue.Add(new GenericStringDecryptor(AsmDef));

            TaskQueue.Add(new Renamer(AsmDef, new RenamingScheme(true)));

            Deobfuscate();
        }

        
    }
}
