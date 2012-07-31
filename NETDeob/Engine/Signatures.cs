using System;
using NETDeob.Core.Deobfuscators.Confuser;
using NETDeob.Core.Deobfuscators.HurpFuscator;
using NETDeob.Core.Deobfuscators.Manco;
using NETDeob.Core.Deobfuscators.Obfusasm;
using NETDeob.Core.Deobfuscators.Unidentified;
using NETDeob.Core.Unpackers.ExePack;
using NETDeob.Core.Unpackers.Mpress;
using NETDeob.Core.Unpackers.NetShrink;
using NETDeob.Core.Unpackers.Netz;
using NETDeob.Core.Unpackers.Rpx;
using NETDeob.Core.Unpackers.Sixxpack;
using NETDeob.Deobfuscators.CodeFort;
using NETDeob.Deobfuscators.CodeWall;
using NETDeob.Deobfuscators.Phoenix_Protector;
using NETDeob.Deobfuscators.Rummage;

using NETDeob.Misc.Structs__Enums___Interfaces.Signatures;

namespace NETDeob.Core.Engine
{
    public static class Signatures
    {
        public struct UnidentifiedSignature : IUnidentifiedSignature
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
        }

        public struct ConfuserSignature1_7_0_0 : IObfuscatorSignature
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
        }

        public struct ConfuserSignature1_8_0_0 : IUnsupportedSignature
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
        }

        public struct ConfuserSignature1_9_0_0 : IUnsupportedSignature
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
        }
       
        public struct PhoenixSignature : IObfuscatorSignature
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
        }
        public struct CodeWallSignature : IUnsupportedSignature
        {
            public string Name
            {
                get { return "CodeWall"; }
            }

            public Version Ver
            {
                get { return new Version(0, 0); }
            }

            public Type DeObfuscator
            {
                get { return typeof(CodeWallDeobfuscator); }
            }
        }
        public struct MancoSignature : IObfuscatorSignature
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
                get { return typeof(MancoDeobfuscator); }
            }
        }
        public struct NetzSignature : IObfuscatorSignature
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
        }
        public struct CodeFortSignature : IUnsupportedSignature
        {
            public string Name
            {
                get { return "CodeFort Obfuscator"; }
            }

            public Version Ver
            {
                get { return new Version(0, 0); }
            }

            public Type DeObfuscator
            {
                get { return typeof(CodeFortDeobfuscator); }
            }
        }
        public struct RummageSignature : IObfuscatorSignature
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
                get { return typeof(RummageDeobfuscator); }
            }
        }
        public struct ObfusasmSignature : IObfuscatorSignature
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
        }

        public struct NetShrinkSignature : IPackerSignature
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
                get { return typeof(NetShrinkUnpacker); }
            }
        }
        public struct MpressSignature : IPackerSignature
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
        }
        public struct RpxSignature : IPackerSignature
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
                get { return typeof(RpxUnpacker); }
            }
        }
        public struct ExePackSignature : IPackerSignature
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
                get { return typeof(ExePackUnpacker); }
            }
        }
        public struct SixxpackSignature : IPackerSignature
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
                get { return typeof(SixxpackUnpacker); }
            }
        }
        public struct HurpFuscatorSignature1_0 : IUnsupportedSignature
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
        }
        public struct HurpFuscatorSignature1_1 : IObfuscatorSignature
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
        }
    }
}
