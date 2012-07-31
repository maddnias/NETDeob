using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Engine;
using NETDeob.Misc;

namespace NETDeob.Deobfuscators.Phoenix_Protector
{
    public static class StringDecryptor
    {
        public static void CleanStrings(ref AssemblyDefinition asmDef)
        {
            var decryptor = FindDecryptor(asmDef);
            TypeDefinition targetType = null;

            Console.WriteLine(decryptor == null
                                  ? "[ERROR] Could not locate decryptor module! EXITING!"
                                  : "Found decryptor module (" + decryptor.Name + ") Proceeding...");

            Console.WriteLine(ObfuscatorContext.Output == ObfuscatorContext.OutputType.Verbose ? "Decrypting strings:" : "Decrypting strings...");

            foreach (var modDef in asmDef.Modules)
                foreach (var typeDef in modDef.Types)
                    foreach (var mDef in typeDef.Methods)
                        for (var i = 0; i < mDef.Body.Instructions.Count; i++)
                        {
                            var instr = mDef.Body.Instructions[i];
                            if (instr.OpCode == OpCodes.Ldstr)
                                if (instr.Next.OpCode == OpCodes.Call)
                                    if (instr.Next.Operand == decryptor)
                                    {
                                        targetType = typeDef;

                                        var oldString = instr.Operand as string;
                                        mDef.Body.Instructions.Remove(instr.Next);
                                        instr.Operand = DecryptString(instr.Operand as string);

                                        if(ObfuscatorContext.Output == ObfuscatorContext.OutputType.Verbose)
                                            Console.WriteLine("[Decrypt] " + oldString + " -> " + instr.Operand );
                                    }
                        }

            Console.WriteLine("All strings decrypted and replaced in the assembly");
            targetType.Methods.Remove(decryptor);
            Console.WriteLine("Removed decryptor module from assembly");
        }

        public static MethodDefinition FindDecryptor(AssemblyDefinition asmDef)
        {
            foreach (var modDef in asmDef.Modules)
                foreach (var typeDef in modDef.Types)
                    foreach (var mDef in typeDef.Methods)
                    {
                        var signature = new ILSignature
                                            {
                                                Start = 1,
                                                StartOpCode = OpCodes.Nop,
                                                Instructions = new List<OpCode>
                                                                   {
                                                                       OpCodes.Callvirt,
                                                                       OpCodes.Stloc_0,
                                                                       OpCodes.Ldloc_0,
                                                                       OpCodes.Newarr,
                                                                       OpCodes.Stloc_1,
                                                                       OpCodes.Ldc_I4_0,
                                                                       OpCodes.Stloc_2
                                                                   }
                                            };

                        if (mDef.HasBody) {
                            if (SignatureFinder.IsMatch(mDef, signature))
                                return mDef;
                        }
                    }

            return null;
        }

        private static string DecryptString(string str)
        {
            var length = str.Length;
            var chArray = new char[length];

            for (var i = 0; i < chArray.Length; i++)
            {
                var ch = str[i];
                var num3 = (byte)(ch ^ (length - i));
                var num4 = (byte)((ch >> 8) ^ i);
                chArray[i] = (char)((num4 << 8) | num3);
            }

            return new string(chArray);
        }
    }
}
