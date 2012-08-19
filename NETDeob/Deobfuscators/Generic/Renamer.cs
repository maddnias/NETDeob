using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;

namespace NETDeob.Core.Deobfuscators.Generic
{
    public struct RenamingScheme
    {
        public bool Namespaces, Methods, Types, Resources, Parameters, Fields, Properties, Events, Delegates;

        public RenamingScheme(bool standardVal)
        {
            Namespaces = standardVal;
            Methods = standardVal;
            Types = standardVal;
            Resources = standardVal;
            Parameters = standardVal;
            Fields = standardVal;
            Properties = standardVal;
            Events = standardVal;
            Delegates = standardVal;
        }
    }

    public class Renamer : AssemblyDeobfuscationTask
    {
        private static RenamingScheme _scheme;

        public Renamer(AssemblyDefinition asmDef, RenamingScheme scheme)
            : base(asmDef)
        {
            _scheme = scheme;
            RoutineDescription = "Symbol renaming";
        }

        public static void RenameMembers()
        {
            int tCount = 0,
                mCount = 0,
                eCount = 0,
                nCount = 0,
                rCount = 0,
                moCount = 0,
                dCounter = 0,
                frmCount = 0;

            var namespaces = new SortedList<string, List<TypeDefinition>>();
            string oldName;

            Logger.VSLog("Renaming symbols...");
            Logger.VLog("");

            foreach (var typeDef in AsmDef.Modules.SelectMany(modDef => modDef.Types))
                if (namespaces.Keys.Contains(typeDef.Namespace))
                    namespaces[typeDef.Namespace].Add(typeDef);
                else
                    namespaces.Add(typeDef.Namespace, new List<TypeDefinition> {typeDef});

            foreach (var ns in namespaces)
            {
                if (_scheme.Namespaces && !ns.Key.EndsWith("My"))
                {
                    oldName = ns.Key;

                    foreach (var typeDef in ns.Value)
                        typeDef.Namespace = "Namespace_" + nCount;

                    Logger.VLog("[Rename(Namespace)] " + oldName + " -> " + "Namespace_" + nCount);

                    nCount++;
                }
            }

            namespaces.Clear();

            foreach (var modDef in AsmDef.Modules)
            {
                modDef.Name = "Module_" + moCount++;

                foreach (
                    var typeDef in
                        modDef.GetAllTypes().Where(
                            tDef => 
                                !tDef.IsRuntimeSpecialName &&
                                !tDef.IsSpecialName &&
                                !(tDef.Name.StartsWith("<") && tDef.Name.EndsWith(">"))))
                {
                    if (typeDef.TopParentType().Namespace.EndsWith("My") || !_scheme.Types)
                        continue;

                    oldName = typeDef.Name;

                    if (typeDef.BaseType == null || typeDef.BaseType.Name != "Form")
                        typeDef.Name = "Type_" + tCount++;
                    else
                        typeDef.Name = "Form_" + frmCount++;
                    

                    Logger.VLog("[Rename(Type)] " + oldName + " -> " + typeDef.Name);
                }

                foreach (var typeDef in modDef.GetAllTypes())
                {
                    if (typeDef.Namespace.EndsWith("My") || !_scheme.Types)
                        continue;

                    if (typeDef.IsGenericInstance || typeDef.IsGenericParameter)
                        continue;

                    #region Methods

                    if (_scheme.Methods)
                        foreach (var mDef in typeDef.Methods.Where(mDef => !mDef.IsRuntimeSpecialName 
                            && !mDef.HasGenericParameters
                            && !mDef.DeclaringType.HasGenericParameters))
                        {
                            oldName = mDef.Name;
                            mDef.Name = "Method_" + mCount++;

                            Logger.VLog("[Rename(Method)] " + oldName + " -> " + mDef.Name);

                            var pCount = 0;
                            foreach (var paramDef in mDef.Parameters)
                            {
                                oldName = paramDef.Name;
                                paramDef.Name = paramDef.ParameterType.Name.ToLower() + "_" + pCount++;

                                Logger.VLog("[Rename(Parameter)] " + oldName + " -> " + paramDef.Name);
                            }
                        }




                    #endregion
                    #region Fields

                    if (_scheme.Fields )
                    {
                        var fCount = 0;
                        foreach (var fieldDef in typeDef.Fields)
                        {
                            if (fieldDef.IsRuntimeSpecialName || fieldDef.DeclaringType.HasGenericParameters)
                                continue;

                            oldName = fieldDef.Name;
                            fieldDef.Name = fieldDef.FieldType.Name.ToLower() + "_" + fCount++;

                            Logger.VLog("[Rename(Field)] " + oldName + " -> " + fieldDef.Name);
                        }
                    }

                    #endregion
                    #region Events

                    if (_scheme.Events)
                        foreach (var eventDef in typeDef.Events)
                        {
                            if (eventDef.IsRuntimeSpecialName || eventDef.IsSpecialName)
                                continue;

                            oldName = eventDef.Name;
                            eventDef.Name = "Event_" + eCount++;

                            Logger.VLog("[Rename(Event)] " + oldName + " -> " + eventDef.Name);
                        }

                    #endregion
                    #region Properties

                    if (_scheme.Properties)
                    {
                        int prCount = 0;
                        foreach (var propDef in typeDef.Properties)
                        {
                            if (propDef.IsRuntimeSpecialName || propDef.IsSpecialName)
                                continue;

                            oldName = propDef.Name;
                            propDef.Name = "prop" + propDef.PropertyType.Name + "_" + prCount++;

                            Logger.VLog("[Rename(Property)] " + oldName + " -> " + propDef.Name);
                        }
                    }

                    #endregion
                    #region Delegates

                    if (typeDef.BaseType == null)
                        continue;

                    if (_scheme.Delegates)
                        if (typeDef.BaseType.ToString().ToLower().Contains("multicastdelegate"))
                        {
                            if (typeDef.IsRuntimeSpecialName || typeDef.IsSpecialName)
                                continue;

                            oldName = typeDef.Name;
                            typeDef.Name = "delegate_" + dCounter++;

                            Logger.VLog("[Rename(Delegate)] " + oldName + " -> " + typeDef.Name);
                        }

                    #endregion
                }

                if (_scheme.Resources)
                    foreach (var res in modDef.Resources)
                    {
                        oldName = res.Name;
                        res.Name = "Resource_" + rCount++;

                        // we need to update all resourcemanager initialization strings if we rename resources
                        AsmDef.ReplaceString(oldName.Replace(".resources", null), res.Name);

                        Logger.VLog("[Rename(Resource)] " + oldName + " -> " + res.Name);
                    }

            }
        }

        [DeobfuscationPhase(1, "Renaming symbols")]
        public static bool Phase1()
        {
            RenameMembers();
            return true;
        }
    }
}
