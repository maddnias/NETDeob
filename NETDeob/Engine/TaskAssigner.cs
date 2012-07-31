using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETDeob.Deobfuscators.CodeWall;
using NETDeob.Deobfuscators.Phoenix_Protector;

namespace NETDeob.Engine
{
    public static class TaskAssigner
    {
        public static void AssignDeobfuscation(AssemblyDefinition asmDef)
        {
            var signature = Identifier.Identify(asmDef);

            if (!(signature is UnidentifiedSignature))
                Logger.VSLog("Obfuscated identified: " + signature.Name);
            else
                Logger.VSLog("No obfuscator identified!");

            Logger.VSLog("Deobfuscating...\n");
            Activator.CreateInstance(signature.DeObfuscator, new object[] {asmDef});
        }
    }
}
