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
            TaskQueue.Add(new MethodCleaner(AsmDef));
            TaskQueue.Add(new MetadataFixer(AsmDef));

            if (Globals.DeobContext.ActiveSignature.Ver.Major == 1 && Globals.DeobContext.ActiveSignature.Ver.Minor == 0)
                if (Globals.DeobContext.DynStringCtx == null)
                    TaskQueue.Add(new Tasks._1_0.StringDecryptor(AsmDef));
                else
                    TaskQueue.Add(new GenericStringDecryptor(AsmDef));
            else
                if (Globals.DeobContext.DynStringCtx == null)
                    TaskQueue.Add(new Tasks._1_1.StringDecryptor(AsmDef));
                else
                    TaskQueue.Add(new GenericStringDecryptor(AsmDef));

            TaskQueue.Add(new Renamer(AsmDef, new RenamingScheme(true)));
            Deobfuscate();
        }
    }
}
