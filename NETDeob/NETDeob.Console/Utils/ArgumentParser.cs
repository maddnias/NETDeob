using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Misc;
using NETDeob.Misc;
using NETDeob._Console.Bases;

namespace NETDeob._Console.Utils
{
    class ArgumentParser
    {
        private List<Command> _commands = new List<Command>
                                             {
                                                 new CmdVerbose(),
                                                 new CmdHelp(),
                                                 new CmdOut(),
                                                 new CmdFetchSignature()
                                             };

        public List<Command> ParsedCommands { get; private set; }

        private string[] _rawInput;

        public ArgumentParser(string[] rawInput)
        {
            if(rawInput.Length == 0)
            {
                new CmdHelp().Display(_commands);
                Console.ReadLine();
                Environment.Exit(-1);
            }

            if(!File.Exists(rawInput[0])){
                Console.WriteLine("Could not find file {0}!", rawInput[0]);
                Console.ReadLine();
                Environment.Exit(-1);
            }

            DeobfuscatorContext.InPath = rawInput[0];

            _rawInput = rawInput.FromIndex(1);
            ParsedCommands = new List<Command>();
        }

        public bool ParseRawInput()
        {
            foreach(var raw in _rawInput)
            {
                var cmd = _commands.FirstOrDefault(c => c.RawCommand.Substring(0, 4) == raw.ToLower().Substring(0, 4));

                if (cmd == null){
                    StaticCommands.DisplayInvalid(raw);
                    return false;
                }

                if (cmd is CmdHelp)
                {
                    (cmd as CmdHelp).Display(_commands);
                    return false;
                }

                if (cmd is CmdOut)
                {
                    if (raw != _rawInput.Last())
                    {
                        (cmd as CmdOut).Display(new object[] {_rawInput, raw});
                        return false;
                    }
                }

                cmd.UserInput = raw.Substring(4);
                ParsedCommands.Add(cmd);
            }

            return true;
        }

    }
}
