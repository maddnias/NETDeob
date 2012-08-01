using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Mono.Cecil;
using NETDeob.Core;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Misc;
using NETDeob.Misc;
using NETDeob._Console.Utils;

namespace NETDeob._Console
{
    public class Program
    {
        private const string Version = "0.3.0";
        private static readonly UnhandledExceptionEventHandler Handler = GlobalExcHandle;

        [STAThread]
        private static void Main(string[] args)
        {
            var parser = new ArgumentParser(args);

            if(!parser.ParseRawInput()){
                Console.ReadLine();
                Environment.Exit(-1);
            }

            ActivateCommands(parser);
            AssemblyDefinition tmpAsm = null;

            try
            {
                tmpAsm = AssemblyDefinition.ReadAssembly(args[0]);
            }
            catch
            {
                Logger.VSLog("File is not a valid .NET PE file!");
                Console.ReadLine();
                Environment.Exit(-1);
            }

            DeobfuscatorContext.OutPath = DeobfuscatorContext.InPath + "_deobf.exe";
            DeobfuscatorContext.AsmDef = tmpAsm;

            Logger.VSLog(string.Concat("NETDeob ", Version, " BETA"));
            Logger.VSLog("");

            var deob = new Deobfuscator(Handler);

            deob.Deobfuscate(tmpAsm);

            Console.Read();
        }

        private static void ActivateCommands(ArgumentParser parser)
        {
            foreach(var cmd in parser.ParsedCommands)
            {
                if (cmd is CmdVerbose)
                    DeobfuscatorContext.Output = DeobfuscatorContext.OutputType.Verbose;
                if (cmd is CmdOut)
                    DeobfuscatorContext.OutPath = cmd.UserInput.Substring(1);
            }
        }

        private static void GlobalExcHandle(object sender, UnhandledExceptionEventArgs e)
        {
            var error = e.ExceptionObject as Exception;
            var errorInformation = new StringBuilder();

            errorInformation.Append("Error message:\r\n\t" + error.Message);
            errorInformation.Append("\r\n\r\nTarget site:\r\n\t" + error.TargetSite);
            errorInformation.Append("\r\n\r\nInner Exception:\r\n\t" + error.InnerException); 
            errorInformation.Append("\r\n\r\nStack trace:\r\n\r\n");

            foreach (var obj in error.StackTrace)
                errorInformation.Append(obj.ToString());

            try
            {
                File.WriteAllText(Path.Combine(Application.StartupPath, "error.txt"),
                                  errorInformation.ToString());
            }
            catch
            {
                // Don't want to go into an endless loop haha
                Console.WriteLine("Could not write error information file!");
            }

            Console.WriteLine(
                "An exception has been thrown which was not handled!\n\nMessage:\n{0}\n\nA text file containing the error information" +
                " has been generated in NETDeob's directory, please send the information to netdeob@gmail.com",
                error.Message);
            Console.ReadLine();
        }
    }
}