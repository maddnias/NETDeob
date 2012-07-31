using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETDeob.Core.Misc;

namespace NETDeob.Core.Engine.Utils.Extensions
{
    public static class ModuleDefinitionExt
    {
        public static IEnumerable<TypeDefinition> GetAllTypes(this ModuleDefinition self)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            return self.Types.SelectMany(
                Functional.Y<TypeDefinition, IEnumerable<TypeDefinition>>(f => type => type.NestedTypes.SelectMany(f).Prepend(type)));
        }

    }
}
