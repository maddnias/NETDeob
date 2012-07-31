using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NETDeob.Core.Engine.Utils
{
    public struct ILSignature
    {
        public int StartIndex;
        public OpCode StartOpCode;
        public List<OpCode> Instructions;
    }

    public static class SignatureFinder
    {
        public static bool IsMatch(MethodDefinition mDef, ILSignature signature)
        {
            if (!mDef.HasBody)
                return false;

            if (mDef.Body.Instructions.Count <= (signature.StartIndex > 0 ? signature.StartIndex : -signature.StartIndex) + signature.Instructions.Count)
                return false;

            Instruction initInstr;

            if (signature.StartOpCode != OpCodes.Nop)
                initInstr = mDef.Body.Instructions.FirstOrDefault(instr => instr.OpCode == signature.StartOpCode);
            else
                initInstr = mDef.Body.Instructions[signature.StartIndex];

            if (initInstr == null)
                return false;

            var flag = true;
            var tmp = initInstr.Previous ?? initInstr;

            foreach (var instr in signature.Instructions)
            {
                tmp = tmp.Next;

                if (instr != tmp.OpCode)
                    flag = false;
            }

            return flag;
        }
    }
}
