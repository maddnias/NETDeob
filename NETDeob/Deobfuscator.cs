using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Misc;

namespace NETDeob.Core
{
    public class Deobfuscator
    {
        //TODO: Add more overloads for Deobfuscate, hopefully more advanced parameters for deobfuscation in the future

        public Deobfuscator(UnhandledExceptionEventHandler excHandler)
        {
            // To keep NETDeob from crashing when an exception occurs
            if(excHandler != null)
                AppDomain.CurrentDomain.UnhandledException += excHandler;
        }

        public void Deobfuscate(DynamicStringDecryptionContetx strCtx = null)
        {
            DeobfuscatorContext.DynStringCtx = strCtx;
            TaskAssigner.AssignDeobfuscation(DeobfuscatorContext.AsmDef);
        }

        public void Deobfuscate(AssemblyDefinition[] asmDefs)
        {
            foreach (var asmDef in asmDefs)
                TaskAssigner.AssignDeobfuscation(asmDef);
        }

        public string FetchSignature()
        {
            var sig = Identifier.Identify(DeobfuscatorContext.AsmDef);
            return string.Format("Name: {0}\r\nVersion: {1}", sig.Name, sig.Ver);
        }
    }
}
