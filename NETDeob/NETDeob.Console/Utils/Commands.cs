using System;
using System.Collections.Generic;
using NETDeob._Console.Bases;

namespace NETDeob._Console.Utils
{
    public static class StaticCommands
    {
        public static void DisplayInvalid(string raw)
        {
            Console.WriteLine("Invalid command: {0}!\n\nType NETDeob.exe -help for more information", raw);
        }
    }

    public class CmdDeobfuscateDirectory : Command
    {
        public CmdDeobfuscateDirectory()
        {
            Name = "Deobfuscate Directory";
            Description = "Tells NETDeob to deobfuscate all available files in specified dir";
            RawCommand = "-ddir";
            Incompabilities = new List<Command>();
        }

        public override void Display(dynamic param)
        {
            throw new NotImplementedException();
        }
    }
    public class CmdVerbose : Command
    {
        public CmdVerbose()
        {
            Name = "Verbose";
            Description = "Decides wether NETDeob should output additional deobfuscation info";
            RawCommand = "-verb";
            Incompabilities = new List<Command>();
        }

        public override void Display(dynamic param)
        {
            throw new NotImplementedException();
        }
    }
    public class CmdFetchSignature : Command
    {
        public CmdFetchSignature()
        {
            Name = "Fetch signature";
            Description = "Only returns the detected protection signature if found";
            RawCommand = "-fsig";
            Incompabilities = new List<Command>();
        }

        public override void Display(dynamic param)
        {
            throw new NotImplementedException();
        }
    }
    public class CmdHelp : Command
    {
        public CmdHelp()
        {
            Name = "Help";
            Description = "Outputs information about the different commands";
            RawCommand = "-help";
            Incompabilities = new List<Command>();
        }

        public override void Display(dynamic param)
        {
            Console.WriteLine("Usage:\n\nNETDeob.exe [filename/-ddir:directory] [-commands] [-out:filename]\n\nHelp:\n");

            foreach (var cmd in param)
            {
                Console.WriteLine("{0}: {1}", cmd.RawCommand, cmd.Description);
            }
        }
    }
    public class CmdOut : Command
    {
        public CmdOut()
        {
            Name = "Out";
            Description = "Allows you to set your own output path (default: filename_deobf.exe)";
            RawCommand = "-out";
            Incompabilities = new List<Command>();
        }

        public override void Display(dynamic param)
        {
            if (param[1] != param[0].Last())
                StaticCommands.DisplayInvalid(param[1]);
        }
    }
}
