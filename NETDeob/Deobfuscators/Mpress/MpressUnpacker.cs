using Mono.Cecil;
using NETDeob.Deobfuscators.Mpress.Tasks;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Deobfuscators.Mpress
{
    public class MpressUnpacker : IDeobfuscator
    {
        public MpressUnpacker(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override void CreateTaskQueue()
        {
            base.IsSave = false;
            base.TaskQueue.Add(new Unpacker());

            Deobfuscate();
        }
    }
}
