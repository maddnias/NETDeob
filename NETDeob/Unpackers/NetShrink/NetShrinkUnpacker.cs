using System;
using Mono.Cecil;
using NETDeob.Core.Unpackers.NetShrink.Tasks;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Core.Unpackers.NetShrink
{
    class NetShrinkUnpacker : AssemblyWorker
    {
        public NetShrinkUnpacker(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override void CreateTaskQueue()
        {
            TaskQueue.Add(new Unpacker(AsmDef));

            Deobfuscate();
        }
    }
}
