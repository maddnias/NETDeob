using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;

namespace NETDeob.Core.Deobfuscators.Generic.Obsolete.Emulation
{
    public class ConstantFolder
    {
        public class UnknownValue
        {
        }

        public enum Operator
        {
            Addition = 0,
            Subtraction = 1,
            Multiplication = 2,
            Xor = 4,
            Divide = 8,
            ShiftLeft = 16,
            ShiftRight = 32,
            Mod = 64
        }
        public struct ExInstruction
        {
            public OpCode @OpCode;
            public dynamic Value;
        }

        public void ApplyFolding(ref MethodDefinition mDef)
        {
            FoldBody(ref mDef);
          
        }

        private static void FoldBody(ref MethodDefinition mDef)
        {
            var body = mDef.Body;
            var tmpBody = body.Instructions;
            var markings = new Dictionary<KeyValuePair<int, int>, ExInstruction>();
            var stack = new Stack<dynamic>(body.MaxStackSize);
            var locals = new Dictionary<int, object>(body.Variables.Count);

            markings.Add(new KeyValuePair<int, int>(0, 0), new ExInstruction()); // Stub

            while (markings.Count > 0)
            {
                markings.Clear();

                for (var i = 0; i < tmpBody.Count; i++)
                {
                    var instr = tmpBody[i];

                    EmulateInstruction(instr, ref tmpBody, ref stack, ref locals, ref markings, ref i);
                }

                FoldAndReplace(ref markings, ref body);
                mDef.Body = body;
                break;
            }
        }

        private static void FoldAndReplace(
            ref Dictionary<KeyValuePair<int, int>, ExInstruction> markings,
            ref MethodBody body)
        {
            var instrList = body.Instructions;
            var foldedBody = new Collection<Instruction>();

            foreach(var marking in markings)
            {
                for(var i = 0;i < instrList.Count;i++)
                {
                    if(marking.Key.Key == i)
                    {
                        for (var x = 0; x < marking.Key.Value +1; x++)
                            instrList.RemoveAt(marking.Key.Key -2);

                        switch(marking.Value.OpCode.Code)
                        {
                            case Code.Ldc_I4:
                                instrList.Insert(marking.Key.Key -3,
                                                 body.GetILProcessor().Create(OpCodes.Ldc_I4, (int)marking.Value.Value));
                                break;

                            case Code.Ldc_I8:
                                instrList.Insert(marking.Key.Key -3,
                                                 body.GetILProcessor().Create(OpCodes.Ldc_I8, (long)marking.Value.Value));
                                break;
                        }

                        break;
                    }
                }
            }

            body.Instructions.Clear();

            //body.SimplifyMacros();

            var ilProc = body.GetILProcessor();

            foreach (var instr in instrList)
                ilProc.Append(instr);

            //body.OptimizeMacros();//it can't resolve the tokens from rocks
        }

        private static void EmulateInstruction(
            Instruction instr,
            ref Collection<Instruction> instrList,
            ref Stack<dynamic> stack,
            ref Dictionary<int, object> locals,
            ref Dictionary<KeyValuePair<int, int>, ExInstruction> markings,
            ref int idx)
        {
            switch(instr.OpCode.Code)
            {
                case Code.Ldc_I4:
                    stack.Push((int) Convert.ChangeType(instr.Operand, typeof (int)));
                    break;

                case Code.Ldc_I4_S:
                    stack.Push((sbyte)Convert.ChangeType(instr.Operand, typeof(sbyte)));
                    break;

                case Code.Ldc_I4_0:
                case Code.Ldc_I4_1:
                case Code.Ldc_I4_2:
                case Code.Ldc_I4_3:
                case Code.Ldc_I4_4:
                case Code.Ldc_I4_5:
                case Code.Ldc_I4_6:
                case Code.Ldc_I4_7:
                case Code.Ldc_I4_8:
                    stack.Push(Int32.Parse(instr.OpCode.Code.ToString().Split('_')[2]));
                    break;

                case Code.Ldc_I4_M1:
                    stack.Push(-1);
                    break;

                case Code.Ldc_I8:
                    stack.Push((long) Convert.ChangeType(instr.Operand, typeof (long)));
                    break;

                case Code.Ldc_R4:
                    stack.Push((float) Convert.ChangeType(instr.Operand, typeof(float)));
                    break;

                case Code.Ldc_R8:
                    stack.Push((double)Convert.ChangeType(instr.Operand, typeof(float)));
                    break;

                case Code.Stloc:
                case Code.Stloc_S:
                    PopToLocal(ref locals, ref stack, instr);
                    break;

                case Code.Stloc_0:
                case Code.Stloc_1:
                case Code.Stloc_2:
                case Code.Stloc_3:
                    PopToLocal(ref locals, ref stack, instr, Int32.Parse(instr.OpCode.Code.ToString().Split('_')[1]));
                    break;

                case Code.Ldloc:
                case Code.Ldloc_S:
                    PushFromLocal(ref locals, ref stack, instr);
                    break;

                case Code.Ldloc_0:
                case Code.Ldloc_1:
                case Code.Ldloc_2:
                case Code.Ldloc_3:
                    PushFromLocal(ref locals, ref stack, instr, Int32.Parse(instr.OpCode.Code.ToString().Split('_')[1]));
                    break;

                case Code.Add:
                case Code.Add_Ovf:
                case Code.Add_Ovf_Un:
                    PerformArithmetic(ref stack, Operator.Addition, ref markings, ref idx);
                    break;

                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                    PerformArithmetic(ref stack, Operator.Subtraction, ref markings, ref idx);
                    break;

                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                    PerformArithmetic(ref stack, Operator.Multiplication, ref markings, ref idx);
                    break;

                case Code.Div:
                case Code.Div_Un:
                    PerformArithmetic(ref stack, Operator.Divide, ref markings, ref idx);
                    break;

                case Code.Shl:
                    PerformArithmetic(ref stack, Operator.ShiftLeft, ref markings, ref idx);
                    break;

                case Code.Shr:
                case Code.Shr_Un:
                    PerformArithmetic(ref stack, Operator.ShiftRight, ref markings, ref idx);
                    break;

                case Code.Rem:
                case Code.Rem_Un:
                    PerformArithmetic(ref stack, Operator.Mod, ref markings, ref idx);
                    break;
                    // We still need to follow control flow to not fuck up stack
                case Code.Br:
                case Code.Br_S:
                case Code.Leave:
                case Code.Leave_S:
                    idx = (instr.Operand as Instruction).GetInstructionIndex(instrList);
                    break;

                case Code.Conv_I1:
                    //stack.Push((sbyte) stack.Pop());
                    break;

                case Code.Conv_I2:
                    //stack.Push((short) stack.Pop());
                    break;

                case Code.Conv_I4:
                    //stack.Push((int) stack.Pop());
                    break;

                case Code.Conv_I8:
                    //stack.Push((long) (stack.Pop() is UnknownValue ? 0 : );
                    break;

                case Code.Call:
                case Code.Calli:
                case Code.Callvirt:
                    EmulateCall(ref stack, instr);
                    break;

                case Code.Ldtoken:
                    stack.Push(new UnknownValue());
                    break;

                case Code.Ldsfld:
                    stack.Push(new UnknownValue());
                    break;

                case Code.Stsfld:
                    stack.Pop();
                    break;

                case Code.Dup:
                    stack.Push(stack.Peek());
                    break;

                case Code.Clt:
                case Code.Clt_Un:
                case Code.Bgt:
                case Code.Bgt_S:
                case Code.Bgt_Un:
                case Code.Bgt_Un_S:
                case Code.Bge:
                case Code.Bge_S:
                case Code.Bge_Un:
                case Code.Bge_Un_S:
                case Code.Blt:
                case Code.Blt_S:
                case Code.Blt_Un:
                case Code.Blt_Un_S:
                case Code.Bne_Un:
                case Code.Bne_Un_S:
                case Code.Beq:
                case Code.Beq_S:
                    PerformComparison(ref stack, instr, ref idx, instrList);
                    break;

                case Code.Box:
                    stack.Push((object) stack.Pop());
                    break;

                case Code.Stobj:
                    stack.Pop();
                    stack.Pop();
                    break;

                case Code.Ldelem_Any:
                case Code.Ldelem_I:
                case Code.Ldelem_I1:
                case Code.Ldelem_I2:
                case Code.Ldelem_I4:
                case Code.Ldelem_I8:
                case Code.Ldelem_R4:
                case Code.Ldelem_R8:
                case Code.Ldelem_Ref:
                case Code.Ldelem_U1:
                case Code.Ldelem_U2:
                case Code.Ldelem_U4:
                    stack.Pop();
                    stack.Pop();
                    stack.Push(new UnknownValue());
                    break;

                case Code.Ldlen:
                    stack.Push(new UnknownValue());
                    break;

                case Code.Ldnull:
                    stack.Push(null);
                    break;

                case Code.Newobj:
                    stack.Push(new UnknownValue());
                    break;
            }
        }

        private static void EmulateCall(
            ref Stack<dynamic> stack,
            Instruction instr)
        {
            var target = (instr.Operand as MethodReference).Resolve();

            foreach (var param in target.Parameters)
                stack.Pop();

            if (target.ReturnType.Name != "Void")
                stack.Push(new UnknownValue());
        }

        private static void PerformComparison(
            ref Stack<dynamic> stack,
            Instruction instr,
            ref int idx,
            Collection<Instruction> instrList)
        {
            var val1 = stack.Pop();
            var val2 = stack.Pop();

            if (!IsIlNumeric(val1) || !IsIlNumeric(val2))
                if (instr.OpCode.StackBehaviourPush == StackBehaviour.Push1)
                {
                    stack.Push(new UnknownValue());
                    return;
                }
                else
                    return;

            switch(instr.OpCode.Code)
            {
                case Code.Clt:
                case Code.Clt_Un:
                    stack.Push(val1 < val2 ? 1 : 0);
                    break;

                case Code.Blt:
                case Code.Blt_S:
                case Code.Blt_Un:
                case Code.Blt_Un_S:
                    idx = (val1 < val2 ? (instr.Operand as Instruction).GetInstructionIndex(instrList) : idx);
                    break;

                case Code.Bgt:
                case Code.Bgt_S:
                case Code.Bgt_Un:
                case Code.Bgt_Un_S:
                    idx = (val1 > val2 ? (instr.Operand as Instruction).GetInstructionIndex(instrList) : idx);
                    break;

                case Code.Bge:
                case Code.Bge_S:
                case Code.Bge_Un:
                case Code.Bge_Un_S:
                    idx = (val1 >= val2 ? (instr.Operand as Instruction).GetInstructionIndex(instrList) : idx);
                    break;

                case Code.Beq:
                case Code.Beq_S:
                    idx = (val1 == val2 ? (instr.Operand as Instruction).GetInstructionIndex(instrList) : idx);
                    break;

                case Code.Bne_Un:
                case Code.Bne_Un_S:
                    idx = (val1 != val2 ? (instr.Operand as Instruction).GetInstructionIndex(instrList) : idx);
                    break;
            }
        }

        private static void PerformArithmetic(
            ref Stack<dynamic> stack,
            Operator @operator,
            ref Dictionary<KeyValuePair<int, int>, ExInstruction> markings,
            ref int idx)
        {
            var val2 = stack.Pop();
            var val1 = stack.Pop();

            if (IsIlNumeric(val1) && IsIlNumeric(val2))
            {
                switch (@operator)
                {
                    case Operator.Addition:
                        stack.Push(val1 + val2);
                        break;

                    case Operator.Subtraction:
                        stack.Push(val1 - val2);
                        break;

                    case Operator.Multiplication:
                        stack.Push(val1*val2);
                        break;

                    case Operator.Xor:
                        stack.Push(val1 ^ val2);
                        break;

                    case Operator.Divide:
                        stack.Push(val1/val2);
                        break;

                    case Operator.ShiftLeft:
                        stack.Push(val1 << val2);
                        break;

                    case Operator.ShiftRight:
                        stack.Push(val1 >> val2);
                        break;

                    case Operator.Mod:
                        stack.Push(val1%val2);
                        break;
                }

                var val = stack.Peek();

                if (val == 0){
                    markings.Add(new KeyValuePair<int, int>(idx, 2), new ExInstruction {OpCode = OpCodes.Ldc_I4_0, Value = null});return;
                }
                if (val == 1){
                    markings.Add(new KeyValuePair<int, int>(idx, 2), new ExInstruction {OpCode = OpCodes.Ldc_I4_1, Value = null});return;
                }
                if (val == 2){
                    markings.Add(new KeyValuePair<int, int>(idx, 2), new ExInstruction { OpCode = OpCodes.Ldc_I4_2, Value = null }); return;
                }
                if (val == 3){
                    markings.Add(new KeyValuePair<int, int>(idx, 2), new ExInstruction {OpCode = OpCodes.Ldc_I4_3, Value = null});return;
                }
                if (val == 4){
                    markings.Add(new KeyValuePair<int, int>(idx, 2), new ExInstruction {OpCode = OpCodes.Ldc_I4_4, Value = null}); return;
                }
                if (val == 5){
                    markings.Add(new KeyValuePair<int, int>(idx, 2), new ExInstruction {OpCode = OpCodes.Ldc_I4_5, Value = null}); return;
                }
                if (val == 6){
                    markings.Add(new KeyValuePair<int, int>(idx, 2), new ExInstruction {OpCode = OpCodes.Ldc_I4_6, Value = null}); return;
                }
                if (val == 7){
                    markings.Add(new KeyValuePair<int, int>(idx, 2), new ExInstruction {OpCode = OpCodes.Ldc_I4_7, Value = null});return;
                }
                if (val == 8){
                    markings.Add(new KeyValuePair<int, int>(idx, 2), new ExInstruction { OpCode = OpCodes.Ldc_I4_8, Value = null }); return;
                }

                if (val is float){
                    markings.Add(new KeyValuePair<int, int>(idx, 2), new ExInstruction {OpCode = OpCodes.Ldc_R4, Value = val}); return;
                }
                if (val is double){
                    markings.Add(new KeyValuePair<int, int>(idx, 2), new ExInstruction { OpCode = OpCodes.Ldc_R8, Value = val }); return;
                }

                if (val >= sbyte.MinValue && val <= sbyte.MaxValue)
                    markings.Add(new KeyValuePair<int, int>(idx, 2), new ExInstruction { OpCode = OpCodes.Ldc_I4_S, Value = val });
                else if(val >= short.MinValue && val <= int.MaxValue)
                    markings.Add(new KeyValuePair<int, int>(idx, 2), new ExInstruction { OpCode = OpCodes.Ldc_I4, Value = val });
                else if (val >= long.MinValue && val <= long.MaxValue)
                    markings.Add(new KeyValuePair<int, int>(idx, 2), new ExInstruction { OpCode = OpCodes.Ldc_I8, Value = val });
            }
            else
                stack.Push(new UnknownValue());
        }

        private static void PopToLocal(
            ref Dictionary<int, object> locals,
            ref Stack<dynamic> stack,
            Instruction instr,
            int index = -1)
        {
            var val = instr.Operand;

            if (IsIlNumeric(val))
                locals[(index == -1 ? (instr.Operand as VariableDefinition).Index : index)] = stack.Pop();
            else
                locals[(index == -1 ? (instr.Operand as VariableDefinition).Index : index)] = new UnknownValue();
        }

        private static void PushFromLocal(
            ref Dictionary<int, object> locals,
            ref Stack<dynamic> stack,
            Instruction instr,
            int index = -1)
        {
            stack.Push(locals[(index == -1 ? (instr.Operand as VariableDefinition).Index : index)]);
        }

        private static bool IsIlNumeric(object value)
        {
            if (value is UnknownValue)
                return false;

            return value is int || value is float || value is long || value is double;
        }
    }
}
