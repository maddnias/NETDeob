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
                                           string.Format(@"d ""{0}"" ""{1}""", file, Globals.DeobContext.OutPath),
                                       RedirectStandardOutput = true,
                                       UseShellExecute = false,
                                       CreateNoWindow = false
                                   }
                           };

            proc.Start();
            proc.WaitForExit();

            if (!File.Exists(Globals.DeobContext.OutPath))
            {
                Logger.VSLog("Failed to decompress data!");
                return false;
            }

            return true;
        }
    }
}
