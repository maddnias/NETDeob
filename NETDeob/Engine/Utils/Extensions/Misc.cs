using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Mono.Cecil;
using NETDeob.Core.Deobfuscators.Generic;

namespace NETDeob.Core.Engine.Utils.Extensions
{
    public static class MiscExt
    {
        public static bool VerifyTop(this Stack<StackTracer.StackEntry> stack)
        {
            var holder = stack.Pop();

            if (holder.IsValueKnown && stack.Peek().IsValueKnown)
            {
                stack.Push(holder);
                return true;
            }

            stack.Push(holder);
            return false;
        }

        public static bool VerifyTop<T>(this Stack<StackTracer.StackEntry> stack)
        {
            var holder = stack.Pop();

            if (stack.VerifyTop())
            {
                if (holder.Value.GetType().CanCastTo<T>(holder.Value) &&
                    stack.Peek().Value.GetType().CanCastTo<T>(stack.Peek().Value))
                {
                    stack.Push(holder);
                    return true;
                }
            }

            stack.Push(holder);
            return false;
        }
        //ugly
        public static bool IsNumeric(this object val)
        {
            return val is int || val is long || val is sbyte || val is short || val is ushort || val is ulong || val is uint || val is byte || val is double || val is decimal || val is float;
        }

        public static bool CanCastTo<T>(this Type from, object val)
        {
            try
            {
                Convert.ChangeType(val, typeof(T));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static object OptimizeValue(this object val)
        {
            if (val is int || val is long)
            {
                var _val = Convert.ToInt64(val);

                if (_val >= byte.MinValue && _val <= byte.MaxValue)
                    return (byte)_val;
                if (_val > byte.MaxValue && _val <= Int32.MaxValue)
                    return (int)_val;

                return _val;
            }

            return val;
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
