using Mono.Cecil;
using NETDeob.Core.Unpackers.Sixxpack.Tasks;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Core.Unpackers.Sixxpack
{
    public class SixxpackUnpacker : AssemblyWorker
    {
        public SixxpackUnpacker(AssemblyDefinition asmDef)
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
