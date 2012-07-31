using Mono.Cecil;
using NETDeob.Core.Unpackers.Rpx.Tasks;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Core.Unpackers.Rpx
{
    public class RpxUnpacker : AssemblyWorker
    {
        public RpxUnpacker(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override void CreateTaskQueue()
        {
            IsSave = false;
            TaskQueue.Add(new Unpacker(AsmDef));

            Deobfuscate();
        }
    }
}
