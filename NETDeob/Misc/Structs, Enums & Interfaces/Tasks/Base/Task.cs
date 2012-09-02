using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;

namespace NETDeob.Misc.Structs__Enums___Interfaces.Tasks.Base
{
    public class Phase
    {
        public uint Id;
        public object Param;
        public MethodInfo Method;
        public string Description;
        public bool IsSpecial;
    }
    public struct PhaseError
    {
        public enum ErrorLevel
        {
            Minor = 0,
            Critical = 1
        }

        public string Message;
        public ErrorLevel Level;

        public string ToString(uint id)
        {
            return string.Format("[{0} error @ phase {1}] {2}", Level, id, Message);
        }
    }

    public abstract class Task
    {
        public static int MarkedMemberCount { get { return Globals.DeobContext.MarkedMembers.Count; } }

        public static AssemblyDefinition AsmDef;
        public object CurrentInstance;
        public string RoutineDescription;

        public static bool EmergencyCancel;

        public static dynamic PhaseParam;
        public static PhaseError PhaseError = new PhaseError { Level = PhaseError.ErrorLevel.Minor, Message = "None" };

        private List<Phase> _phases;

        protected Task(AssemblyDefinition asmDef)
        {
            AsmDef = asmDef;
            _phases = new List<Phase>();

            foreach (var mInf in GetType().GetMethods())
            {
                if (mInf.GetCustomAttributes(false).Length == 0)
                    continue;

                var inf = mInf;
                foreach (var method in mInf.GetCustomAttributes(false).OfType<DeobfuscationPhase>().Select(attrib => inf))
                {
                    var tmpPhase = new Phase
                    {
                        Method = method,
                        Id = (uint)(method.GetCustomAttributes(false)[0] as DeobfuscationPhase).ID,
                        Description =
                            (method.GetCustomAttributes(false)[0] as DeobfuscationPhase).Description,
                        IsSpecial =
                            (method.GetCustomAttributes(false)[0] as DeobfuscationPhase).IsSpecial,
                        Param = null
                    };

                    _phases.Add(tmpPhase);
                }
            }

            _phases = _phases.OrderBy(phase => (phase.Method.GetCustomAttributes(false)[0] as DeobfuscationPhase).ID).ToList();
        }

        public void PerformTask()
        {
            foreach (var phase in _phases)
            {
                if (EmergencyCancel)
                {
                    EmergencyCancel = false;
                    Logger.VSLog(PhaseError.ToString(phase.Id));
                    break;
                }

                Logger.VSLog(string.Format("\nPhase {0}: {1}", phase.Id, phase.Description));

                if (!(bool)phase.Method.Invoke(this, new object[] { }))
                    if (PhaseError.Level == PhaseError.ErrorLevel.Critical)
                        throw new Exception(PhaseError.ToString(phase.Id));
                    else
                    {
                        Logger.VSLog(PhaseError.ToString(phase.Id));
                        break;
                    }
            }

            if (EmergencyCancel)
                Logger.VSLog(PhaseError.ToString(_phases[0].Id));

            EmergencyCancel = false;
        }
        // For inlining
        public static T MarkMember<T>(object member, object parentMember = null, string ID = null)
        {
            MarkMember(member, parentMember, ID);
            return (T)member;
        }
        public static void MarkMember(object member, object parentMember = null, string ID = null)
        {
            if (member == null)
                return;

            if (Globals.DeobContext.MarkedMembers.Any(mm => mm.Member == member))
                return;

            var tmpMember = new MarkedMember { Member = member };

            if (member is TypeDefinition)
                tmpMember.Type = MemberType.Type;
            else if (member is MethodDefinition)
                tmpMember.Type = MemberType.Method;
            else if (member is FieldDefinition)
                tmpMember.Type = MemberType.Field;
            else if (member is Delegate)
                tmpMember.Type = MemberType.Delegate;
            else if (member is PropertyDefinition)
                tmpMember.Type = MemberType.Property;
            else if (member is EmbeddedResource)
                tmpMember.Type = MemberType.Resource;
            else if (member is CustomAttribute)
                tmpMember.Type = MemberType.Attribute;
            else if (member is Instruction)
                tmpMember.Type = MemberType.Instruction;
            else if (member is AssemblyNameReference)
                tmpMember.Type = MemberType.AssemblyReference;

            tmpMember.ID = ID ?? "";
            tmpMember.ParentMember = parentMember;
            Globals.DeobContext.MarkedMembers.Add(tmpMember);
        }
        public static void RemoveMark(string ID)
        {
            Globals.DeobContext.MarkedMembers.Remove(
                Globals.DeobContext.MarkedMembers.FirstOrDefault(m => m.ID == ID));
        }
        public static void ThrowPhaseError(string message, int level, bool emergency)
        {
            PhaseError = new PhaseError
                             {
                                 Message = message,
                                 Level = (PhaseError.ErrorLevel) level
                             };

            EmergencyCancel = emergency;
        }
    }
}
