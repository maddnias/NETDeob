using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace NETDeob.Core.Engine.Utils.Extensions
{
    public static class InstructionExt
    {
        public static bool IsCall(this Instruction instr)
        {
            return instr.OpCode == OpCodes.Call ||
                   instr.OpCode == OpCodes.Calli ||
                   instr.OpCode == OpCodes.Callvirt;
        }

        public static bool IsLdcI4WOperand(this Instruction instr)
        {
            return instr.OpCode == OpCodes.Ldc_I4 ||
                   instr.OpCode == OpCodes.Ldc_I4_S;
        }

        public static bool IsLdcI8WOperand(this Instruction instr)
        {
            return instr.OpCode == OpCodes.Ldc_I8;
        }

        public static bool IsLdcI4(this Instruction instr)
        {
            return instr.OpCode == OpCodes.Ldc_I4 ||
                   instr.OpCode == OpCodes.Ldc_I4_0 ||
                   instr.OpCode == OpCodes.Ldc_I4_1 ||
                   instr.OpCode == OpCodes.Ldc_I4_2 ||
                   instr.OpCode == OpCodes.Ldc_I4_3 ||
                   instr.OpCode == OpCodes.Ldc_I4_4 ||
                   instr.OpCode == OpCodes.Ldc_I4_5 ||
                   instr.OpCode == OpCodes.Ldc_I4_6 ||
                   instr.OpCode == OpCodes.Ldc_I4_7 ||
                   instr.OpCode == OpCodes.Ldc_I4_8 ||
                   instr.OpCode == OpCodes.Ldc_I4_M1 ||
                   instr.OpCode == OpCodes.Ldc_I4_S;
        }


        public static bool IsTarget(this Instruction instr, MethodBody body)
        {
            return body.Instructions.Where(OnFunc).Any(instr1 => instr1.Operand as Instruction == instr);
        }

        private static bool OnFunc(Instruction instr)
        {
            return instr.IsUnconditionalJump();
        }

        public static int GetLdcI4(this Instruction instr)
        {
            switch (instr.OpCode.Code)
            {
                case Code.Ldc_I4_0:
                case Code.Ldc_I4_1:
                case Code.Ldc_I4_2:
                case Code.Ldc_I4_3:
                case Code.Ldc_I4_4:
                case Code.Ldc_I4_5:
                case Code.Ldc_I4_6:
                case Code.Ldc_I4_7:
                case Code.Ldc_I4_8:
                    return Int32.Parse(instr.OpCode.Code.ToString().Split('_')[2]); // Lazy :)

                case Code.Ldc_I4_M1:
                    return -1;

                case Code.Ldc_I4:
                case Code.Ldc_I4_S:
                    return (int)Convert.ChangeType(instr.Operand, typeof(int)); // No idea why I have to cast it this way

                default:
                    throw new Exception("Internal invalid instruction!");
            }
        }

        public static bool IsStLoc(this Instruction instr)
        {
            return instr.OpCode == OpCodes.Stloc ||
                   instr.OpCode == OpCodes.Stloc_0 ||
                   instr.OpCode == OpCodes.Stloc_1 ||
                   instr.OpCode == OpCodes.Stloc_2 ||
                   instr.OpCode == OpCodes.Stloc_3 ||
                   instr.OpCode == OpCodes.Stloc_S;

        }

        public static bool IsLdLoc(this Instruction instr)
        {
            return instr.OpCode == OpCodes.Ldloc ||
                   instr.OpCode == OpCodes.Ldloc_0 ||
                   instr.OpCode == OpCodes.Ldloc_1 ||
                   instr.OpCode == OpCodes.Ldloc_2 ||
                   instr.OpCode == OpCodes.Ldloc_3 ||
                   instr.OpCode == OpCodes.Ldloc_S;

        }

        public static bool IsUnconditionalJump(this Instruction instr)
        {
            return
                instr.OpCode == OpCodes.Br ||
                instr.OpCode == OpCodes.Br_S ||
                instr.OpCode == OpCodes.Jmp ||
                instr.OpCode == OpCodes.Leave_S ||
                instr.OpCode == OpCodes.Leave;
        }

        public static bool IsConditionalJump(this Instruction instr)
        {
            return
                instr.OpCode == OpCodes.Brtrue ||
                instr.OpCode == OpCodes.Brtrue_S ||
                instr.OpCode == OpCodes.Brfalse ||
                instr.OpCode == OpCodes.Brfalse_S ||
                instr.OpCode == OpCodes.Ble ||
                instr.OpCode == OpCodes.Ble_S ||
                instr.OpCode == OpCodes.Ble_Un ||
                instr.OpCode == OpCodes.Ble_Un_S ||
                instr.OpCode == OpCodes.Blt ||
                instr.OpCode == OpCodes.Blt_S ||
                instr.OpCode == OpCodes.Blt_Un ||
                instr.OpCode == OpCodes.Blt_Un_S ||
                instr.OpCode == OpCodes.Bge ||
                instr.OpCode == OpCodes.Bge_S ||
                instr.OpCode == OpCodes.Bge_Un ||
                instr.OpCode == OpCodes.Bge_Un_S ||
                instr.OpCode == OpCodes.Beq ||
                instr.OpCode == OpCodes.Beq_S;
        }

        public static int GetInstructionIndex(this Instruction instr, Collection<Instruction> instructions)
        {
            var count = 0;

            while (instructions[count++] != instr)
                // ReSharper disable RedundantJumpStatement
                continue;
            // ReSharper restore RedundantJumpStatement

            return count;
        }

    }
}
