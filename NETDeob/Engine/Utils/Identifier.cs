using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Plugins;
using NETDeob.Misc.Structs__Enums___Interfaces.Signatures;

namespace NETDeob.Core.Engine.Utils
{
    public class Identifier
    {
        public Identifier()
        {
            Signatures = new List<ISignature>();

            foreach(var type in typeof(Signatures).GetNestedTypes())
                Signatures.Add(Activator.CreateInstance(type) as ISignature);
        }

        public delegate ISignature IdentifierTask(AssemblyDefinition asmDef, out bool found);

        public static List<ISignature> Signatures;

        public static ISignature Identify(AssemblyDefinition asmDef)
        {
            ISignature outSig;

            return ((outSig = Signatures.FirstOrDefault(x => x.IsDetect(asmDef))) == null
                        ? Activator.CreateInstance<IUnidentifiedSignature>()
                        : outSig);
        }

        public static void RegisterPlugin(IPlugin plugin, bool favorPlugins)
        {
            Signatures = (List<ISignature>) Signatures.Prepend(plugin.Signature);
        }

        public static ISignature IdentifyConfuser(AssemblyDefinition asmDef, out bool found)
        {
            var pattern = new Regex("Confuser v[0-9].[0-9].[0-9].[0-9]");
            var match = pattern.Match(File.ReadAllText(Globals.DeobContext.InPath));
          
            if(match.Success)
            {
                found = true;

                if (match.Groups[0].Value == "Confuser v1.6.0.0")
                    return new Signatures.ConfuserSignature1_6_0_0();
                if (match.Groups[0].Value == "Confuser v1.7.0.0")
                    return new Signatures.ConfuserSignature1_7_0_0();
                if (match.Groups[0].Value == "Confuser v1.8.0.0")
                    return new Signatures.ConfuserSignature1_8_0_0();
                if (match.Groups[0].Value == "Confuser v1.9.0.0")
                    return new Signatures.ConfuserSignature1_9_0_0();
            }

            found = false;
            return new Signatures.UnidentifiedSignature();
        }

        public static ISignature IdentifyHurpFuscator(AssemblyDefinition asmDef, out bool found)
        {
            foreach (var mDef in asmDef.FindMethods(m => m.HasBody))
            {
                if (mDef.Parameters.Count == 2 &&
                    mDef.Body.Variables.Count == 9 &&
                    mDef.ReturnType.ToString().ToLower().Contains("string") &&
                    mDef.Body.Instructions.GetOpCodeCount(OpCodes.Callvirt) == 7)
                {
                    found = true;
                    return new Signatures.HurpFuscatorSignature1_0();
                }

                if(mDef.Parameters.Count == 1 &&
                    mDef.Body.Variables.Count == 7 &&
                    mDef.ReturnType.ToString().ToLower().Contains("string") &&
                    mDef.Body.Instructions.GetOpCodeCount(OpCodes.Callvirt) == 7 &&
                    mDef.Body.Instructions.GetOpCodeCount(OpCodes.Ldstr) == 2)
                {
                    found = true;
                    return new Signatures.HurpFuscatorSignature1_1();
                }
            }

            found = false;
            return new Signatures.UnidentifiedSignature();
        }
    }
}
