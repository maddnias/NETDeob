using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETDeob.Core.Engine.Utils.Extensions
{
    static class Functional
    {
        public static Func<A, R> Y<A, R>(Func<Func<A, R>, Func<A, R>> f)
        {
            Func<A, R> g = null;
            g = f(a => g(a));
            return g;
        }

        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return PrependIterator(source, element);
        }

        static IEnumerable<TSource> PrependIterator<TSource>(IEnumerable<TSource> source, TSource element)
        {
            yield return element;

            foreach (var item in source)
                yield return item;
        }
    }
}
