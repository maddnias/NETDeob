using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace NETDeob.Core.Engine.Utils.Extensions
{
    public static class InstructionCollection
    {
        public static Collection<Instruction> SplitInstructions(this Collection<Instruction> instructions, int offset)
        {
            var outCollection = new Collection<Instruction>();

            foreach (var instr in instructions.Where(instr => instr.Offset > offset))
                outCollection.Add(instr);

            return outCollection;
        }

        public static Instruction DoUntil(this Collection<Instruction> instrList, Instruction curInstr, Predicate<Instruction> pred, bool forward)
        {
            var tmpInstr = curInstr;

            while (!pred((tmpInstr = (forward ? tmpInstr.Next : tmpInstr.Previous))))
                continue;

            return tmpInstr;
        }

        public static T GetOperandAt<T>(this Collection<Instruction> instructions, OpCode opCode, int index)
        {
            for (int i = 0, x = 0; i < instructions.Count; i++)
                if (instructions[i].OpCode == opCode)
                    if (x++ == index)
                        return (T)instructions[i].Operand;

            return default(T);
        }

        public static T GetOperandAt<T>(this Collection<Instruction> instructions, Predicate<Instruction> pred, int index)
        {
            for (int i = 0, x = 0; i < instructions.Count; i++)
                if (pred(instructions[i]))
                    if (x++ == index)
                        return (T)instructions[i].Operand;

            return default(T);
        }

        public static int GetOpCodeCount(this Collection<Instruction> instructions, OpCode opCode)
        {
            if (instructions.Count == 0)
                return 0;

            return instructions.Count(instr => instr.OpCode == opCode);
        }

        public static Collection<Instruction> FindInstructions(this Collection<Instruction> instructions, OpCode[] opCodes)
        {
            var outCollection = new Collection<Instruction>();

            foreach (var instr in instructions.Where(instr => opCodes.Contains(instr.OpCode)))
                outCollection.Add(instr);

            return outCollection;
        }

        public static List<Instruction> GetInstructionBlock(this Collection<Instruction> instructions, int startIndex, Predicate<Instruction> ender)
        {
            var outList = new List<Instruction>();
            var instr = instructions[++startIndex];

            while (!(ender(instr.Next)))
            {
                outList.Add(instr);
                instr = instr.Next;
            }

            outList.Add(instr);

            return outList;
        }

        public static IEnumerable<Instruction> SliceBlock(this Collection<Instruction> instrList, Instruction start, int count)
        {
            var curInstr = start.Next;

            for (var i = ++count; i > 0; i--)
            {
                yield return curInstr.Previous;
                curInstr = curInstr.Previous;
            }
        }

        public static List<Instruction> GetInstructionBlock(this Collection<Instruction> instructions, int startIndex, int count)
        {
            var outList = new List<Instruction>();

            for (var i = startIndex; i < startIndex + count; i++)
                outList.Add(instructions[i]);

            return outList;
        }

        public static Instruction FirstOfOpCode(this Collection<Instruction> instructions, OpCode opCode)
        {
            return instructions.FirstOrDefault(t => t.OpCode == opCode);
        }

        public static Instruction FindInstruction(this Collection<Instruction> instructions, Predicate<Instruction> pred, int index)
        {
            for (int i = 0, idx = 0; i < instructions.Count; i++)
                if (pred(instructions[i]))
                    if (idx++ == index)
                        return instructions[i];

            return null;
        }

        public static Instruction FindInstruction(this Collection<Instruction> instructions, Predicate<Instruction> pred, int index, int start)
        {
            for (int i = start, idx = 0; i < instructions.Count; i++)
                if (pred(instructions[i]))
                    if (idx++ == index)
                        return instructions[i];

            return null;
        }

        public static Instruction FirstOfOpCode(this Collection<Instruction> instructions, Predicate<OpCode> opCode, int index)
        {
            for (int i = 0, x = 0; i < instructions.Count; i++)
                if (opCode(instructions[i].OpCode))
                {
                    if (++x == index)
                        return instructions[i];
                }

            return null;
        }
    }
}
