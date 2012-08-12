using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NETDeob.Core.Plugins;

namespace NETDeob.Core
{
    public class PluginLoader
    {
        private string _path;

        internal PluginLoader(string path)
        {
            _path = path;
        }

        public List<IPlugin> GetPlugins()
        {
            try
            {
                var pluginPathCatalog = new DirectoryCatalog(_path);
                var container = new CompositionContainer(pluginPathCatalog);
                var plugins = container.GetExportedValues<IPlugin>().ToList();

                Engine.Utils.Logger.VSLog(string.Format("Plugins: Found {0} plugins", plugins.Count));
                return plugins;
            }
            catch
            {
                Engine.Utils.Logger.VSLog(
                    "Warning: Error occured in plugin load (invalid plugin load path?), disabling plugin support for this run...");
                return new List<IPlugin>();
            }
        }
    }
}
