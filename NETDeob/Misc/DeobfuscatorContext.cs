using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Misc.Structs__Enums___Interfaces.Signatures;

namespace NETDeob.Core.Misc
{
    public class DeobfuscatorContext
    {
        public DeobfuscatorContext()
        {
            SigIdentifier = new Identifier();
        }

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
        public class DynamicStringDecryptionContext
        {
            public StringDecryption DecryptionType;
            public int AssociatedToken;
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
        public enum OutputType
        {
            Subtle = 0,
            Verbose = 1
        }

        public bool Debug = false;
        public OutputType Output = OutputType.Subtle;
        public DeobfuscatorOptions Options;
        public string InPath;
        public string OutPath;
        public AssemblyDefinition AsmDef;
        public List<MarkedMember> MarkedMembers = new List<MarkedMember>();
        public List<ResEx> ResStreams = new List<ResEx>();
        public DynamicStringDecryptionContext DynStringCtx;

        public Identifier SigIdentifier;
        public ISignature ActiveSignature;
    }
}