using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Misc;

namespace NETDeob.Deobfuscators.Generic.Control_Flow
{
    public class ILBlock
    {
        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }

        public ILBlock NextBlock { get; private set; }
        
        public static List<ILBlock> RetrieveBlocks(MethodDefinition mDef)
        {
            var outList = new List<ILBlock>();

            if (mDef == null || !mDef.HasBody)
                return outList;

            int fIndex = 0, lIndex = 0;
            var instrList = mDef.Body.Instructions;

            while (lIndex < instrList.Count)
            {
                if (IsDelimiter(instrList[lIndex]) || lIndex + 1 >= instrList.Count)
                {
                    outList.Add(new ILBlock
                                    {
                                        StartIndex = fIndex,
                                        EndIndex = lIndex
                                    });

                    fIndex = lIndex + 1;
                    lIndex = fIndex;
                }
                else
                    lIndex++;
            }

            LinkBlocks(ref outList, mDef);

            return outList;
        }

        private static void LinkBlocks(ref List<ILBlock> blocks, MethodDefinition mDef)
        {
            var instrList = mDef.Body.Instructions;

            foreach(var block in blocks)
            {
                Instruction target = null;

                if (instrList[block.EndIndex].Operand is Instruction)
                    target = instrList[block.EndIndex].Operand as Instruction;

                if (target == null) continue;
                var targetIdx = instrList.IndexOf(target);

                foreach(var block2 in blocks)
                {
                    if (block == block2)
                        continue;

                    if(block2.StartIndex <= targetIdx && targetIdx <= block2.EndIndex)
                        block.NextBlock = block2;
                }
            }
        }

        private static bool IsDelimiter(Instruction ins)
        {
            return ins.OpCode.FlowControl == FlowControl.Branch ||
                   ins.OpCode.FlowControl == FlowControl.Return ||
                   ins.OpCode.FlowControl == FlowControl.Throw;
        }
    }
}
