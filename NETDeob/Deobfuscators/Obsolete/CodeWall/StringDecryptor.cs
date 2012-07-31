using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Deobfuscators.Generic;
using NETDeob.Engine;
using NETDeob.Misc;

namespace NETDeob.Deobfuscators.CodeWall
{
    public static class StringDecryptor
    {
        public static void CleanStrings(ref AssemblyDefinition asmDef)
        {
            if (ObfuscatorContext.Output == ObfuscatorContext.OutputType.Verbose) 
                Console.WriteLine("Decrypting strings:\n");

            var decryptors = FindDecryptors(asmDef);

            Console.WriteLine("Found " + decryptors.Count + " decryptor modules");

            if (ObfuscatorContext.Output == ObfuscatorContext.OutputType.Subtle)
                Console.WriteLine("Decrypting strings...");

            foreach (var modDef in asmDef.Modules)
                foreach (var typeDef in modDef.Types)
                    foreach (var mDef in typeDef.Methods)
                        for (var i = 0; i < mDef.Body.Instructions.Count; i++)
                        {
                            if(mDef.Body.Instructions[i].OpCode == OpCodes.Call)
                                if (decryptors.Contains(mDef.Body.Instructions[i].Operand))
                                {
                                    var target = decryptors.First(method => method == mDef.Body.Instructions[i].Operand);
                                    var decModifiers = FindDecryptionModifiers(target);
                                    var oldStr = "";
                                    var decKey = new int[3];

                                    decKey[0] = (Int32) mDef.Body.Instructions[i].Previous.Previous.Previous.Operand;
                                    decKey[1] = (Int32) mDef.Body.Instructions[i].Previous.Previous.Operand;
                                    decKey[2] = (Int32) mDef.Body.Instructions[i].Previous.Operand;

                                    if (ObfuscatorContext.Output == ObfuscatorContext.OutputType.Verbose)
                                        oldStr = string.Concat("Key(", decKey[0].ToString(), " & ", decKey[1].ToString(),
                                                               " & ",
                                                               decKey[2], ")");

                                    i -= 4;

                                    mDef.Body.Instructions.Remove(mDef.Body.Instructions[++i]);
                                    mDef.Body.Instructions.Remove(mDef.Body.Instructions[i]);
                                    mDef.Body.Instructions.Remove(mDef.Body.Instructions[i]);
                                    mDef.Body.Instructions.Remove(mDef.Body.Instructions[i]);

                                    var resData =
                                        (mDef.Module.Resources.First(
                                            res => res.Name == FindStringResource(target, mDef.Module)) as
                                         EmbeddedResource).GetResourceData();

                                    mDef.Body.Instructions.Insert(i,
                                                                  mDef.Body.GetILProcessor().Create(OpCodes.Ldstr,
                                                                                                    DecryptString(
                                                                                                        decKey,
                                                                                                        decModifiers,
                                                                                                        resData,
                                                                                                        asmDef)));
                                    if (ObfuscatorContext.Output == ObfuscatorContext.OutputType.Verbose)
                                        Console.WriteLine(
                                            "[Decrypt] " + oldStr + " -> " + mDef.Body.Instructions[i].Operand);
                                }
                        }

            Console.WriteLine("All strings decrypted and replaced in assembly");
            Console.WriteLine("Removed all decryptor modules from assembly");

            foreach (var method in decryptors)
            {
                var targetMod = method.Module;
                var resName = FindStringResource(method, method.Module);

                targetMod.Resources.Remove(targetMod.Resources.First(res => res.Name == resName));
                targetMod.Types.Remove(method.DeclaringType);
            }

            Console.WriteLine("Removed all resources containing encrypted strings from assembly");

            Renamer.RenameMembers(ref asmDef, new RenamingScheme(false) {Resources = true});
        }

        private static string DecryptString(int[] decKey, int[] decModifiers, byte[] resData, AssemblyDefinition asmDef)
        {
            var key = decKey[0] ^ decModifiers[0];
            int num2 = decKey[1];

            byte[] publicKeyToken = asmDef.Name.PublicKeyToken;

            if ((publicKeyToken != null) && (publicKeyToken.Length == 8))
            {
                int num3 = BitConverter.ToInt32(publicKeyToken, 0);
                int num4 = BitConverter.ToInt32(publicKeyToken, 4);
                num2 ^= num3 ^ num4;
            }
            else
            {
                num2 ^= decModifiers[1];
            }

            int num5 = decKey[2] ^ decModifiers[2];
            byte[] bytes = new byte[num5];
            int num6 = 0;

            for (int i = num2; i < (num2 + num5); i++)
            {
                bytes[num6++] = resData[i];
            }

            byte[] buffer4 = new byte[num5];
            new Random(key).NextBytes(buffer4);

            for (int j = 0; j < num5; j++)
            {
                bytes[j] = (byte)(bytes[j] ^ buffer4[j]);
            }

            string str = Encoding.Unicode.GetString(bytes);

            return str;

        }

        private static List<MethodDefinition> FindDecryptors(AssemblyDefinition asmDef)
        {
            var signature = new ILSignature
            {
                Start = 0,
                StartOpCode = OpCodes.Ldsfld,
                Instructions = new List<OpCode>
                                                       {
                                                           OpCodes.Ldsfld,
                                                           OpCodes.Dup,
                                                           OpCodes.Stloc_S,
                                                           OpCodes.Call,
                                                           OpCodes.Ldarg_0,
                                                           OpCodes.Ldc_I4
                                                       }
            };

            
            var decryptors = new List<MethodDefinition>();

            foreach (var modDef in asmDef.Modules)
                foreach (var typeDef in modDef.Types)
                    foreach (var mDef in typeDef.Methods)
                        if (SignatureFinder.IsMatch(mDef, signature))
                            decryptors.Add(mDef);

            return decryptors;
        }

        private static string FindStringResource(MethodDefinition decryptor, ModuleDefinition modDef)
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

            for (int x = 0; x < target.Body.Instructions.Count; x++)
            {
                if (target.Body.Instructions[x].OpCode == OpCodes.Xor)
                    tmp++;

                if (target.Body.Instructions[x].OpCode == OpCodes.Xor && tmp == 1)
                    decModifiers[0] = (Int32)target.Body.Instructions[x].Previous.Operand;

                if (target.Body.Instructions[x].OpCode == OpCodes.Xor && tmp == 4)
                    decModifiers[1] = (Int32)target.Body.Instructions[x].Previous.Operand;

                else if (target.Body.Instructions[x].OpCode == OpCodes.Xor && tmp == 5)
                    decModifiers[2] = (Int32)target.Body.Instructions[x].Previous.Operand;
            }

            return decModifiers;
        }
    }

}
