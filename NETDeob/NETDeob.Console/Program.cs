using System;
using Mono.Cecil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Misc;
using NETDeob.Misc;
using NETDeob._Console.Utils;

namespace NETDeob._Console
{
    public class Program
    {
        private const string Version = "0.3.0 - Private revision";

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

            TaskAssigner.AssignDeobfuscation(tmpAsm);

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
    }
}