using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETDeob.Core.Engine.Utils.Extensions
{
    public static class StringExt
    {
        public static string Truncate(this string str, int count)
        {
            return (str.Length > count ? str.Substring(0, count) + "..." : str);
        }
    }
}
