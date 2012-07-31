using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;

namespace NETDeob.Deobfuscators.CodeWall.Tasks
{
    public class StringDecryptor : AssemblyDeobfuscationTask
    {
        public class CodewallEntry : DecryptionContext
        {
            public int Key1;
            public int Key2;
            public int Key3;
            public int Modifier1;
            public int Modifier2;
            public int Modifier3;

            public int InsertIndex;

            public MethodDefinition SourceMethod;
            public MethodDefinition DecryptionMethod;
            public List<Instruction> Instructions;

            public override string ToString()
            {
                return string.Format("[Decrypt] Key({0}, {1}, {2}) -> {3}", Key1, Key2, Key3, PlainText);
            }
        }

        public StringDecryptor(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "String decryption";
        }

        [DeobfuscationPhase(1, "Locating decryptor methods")]
        public static bool Phase1()
        {
            PhaseParam = FindDecryptors();

            foreach (var mDef in PhaseParam)
                MarkMember(mDef);

            Logger.VSLog("Found " + PhaseParam.Count + " decryptor methods...");
            return true;
        }

        [DeobfuscationPhase(2, "Construct decryption entries")]
        public static bool Phase2()
        {
            ConstructEntries();

            Logger.VSLog(string.Format("{0} entries were constructed...", PhaseParam.Count));
            return true;
        }

        [DeobfuscationPhase(3, "Decrypt strings & process entries")]
        public static bool Phase3()
        {
            ProcessEntries();

            Logger.VSLog(string.Format("All entries have been processed..."));
            Logger.VSLog(string.Format("All encrypted strings have been replaced with plaintext..."));

            return true;
        }

        private static void ProcessEntries()
        {
            foreach (var entry in (PhaseParam as List<CodewallEntry>))
            {
                var resData =
                    (MarkMember<EmbeddedResource>(entry.SourceMethod.Module.Resources.First(
                        res => res.Name == FindStringResource(entry.DecryptionMethod)))).GetResourceData();

                var ilProc = entry.SourceMethod.Body.GetILProcessor();

                entry.SourceMethod.Body.Instructions.Insert(entry.InsertIndex,
                                                            ilProc.Create(OpCodes.Ldstr,
                                                                          DecryptString(
                                                                              new[]
                                                                                  {
                                                                                      entry.Key1,
                                                                                      entry.Key2, 
                                                                                      entry.Key3
                                                                                  },
                                                                              new[]
                                                                                  {
                                                                                      entry.Modifier1,
                                                                                      entry.Modifier2,
                                                                                      entry.Modifier3
                                                                                  },
                                                                              resData,
                                                                              AsmDef)));
            }


            foreach (var entry in PhaseParam)
                foreach (var instr in entry.Instructions)
                    MarkMember(instr, entry.SourceMethod);
        }
        private static void ConstructEntries()
        {
            var decryptedStrings = new List<CodewallEntry>();

            foreach (var mDef in from modDef in AsmDef.Modules from typeDef in modDef.Types from mDef in typeDef.Methods where mDef.HasBody select mDef)
            {
                var decryptors = (List<MethodDefinition>)PhaseParam;

               // //mDef.Body.SimplifyMacros();

                for (var i = 0; i < mDef.Body.Instructions.Count; i++)
                {
                    if (mDef.Body.Instructions[i].OpCode == OpCodes.Call)
                        if (decryptors.Contains(mDef.Body.Instructions[i].Operand))
                        {
                            var tmpEntry = new CodewallEntry
                                               {
                                                   Instructions = new List<Instruction>(),
                                                   DecryptionMethod =
                                                       decryptors.First(
                                                           method => method == mDef.Body.Instructions[i].Operand)
                                               };

                            var decModifiers = FindDecryptionModifiers(tmpEntry.DecryptionMethod);

                            tmpEntry.Modifier1 = decModifiers[0];
                            tmpEntry.Modifier2 = decModifiers[1];
                            tmpEntry.Modifier3 = decModifiers[2];
                            tmpEntry.Key1 = (Int32)mDef.Body.Instructions[i].Previous.Previous.Previous.Operand;
                            tmpEntry.Key2 = (Int32)mDef.Body.Instructions[i].Previous.Previous.Operand;
                            tmpEntry.Key3 = (Int32)mDef.Body.Instructions[i].Previous.Operand;

                            tmpEntry.SourceMethod = mDef;

                            i -= 4;

                            tmpEntry.Instructions.Add(mDef.Body.Instructions[++i]);
                            tmpEntry.Instructions.Add(mDef.Body.Instructions[++i]);
                            tmpEntry.Instructions.Add(mDef.Body.Instructions[++i]);
                            tmpEntry.Instructions.Add(mDef.Body.Instructions[++i]);

                            tmpEntry.InsertIndex = i;

                            decryptedStrings.Add(tmpEntry);

                            i += 4;
                        }
                }

               // //mDef.Body.OptimizeMacros();
            }
            PhaseParam = decryptedStrings;
        }
        private static List<MethodDefinition> FindDecryptors()
        {
            return
                AsmDef.FindMethods(
                    mDef =>
                    mDef.Body.Instructions.GetOpCodeCount(OpCodes.Xor) == 6 &&
                    mDef.Body.Instructions.GetOpCodeCount(OpCodes.Ldc_I4) == 3).ToList();
        }
        private static string FindStringResource(MethodDefinition decryptor)
        {
            var body = decryptor.Body;
            var count = 0;

            while(count < body.Instructions.Count)
            {
                if (body.Instructions[count].OpCode == OpCodes.Ldstr)
                    if((body.Instructions[count].Operand as string).Length == 32)
                        return body.Instructions[count].Operand as string;
                count++;
            }

            return null;
        }
        private static int[] FindDecryptionModifiers(MethodDefinition mDef)
        {
            int tmp = 0;
            var target = mDef;
            var decModifiers = new int[3];

            foreach (Instruction t in target.Body.Instructions)
            {
                if (t.OpCode == OpCodes.Xor)
                    tmp++;

                if (t.OpCode == OpCodes.Xor && tmp == 1)
                    decModifiers[0] = (Int32)t.Previous.Operand;

                if (t.OpCode == OpCodes.Xor && tmp == 4)
                    decModifiers[1] = (Int32)t.Previous.Operand;

                else if (t.OpCode == OpCodes.Xor && tmp == 5)
                    decModifiers[2] = (Int32)t.Previous.Operand;
            }

            return decModifiers;
        }

        #region Reversed methods

        private static string DecryptString(int[] decKey, int[] decModifiers, byte[] resData, AssemblyDefinition asmDef)
        {
            var key = decKey[0] ^ decModifiers[0];
            var key2 = decKey[1];
            var key3 = decKey[2] ^ decModifiers[2];
            var buf = new byte[key3];

            var publicKeyToken = asmDef.Name.PublicKeyToken;

            if(publicKeyToken.Length == 8)
                key2 ^= BitConverter.ToInt32(publicKeyToken, 0) ^ BitConverter.ToInt32(publicKeyToken, 4);
            else
                key2 ^= decModifiers[1];

            for (int i = key2, x = 0; i < (key2 + key3); i++)
                buf[x++] = resData[i];

            byte[] buf2 = new byte[buf.Length];
            new Random(key).NextBytes(buf2);

            for (var i = 0; i < key3; i++)
                buf[i] = (byte)(buf[i] ^ buf2[i]);

            return Encoding.Unicode.GetString(buf);
        }

        #endregion
    }

}
