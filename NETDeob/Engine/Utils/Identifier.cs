using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Core.Plugins;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Signatures;

namespace NETDeob.Core.Engine.Utils
{
    public static class Identifier
    {
        public delegate ISignature IdentifierTask(AssemblyDefinition asmDef, out bool found);
        private static List<IdentifierTask> IdentifierTasks = new List<IdentifierTask>
                                                                  {
                                                                      new IdentifierTask(IdentifyConfuser),
                                                                      new IdentifierTask(IdentifyPhoenixProtector),
                                                                      new IdentifierTask(IdentifyManco),
                                                                      new IdentifierTask(IdentifyNetz),
                                                                      new IdentifierTask(IdentifyNetShrink), 
                                                                      new IdentifierTask(IdentifyMpress),
                                                                      new IdentifierTask(IdentifyRpx),
                                                                      new IdentifierTask(IdentifyExePack),
                                                                      new IdentifierTask(IdentifySixxpack),
                                                                      new IdentifierTask(IdentifyRummage),
                                                                      new IdentifierTask(IdentifyObfusasm),
                                                                      new IdentifierTask(IdentifyHurpFuscator)
                                                                  };

        public static ISignature Identify(AssemblyDefinition asmDef)
        {
            foreach (var task in IdentifierTasks)
            {
                bool found;
                var signature = task(asmDef, out found);

                if (found)
                {
                    if (!DeobfuscatorContext.Debug)
                    {
                        DeobfuscatorContext.ActiveSignature = signature;
                        return signature;
                    }

                    return signature;
                }
            }

            return new Signatures.UnidentifiedSignature();
        }

        public static void RegisterPlugin(IPlugin plugin, bool favorPlugins)
        {
            plugin.RegisterIdentifierTasks(x =>
                                               {
                                                   if (favorPlugins)
                                                       IdentifierTasks = IdentifierTasks.Prepend(x).ToList();
                                                   else
                                                       IdentifierTasks.Add(x);
                                               }

                );
        }

        private static ISignature IdentifyPhoenixProtector(AssemblyDefinition asmDef, out bool found)
        {
            found = false;

            if (asmDef.FindMethod(mDef => 
                mDef.Body.Instructions.GetOpCodeCount(OpCodes.Xor) == 2 && 
                mDef.Body.Instructions.FirstOfOpCode(OpCodes.Shl) != null &&
                mDef.Body.Instructions.FirstOfOpCode(OpCodes.Or) != null) != null || (asmDef.EntryPoint.Name.StartsWith("?") && asmDef.EntryPoint.Name.EndsWith("?")))
            {
                found = true;
                return new Signatures.PhoenixSignature();
            }

            return new Signatures.UnidentifiedSignature();
        }
/*
        private static ISignature IdentifyCodeWall(AssemblyDefinition asmDef, out bool found)
        {
            found = false;

            if (asmDef.FindMethods(
                    mDef =>
                    mDef.Body.Instructions.GetOpCodeCount(OpCodes.Xor) == 6 &&
                    mDef.Body.Instructions.GetOpCodeCount(OpCodes.Ldc_I4) == 3).Count >= 1)
            {
                found = true;
                return new Signatures.CodeWallSignature();
            }

            return new Signatures.UnidentifiedSignature();
        }
*/
        private static ISignature IdentifyConfuser(AssemblyDefinition asmDef, out bool found)
        {
            var pattern = new Regex("Confuser v[0-9].[0-9].[0-9].[0-9]");
            var match = pattern.Match(File.ReadAllText(DeobfuscatorContext.InPath));
          
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

               // return new Signatures.ConfuserSignature(match.Groups[0].Value, Signatures.ConfuserSignature.GetInternalVersion(match.Groups[0].Value));
            }

            found = false;
            return new Signatures.UnidentifiedSignature();
        }
        private static ISignature IdentifyManco(AssemblyDefinition asmDef, out bool found)
        {
            if (asmDef.Modules.SelectMany(modDef => modDef.Types).Any(typeDef => typeDef.Name.Contains("();\t")))
            {
                found = true;
                return new Signatures.MancoSignature();
            }

            found = false;

            return new Signatures.UnidentifiedSignature();
        }
/*
        private static ISignature IdentifyCodeFort(AssemblyDefinition asmDef, out bool found)
        {
            if (asmDef.Modules.SelectMany(modDef => modDef.Types).Any(typeDef => typeDef.Namespace.Contains("___codefort")))
            {
                found = true;
                return new Signatures.CodeFortSignature();
            }

            found = false;

            return new Signatures.UnidentifiedSignature();
        }
*/
        private static ISignature IdentifyNetz(AssemblyDefinition asmDef, out bool found)
        {
            if (asmDef.Modules.SelectMany(modDef => modDef.Types).Any(typeDef => typeDef.Namespace.Contains("netz")))
            {
                found = true;
                return new Signatures.NetzSignature();
            }

            found = false;
            return new Signatures.UnidentifiedSignature();
        }
        private static ISignature IdentifyNetShrink(AssemblyDefinition asmDef, out bool found)
        {
            var signature = new ILSignature
                                {
                                    StartIndex = 0,
                                    StartOpCode = OpCodes.Nop,
                                    Instructions = new List<OpCode>
                                                       {
                                                           OpCodes.Stloc_0,
                                                           OpCodes.Call,
                                                           OpCodes.Ldc_I4_3,
                                                           OpCodes.Ldc_I4_1,
                                                           OpCodes.Newobj,
                                                           OpCodes.Stloc_1,
                                                           OpCodes.Ldloc_1,
                                                           OpCodes.Ldc_I4_S,
                                                           OpCodes.Conv_I8
                                                       }
                                };

            if (SignatureFinder.IsMatch(asmDef.EntryPoint, signature))
            {
                found = true;
                return new Signatures.NetShrinkSignature();
            }

            found = false;
            return new Signatures.UnidentifiedSignature();
        }
        private static ISignature IdentifyMpress(AssemblyDefinition asmDef, out bool found)
        {
            if (asmDef.Modules.SelectMany(modDef => modDef.Types).Any(typeDef => typeDef.Namespace == "mpress"))
            {
                found = true;
                return new Signatures.MpressSignature();
            }

            found = false;
            return new Signatures.UnidentifiedSignature();
        }
        private static ISignature IdentifyRpx(AssemblyDefinition asmDef, out bool found)
        {
            var signature = new ILSignature
                                {
                                    StartIndex = 0,
                                    StartOpCode = OpCodes.Nop,
                                    Instructions = new List<OpCode>
                                                       {
                                                           OpCodes.Call,
                                                           OpCodes.Callvirt,
                                                           OpCodes.Ldstr,
                                                           OpCodes.Callvirt,
                                                           OpCodes.Stloc_0,
                                                           OpCodes.Ldloc_0,
                                                           OpCodes.Ldc_I4_3,
                                                           OpCodes.Ldc_I4_1,
                                                           OpCodes.Call,
                                                           OpCodes.Stloc_1,
                                                           OpCodes.Ldloc_1,
                                                           OpCodes.Ldstr
                                                       }
                                };

            if(SignatureFinder.IsMatch(asmDef.EntryPoint, signature))
            {
                found = true;
                return new Signatures.RpxSignature();
            }

            found = false;
            return new Signatures.UnidentifiedSignature();
        }
        private static ISignature IdentifyExePack(AssemblyDefinition asmDef, out bool found)
        {
            //Lets just do the whole method lol
            var signature = new ILSignature
                                {
                                    StartIndex = 0,
                                    StartOpCode = OpCodes.Nop,
                                    Instructions = new List<OpCode>
                                                       {
                                                           OpCodes.Dup,
                                                           OpCodes.Ldnull,
                                                           OpCodes.Ldftn,
                                                           OpCodes.Newobj,
                                                           OpCodes.Callvirt,
                                                           OpCodes.Ldstr,
                                                           OpCodes.Newobj,
                                                           OpCodes.Ldnull,
                                                           OpCodes.Ldarg_0,
                                                           OpCodes.Callvirt,
                                                           OpCodes.Ret
                                                       }
                                };

            if (SignatureFinder.IsMatch(asmDef.EntryPoint, signature))
            {
                found = true;
                return new Signatures.ExePackSignature();
            }

            found = false;
            return new Signatures.UnidentifiedSignature();
        }
        private static ISignature IdentifySixxpack(AssemblyDefinition asmDef, out bool found)
        {
            var signature = new ILSignature
                                {
                                    StartIndex = 0,
                                    StartOpCode = OpCodes.Nop,
                                    Instructions = new List<OpCode>
                                                       {
                                                           OpCodes.Stloc_0,
                                                           OpCodes.Ldloc_0,
                                                           OpCodes.Ldnull,
                                                           OpCodes.Ldftn,
                                                           OpCodes.Newobj,
                                                           OpCodes.Callvirt,
                                                           OpCodes.Leave_S,
                                                           OpCodes.Pop,
                                                           OpCodes.Leave_S,
                                                           OpCodes.Ldc_I4_1
                                                       }
                                };

            if (SignatureFinder.IsMatch(asmDef.EntryPoint, signature))
            {
                found = true;
                return new Signatures.SixxpackSignature();
            }

            found = false;
            return new Signatures.UnidentifiedSignature();
        }
        private static ISignature IdentifyRummage(AssemblyDefinition asmDef, out bool found)
        {
            foreach(var type in asmDef.MainModule.Types)
                if (type.GetConstructors() != null)
                {
                    var target = type.GetConstructors().FirstOrDefault(obj => obj.Name == ".cctor");

                    if (target == null)
                        continue;

                    var call = target.Body.Instructions.FirstOrDefault(instr => instr.OpCode == OpCodes.Call);

                    if (call == null || call.Operand == null)
                        continue;

                    target = (call.Operand as MethodReference).Resolve();

                    if (!target.HasBody)
                        continue;

                    if (target.Body.Instructions.GetOpCodeCount(OpCodes.Xor) == 4)
                    {
                        found = true;
                        return new Signatures.RummageSignature();
                    }

                }

            found = false;
            return new Signatures.UnidentifiedSignature();
        }
        private static ISignature IdentifyObfusasm(AssemblyDefinition asmDef, out bool found)
        {
            MethodDefinition target;

            foreach(var type in asmDef.MainModule.Types)
                if((target = type.GetStaticConstructor()) != null)
                {
                    if(target.Body.Instructions.GetOpCodeCount(OpCodes.Stsfld) == 2 &&
                        target.Body.Instructions.GetOpCodeCount(OpCodes.Ldtoken) == 1 &&
                        target.Body.Instructions.GetOpCodeCount(OpCodes.Newarr) == 2 &&
                        type.Fields.Count == 3)
                    {
                        found = true;
                        return new Signatures.ObfusasmSignature();
                    }
                }

            found = false;
            return new Signatures.UnidentifiedSignature();
        }
        private static ISignature IdentifyHurpFuscator(AssemblyDefinition asmDef, out bool found)
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
