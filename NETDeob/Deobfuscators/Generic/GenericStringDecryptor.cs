using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using Ctx = NETDeob.Core.Misc.DeobfuscatorContext;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using ROpCodes = System.Reflection.Emit.OpCodes;

namespace NETDeob.Core.Deobfuscators.Generic
{
    public class Decryptor
    {
        public MethodDefinition Method;
        public int ParameterCount;
        public bool Cast;
    }

    public class GenericDecryptionContext : DecryptionContext
    {
        public Instruction Call;
        public List<Instruction> BadInstructions = new List<Instruction>();
        public MethodDefinition Source;
        public Decryptor Target;
        public int AssociatedToken;

        public object Param;

        public IEnumerable<object> FetchParameters()
        {
            var st = new StackTracer(Source.Body, 1000);
            BadInstructions = st.TraceCall(Call).ToList();

            return st.Stack.Select(entry => entry.Value.OptimizeValue());
        }

        //private IEnumerable<object> FetchParametersInternal()
        //{
        //    foreach (var instr in BadInstructions)
        //        if (instr.IsLdcI4())
        //            yield return instr.GetLdcI4().OptimizeValue();
        //        else if (instr.IsLdcI8WOperand())
        //            yield return instr.GetLdcI8().OptimizeValue();
        //        else if (instr.OpCode == OpCodes.Ldstr)
        //            yield return instr.Operand as string;
        //}

        //private IEnumerable<object> FetchParametersWithStackTracer()
        //{
        //    Logger.VSLog("");
        //    Logger.VSLog("StackTrace: " + Source.FullName);

        //    var tracer = new StackTracer(Source.Body);
        //    tracer.TraceUntil(BadInstructions[0]);

        //    // have to reverse the stack to pass parameters correctly
        //    var reverseStack = new Stack<StackTracer.StackEntry>();
        //    for (var i = 0; i < Target.ParameterCount; i++)
        //        reverseStack.Push(tracer.Stack.Pop());
        //    for (var i = 0; i < Target.ParameterCount; i++)
        //        yield return reverseStack.Pop().Value;
        //}

        public override string ToString()
        {
            return string.Format(@"[Decrypt] ({0}) -> ""{1}""",
                                 FetchParameters().Aggregate("", (current, param) => current + param.ToString() + ", ").TrimEnd(new[] { ',', ' '}),
                                 PlainText);
        }
    }

    public class GenericStringDecryptor : AssemblyDeobfuscationTask, IStringDecryptor<GenericDecryptionContext>
    {
        private static Assembly _rAssembly = Assembly.LoadFile(Ctx.InPath);

        public GenericStringDecryptor(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        [DeobfuscationPhase(1, "Locate decryption method(s)")]
        public bool Phase1()
        {
            var decMethods = YieldDecryptionMethods().ToList();

            if (decMethods.Count == 0)
            {
                ThrowPhaseError("Could not locate any decryptor method!", 1, false);
                return false;
            }

            decMethods.ForEach(dm => Logger.VSLog("Found decryptor method at " + dm.Method.Name));
            Logger.VLog(string.Format("Detected {0} parameters: ({1})", decMethods[0].ParameterCount,
                                      decMethods[0].Method.Parameters.Aggregate("",
                                                                                (current, param) =>
                                                                                current + param.ParameterType.ToString() + ", ").
                                          TrimEnd(new[] {',', ' '})));

            PhaseParam = decMethods;
            return true;
        }

        [DeobfuscationPhase(2, "Analyze assembly for strings")]
        public bool Phase2()
        {
            var decMethods = PhaseParam as List<Decryptor>;
            var calls = YieldDecryptionCalls(decMethods).ToList();

            foreach (var call in calls)
                Logger.VLog(string.Format("Call from {0} -> {1}", call.Item2.Name,
                                          (call.Item1.Operand as MethodReference).Resolve().Name));

            Logger.VSLog(string.Format("Found {0} references to decryption methods...", calls.Count));

            PhaseParam = new object[] { decMethods, calls };
            return true;
        }

        [DeobfuscationPhase(3, "Construct decryption entries")]
        public bool Phase3()
        {
            var decMethods = PhaseParam[0] as List<Decryptor>;
            var calls = PhaseParam[1] as List<Tuple<Instruction, MethodDefinition>>;

            var ctxList = ConstructEntries(new object[] { decMethods, calls }).ToList();
            Logger.VSLog(string.Format("Constructed {0} decryption entries", ctxList.Count));

            PhaseParam = ctxList;
            return true;
        }

        [DeobfuscationPhase(4, "Decrypt entries")]
        public bool Phase4()
        {
            var ctxList = PhaseParam as List<GenericDecryptionContext>;

            for (int i = 0; i < ctxList.Count; i++)
            {
                var ctx = ctxList[i];
                DecryptEntry(ref ctx);

                Logger.VLog(ctx.ToString());
            }

            Logger.VSLog(string.Format("Decrypted {0} strings...", ctxList.Count));

            PhaseParam = ctxList;
            return true;
        }

        [DeobfuscationPhase(5, "Process entries")]
        public bool Phase5()
        {
            var ctxList = PhaseParam as List<GenericDecryptionContext>;

            foreach (var ctx in ctxList)
                ProcessEntry(ctx);

            return true;
        }

        public bool BaseIsDecryptor(params object[] param)
        {
            var mDef = param[0] as MethodDefinition;
            return Ctx.DynStringCtx.AssociatedTokens.Any(token => mDef.MetadataToken.ToInt32() == Ctx.DynStringCtx.AssociatedTokens[0]);
        }
        public void InitializeDecryption(object param)
        {
            throw new NotImplementedException();
        }
        public void DecryptEntry(ref GenericDecryptionContext entry)
        {
            entry.PlainText = DynamicDecrypt(entry.AssociatedToken, entry);
        }
        public void ProcessEntry(GenericDecryptionContext entry)
        {
            var ilProc = entry.Source.Body.GetILProcessor();

            ilProc.InsertBefore(entry.BadInstructions[0], ilProc.Create(OpCodes.Ldstr, entry.PlainText));

            for (var i = 0; i < entry.BadInstructions.Count; i++)
                MarkMember(entry.BadInstructions[i], entry.Source);
        }
        public IEnumerable<GenericDecryptionContext> ConstructEntries(object param)
        {
            var decMethods = (param as object[])[0] as List<Decryptor>;
            var calls = (param as object[])[1] as List<Tuple<Instruction, MethodDefinition>>;

            return calls.Select(call => new GenericDecryptionContext
                                            {
                                                Call = call.Item1,
                                                //BadInstructions =
                                                //    call.Item2.Body.Instructions.SliceBlock(call.Item1,
                                                //                                            decMethods.First(
                                                //                                                dm =>
                                                //                                                dm.Method == (call.Item1.Operand as MethodReference).Resolve()).
                                                //                                                Method.Parameters.Count).ToList(),
                                                Source = call.Item2,
                                                Target =
                                                    decMethods.First(
                                                        dm =>
                                                        dm.Method == (call.Item1.Operand as MethodReference).Resolve()),
                                                AssociatedToken = (call.Item1.Operand as MethodReference).Resolve().MetadataToken.ToInt32()
                                            });
        }

        public string DynamicDecrypt(int token, GenericDecryptionContext ctx)
        {
            var method = ResolveReflectionMethod(token);
            var parameters = ctx.FetchParameters().Reverse();
            return (string)method.Invoke(null, parameters.ToArray());
        }
        public MethodBase ResolveReflectionMethod(int token)
        {
            return _rAssembly.GetModules()[0].ResolveMethod(token);
        }
        public IEnumerable<Decryptor> YieldDecryptionMethods()
        {
            return AsmDef.FindMethods(m => true).Where(mDef => BaseIsDecryptor(mDef)).Select(mDef => new Decryptor
            {
                Method = mDef,
                ParameterCount = mDef.Parameters.Count,
                Cast = false
            });
        }
        public IEnumerable<Tuple<Instruction, MethodDefinition>> YieldDecryptionCalls(List<Decryptor> decryptors)
        {
            return decryptors.SelectMany(decryptor => decryptor.Method.FindAllReferences(AsmDef.MainModule));
        }
    }
}
