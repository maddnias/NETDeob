using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Deobfuscators.Generic;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators.CodeWall.Tasks;
using NETDeob.Deobfuscators.Generic;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Deobfuscators.CodeWall
{
    class CodeWallDeobfuscator : AssemblyWorker
    {
        public CodeWallDeobfuscator(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public override void CreateTaskQueue()
        {

            if (IsAssemblyEncrypted())
            {
                Logger.VSLog("Detected assembly encryption; dumping original assembly...");
                //TaskQueue.Add(new StubDumper2(AsmDef));
            }

            TaskQueue.Add(new MethodCleaner2(AsmDef));
            TaskQueue.Add(new StringDecryptor(AsmDef));
            TaskQueue.Add(new Renamer(AsmDef, new RenamingScheme(true) { Resources = false }));
           

            Deobfuscate();
        }


        public bool IsAssemblyEncrypted()
        {
            var target = AsmDef.EntryPoint;

            return target.Body.Instructions.GetOpCodeCount(OpCodes.Ldc_I4) == 5 &&
                   target.Body.Instructions.GetOpCodeCount(OpCodes.Call) == 7;
        }
    }
}
