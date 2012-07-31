using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Deobfuscators.NetShrink
{
    class NetShrinkUnpacker : IDeobfuscator
    {
        public NetShrinkUnpacker(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override void CreateTaskQueue()
        {
            throw new NotImplementedException();
        }
    }
}
