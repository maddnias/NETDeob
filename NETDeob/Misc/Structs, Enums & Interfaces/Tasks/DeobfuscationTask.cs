using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Misc.Structs__Enums___Interfaces.Tasks.Base;

namespace NETDeob.Deobfuscators
{
    public class MetadataDeobfuscationTask : Task
    {
        public MetadataDeobfuscationTask(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }
    }

    public class AssemblyDeobfuscationTask : Task
    {
        public AssemblyDeobfuscationTask(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }
    }
}
