using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Deobfuscators.CodeFort;
using NETDeob.Deobfuscators.CodeWall;
using NETDeob.Deobfuscators.Confuser;
using NETDeob.Deobfuscators.Manco;
using NETDeob.Deobfuscators.Mpress;
using NETDeob.Deobfuscators.NetShrink;
using NETDeob.Deobfuscators.Netz;
using NETDeob.Deobfuscators.Phoenix_Protector;
using NETDeob.Deobfuscators.Rpx;
using NETDeob.Deobfuscators.Unidentified;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces;

namespace NETDeob.Engine
{
    public interface ISignature
    {
        string Name { get; }
        Type DeObfuscator { get; }
    }

    public struct PhoenixSignature : ISignature
    {
        public string Name
        {
            get { return "Phoenix Protector"; }
        }

        public Type DeObfuscator
        {
            get { return typeof (PhoenixProtectorDeobfuscator); }
        }
    }
    public struct CodeWallSignature : ISignature
    {
        public string Name
        {
            get { return "CodeWall"; }
        }

        public Type DeObfuscator
        {
            get { return typeof (CodeWallDeobfuscator); }
        }
    }
    public struct UnidentifiedSignature : ISignature
    {
        public string Name
        {
            get { return "Unidentified"; }
        }

        public Type DeObfuscator
        {
            get { return typeof (UnknownDeobfuscator); }
        }
    }
    public struct ConfuserSignature : ISignature
    {
        public string Name
        {
            get { return "Confuser"; }
        }

        public Type DeObfuscator
        {
            get { return typeof(ConfuserDeobfuscator); }
        }
    }
    public struct MancoSignature : ISignature
    {
        public string Name
        {
            get { return "Manco.NET Obfuscator"; }
        }

        public Type DeObfuscator
        {
            get { return typeof(MancoDeobfuscator); }
        }
    }
    public struct NetzSignature : ISignature
    {
        public string Name
        {
            get { return "NetZ .NET Packer"; }
        }

        public Type DeObfuscator
        {
            get { return typeof(NetzUnpacker); }
        }
    }
    public struct CodeFortSignature : ISignature
    {
        public string Name
        {
            get { return "CodeFort Obfuscator"; }
        }

        public Type DeObfuscator
        {
            get { return typeof(CodeFortDeobfuscator); }
        }
    }
    public struct NetShrinkSignature : ISignature
    {
        public string Name
        {
            get { return ".NET Shrink"; }
        }

        public Type DeObfuscator
        {
            get { return typeof(NetShrinkUnpacker); }
        }
    }
    public struct MpressSignature : ISignature
    {
        public string Name
        {
            get { return "Mpress .NET Packer"; }
        }

        public Type DeObfuscator
        {
            get { return typeof(MpressUnpacker); }
        }
    }
    public struct RpxSignature : ISignature
    {
        public string Name
        {
            get { return "Rpx .NET Packer"; }
        }

        public Type DeObfuscator
        {
            get { return typeof(RpxUnpacker); }
        }
    }

    public static class Identifier
    {
        private delegate ISignature IdentifierTask(AssemblyDefinition asmDef, out bool found);
        private static readonly List<IdentifierTask> IdentifierTasks = new List<IdentifierTask>
                                                                  {
                                                                      new IdentifierTask(IdentifyPhoenixProtector),
                                                                      new IdentifierTask(IdentifyCodeWall),
                                                                      new IdentifierTask(IdentifyConfuser),
                                                                      new IdentifierTask(IdentifyManco),
                                                                      new IdentifierTask(IdentifyCodeFort),
                                                                      new IdentifierTask(IdentifyNetz),
                                                                      new IdentifierTask(IdentifyNetShrink), 
                                                                      new IdentifierTask(IdentifyMpress),
                                                                      new IdentifierTask(IdentifyRpx)
                                                                  };

        public static ISignature Identify(AssemblyDefinition asmDef)
        {
            foreach(var task in IdentifierTasks)
            {
                var found = false;
                var signature = task(asmDef, out found);

                if (found)
                    return signature;
            }

            return new PhoenixSignature();
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
                return new PhoenixSignature();
            }

            return new UnidentifiedSignature();
        }
        private static ISignature IdentifyCodeWall(AssemblyDefinition asmDef, out bool found)
        {
            found = false;

            if (asmDef.FindMethods(
                    mDef =>
                    mDef.Body.Instructions.GetOpCodeCount(OpCodes.Xor) == 6 &&
                    mDef.Body.Instructions.GetOpCodeCount(OpCodes.Ldc_I4) == 3).Count >= 1)
            {
                found = true;
                return new CodeWallSignature();
            }

            return new UnidentifiedSignature();
        }
        private static ISignature IdentifyConfuser(AssemblyDefinition asmDef, out bool found)
        {
            if (asmDef.Modules.SelectMany(modDef => modDef.Types).Any(typeDef => typeDef.Name.ToLower() == "confusedbyattribute"))
            {
                found = true;
                return new ConfuserSignature();
            }

            found = false;

            return new UnidentifiedSignature();
        }
        private static ISignature IdentifyManco(AssemblyDefinition asmDef, out bool found)
        {
            if (asmDef.Modules.SelectMany(modDef => modDef.Types).Any(typeDef => typeDef.Name.Contains("();\t")))
            {
                found = true;
                return new MancoSignature();
            }

            found = false;

            return new UnidentifiedSignature();
        }
        private static ISignature IdentifyCodeFort(AssemblyDefinition asmDef, out bool found)
        {
            if (asmDef.Modules.SelectMany(modDef => modDef.Types).Any(typeDef => typeDef.Namespace.Contains("___codefort")))
            {
                found = true;
                return new CodeFortSignature();
            }

            found = false;

            return new UnidentifiedSignature();
        }
        private static ISignature IdentifyNetz(AssemblyDefinition asmDef, out bool found)
        {
            if (asmDef.Modules.SelectMany(modDef => modDef.Types).Any(typeDef => typeDef.Namespace.Contains("netz")))
            {
                found = true;
                return new NetzSignature();
            }

            found = false;
            return new UnidentifiedSignature();
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
                return new NetShrinkSignature();
            }

            found = false;
            return new UnidentifiedSignature();
        }
        private static ISignature IdentifyMpress(AssemblyDefinition asmDef, out bool found)
        {
            if (asmDef.Modules.SelectMany(modDef => modDef.Types).Any(typeDef => typeDef.Namespace.Contains("mpress")))
            {
                found = true;
                return new MpressSignature();
            }

            found = false;
            return new UnidentifiedSignature();
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
                return new RpxSignature();
            }

            found = false;
            return new UnidentifiedSignature();
        }
    }
}
