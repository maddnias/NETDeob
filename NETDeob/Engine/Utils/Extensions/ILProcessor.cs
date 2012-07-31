using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace NETDeob.Core.Engine.Utils.Extensions
{
    public static class ILProcessorExt
    {
        public static void RemoveBlock(this ILProcessor ilProc, Instruction start, Instruction end)
        {
            var curInstr = start;

            while (curInstr != null && curInstr.Next != end.Next)
            {
                curInstr = curInstr.Next;
                ilProc.Remove(curInstr.Previous);
            }
        }
    }
}
