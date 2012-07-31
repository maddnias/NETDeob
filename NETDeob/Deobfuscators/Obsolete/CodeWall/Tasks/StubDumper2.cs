using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Deobfuscators;

namespace NETDeob.Core.Deobfuscators.Obsolete.CodeWall.Tasks
{
    class StubDumper2 : AssemblyDeobfuscationTask
    {
        public StubDumper2(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public bool Phase1()
        {
            var target = AsmDef.EntryPoint;

            foreach(var instr in target.Body.Instructions)
                if (instr.OpCode == OpCodes.Call)
                {
                    target = instr.Operand as MethodDefinition;

                    if (target == null)
                        continue;

                    if (target.Body.Instructions.GetOpCodeCount(OpCodes.Ldc_I4) == 6 &&
                        target.Body.Instructions.GetOpCodeCount(OpCodes.Call) == 5)
                        break;
                }

            foreach(var instr in target.Body.Instructions)
                if (instr.OpCode == OpCodes.Call)
                {
                    var tmpCall = instr.Operand as MethodDefinition;

                    if (tmpCall.Parameters[0].ParameterType.Name == "Assembly")
                    {
                        break;
                    }
                }

            target =
                target.Body.Instructions.First(
                    instr =>
                    instr.OpCode == OpCodes.Castclass).Previous.Operand as MethodDefinition;


            var targetInstr = target.Body.Instructions.First(
                instr =>
                instr.OpCode == OpCodes.Call && instr.Operand.ToString().Contains("Assembly::Load(System.Byte[])")).Previous.Previous;

            var ilProc = target.Body.GetILProcessor();

            ilProc.InsertAfter(targetInstr,
                           ilProc.Create(OpCodes.Ldstr, "test"));

            ilProc.InsertAfter(targetInstr.Next.Next.Next, ilProc.Create(OpCodes.Ldarg, target.Parameters[0]));

            return true;
        }
    }
}
