using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NETDeob.Core.Engine.Utils.Extensions
{
    public static class MiscExt
    {
        public static bool SafeRefCheck<T>(this T member1, T member2) where T : MemberReference
        {
            return ((dynamic) member1).Resolve() == member2;
        }
    }
}
