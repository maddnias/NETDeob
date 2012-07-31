using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Deobfuscators.Generic.Control_Flow;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Misc.Structs__Enums___Interfaces.Tasks.Base;

namespace NETDeob.Core.Deobfuscators.Generic.Obsolete
{
    /*
     * Thanks 0xd4d and simple assembly explorer for parts of the methodcleaner!
     * No straight copying code but the general idea 
     */

    class MethodCleaner : AssemblyDeobfuscationTask
    {
        public MethodCleaner(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "Clean control flow";
        }

        [DeobfuscationPhase(1, "Construct IL blocks")]
        public static bool Phase1()
        {
            var blockList = AsmDef.FindMethods(m => true).ToDictionary(mDef => mDef, ILBlock.RetrieveBlocks);
            PhaseParam = blockList;

            Logger.VSLog(string.Format("{0} IL blocks constructed...", blockList.Count));

            return true;
        }

        [DeobfuscationPhase(2, "Merging blocks")]
        public static bool Phase2()
        {
            var blockList = PhaseParam as Dictionary<MethodDefinition, List<ILBlock>>;
            var finalList = new Dictionary<MethodDefinition, List<Instruction>>();

            foreach (var entry in blockList)
            {
                var blocks = entry.Value;
                var mDef = entry.Key;
                var cleanBody = new List<Instruction>();
                var tmpBlock = blocks.Find(block => block.StartIndex == 0);
                Instruction instr;

                while (tmpBlock.NextBlock != null)
                {
                    //mDef.Body.SimplifyMacros();
                    instr = mDef.Body.Instructions[tmpBlock.StartIndex];

                    while (mDef.Body.Instructions.IndexOf(instr.Next) <= tmpBlock.EndIndex)
                    {
                        cleanBody.Add(instr);
                        instr = instr.Next;

                        if (instr == null)
                            break;

                        if(cleanBody.Count >= 10000)
                        {
                            PhaseError = new PhaseError
                                             {
                                                 Level = PhaseError.ErrorLevel.Minor,
                                                 Message = "Internal block error!"
                                             };
                            return false;
                        }
                    }

                    tmpBlock = tmpBlock.NextBlock;
                }

                instr = mDef.Body.Instructions[tmpBlock.StartIndex];

                while (mDef.Body.Instructions.IndexOf(instr.Next) <= tmpBlock.EndIndex)
                {
                    cleanBody.Add(instr);
                    instr = instr.Next;

                    if (instr == null)
                        break;
                }

                if (cleanBody[cleanBody.Count -1].OpCode != OpCodes.Ret)
                    cleanBody.Add(mDef.Body.GetILProcessor().Create(OpCodes.Ret));

                CleanJumps(ref cleanBody);

                finalList.Add(mDef, cleanBody);
            }

            blockList.Clear();
            PhaseParam = finalList;

            return true;
        }

        [DeobfuscationPhase(3, "Replacing old bodies with clean")]
        public static bool Phase3()
        {
            var blockList = PhaseParam as Dictionary<MethodDefinition, List<Instruction>>;

            foreach (var entry in blockList)
            {
                var mDef = entry.Key;
                var cleanBody = entry.Value;

                //mDef.Body.SimplifyMacros();

                mDef.Body.Instructions.Clear();
                var ilProc = mDef.Body.GetILProcessor();

                foreach (var iinstr in cleanBody)
                    ilProc.Append(iinstr);

                //mDef.Body.OptimizeMacros();
            }

            return true;
        }

        private static void CleanJumps(ref List<Instruction> instrList)
        {
            for (var i = 0; i < instrList.Count; i++)
                if (instrList[i].IsUnconditionalJump())
                    if ((instrList[i].Operand as Instruction) == instrList[i +1]) // No need for an un.jump 1 instruction forward. :)
                        instrList.RemoveAt(i);
        }
    }
}
