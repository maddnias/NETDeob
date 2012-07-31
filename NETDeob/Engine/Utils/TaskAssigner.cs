using System;
using Mono.Cecil;
using NETDeob.Core.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Signatures;

namespace NETDeob.Core.Engine.Utils
{
    public static class TaskAssigner
    {
        public static void AssignDeobfuscation(AssemblyDefinition asmDef)
        {
            var signature = Identifier.Identify(asmDef);

            if (signature is IObfuscatorSignature)
                Logger.VSLog("Obfuscator identified: " + signature.Name);
            else if (signature is IPackerSignature)
                Logger.VSLog("Packer identified: " + signature.Name);
            else if (signature is IUnidentifiedSignature)
                Logger.VSLog("No protection identified!");
            else
            {
                Logger.VSLog("Unsupported protection: " + signature.Name);
                return;
            }

            Logger.VSLog("Cleaning assembly...\n----------------------------");
            Activator.CreateInstance(signature.DeObfuscator, new object[] {asmDef});
        }
    }
}
