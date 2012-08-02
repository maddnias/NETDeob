using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NETDeob.Core.Engine.Utils.Extensions
{
    public static class MiscExt
    {
        public static object OptimizeValue(this object val)
        {
            var _val = Convert.ToInt64(val);

            if (_val >= byte.MinValue && _val <= byte.MaxValue)
                return (byte)_val;

            return _val;
        }

        public static bool SafeRefCheck<T>(this T member1, T member2) where T : MemberReference
        {
            return ((dynamic) member1).Resolve() == member2;
        }

        public static int CalcChildMembers(this object parentMember)
        {
            var outNum = 0;

            if (parentMember is TypeDefinition)
            {
                var tDef = parentMember as TypeDefinition;

                outNum += tDef.NestedTypes.Sum(nestedType => CalcChildMembers(nestedType));
                outNum += tDef.Methods.Count;
                outNum += tDef.Fields.Count;
                outNum += tDef.Events.Count;
                outNum += tDef.Properties.Sum(propDef => CalcChildMembers(propDef));
            }

            if(parentMember is PropertyDefinition)
            {
                var pDef = parentMember as PropertyDefinition;

                if (pDef.GetMethod != null)
                    outNum++;

                if (pDef.SetMethod != null)
                    outNum++;
            }

            return (outNum == 0 ? 1 : outNum);
        }
    }
}
