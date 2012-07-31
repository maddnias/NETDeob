using System;
using Mono.Cecil;
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
            throw new NotImplementedException();
        }
    }
}
