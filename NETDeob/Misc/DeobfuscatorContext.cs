using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using NETDeob.Misc.Structs__Enums___Interfaces.Signatures;

namespace NETDeob.Core.Misc
{
    public enum MemberType
    {
        Type = 0,
        Method = 1,
        Field = 2,
        Property = 4,
        Resource = 8,
        Delegate = 16,
        Attribute = 32,
        Instruction = 64,
        AssemblyReference = 128
    }

    public enum StringDecryption
    {
        Explicit = 0,
        Dynamic = 1,
        DynamicBrute = 2
    }

    public class DynamicStringDecryptionContetx
    {
        public StringDecryption DecryptionType;
        public List<int> AssociatedTokens;
    }

    public class MarkedMember
    {
        public string ID;
        public object Member;
        public object ParentMember;
        public MemberType @Type;
    }

    public class ResEx
    {
        public string Name;
        public Stream ResStream;
    }

    public static class DeobfuscatorContext
    {
        public enum OutputType
        {
            Subtle = 0,
            Verbose = 1
        }

        public static bool Debug = false;

        public static OutputType Output;

        public static DeobfuscatorOptions Options = new DeobfuscatorOptions();
        public static string InPath;
        public static string OutPath;
        public static AssemblyDefinition AsmDef;
        public static List<MarkedMember> MarkedMembers = new List<MarkedMember>();
        public static List<ResEx> ResStreams = new List<ResEx>();
        public static DynamicStringDecryptionContetx DynStringCtx;

        public static ISignature ActiveSignature;
    }
}