using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETDeob.Deobfuscators.Netz.Tasks;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Deobfuscators.Netz
{
    public class NetzUnpacker : IDeobfuscator
    {
        public NetzUnpacker(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override void CreateTaskQueue()
        {
            base.IsSave = false;
            base.TaskQueue.Add(new Unpacker(AsmDef));

            Deobfuscate();
        }
    }
}
