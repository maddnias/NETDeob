using System.Collections.Generic;
using Mono.Cecil;
using NETDeob.Core.Deobfuscators.Confuser.Obsolete.Utils;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;

namespace NETDeob.Deobfuscators.Confuser.Obsolete
{
    class ConstantsDecryptor : AssemblyDeobfuscationTask
    {
        public struct DecryptionInfo
        {
            public MethodDefinition Method;
            public DynamicEvaluator[] Evaluators;

            public int Key1;
            public int Key2;

            public long[] DecryptionModifiers;
        }

        public ConstantsDecryptor(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        [DeobfuscationPhase(1, "Locate decryption methods")]
        public static bool Phase1()
        {
            var testList = new List<DecryptionInfo>();

            //var mDef = AsmDef.MainModule.Types.First(t => t.Name == "<Module>").GetStaticConstructor();
            //new ConstantFolder().ApplyFolding(ref mDef);

            //for (int i = 0; i < AsmDef.FindMethods(m => true).Count; i++)
            //{
            //    var mDef = AsmDef.FindMethods(m => true)[i];
            //    foreach (var instr in mDef.Body.Instructions)
            //        if (instr.OpCode == OpCodes.Call && instr.Operand != null)
            //            if (IsDecryptor((instr.Operand as MethodReference).Resolve()))
            //            {
            //                var _mDef = (instr.Operand as MethodReference).Resolve();
            //                new ConstantFolder().ApplyFolding(ref _mDef);

            //                break;
            //            }
            //}

            PhaseParam = testList;

            return true;
        }

        public static bool IsDecryptor(MethodDefinition mDef)
        {
            if(mDef.HasParameters)
                    if(mDef.Parameters.Count == 2)
                        if (mDef.Parameters[0].ParameterType.Name == "UInt32" && mDef.Parameters[1].ParameterType.Name == "UInt64")
                        {
                            return true;
                        }

            return false;
        }

        //// OBSOLETE
        //public static DynamicEvaluator[] ParseEvaluators(MethodDefinition mDef)
        //{
        //    var outList = new List<DynamicEvaluator>();
        //    var instrList = mDef.Body.Instructions;
        //    ExpressionScheme expScheme;
        //    var idx = 0;
        //    var ptr = 0;

        //    #region Mutations

        //    /* First mutation */

        //    idx = instrList.FindInstruction(instr => instr.IsStLoc(), 0).GetInstructionIndex(instrList);
        //    expScheme =
        //        new ExpressionScheme(instrList.GetInstructionBlock(--idx,
        //                                                           (instr =>
        //                                                            instr.OpCode == OpCodes.Ldloc ||
        //                                                            instr.OpCode == OpCodes.Ldloc_S)));
        //    ptr += idx + expScheme.PartCount;

        //    /* Second mutation (hash) */

        //    idx = instrList.FindInstruction(instr => instr.IsStLoc(), 0, ptr).GetInstructionIndex(instrList);
        //    expScheme =
        //        new ExpressionScheme(instrList.GetInstructionBlock(--idx,
        //                                                           (instr =>
        //                                                            instr.IsLdLoc())));
        //    ptr += idx + expScheme.PartCount;

        //    idx = instrList.FindInstruction(instr => instr.IsStLoc(), 0, ptr).GetInstructionIndex(instrList);
        //    expScheme =
        //        new ExpressionScheme(instrList.GetInstructionBlock(--idx,
        //                                                           (instr =>
        //                                                            instr.IsStLoc())));
        //    outList.Add(new DynamicEvaluator(expScheme));
        //    /* Third mutation (hash) */

        //    idx = instrList.FindInstruction(instr => instr.IsLdLoc(), 9).GetInstructionIndex(instrList);
        //    expScheme =
        //        new ExpressionScheme(instrList.GetInstructionBlock(idx,
        //                                                           (instr =>
        //                                                            instr.OpCode == OpCodes.Conv_U8)));

        //    outList.Add(new DynamicEvaluator(expScheme));

        //    #endregion

        //    return outList.ToArray();
        //}
        //public static long[] ParseModifiers(MethodDefinition mDef)
        //{
        //    var modifiers = new long[3];
        //    var instrList = mDef.Body.Instructions;

        //    modifiers[0] = (long)Convert.ChangeType(instrList.FindInstruction(instr => instr.IsStLoc(), 1).Next.Operand, typeof(long));
        //    modifiers[1] = (long)Convert.ChangeType(instrList.FindInstruction(instr => instr.IsStLoc(), 2).Next.Operand, typeof(long));

        //    return modifiers;
        //}

        //#region Decryption

        //private static byte[] _key;

        //static T smethod_0<T>(DecryptionInfo decInfo)
        //{
        //    object obj2;
        //    uint num6 = (uint) (decInfo.Method.DeclaringType.MetadataToken.ToUInt32()*decInfo.Key1);
        //    ulong num9 = decInfo.Evaluators[0].EvaluateExpression<ulong>()*num6;

        //    // Redundant initializers

        //    ulong num = ((ulong)decInfo.DecryptionModifiers[0])*num9; // Seems to be constant
        //    ulong num7 = ((ulong)decInfo.DecryptionModifiers[1])*num9; // Seems to be constant

        //    num9 *= num9;

        //    ulong num5 = 14695981039346656037L; // Seems to be constant
        //    while (num9 != 0L)
        //    {
        //        num5 *= decInfo.Evaluators[1].EvaluateExpression<ulong>();
        //        num5 = (num5 ^ num9) + ((num ^ num7)*(decInfo.Evaluators[2].EvaluateExpression<ulong>()));
        //        num *= 2166136261L; // Constant?
        //        num7 *= 2731457202L; // Constant?
        //        num9 = num9 >> 8;
        //    }

        //    ulong num3 = num5 ^ (ulong)decInfo.DecryptionModifiers[2];
        //    uint key = (uint) (num3 >> 32); // Seems to always evaluate to 32
        //    uint num8 = (uint) num3;

        //    byte[] destinationArray = new byte[num8];

        //    Array.Copy(_key, key, destinationArray, 0L, (long)num8);

        //    byte[] bytes = BitConverter.GetBytes((int) (MethodBase.GetCurrentMethod().MetadataToken ^ decInfo.DecryptionModifiers[2]));

        //    for (int i = 0; i < destinationArray.Length; i++)
        //    {
        //        destinationArray[i] = (byte) (destinationArray[i] ^ bytes[(int) ((IntPtr) ((key + i)%(4L)))]);
        //    }
        //    if (typeof (T) == typeof (string))
        //    {
        //        obj2 = Encoding.UTF8.GetString(destinationArray);
        //    }
        //    else
        //    {
        //        T[] dst = new T[1];
        //        Buffer.BlockCopy(destinationArray, 0, dst, 0, Marshal.SizeOf(default(T)));
        //        obj2 = dst[0];
        //    }

        //    return (T) obj2;
        //}

        //#endregion
    }
}
