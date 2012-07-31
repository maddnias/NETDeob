using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NETDeob.Core.Engine.Utils.Extensions
{
    public static class TypeDefinitionExt
    {
        public static IEnumerable<MethodDefinition> GetConstructors(this TypeDefinition typeDef)
        {
            return typeDef.Methods.Where(m => m.IsConstructor);
        }

        public static MethodDefinition GetStaticConstructor(this TypeDefinition typeDef)
        {
            return typeDef.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);
        }
    }
}
