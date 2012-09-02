using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;

namespace NETDeob.Deobfuscators.Confuser.Tasks._1_7
{
    public class ProxyResolver : AssemblyDeobfuscationTask
    {
        private static List<Instruction> lstCallInstructions;

        public ProxyResolver(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "Removes and resolves any proxy-delegates (call and newobj)";
        }

        public class AntiProxyParams
        {
            public List<ProxyType> lstProxyTypes;

            public MethodDefinition ResolveMethodMD;
            public MethodDefinition ResolveFieldMD;

            public Int32 XORTokenMethod;
            public Int32 XORTokenField;

            public Assembly asmReflection;
        }

        public class ProxyType
        {
            public TypeDefinition Type;
            public FieldDefinition[] arFields;

            public MethodDefinition CCTor;
            public MethodDefinition[] arMethods;

            public ProxyTypeDelegate[] arProxyTypeDelegate;

            public FieldInfo[] arFieldReflection;

            private int iNumberOfMembers;

            public ProxyType(TypeDefinition typeProxy)
            {
                this.Type = typeProxy;
                this.CCTor = typeProxy.Methods.First(m => m.IsConstructor && m.Name == ".cctor");
                iNumberOfMembers = (CCTor.Body.Instructions.Count - 1) / 2;

                this.arFields = new FieldDefinition[iNumberOfMembers];
                this.arFieldReflection = new FieldInfo[iNumberOfMembers];
                this.arMethods = new MethodDefinition[iNumberOfMembers];
                this.arProxyTypeDelegate = new ProxyTypeDelegate[iNumberOfMembers];
            }

            public bool InitProxyType(Int32 TokenResolveField, Int32 TokenResolveMethod, Assembly asmReflection)
            {

                for (int i = 0; i < iNumberOfMembers; i++)
                {
                    this.arFields[i] = (FieldDefinition)this.CCTor.Body.Instructions[i * 2].Operand;
                    this.arFieldReflection[i] = asmReflection.GetModules()[0].ResolveField(arFields[i].MetadataToken.ToInt32());
                }

                for (int i = 0; i < iNumberOfMembers; i++)
                {
                    Int32 TokenOfCurrentInstruction = ((MethodDefinition)this.CCTor.Body.Instructions[(i * 2) + 1].Operand).MetadataToken.ToInt32();

                    this.arProxyTypeDelegate[i] = (TokenOfCurrentInstruction == TokenResolveField) ? ProxyTypeDelegate.NewObjectCall : ProxyTypeDelegate.DirectMethodCall;
                }
                for (int i = 0; i < iNumberOfMembers; i++)
                {
                    for (int j = 0; j < this.Type.Methods.Count; j++)
                    {
                        // Also per Lamda-expression possible?
                        if (Type.Methods[j].Name == ".cctor" || Type.Methods[j].Name == ".ctor" || Type.Methods[j].Name == "Invoke")
                        {
                            continue;
                        }

                        if (LoadsField(this.Type.Methods[j], this.arFields[i]))
                        {
                            this.arMethods[i] = this.Type.Methods[j];
                        }
                    }
                }

                return true;
            }

            private bool LoadsField(MethodDefinition MD, FieldDefinition FD)
            {
                if (MD.Body.Instructions[0].OpCode.Code == Mono.Cecil.Cil.Code.Ldsfld)
                {
                    FieldDefinition FIELD = (FieldDefinition)MD.Body.Instructions[0].Operand;

                    if (FIELD == FD)
                        return true;
                }

                return false;
            }
        }

        [DeobfuscationPhase(1, "Locate any proxy-types and save them in the list")]
        public static bool Phase1()
        {
            PhaseParam = new AntiProxyParams();
            PhaseParam.lstProxyTypes = new List<ProxyType>();

            foreach (var Type in AsmDef.MainModule.Types)
            {
                //if (Type.
                MethodDefinition[] MDs = Type.Methods.Where(m => m.IsConstructor && m.Name == ".cctor").ToArray();
                if (MDs.Length == 0)
                    continue;

                MethodDefinition cctor = MDs[0];

                if (cctor.Body.Instructions[0].OpCode.ToString().Contains("ldtoken"))
                {
                    PhaseParam.lstProxyTypes.Add(new ProxyType(Type));
                }
            }

            return true;
        }

        [DeobfuscationPhase(2, "Find the resolver functions and their xor-keys")]
        public static bool Phase2()
        {
            AntiProxyParams Params = PhaseParam;
            String InstructionString = String.Empty;

            foreach (var Type in AsmDef.MainModule.Types)
            {
                foreach (var Method in Type.Methods)
                {
                    if (!Method.HasBody)
                        continue;

                    foreach (var Instruc in Method.Body.Instructions)
                    {
                        InstructionString = Instruc.GetInstructionString();

                        if (!InstructionString.Contains("Emit"))
                            continue;

                        if (InstructionString.Contains("Newobj"))
                            Params.ResolveFieldMD = Method;

                        if (InstructionString.Contains("Castclass"))
                            Params.ResolveMethodMD = Method;
                    }
                }
            }

            Params.XORTokenField = GetInt32TokenOfDecodeFunction(Params.ResolveFieldMD);
            Params.XORTokenMethod = GetInt32TokenOfDecodeFunction(Params.ResolveMethodMD);

            return true;
        }

        [DeobfuscationPhase(3, "Init and resolve proxies")]
        public static bool Phase3()
        {
            AntiProxyParams Params = PhaseParam;
            Params.asmReflection = Assembly.LoadFile(Globals.DeobContext.InPath);

            InitMethodCallList();

            foreach (var PT in Params.lstProxyTypes)
            {
                PT.InitProxyType(Params.ResolveFieldMD.MetadataToken.ToInt32(), Params.ResolveMethodMD.MetadataToken.ToInt32(), Params.asmReflection);

                DoAntiProxy(PT, Params);
                //MarkMember(PT.Type);
            }

            return true;
        }

        [DeobfuscationPhase(4, "Clean linked method names")]
        public static bool Phase4()
        {
            // this phase need cleanup badly!
            int iTypeCounter = 0;
            Dictionary<String, MethodDefinition> dicMethods = new Dictionary<string, MethodDefinition>();
            List<Instruction> lstInstructions = new List<Instruction>();

            foreach (var Type in AsmDef.MainModule.Types)
            {
                foreach (var Method in Type.Methods)
                {
                    if (!Method.HasBody) continue;

                    foreach (var Instruction in Method.Body.Instructions)
                    {
                        lstInstructions.Add(Instruction);
                    }
                }
            }
            for (int i = 0; i < AsmDef.MainModule.Types.Count; i++)
            {
                if (AsmDef.MainModule.Types[i].Namespace != "")
                {
                    continue;
                }
                for (int j = 0; j < AsmDef.MainModule.Types[i].Methods.Count; j++)
                {
                    if (AsmDef.MainModule.Types[i].Methods[j].Name == ".ctor" || AsmDef.MainModule.Types[i].Methods[j].Name == ".cctor")
                        continue;

                    if (!dicMethods.ContainsKey(AsmDef.MainModule.Types[i].Methods[j].Name))
                        dicMethods.Add(AsmDef.MainModule.Types[i].Methods[j].Name, AsmDef.MainModule.Types[i].Methods[j]);

                    AsmDef.MainModule.Types[i].Methods[j].Name = String.Format("M_{0}_{1}", j, AsmDef.MainModule.Types[i].Methods[j].ReturnType.Name);

                    for (int x = 0; x < AsmDef.MainModule.Types[i].Methods[j].Parameters.Count; x++)
                    {
                        AsmDef.MainModule.Types[i].Methods[j].Parameters[x].Name = String.Format("Arg_{0}_{1}", x, AsmDef.MainModule.Types[i].Methods[j].Parameters[x].ParameterType.Name);
                    }
                }
            }

            foreach (Instruction Instr in lstInstructions)
            {
                if (Instr.Operand is MethodReference)
                {
                    MethodReference MR = Instr.Operand as MethodReference;

                    if (dicMethods.ContainsKey(MR.Name))
                    {
                        Instr.Operand = dicMethods[(Instr.Operand as MethodReference).Name];
                    }
                }
            }

            return true;
        }


        private static Instruction[] GetInstructionsWithMethodCall(MethodDefinition MD)
        {
            return lstCallInstructions.Where(m => m.Operand == MD).ToArray();
        }

        private static void InitMethodCallList()
        {
            lstCallInstructions = new List<Instruction>();

            foreach (var Type in AsmDef.MainModule.Types)
            {
                foreach (var Method in Type.Methods)
                {
                    if (!Method.HasBody)
                        continue;

                    foreach (var Instr in Method.Body.Instructions)
                    {
                        if (Instr.ToString().Contains("call"))
                        {
                            lstCallInstructions.Add(Instr);
                        }
                    }
                }
            }
        }

        private static bool DoAntiProxy(ProxyType PT, AntiProxyParams Params)
        {
            FunctionCallType CT = FunctionCallType.Call;

            for (int iIndex = 0; iIndex < PT.arMethods.Length; iIndex++)
            {
                if (PT.arProxyTypeDelegate[iIndex] == ProxyTypeDelegate.NewObjectCall)
                {
                    Int32 TokenOfOriginalCall = GetTokenForFieldDecode(PT, iIndex, Params);

                    MethodReference MR = AsmDef.MainModule.Import(Params.asmReflection.GetModules()[0].ResolveMethod(TokenOfOriginalCall));
                    Instruction[] arIns = GetInstructionsWithMethodCall(PT.arMethods[iIndex]).ToArray();

                    for (int i = 0; i < arIns.Length; i++)
                    {
                        arIns[i].OpCode = OpCodes.Newobj;
                        arIns[i].Operand = MR;
                    }
                }

                if (PT.arProxyTypeDelegate[iIndex] == ProxyTypeDelegate.DirectMethodCall)
                {
                    Int32 TokenOfOriginalCall = GetTokenForMethodDecode(PT, iIndex, Params, out CT);

                    ProxyType PT2 = null;
                    MethodReference MR = AsmDef.MainModule.Import(Params.asmReflection.GetModules()[0].ResolveMethod(TokenOfOriginalCall));

                    ProxyType[] NestedPT = Params.lstProxyTypes.Where(m => m.Type.Name == MR.DeclaringType.Name).ToArray();

                    if (NestedPT.Length != 0)
                        PT2 = NestedPT[0];

                    /*foreach (var PTNew in AP17.lstProxyTypes)
                    {
                        if (MR.DeclaringType.Name == PTNew.Type.Name)
                        {
                            PT2 = new ProxyType(PTNew.Type, AP17);
                        }
                    }*/

                    // PT2 = Second stage proxy -> proxy followed by a proxy by a method/newobj
                    if (PT2 != null)
                    {
                        Instruction[] arIns = GetInstructionsWithMethodCall(PT.arMethods[iIndex]);

                        if (PT2.arProxyTypeDelegate[0] == ProxyTypeDelegate.NewObjectCall)
                        {
                            Int32 TokenOfOriginalCall2 = GetTokenForFieldDecode(PT2, 0, Params);

                            MethodReference MR2 = AsmDef.MainModule.Import(Params.asmReflection.GetModules()[0].ResolveMethod(TokenOfOriginalCall2));

                            for (int i = 0; i < arIns.Length; i++)
                            {
                                arIns[i].OpCode = OpCodes.Newobj;
                                arIns[i].Operand = MR2;
                            }
                        }
                    }
                    else
                    {
                        Instruction[] arIns = GetInstructionsWithMethodCall(PT.arMethods[iIndex]);
                        for (int i = 0; i < arIns.Length; i++)
                        {
                            if (CT == FunctionCallType.Call)
                            {
                                arIns[i].OpCode = OpCodes.Call;
                                arIns[i].Operand = MR;
                            }
                            if (CT == FunctionCallType.Callvirt)
                            {
                                arIns[i].OpCode = OpCodes.Callvirt;
                                arIns[i].Operand = MR;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private static int GetTokenForMethodDecode(ProxyType PT, int iIndex, AntiProxyParams Params, out FunctionCallType CT)
        {
            FieldInfo fieldFromHandle = PT.arFieldReflection[iIndex];
            Assembly executingAssembly = Params.asmReflection;

            char[] array = new char[fieldFromHandle.Name.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (char)((int)((byte)fieldFromHandle.Name[i]) ^ i);
            }
            byte[] array2 = Convert.FromBase64String(new string(array));
            CT = (array2[0] == 13) ? FunctionCallType.Callvirt : FunctionCallType.Call;

            return BitConverter.ToInt32(array2, 1) ^ Params.XORTokenMethod;
        }

        private static int GetTokenForFieldDecode(ProxyType PT, int iIndex, AntiProxyParams Params)
        {
            FieldInfo fieldFromHandle = PT.arFieldReflection[iIndex];

            Assembly executingAssembly = Params.asmReflection;

            char[] array = new char[fieldFromHandle.Name.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (char)((int)((byte)fieldFromHandle.Name[i]) ^ i);
            }
            return BitConverter.ToInt32(Convert.FromBase64String(new string(array)), 0) ^ Params.XORTokenField;
        }

        private static Int32 GetInt32TokenOfDecodeFunction(MethodDefinition MD)
        {
            List<Instruction> lstInstructions = new List<Instruction>(MD.Body.Instructions);

            for (int i = 0; i < lstInstructions.Count; i++)
            {
                if (lstInstructions[i].OpCode.Code == Code.Call && lstInstructions[i].ToString().Contains("BitConverter"))
                {
                    Instruction BrInstruc = lstInstructions[i + 1];
                    Instruction FinalTokenLoad = null;

                    if (BrInstruc.OpCode.Code == Code.Br)
                    {
                        FinalTokenLoad = (Instruction)BrInstruc.Operand;
                    }
                    else
                    {
                        FinalTokenLoad = lstInstructions[i + 1];
                    }

                    return (Int32)FinalTokenLoad.Operand;
                }
            }

            return 0;
        }

        public enum ProxyTypeDelegate
        {
            NewObjectCall,
            DirectMethodCall,
        }
        public enum FunctionCallType
        {
            Call,
            Callvirt
        }
    }
}
