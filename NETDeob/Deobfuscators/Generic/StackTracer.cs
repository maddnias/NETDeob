using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NETDeob.Core.Deobfuscators.Generic
{
    public class StackTracer
    {
        private MethodBody _methodBody;

        #region Execution state
        private int _instructionPointer = 0;
        private Stack<StackEntry> _stack = new Stack<StackEntry>();
        #endregion

        public Stack<StackEntry> Stack { get { return _stack; } } 

        public StackTracer(MethodBody methodBody)
        {
            _methodBody = methodBody;
        }

        public void TraceUntil(Instruction instruction)
        {
            Trace(() => _methodBody.Instructions[_instructionPointer] != instruction);
        }

        private void Trace(Func<bool> continueCondition)
        {
            while (continueCondition())
            {
                var instruction = _methodBody.Instructions[_instructionPointer];
                _instructionPointer = ExecuteInstruction(instruction);
            }
        }

        private int ExecuteInstruction(Instruction instruction)
        {
            switch (instruction.OpCode.StackBehaviourPush)
            {
                case StackBehaviour.Push0:
                case StackBehaviour.Push1:
                case StackBehaviour.Pushi:
                case StackBehaviour.Pushi8:
                case StackBehaviour.Pushr4:
                case StackBehaviour.Pushr8:
                case StackBehaviour.Pushref:
                    _stack.Push(new StackEntry(instruction, instruction.Operand));
                    break;
                case StackBehaviour.Push1_push1:
                    _stack.Push(new StackEntry(instruction, null));
                    _stack.Push(new StackEntry(instruction, null));
                    break;
            }
            if (instruction.OpCode.OperandType == OperandType.InlineBrTarget ||
                instruction.OpCode.OperandType == OperandType.ShortInlineBrTarget)
            {
                return _methodBody.Instructions.IndexOf(instruction);
            }
            return ++_instructionPointer;
        }

        public class StackEntry
        {
            public StackEntry(Instruction by, object value)
            {
                PushedBy = by;
                Value = value;
            }
            public Instruction PushedBy;
            public object Value;
        }
    }
}
