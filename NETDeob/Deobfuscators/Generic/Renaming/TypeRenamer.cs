using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NETDeob.Core.Deobfuscators.Generic.Renaming
{
    class TypeRenamer : IMemberRenamer<TypeDefinition>
    {
        public TypeRenamer(TypeDefinition member)
            : base(member)
        {
        }

        public override int NameIdx
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override Dictionary<int, Tuple<string, string>> StandardNames
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override void Rename()
        {
            throw new NotImplementedException();
        }
    }
}
