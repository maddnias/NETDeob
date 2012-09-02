using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Core.Misc.Structs__Enums___Interfaces;
using NETDeob.Core.Plugins;

namespace NETDeob.Core
{
    public class Deobfuscator
    {
        //TODO: Add more overloads for Deobfuscate, hopefully more advanced parameters for deobfuscation in the future

        private DeobfuscatorOptions _options;
        private List<string> _registeredPlugins = new List<string>();
        
        public Deobfuscator(DeobfuscatorContext context)
        {
            Globals.DeobContext = context;

            Globals.Bugster = new BugReporter("150fa190dbb7a61815b4103fee172550", new NETDeobExceptionFormatter());
            AppDomain.CurrentDomain.UnhandledException += Globals.Bugster.UnhandledExceptionHandler;
            Globals.Bugster.ReportCompleted += (o, e) =>
                                                   {
                                                       if (e.WasSuccesful)
                                                       {
                                                           Console.WriteLine(
                                                               "\nAn unhandled exception have occured and caused NETDeob to terminate!\n\nAn automatic report have been sent to the author.");
                                                           Console.ReadLine();
                                                           Environment.Exit(-1);
                                                       }
                                                       else
                                                       {
                                                           Console.WriteLine("Contact author!");
                                                           Console.ReadLine();
                                                           Environment.Exit(-1);
                                                       }
                                                   };
            
            _options = Globals.DeobContext.Options = Globals.DeobContext.Options ?? new DeobfuscatorOptions();
        }

        public void Deobfuscate()
        {
            if (Globals.DeobContext.Options.UnhandledExceptionHandler != null)
                AppDomain.CurrentDomain.UnhandledException += Globals.DeobContext.Options.UnhandledExceptionHandler;

            LoadPlugins();
            TaskAssigner.AssignDeobfuscation(Globals.DeobContext.AsmDef);
        }

        public void Deobfuscate(AssemblyDefinition[] asmDefs)
        {
            LoadPlugins();
            foreach (var asmDef in asmDefs)
                TaskAssigner.AssignDeobfuscation(asmDef);
        }

        public string FetchSignature()
        {
            LoadPlugins();
            var sig = Identifier.Identify(Globals.DeobContext.AsmDef);
            return string.Format("Name: {0}\r\nVersion: {1}", sig.Name, sig.Ver);
        }

        private void LoadPlugins()
        {
            if (_options.LoadPlugins)
            {
                var loader = new PluginLoader(_options.PluginLoadPath);
                var plugins = loader.GetPlugins();
                foreach (var plugin in plugins)
                {
                    // prevent same identifiers from being registered multiple times
                    if (_registeredPlugins.Contains(BuildPluginUID(plugin))) continue;

                    Identifier.RegisterPlugin(plugin, _options.PreferPluginsOverBuiltinIdentifiers);
                    _registeredPlugins.Add(BuildPluginUID(plugin));
                }
            }
        }

        private string BuildPluginUID(IPlugin plugin)
        {
            return string.Format("{0}-v{1}-{2}", plugin.Name, plugin.Version, plugin.Author);
        }
    }

    public class DeobfuscatorOptions
    {
        public UnhandledExceptionEventHandler UnhandledExceptionHandler;

        public bool LoadPlugins = false;
        public string PluginLoadPath = "plugins";
        /// <summary>
        /// Should plugins be preferred over built-in deobfuscator modules in case of multipler identifiers matching the assembly
        /// </summary>
        public bool PreferPluginsOverBuiltinIdentifiers = true;
    }
}
