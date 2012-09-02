using System.Reflection;
using Mono.Cecil;
using NETDeob.Core.Deobfuscators.Confuser.Tasks._1_7;
using NETDeob.Core.Deobfuscators.Generic;
using NETDeob.Core.Engine;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators.Confuser.Tasks.Common;
using NETDeob.Deobfuscators.Confuser.Tasks._1_7;
using NETDeob.Deobfuscators.Generic;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Core.Deobfuscators.Confuser
{
    class ConfuserDeobfuscator : AssemblyWorker
    {
        public bool IsPacked;

        public ConfuserDeobfuscator(AssemblyDefinition asmDef) 
            : base(asmDef)
        {
            
        }

        public override void CreateTaskQueue()
        {
            switch((Globals.DeobContext.ActiveSignature).Ver.Minor)
            {
                case 7:
                    TaskQueue.Add(new MetadataFixer(AsmDef));
                    //TaskQueue.Add(new MethodCleaner2(AsmDef));
                    //TaskQueue.Add(new ProxyResolver(AsmDef));
                    TaskQueue.Add(new MethodCleaner(AsmDef));
                    TaskQueue.Add(new ResourceDecryptor(AsmDef));
                    //TaskQueue.Add(new ProxyResolver2(AsmDef));
                    TaskQueue.Add(new ConstantsDecryptor(AsmDef));
                    TaskQueue.Add(new StackUnderflowCleaner(AsmDef));
                    TaskQueue.Add(new WatermarkRemover(AsmDef));
                    TaskQueue.Add(new AntiDump(AsmDef));
                    TaskQueue.Add(new AntiDebug(AsmDef));
                    TaskQueue.Add(new AntiILDasm(AsmDef));
                    TaskQueue.Add(new Renamer(AsmDef, new RenamingScheme(true) {Resources = false}));
                    break;

                case 8:
                    TaskQueue.Add(new MetadataFixer(AsmDef));
                    TaskQueue.Add(new WatermarkRemover(AsmDef));
                    break;

                case 9:
                    TaskQueue.Add(new MetadataFixer(AsmDef));
                    TaskQueue.Add(new ProxyResolver2(AsmDef));
                    break;
            }

            Deobfuscate();
        }
    }
}
