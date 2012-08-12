using System.Linq;
using Mono.Cecil;
using NETDeob.Core.Deobfuscators.Generic;
using NETDeob.Core.Deobfuscators.Rummage.Tasks;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Core.Deobfuscators.Rummage
{
    class RummageDeobfuscator : AssemblyWorker
    {
        //TODO: Find out why this leave some unreferenced types in the assembly...

        public RummageDeobfuscator(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override void CreateTaskQueue()
        {
            TaskQueue.Add(new MethodCleaner(AsmDef));
            TaskQueue.Add(new RummageStringDecryptor(AsmDef));
            TaskQueue.Add(new Renamer(AsmDef, new RenamingScheme(true)));

            Deobfuscate();
        }
    }
}
