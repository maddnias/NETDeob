using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;

namespace NETDeob.Core.Deobfuscators.Generic
{
    public class GenericResourceDumper : UnpackingTask
    {
        private Assembly _rAssembly = Assembly.LoadFile(DeobfuscatorContext.InPath);

        public GenericResourceDumper(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        [DeobfuscationPhase(1, "Locate resource resolver method")]
        public bool Phase1()
        {
            var resolverCall = FindResolverCall();

            if(resolverCall.Item1 == null || resolverCall.Item2 == null)
            {
                ThrowPhaseError("Could not locate any resource resolver", 0, true);
                return true;
            }

            Logger.VSLog("Found resolver method at " + resolverCall.Item2.Name.Truncate(10));
            var targetMethod = (resolverCall.Item2.Body.Instructions.DoUntil(resolverCall.Item1,
                                                                            i => i.OpCode == OpCodes.Ldftn, false).Operand as MethodReference).Resolve();

            if(targetMethod == null)
            {
                ThrowPhaseError("Could not locate any resource resolver", 0, true);
                return true;
            }

            PhaseParam = targetMethod;
            return true;
        }

        [DeobfuscationPhase(2, "Invoke resolver and retrieve assembly")]
        public bool Phase2()
        {
            var token = (PhaseParam as MethodDefinition).MetadataToken.ToInt32();
            var rEvent = new ResolveEventArgs("ConsoleApplication1.Properties.Resources.resources", _rAssembly);
            var asm = DynamicDecrypt<Assembly>(token, new object[] {null, rEvent});

            if (asm == null)
            {
                ThrowPhaseError("Failed to dynamically resolve resource!", 0, false);
                return false;
            }

            Logger.VLog("Successfully invoked resolver...");

            PhaseParam = asm;
            return true;
        }

        [DeobfuscationPhase(3, "Inject old resources")]
        public bool Phase3()
        {
            var asm = PhaseParam as Assembly;

            foreach(var resName in asm.GetManifestResourceNames())
            {
                var res = asm.GetManifestResourceStream(resName);
                var resBuff = new byte[res.Length];

                res.Read(resBuff, 0, resBuff.Length);

                AsmDef.MainModule.Resources.Add(new EmbeddedResource(resName, ManifestResourceAttributes.Public, resBuff));
                Logger.VLog(string.Format(@"Injected decrypted resource ""{0}""", resName.Truncate(40)));
            }

            Logger.VSLog(asm.GetManifestResourceNames().Length + " resources were injected...");

            return true;
        }

        public T DynamicDecrypt<T>(int token, object[] @params)
        {
            var method = ResolveReflectionMethod(token);
            return (T) method.Invoke(null, @params);
        }
        public MethodBase ResolveReflectionMethod(int token)
        {
            return _rAssembly.GetModules()[0].ResolveMethod(token);
        }

        public Tuple<Instruction, MethodDefinition> FindResolverCall()
        {
            foreach(var mDef in AsmDef.FindMethods(m => m.HasBody))
            {
                foreach(var instr in mDef.Body.Instructions)
                {
                    if (instr.OpCode.OperandType != OperandType.InlineMethod || instr.OpCode != OpCodes.Callvirt)
                        continue;

                    if ((instr.Operand as MethodReference).Resolve().MetadataToken.ToInt32() == typeof(AppDomain).GetMethod("add_ResourceResolve").MetadataToken)
                        return new Tuple<Instruction, MethodDefinition>(instr, mDef);
                }
            }

            return new Tuple<Instruction, MethodDefinition>(null, null);
        }
    }
}
