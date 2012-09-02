using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Deobfuscators.Confuser;
using NETDeob.Core.Deobfuscators.HurpFuscator;
using NETDeob.Core.Deobfuscators.Manco;
using NETDeob.Core.Deobfuscators.Obfusasm;
using NETDeob.Core.Deobfuscators.Rummage;
using NETDeob.Core.Deobfuscators.Unidentified;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Unpackers.ExePack;
using NETDeob.Core.Unpackers.Mpress;
using NETDeob.Core.Unpackers.NetShrink;
using NETDeob.Core.Unpackers.Netz;
using NETDeob.Core.Unpackers.Rpx;
using NETDeob.Core.Unpackers.Sixxpack;
using NETDeob.Deobfuscators.Phoenix_Protector;
using NETDeob.Misc.Structs__Enums___Interfaces.Signatures;

namespace NETDeob.Core.Engine
{
    public static class Signatures
    {
        public class ConfuserSignature1_6_0_0 : IUnsupportedSignature
        {
            public string Name
            {
                get { return "Confuser 1.6.0.0"; }
            }

            public Version Ver
            {
                get { return new Version(1, 6); }
            }

            public Type DeObfuscator
            {
                get { return typeof(ConfuserDeobfuscator); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return asmDef =>
                    {
                        var found = false;
                        ISignature sig;

                        if (
                            !((sig = Identifier.IdentifyConfuser(asmDef, out found)) is UnidentifiedSignature))
                            if (found)
                                return sig is ConfuserSignature1_6_0_0;
                        return false;
                    };
                }
            }
        }
        public class ConfuserSignature1_7_0_0 : IObfuscatorSignature
        {
            public string Name
            {
                get { return "Confuser 1.7.0.0"; }
            }

            public Version Ver
            {
                get { return new Version(1, 7); }
            }

            public Type DeObfuscator
            {
                get { return typeof(ConfuserDeobfuscator); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return asmDef =>
                    {
                        var found = false;
                        ISignature sig;

                        if (
                            !((sig = Identifier.IdentifyConfuser(asmDef, out found)) is UnidentifiedSignature))
                            if (found)
                                return sig is ConfuserSignature1_7_0_0;
                        return false;
                    };
                }
            }
        }
        public class ConfuserSignature1_8_0_0 : IUnsupportedSignature
        {
            public string Name
            {
                get { return "Confuser 1.8.0.0"; }
            }

            public Version Ver
            {
                get { return new Version(1, 8); }
            }

            public Type DeObfuscator
            {
                get { return typeof(ConfuserDeobfuscator); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return asmDef =>
                    {
                        var found = false;
                        ISignature sig;

                        if (
                            !((sig = Identifier.IdentifyConfuser(asmDef, out found)) is UnidentifiedSignature))
                            if (found)
                                return sig is ConfuserSignature1_8_0_0;
                        return false;
                    };
                }
            }
        }
        public class ConfuserSignature1_9_0_0 : IUnsupportedSignature
        {
            public string Name
            {
                get { return "Confuser 1.9.0.0"; }
            }

            public Version Ver
            {
                get { return new Version(1, 9); }
            }

            public Type DeObfuscator
            {
                get { return typeof(ConfuserDeobfuscator); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return asmDef =>
                    {
                        var found = false;
                        ISignature sig;

                        if (
                            !((sig = Identifier.IdentifyConfuser(asmDef, out found)) is UnidentifiedSignature))
                            if (found)
                                return sig is ConfuserSignature1_9_0_0;
                        return false;
                    };
                }
            }
        }
        public class MancoSignature : IObfuscatorSignature
        {
            public string Name
            {
                get { return "Manco.NET Obfuscator"; }
            }

            public Version Ver
            {
                get { return new Version(0, 0); }
            }

            public Type DeObfuscator
            {
                get { return typeof (MancoDeobfuscator); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return asmDef => asmDef.Modules.SelectMany(modDef => modDef.Types).Any(
                        typeDef => typeDef.Name.Contains("();\t"));
                }
            }
        }
        public class PhoenixSignature : IObfuscatorSignature
        {
            public string Name
            {
                get { return "Phoenix Protector"; }
            }

            public Version Ver
            {
                get { return new Version(0, 0); }
            }

            public Type DeObfuscator
            {
                get { return typeof(PhoenixProtectorDeobfuscator); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return asmDef =>
                    {
                        return asmDef.FindMethod(mDef =>
                                                 mDef.Body.Instructions.GetOpCodeCount(OpCodes.Xor) == 2 &&
                                                 mDef.Body.Instructions.FirstOfOpCode(OpCodes.Shl) != null &&
                                                 mDef.Body.Instructions.FirstOfOpCode(OpCodes.Or) != null) != null || (asmDef.EntryPoint.Name.StartsWith("?") && asmDef.EntryPoint.Name.EndsWith("?"));
                    };
                }
            }
        }
        public class NetzSignature : IObfuscatorSignature
        {
            public string Name
            {
                get { return "NetZ .NET Packer"; }
            }

            public Version Ver
            {
                get { return new Version(0, 0); }
            }

            public Type DeObfuscator
            {
                get { return typeof(NetzUnpacker); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return asmDef => (asmDef.Modules.SelectMany(modDef => modDef.Types).Any(
                        typeDef => typeDef.Namespace.Contains("netz")));
                }
            }
        }
        public class RummageSignature : IObfuscatorSignature
        {
            public string Name
            {
                get { return "Rummage Obfuscator"; }
            }

            public Version Ver
            {
                get { return new Version(0, 0); }
            }

            public Type DeObfuscator
            {
                get { return typeof (RummageDeobfuscator); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return asmDef =>
                               {
                                   foreach (var type in asmDef.MainModule.Types)
                                       if (type.GetConstructors() != null)
                                       {
                                           var target =
                                               type.GetConstructors().FirstOrDefault(obj => obj.Name == ".cctor");

                                           if (target == null)
                                               continue;

                                           var call =
                                               target.Body.Instructions.FirstOrDefault(
                                                   instr => instr.OpCode == OpCodes.Call);

                                           if (call == null || call.Operand == null)
                                               continue;

                                           target = (call.Operand as MethodReference).Resolve();

                                           if (!target.HasBody)
                                               continue;

                                           if (target.Body.Instructions.GetOpCodeCount(OpCodes.Xor) == 4)
                                               return true;
                                       }

                                   return false;
                               };
                }
            }
        }
        public class ObfusasmSignature : IObfuscatorSignature
        {
            public string Name
            {
                get { return "Obfusasm Obfuscator"; }
            }

            public Version Ver
            {
                get { return new Version(0, 0); }
            }

            public Type DeObfuscator
            {
                get { return typeof (ObfusasmDeobfuscator); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return asmDef =>
                               {
                                   MethodDefinition target;

                                   foreach (var type in asmDef.MainModule.Types)
                                       if ((target = type.GetStaticConstructor()) != null)
                                       {
                                           if (target.Body.Instructions.GetOpCodeCount(OpCodes.Stsfld) == 2 &&
                                               target.Body.Instructions.GetOpCodeCount(OpCodes.Ldtoken) == 1 &&
                                               target.Body.Instructions.GetOpCodeCount(OpCodes.Newarr) == 2 &&
                                               type.Fields.Count == 3)
                                               return true;
                                       }

                                   return false;
                               };
                }
            }
        }
        public class NetShrinkSignature : IPackerSignature
        {
            public string Name
            {
                get { return ".NET Shrink"; }
            }

            public Version Ver
            {
                get { return new Version(0, 0); }
            }

            public Type DeObfuscator
            {
                get { return typeof (NetShrinkUnpacker); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return asmDef =>
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
                                       return true;

                                   return false;
                               };
                }
            }
        }
        public class MpressSignature : IPackerSignature
        {
            public string Name
            {
                get { return "Mpress .NET Packer"; }
            }

            public Version Ver
            {
                get { return new Version(0, 0); }
            }

            public Type DeObfuscator
            {
                get { return typeof(MpressUnpacker); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return
                        asmDef =>
                        asmDef.Modules.SelectMany(modDef => modDef.Types).Any(typeDef => typeDef.Namespace == "mpress");
                }
            }
        }
        public class RpxSignature : IPackerSignature
        {
            public string Name
            {
                get { return "Rpx .NET Packer"; }
            }

            public Version Ver
            {
                get { return new Version(0, 0); }
            }

            public Type DeObfuscator
            {
                get { return typeof (RpxUnpacker); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return asmDef =>
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

                                   if (SignatureFinder.IsMatch(asmDef.EntryPoint, signature))
                                       return true;
                                   return false;
                               };
                }
            }
        }
        public class ExePackSignature : IPackerSignature
        {
            public string Name
            {
                get { return "ExePack.NET Packer"; }
            }

            public Version Ver
            {
                get { return new Version(0, 0); }
            }

            public Type DeObfuscator
            {
                get { return typeof (ExePackUnpacker); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return asmDef =>
                               {
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
                                       return true;

                                   return false;
                               };
                }
            }
        }
        public class SixxpackSignature : IPackerSignature
        {
            public string Name
            {
                get { return "Sixxpack .NET Packer"; }
            }

            public Version Ver
            {
                get { return new Version(0, 0); }
            }

            public Type DeObfuscator
            {
                get { return typeof (SixxpackUnpacker); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return asmDef =>
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
                                       return true;

                                   return false;
                               };
                }
            }
        }
        public class HurpFuscatorSignature1_0 : IObfuscatorSignature
        {
            public string Name
            {
                get { return "HurpFuscator 1.0"; }
            }

            public Version Ver
            {
                get { return new Version(1, 0); }
            }

            public Type DeObfuscator
            {
                get { return typeof (HurpDeobfuscator); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return asmDef =>
                               {
                                   var found = false;
                                   ISignature sig;

                                   if (
                                       !((sig = Identifier.IdentifyHurpFuscator(asmDef, out found)) is
                                         UnidentifiedSignature))
                                       if (found)
                                           return sig is HurpFuscatorSignature1_0;

                                   return false;
                               };
                }
            }
        }
        public class HurpFuscatorSignature1_1 : IObfuscatorSignature
        {
            public string Name
            {
                get { return "HurpFuscator 1.1"; }
            }

            public Version Ver
            {
                get { return new Version(1, 1); }
            }

            public Type DeObfuscator
            {
                get { return typeof(HurpDeobfuscator); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get
                {
                    return asmDef =>
                    {
                        var found = false;
                        ISignature sig;

                        if (
                            !((sig = Identifier.IdentifyHurpFuscator(asmDef, out found)) is
                              UnidentifiedSignature))
                            if (found)
                                return sig is HurpFuscatorSignature1_1;

                        return false;
                    };
                }
            }
        }
        public class UnidentifiedSignature : IUnidentifiedSignature
        {
            public string Name
            {
                get { return "Unidentified"; }
            }

            public Version Ver
            {
                get { return new Version(0, 0); }
            }

            public Type DeObfuscator
            {
                get { return typeof(UnknownDeobfuscator); }
            }

            public Func<AssemblyDefinition, bool> IsDetect
            {
                get { return asmDef => true; }
            }
        }
    }
}
