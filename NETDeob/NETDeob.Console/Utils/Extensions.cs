using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETDeob._Console.Utils
{
    static class Extensions
    {
        public static T[] FromIndex<T>(this T[] inArray, int index)
        {
            var outList = new List<T>();

            for (var i = index; i < inArray.Length; i++)
                outList.Add(inArray[i]);

            return outList.ToArray();
        }
    }
}
