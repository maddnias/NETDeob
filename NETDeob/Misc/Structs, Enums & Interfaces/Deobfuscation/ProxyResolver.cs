using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETDeob.Core.Misc.Structs__Enums___Interfaces.Deobfuscation
{
    public interface IProxyContext {}
    public interface IProxyResolver<in T> where T : IProxyContext
    {
        IEnumerable<TU> YieldProxyTypes<TU>(T ctx);
        IEnumerable<TU> YieldProxyCalls<TU>(T ctx);
        bool BaseProxyCheck(dynamic param, T ctx);
        void ResolveMethod(dynamic param, T ctx);
    }
}
