using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Core.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Deobfuscators;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;

namespace NETDeob.Core.Deobfuscators.Confuser.Tasks._1_7
{
    public class BasicResolverContext : IProxyContext
    {
        public class ProxyCall
        {
            public Instruction Call;
            public MethodDefinition Source;
            public TypeDefinition AssociatedProxyType;
        }

        public Assembly RAssembly = Assembly.LoadFile(DeobfuscatorContext.InPath);
        public MethodDefinition Resolver;
        public List<TypeDefinition> ProxyTypes;
        public List<ProxyCall> ProxyCalls;

        public int Modifier;

        public override string ToString()
        {
            return "Modifier: " + Modifier;
        }
    }

    class ProxyResolver2 : AssemblyDeobfuscationTask, IProxyResolver<BasicResolverContext>
    {
        public ProxyResolver2(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "Resolve all proxy calls";
        }

        [DeobfuscationPhase(1, "Locate dynamic method resolver")]
        public bool Phase1()
        {
            var rType =
                AsmDef.MainModule.Types.First(t => t.Methods.FirstOrDefault(
                    m =>
                    m.Parameters.Count == 1 && m.Parameters[0].ParameterType.ToString().Contains("RuntimeFieldHandle")) != null);

            var resolver =
                rType.Methods.First(
                    m =>
                    m.Parameters.Count == 1 && m.Parameters[0].ParameterType.ToString().Contains("RuntimeFieldHandle"));

            if(resolver == null){
                ThrowPhaseError("Could not locate proxy resolver!", 0, true);
                return false;
            }

           // MarkMember(resolver);

            var ctx = new BasicResolverContext
                          {
                              Modifier = resolver.Body.Instructions.GetOperandAt<int>(OpCodes.Ldc_I4, 0),
                              ProxyTypes = new List<TypeDefinition>(),
                              ProxyCalls = new List<BasicResolverContext.ProxyCall>(),
                              Resolver = resolver
                          };

            Logger.VSLog("Located proxy resolver method at " + resolver.Name.Truncate(10));
            Logger.VLog(ctx.ToString()); // print XOR modifier

            PhaseParam = ctx;
            return true;
        }

        [DeobfuscationPhase(2, "Locate proxy types")]
        public bool Phase2()
        {
            var ctx = PhaseParam as BasicResolverContext;

            ctx.ProxyTypes.AddRange(YieldProxyTypes<TypeDefinition>(ctx));
            ctx.ProxyCalls.AddRange(YieldProxyCalls<BasicResolverContext.ProxyCall>(ctx));

            return true;
        }

        [DeobfuscationPhase(3, "Resolve methods")]
        public bool Phase3()
        {
            var ctx = PhaseParam as BasicResolverContext;

            foreach (var pt in ctx.ProxyTypes)
                foreach (var call in ctx.ProxyCalls.Where(c => c.AssociatedProxyType == pt))
                    ResolveMethod(call, ctx);

            return true;
        }

        public byte[] DeMangle(FieldInfo field)
        {
            if (field == null)
                return null;

            var chArray = new char[field.Name.Length];
            
            for (var i = 0; i < chArray.Length; i++)
                chArray[i] = (char)(((byte)field.Name[i]) ^ i);

            return Convert.FromBase64String(new string(chArray));
        }

        public IEnumerable<TU> YieldProxyTypes<TU>(BasicResolverContext ctx)
        {
            return AsmDef.MainModule.GetAllTypes().Where(t => BaseProxyCheck(t, ctx)).Select(type => type).Cast<TU>();
        }
        public IEnumerable<TU> YieldProxyCalls<TU>(BasicResolverContext ctx)
        {
            foreach (var mDef in AsmDef.FindMethods(m => m.HasBody))
            {
                TypeDefinition targetType;

                foreach (var instr in mDef.Body.Instructions.Where(i => i.OpCode == OpCodes.Call))
                    if (ctx.ProxyTypes.Contains(
                            (targetType = (instr.Operand as MethodReference).Resolve().DeclaringType)))
                    {
                        yield return (TU) Convert.ChangeType(new BasicResolverContext.ProxyCall
                                                                {
                                                                    AssociatedProxyType = targetType,
                                                                    Call = instr,
                                                                    Source = mDef
                                                                }, typeof (TU));
                    }
            }
        }
        public bool BaseProxyCheck(dynamic param, BasicResolverContext ctx)
        {
            return param.BaseType != null &&
                   param.BaseType.Name.Contains("MulticastDelegate") &&
                   (param as TypeDefinition).GetStaticConstructor() != null &&
                   (param as TypeDefinition).GetStaticConstructor().Body.Instructions.GetOperandAt<MethodReference>
                       (OpCodes.Call, 0).Resolve() == ctx.Resolver;
        }
        public void ResolveMethod(dynamic param, BasicResolverContext ctx)
        {
            MethodReference resolvedMethod = null;
            MethodDefinition resolvedCtor = null;
            var call = param as BasicResolverContext.ProxyCall;

            var resolvedField = ctx.RAssembly.Resolve<FieldInfo>(call.AssociatedProxyType.Fields[0].MetadataToken);

            try
            {
                resolvedMethod = AsmDef.MainModule.LookupToken(BitConverter.ToInt32(DeMangle(resolvedField), 1) ^
                                                                ctx.Modifier) as MethodReference;
            } catch { }

            try
            {
                resolvedCtor = AsmDef.MainModule.LookupToken(BitConverter.ToInt32(DeMangle(resolvedField), 0) ^
                                                             ctx.Modifier) as MethodDefinition;
            } catch { }

            if (resolvedCtor == null && resolvedMethod == null)
                ThrowPhaseError("Failed to resolve method/constructor!", 1, false);

            var ilProc = call.Source.Body.GetILProcessor();

            if (resolvedMethod != null)
                ilProc.Replace(call.Call, ilProc.Create(OpCodes.Call, resolvedMethod));
            else if (resolvedCtor != null)
                ilProc.Replace(call.Call, ilProc.Create(OpCodes.Newobj, resolvedCtor));

           // MarkMember(call.AssociatedProxyType);
        }
    }
}
