using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NETDeob.Core.Engine.Utils;

namespace NETDeob.Core.Plugins
{
    [System.ComponentModel.Composition.InheritedExport]
    public interface IPlugin
    {
        string Name { get; }
        string Author { get; }
        string Version { get; }

        void RegisterIdentifierTasks(Action<Identifier.IdentifierTask> register);
    }
}
