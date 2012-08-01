using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETDeob.Core.Engine.Utils;

namespace NETDeob.Core
{
    public class Deobfuscator
    {
        //TODO: Add more overloads for Deobfuscate, hopefully more advanced parameters in the future

        public Deobfuscator(UnhandledExceptionEventHandler excHandler)
        {
            // To keep NETDeob from crashing when an exception occurs
            AppDomain.CurrentDomain.UnhandledException += excHandler;
        }

        public void Deobfuscate(AssemblyDefinition asmDef)
        {
            TaskAssigner.AssignDeobfuscation(asmDef);
        }

        public void Deobfuscate(AssemblyDefinition[] asmDefs)
        {
            foreach (var asmDef in asmDefs)
                TaskAssigner.AssignDeobfuscation(asmDef);
        }
    }
}
