using System;
using NETDeob.Core.Misc;

namespace NETDeob.Core.Engine.Utils
{
    public static class Logger
    {
        //Bit of redundant code here but whatever

        public static void VLog(string message)
        {
            if (Globals.DeobContext.Output == DeobfuscatorContext.OutputType.Verbose)
                Console.WriteLine(message);
        }

        public static void SLog(string message)
        {
            if (Globals.DeobContext.Output == DeobfuscatorContext.OutputType.Subtle)
                Console.WriteLine(message);
        }

        public static void VSLog(string message)
        {
            if (Globals.DeobContext.Output == DeobfuscatorContext.OutputType.Subtle || Globals.DeobContext.Output == DeobfuscatorContext.OutputType.Verbose)
                Console.WriteLine(message);
        }
    }
}
