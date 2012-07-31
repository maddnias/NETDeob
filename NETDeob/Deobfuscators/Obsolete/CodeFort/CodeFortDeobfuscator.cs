using Mono.Cecil;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Deobfuscators.CodeFort
{
    class CodeFortDeobfuscator : AssemblyWorker
    {
        public CodeFortDeobfuscator(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override void CreateTaskQueue()
        {
            //TaskQueue.Add(new Renamer(AsmDef, new RenamingScheme(true)));
            //TaskQueue.Add(new StringDecryptor(AsmDef));

            Deobfuscate();
        }
    }
}
