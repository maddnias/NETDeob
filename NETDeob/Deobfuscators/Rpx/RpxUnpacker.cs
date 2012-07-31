using Mono.Cecil;
using NETDeob.Deobfuscators.Rpx.Tasks;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Deobfuscators.Rpx
{
    public class RpxUnpacker : IDeobfuscator
    {
        public RpxUnpacker(AssemblyDefinition asmDef)
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
