using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using NETDeob.Core.Misc;
using NETDeob.Misc;

namespace NETDeob.Core.Engine.Utils
{
    public static class LzmatWrapper
    {
        public static bool Decompress(string file)
        {
            var proc = new Process
                           {
                               StartInfo =
                                   {
                                       FileName = Path.Combine(Application.StartupPath, "test_lzmat.exe"),
                                       Arguments =
                                           string.Format(@"d ""{0}"" ""{1}""", file, DeobfuscatorContext.OutPath),
                                       RedirectStandardOutput = true,
                                       UseShellExecute = false,
                                       CreateNoWindow = false
                                   }
                           };

            proc.Start();
            proc.WaitForExit();

            if (!File.Exists(DeobfuscatorContext.OutPath))
            {
                Logger.VSLog("Failed to decompress data!");
                return false;
            }

            return true;
        }
    }
}
