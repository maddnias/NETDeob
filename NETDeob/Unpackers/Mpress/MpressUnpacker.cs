using Mono.Cecil;
using NETDeob.Core.Unpackers.Mpress.Tasks;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Core.Unpackers.Mpress
{
    public class MpressUnpacker : AssemblyWorker
    {
        public MpressUnpacker(AssemblyDefinition asmDef)
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
