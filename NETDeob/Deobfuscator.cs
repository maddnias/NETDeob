using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Misc;
using NETDeob.Core.Plugins;

namespace NETDeob.Core
{
    public class Deobfuscator
    {
        //TODO: Add more overloads for Deobfuscate, hopefully more advanced parameters for deobfuscation in the future

        private DeobfuscatorOptions _options = DeobfuscatorContext.Options;
        private List<string> _registeredPlugins = new List<string>();
        

        //public Deobfuscator(DeobfuscatorOptions options)
        //{
        //    _options = options;

        //    // To keep NETDeob from crashing when an exception occurs
        //    if(options.UnhandledExceptionHandler != null)
        //        AppDomain.CurrentDomain.UnhandledException += options.UnhandledExceptionHandler;
        //}

        public void Deobfuscate(DynamicStringDecryptionContetx strCtx = null)
        {
            if (DeobfuscatorContext.Options.UnhandledExceptionHandler != null)
                AppDomain.CurrentDomain.UnhandledException += DeobfuscatorContext.Options.UnhandledExceptionHandler;

            LoadPlugins();
            DeobfuscatorContext.DynStringCtx = strCtx;
            TaskAssigner.AssignDeobfuscation(DeobfuscatorContext.AsmDef);
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
            var sig = Identifier.Identify(DeobfuscatorContext.AsmDef);
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
