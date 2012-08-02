using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETDeob.Core.Deobfuscators.Generic;
using NETDeob.Core.Deobfuscators.HurpFuscator.Tasks;
using NETDeob.Core.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Core.Deobfuscators.HurpFuscator
{
    class HurpDeobfuscator : AssemblyWorker
    {
        public HurpDeobfuscator(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override void CreateTaskQueue()
        {
            TaskQueue.Add(new MethodCleaner2(AsmDef));
            TaskQueue.Add(new MetadataFixer(AsmDef));

            if(DeobfuscatorContext.ActiveSignature.Ver.Major == 1 && DeobfuscatorContext.ActiveSignature.Ver.Minor == 0)
                TaskQueue.Add(new Tasks._1_0.StringDecryptor(AsmDef));
            else
                TaskQueue.Add(new Tasks._1_1.StringDecryptor(AsmDef));

            TaskQueue.Add(new Renamer(AsmDef, new RenamingScheme(true)));
            Deobfuscate();
        }
    }
}
