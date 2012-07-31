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

        public static OutputType Output;

        public static string InPath;
        public static string OutPath;
        public static AssemblyDefinition AsmDef;
        public static List<MarkedMember> MarkedMembers = new List<MarkedMember>();
        public static List<ResEx> ResStreams = new List<ResEx>();

        public static ISignature ActiveSignature;
    }
}