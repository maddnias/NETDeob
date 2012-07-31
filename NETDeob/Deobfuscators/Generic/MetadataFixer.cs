using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using GenericParameterAttributes = Mono.Cecil.GenericParameterAttributes;

namespace NETDeob.Core.Deobfuscators.Generic
{
    public class MetadataFixer : AssemblyDeobfuscationTask
    {
        public MetadataFixer(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "Find and remove invalid metadata";
        }

        [DeobfuscationPhase(1, "Analyze metadata")]
        public bool Phase1()
        {
            var invalidMembers = new List<object>();

            invalidMembers.AddRange(AnalyzeAssemblyReferences());
            invalidMembers.AddRange(AnalyzeResources());
            invalidMembers.AddRange(AnalyzeSecurityDeclarations());
            invalidMembers.AddRange(AnalyzeModules());
            invalidMembers.AddRange(AnalyzeTypeDefs());

            if (AsmDef.FullName.Split(',')[0] == "")
                AsmDef.Name = new AssemblyNameDefinition("deobf", new Version(1, 0, 0, 0));

            PhaseParam = invalidMembers;
            return true;
        }

        [DeobfuscationPhase(2, "Mark invalid members for removal")]
        public bool Phase2()
        {
            foreach (var member in PhaseParam)
                MarkMember(member);

            return true;
        }

        //[DeobfuscationPhase(3, "Re-load cleaned assembly")]
        //public bool Phase3()
        //{
        //    AsmDef.Write(DeobfuscatorContext.InPath + "_mdfix.exe");
        //    DeobfuscatorContext.InPath = DeobfuscatorContext.InPath + "_mdfix.exe";
        //    DeobfuscatorContext.AsmDef = AssemblyDefinition.ReadAssembly(DeobfuscatorContext.InPath);

        //    return true;
        //}

        private static IEnumerable<object> AnalyzeAssemblyReferences()
        {
            var outList = new List<object>();

            foreach(var asmRef in AsmDef.MainModule.AssemblyReferences)
            {
                try
                {
                    Assembly.Load(asmRef.FullName);
                }
                catch (FileLoadException e)
                {
                    if (asmRef.Name.Length <= 1)
                    {
                        Logger.VSLog(string.Format("Found invalid assembly reference with MDTok: {0}...",
                                                   asmRef.MetadataToken.ToInt32()));
                        outList.Add(asmRef);
                    }
                }
            }

            return outList;
        }
        private static IEnumerable<object> AnalyzeResources()
        {
            foreach (var res in AsmDef.MainModule.Resources)
            {
                var resStream = (res as EmbeddedResource).GetResourceStream();
                if (resStream.Length != 0 && res.Name.Length <= 1000) continue;

                Console.WriteLine("Found invalid resource...");
                yield return res;
            }
        }

        private static IEnumerable<object> AnalyzeSecurityDeclarations()
        {
            yield return null;
        }
        private static IEnumerable<object> AnalyzeModules()
        {
            return AsmDef.Modules.Where(mod => mod != AsmDef.MainModule);
        }

        private static IEnumerable<object> AnalyzeTypeDefs()
        {
            foreach (var typeDef in AsmDef.MainModule.Types)
            {
                try
                {
                    foreach (var gParam in typeDef.GenericParameters)
                        if (gParam.Attributes == (GenericParameterAttributes)0xffff)
                            MarkMember(gParam);
                }
                catch {}

                if (typeDef.Name.Length >= 1000)
                    yield return typeDef;
            }
        }
    }
}
