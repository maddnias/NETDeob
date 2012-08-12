using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Deobfuscators;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;

namespace NETDeob.Core.Deobfuscators.Generic.Renaming
{
    public class Renamer2 : AssemblyDeobfuscationTask
    {
        private bool RenameTypes { get; set; }
        private bool RenameMethods { get; set; }
        private bool RenameResources { get; set; }
        private bool RenameFields { get; set; }
        private bool RenameProperties { get; set; }
        private bool RenameEvents { get; set; }

        public Renamer2(AssemblyDefinition asmDef, RenamingScheme scheme)
            : base(asmDef)
        {
            RenameTypes = scheme.Types;
            RenameMethods = scheme.Methods;
            RenameResources = scheme.Resources;
            RenameFields = scheme.Fields;
            RenameProperties = scheme.Properties;
            RenameEvents = scheme.Events;
        }

        [DeobfuscationPhase(1, "")]
        public bool Phase1()
        {
            var members = new Dictionary<IMemberDefinition, IMemberRenamer>();

            foreach(var typeDef in AsmDef.MainModule.GetAllTypes())
            {
                members.Add(typeDef, new TypeRenamer(typeDef));
            }

            return true;
        }
    }
}
