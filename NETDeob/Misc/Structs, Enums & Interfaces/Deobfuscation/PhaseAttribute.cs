using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation
{
    public class PhaseAttribute : Attribute
    {
        public PhaseAttribute(int id, string description)
        {
            ID = id;
            Description = description;
            IsSpecial = false;
        }
        public PhaseAttribute(int id, string description, bool isSpecial)
        {
            ID = id;
            Description = description;
            IsSpecial = isSpecial;
        }

        public int ID { get; private set; }
        public string Description { get; private set; }
        public bool IsSpecial { get; private set; }
    }

    public class MetadataPhase : PhaseAttribute
    {
        public MetadataPhase(int id, string description)
            : base(id, description)
        {
        }

        public MetadataPhase(int id, string description, bool isSpecial)
            : base(id, description, isSpecial)
        {
        }
    }

    public class DeobfuscationPhase : PhaseAttribute
    {
        public DeobfuscationPhase(int id, string description)
            : base(id, description)
        {
        }

        public DeobfuscationPhase(int id, string description, bool isSpecial)
            : base(id, description, isSpecial)
        {
        }
    }
}
