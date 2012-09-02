using System.Collections.Generic;

namespace NETDeob._Console
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
