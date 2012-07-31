using Mono.Cecil;
using NETDeob.Core.Unpackers.Netz.Tasks;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Core.Unpackers.Netz
{
    public class NetzUnpacker : AssemblyWorker
    {
        public NetzUnpacker(AssemblyDefinition asmDef)
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
