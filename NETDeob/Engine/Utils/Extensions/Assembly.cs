using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace NETDeob.Core.Engine.Utils.Extensions
{
    public static class AssemblyExt
    {
        public static T Resolve<T>(this Assembly asm, MetadataToken token, object param = null) where T : MemberInfo
        {
            var mModule = asm.GetModules()[0];

            if (typeof(T) == typeof(FieldInfo))
                try
                {
                    return FieldInfo.GetFieldFromHandle(mModule.ResolveField(token.ToInt32()).FieldHandle) as T;
                }
                catch
                {
                    return default(T);
                }


            return default(T);
        }
    }
}
