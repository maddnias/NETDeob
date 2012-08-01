using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils.Extensions;
using OpCode = System.Reflection.Emit.OpCode;

namespace NETDeob.Core.Deobfuscators.Confuser.Obsolete.Utils
{
    public struct ExInstruction
    {
        public dynamic Value;
        public OpCode Code;
    }

    public class ExpressionScheme
    {
        private interface IExpressionPart { }

        public enum Operator
        {
            Addition = 0,
            Subtraction = 1,
            Multiplication = 2,
            Xor = 4
        }

        public struct Value : IExpressionPart
        {
            public object Val;
        }
        public struct ExpOperator : IExpressionPart
        {
            public Operator @Operator;
        }

        public int PartCount { get { return _parts.Count; } }
        private List<IExpressionPart> _parts;

        public ExpressionScheme(IEnumerable<Instruction> instrList)
        {
            _parts = new List<IExpressionPart>();

            foreach(var instr in instrList)
            {
                if (instr.IsLdcI4WOperand() || instr.IsLdcI8WOperand())
                    _parts.Add(new Value { Val = instr.Operand });
                else 
                    switch(instr.OpCode.Code)
                    {
                        case Code.Add:
                            _parts.Add(new ExpOperator {Operator = Operator.Addition});
                            break;
                        case Code.Sub:
                            _parts.Add(new ExpOperator { Operator = Operator.Subtraction });
                            break;
                        case Code.Mul:
                            _parts.Add(new ExpOperator { Operator = Operator.Multiplication });
                            break;
                        case Code.Xor:
                            _parts.Add(new ExpOperator { Operator = Operator.Xor });
                            break;
                    }
            }
        }

        public IEnumerable<ExInstruction> ParseInstructions()
        {
            foreach(var part in _parts)
            {
                if (part is Value)
                {
                    var val = ((Value)part).Val;

                    if (val is int)
                        yield return new ExInstruction
                                        {
                                            Code = System.Reflection.Emit.OpCodes.Ldc_I4,
                                            Value = val
                                        };
                    else if (val is long)
                        yield return new ExInstruction
                                         {
                                             Code = System.Reflection.Emit.OpCodes.Ldc_I8,
                                             Value = val
                                         };
                }
                else if (part is ExpOperator)
                {
                    switch (((ExpOperator)part).Operator)
                    {
                        case Operator.Addition:
                            yield return new ExInstruction
                                            {
                                                Code = System.Reflection.Emit.OpCodes.Add,
                                                Value = null
                                            };
                            break;

                        case Operator.Multiplication:
                            yield return new ExInstruction
                            {
                                Code = System.Reflection.Emit.OpCodes.Mul,
                                Value = null
                            };
                            break;

                        case Operator.Subtraction:
                            yield return new ExInstruction
                            {
                                Code = System.Reflection.Emit.OpCodes.Sub,
                                Value = null
                            };
                            break;

                        case Operator.Xor:
                            yield return new ExInstruction
                            {
                                Code = System.Reflection.Emit.OpCodes.Xor,
                                Value = null
                            };
                            continue;
                    }
                }
                else
                    throw new Exception("Internal error");
            }
        }
        
        public static ExpressionScheme Combine(ExpressionScheme scheme1, ExpressionScheme scheme2)
        {
            var tmp = new ExpressionScheme(null)
                       {
                           _parts = new List<IExpressionPart>()
                       };

            tmp._parts.AddRange(scheme1._parts.ToArray());
            tmp._parts.AddRange(scheme2._parts.ToArray());

            return tmp;
        }

    }

    public class DynamicEvaluator
    {
        private ExpressionScheme _scheme;

        public DynamicEvaluator(ExpressionScheme scheme)
        {
            _scheme = scheme;
        }

        public T EvaluateExpression<T>()
        {
            var evaluator = new DynamicMethod("eval", typeof (T), new Type[] {});
            var ilGen = evaluator.GetILGenerator();
            var parts = _scheme.ParseInstructions();

            foreach (var part in parts)
                if (part.Value == null)
                    ilGen.Emit(part.Code);
                else
                    ilGen.Emit(part.Code, part.Value);

            ilGen.Emit(System.Reflection.Emit.OpCodes.Ret);

            return (T) evaluator.Invoke(null, new object[] {});
        }
    }
}
