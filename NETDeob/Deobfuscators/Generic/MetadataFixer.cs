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
            var outList = new List<object>();

            foreach (var res in AsmDef.MainModule.Resources)
            {
                try
                {
                    var resStream = (res as EmbeddedResource).GetResourceStream();
                    if (resStream.Length != 0 && res.Name.Length <= 1000) continue;

                    Console.WriteLine("Found invalid resource...");
                    outList.Add(res);
                }
                catch
                {

                    Console.WriteLine("Found invalid resource...");
                    outList.Add(res);
                }
            }

            return outList;
        }
        private static IEnumerable<object> AnalyzeSecurityDeclarations()
        {
            var outList = new List<object>();
            return outList;
        }
        private static IEnumerable<object> AnalyzeModules()
        {
            var outList = new List<object>();

            foreach (var modDef in AsmDef.Modules.Where(mod => mod != AsmDef.MainModule))
                outList.Add(modDef);

            return outList;
        }
        private static IEnumerable<object> AnalyzeTypeDefs()
        {
            var outList = new List<object>();

            foreach (var typeDef in AsmDef.MainModule.Types)
            {
                try
                {
                    for (int i = 0; i < typeDef.GenericParameters.Count; i++)
                    {
                        var gParam = typeDef.GenericParameters[i];

                        if (gParam.Attributes == (GenericParameterAttributes) 0xffff)
                            typeDef.GenericParameters.Remove(gParam);
                    }
                }
                catch {}

                if (typeDef.Name.Length >= 1000)
                    MarkMember(typeDef);
            }


            return outList;
        }
    }
}
