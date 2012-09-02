using System;
using System.Globalization;
using System.IO;
using Mono.Cecil;
using NETDeob.Core;
using NETDeob.Core.Misc;
using NETPack.Console;

namespace NETDeob._Console
{
    public class Program
    {
        private const string Version = "0.3.1";

        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length == 0)
                return;

            var ctx = new DeobfuscatorContext();
            var optionSet = new OptionSet()
                                {
                                    {"st=|strtok=", t =>
                                                            {
                                                                if (t != null)
                                                                   ctx.DynStringCtx = new DeobfuscatorContext.DynamicStringDecryptionContext
                                                                                                           {
                                                                                                               AssociatedToken = Int32.Parse(t, NumberStyles.HexNumber),
                                                                                                               DecryptionType = DeobfuscatorContext.StringDecryption.Dynamic
                                                                                                           };
                                                            }},
                                     {"v|verbose", v =>
                                                        {
                                                           if (v != null)
                                                               ctx.Output = DeobfuscatorContext.OutputType.Verbose;
                                                         }},
                                     { "d|debug", d =>
                                                     {
                                                          if(d != null)
                                                             ctx.Debug = true;
                                                     }},

                                     {"fsig|fetchsignature", fs =>
                                                                {
                                                                    if(fs != null)
                                                                    {
                                                                        Console.WriteLine(new Deobfuscator(ctx).FetchSignature());
                                                                        Console.ReadLine();
                                                                        Environment.Exit(-1);
                                                                    }
                                                                }},
                                     {"o=|output=", o =>
                                                                {
                                                                    if (o != null)
                                                                        ctx.OutPath = o;
                                                                }},
                                     {"pp=|pluginpath=", pp =>
                                                                {
                                                                    if(pp != null)
                                                                    {
                                                                        ctx.Options = new DeobfuscatorOptions();
                                                                        ctx.Options.LoadPlugins = true;
                                                                        ctx.Options.PluginLoadPath = pp;
                                                                    }
                                                                }},
                                     {"prp|preferplugins", prp =>
                                                                 {
                                                                     if(prp != null)
                                                                     {
                                                                         ctx.Options.LoadPlugins = true;
                                                                         ctx.Options.PreferPluginsOverBuiltinIdentifiers = true;
                                                                     }
                                                                 }}
                                };

            AssemblyDefinition tmpAsm = null;

            try
            {
                tmpAsm = AssemblyDefinition.ReadAssembly((ctx.InPath = args[0]));
            }
            catch
            {
                Console.WriteLine("File is not a valid .NET PE file!");
                Console.ReadLine();
                Environment.Exit(-1);
            }

            ctx.AsmDef = tmpAsm;

            Console.WriteLine(string.Concat("NETDeob ", Version, " BETA"));
            Console.WriteLine();

            ctx.OutPath = ctx.OutPath ?? Path.Combine(Path.GetDirectoryName(ctx.InPath), tmpAsm.Name.Name + "_deobf.exe");

            try
            {
                optionSet.Parse(args.FromIndex(1));
            }
            catch (OptionException e)
            {
                Console.WriteLine("Invalid parameters supplied!");
                Console.ReadLine();
                Environment.Exit(-1);
            }

            var deob = new Deobfuscator(ctx);
            deob.Deobfuscate();

            Console.Read();
        }
    }
}