using System;
using System.Collections.Generic;
using System.Globalization;
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
            bool autoDeob;
            int token;

            if(!parser.ParseRawInput()){
                Console.ReadLine();
                Environment.Exit(-1);
            }

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

            DeobfuscatorContext.AsmDef = tmpAsm;

            Logger.VSLog(string.Concat("NETDeob ", Version, " BETA"));
            Logger.VSLog("");

            DeobfuscatorContext.OutPath = DeobfuscatorContext.InPath + "_deobf.exe";
            ActivateCommands(parser, out autoDeob, out token);

            DeobfuscatorContext.Options.UnhandledExceptionHandler = GlobalExcHandle;


            if (autoDeob)
            {
                var deob = new Deobfuscator();
                deob.Deobfuscate(token == 0
                                     ? null
                                     : new DynamicStringDecryptionContetx
                                           {
                                               AssociatedTokens = new List<int> { token },
                                               DecryptionType = StringDecryption.Dynamic
                                           });
            }

            Console.Read();
        }

        private static void ActivateCommands(ArgumentParser parser, out bool autoDeob, out int token)
        {
            autoDeob = true;
            token = 0;

            foreach (var cmd in parser.ParsedCommands)
            {
                if (cmd is CmdVerbose)
                    DeobfuscatorContext.Output = DeobfuscatorContext.OutputType.Verbose;
                if (cmd is CmdOut)
                    DeobfuscatorContext.OutPath = cmd.UserInput.Substring(1);
                if (cmd is CmdFetchSignature)
                {
                    autoDeob = false;
                    Logger.VSLog(new Deobfuscator().FetchSignature());
                }
                if (cmd is CmdDynamicStringDecryption)
                    token = Int32.Parse(cmd.UserInput.Split(':')[1].Trim(), NumberStyles.HexNumber);
                if (cmd is CmdDebug)
                    DeobfuscatorContext.Debug = true;
                if (cmd is CmdPluginPath)
                {
                    DeobfuscatorContext.Options.LoadPlugins = true;
                    DeobfuscatorContext.Options.PluginLoadPath = cmd.UserInput.Substring(9);
                }
                if (cmd is CmdPreferPlugins)
                {
                    DeobfuscatorContext.Options.PreferPluginsOverBuiltinIdentifiers = true;
                    DeobfuscatorContext.Options.LoadPlugins = true;
                }
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