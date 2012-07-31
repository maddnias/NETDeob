using Mono.Cecil;
using NETDeob.Core.Unpackers.ExePack.Tasks;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Core.Unpackers.ExePack
{
    public class ExePackUnpacker : AssemblyWorker
    {
        public ExePackUnpacker(AssemblyDefinition asmDef)
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
