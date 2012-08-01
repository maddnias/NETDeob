using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Core.Engine.Utils.Extensions;

namespace NETDeob.Core.Deobfuscators.Manco.Tasks
{
    public enum Algorithm
    {
        Plain = 5,
        DES = 0,
        RC2 = 1,
        Rijndael = 2,
        TripleDES = 3
    }

    internal class MancoContext : DecryptionContext
    {
        public Instruction Caller;
        public MethodDefinition Source;
        public FieldDefinition TargetField;
        public string OrigString;

        public override string ToString()
        {
            return string.Format(@"[Decrypt] ""{0}"" -> ""{1}""", OrigString.Truncate(10), PlainText);
        }
    }

    internal class StringDecryptor : AssemblyDeobfuscationTask, IStringDecryptor<MancoContext>
    {
        string _key;
        Algorithm _strAlgo;
        MethodDefinition _decryptor;

        public StringDecryptor(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "String decryption";
        }

        [DeobfuscationPhase(1, "Locating decryptor method")]
        public bool Phase1()
        {
            var targetType = AsmDef.MainModule.GetAllTypes().FirstOrDefault(t => BaseIsDecryptor(t));
            _decryptor = ExtractDecryptor(targetType, out _key, out _strAlgo);

            if (_decryptor == null)
            {
                ThrowPhaseError("Could not locate decryptor method!", 1, false);
                return false;
            }

            MarkMember(_decryptor.DeclaringType);

            if (_key == null && _strAlgo != Algorithm.Plain)
            {
                ThrowPhaseError("Key is null!", 1, false);
                return false;
            }

            Logger.VSLog("Found decryptor method at " + _decryptor.Name.Truncate(5));
            Logger.VSLog("Identified string encryption algorithm is " + _strAlgo.ToString());

            return true;
        }

        [DeobfuscationPhase(2, "Construct decryption entries")]
        public bool Phase2()
        {
            var entryList = ConstructEntries(null).ToList();

            PhaseParam = entryList;
            Logger.VSLog(string.Format("{0} decryption entries constructed", entryList.Count));
            return true;
        }

        [DeobfuscationPhase(3, "Decrypt strings")]
        public bool Phase3()
        {
            for (var i = 0; i < (PhaseParam as List<MancoContext>).Count; i++)
            {
                var entry = (PhaseParam as List<MancoContext>)[i];
                DecryptEntry(ref entry);

                Logger.VLog(entry.ToString());
            }

            Logger.VSLog("All strings decrypted...");
            return true;
        }

        [DeobfuscationPhase(4, "Process entries", true)]
        public bool CleanUp()
        {
            foreach (var entry in PhaseParam)
                ProcessEntry(entry);

            return true;
        }

        #region Reversed methods

        private static string DecryptString(string paramStr, string key, Algorithm strAlgo)
        {
            #region Plain

            if (strAlgo == Algorithm.Plain)
                if (paramStr != string.Empty)
                {
                    int num2;
                    int length = (paramStr.Length/2) + (paramStr.Length%2);
                    string str = paramStr.Substring(0, length);
                    string str2 = paramStr.Substring(length);
                    StringBuilder builder = new StringBuilder(paramStr.Length);
                    char[] chArray = new char[str.Length];
                    char[] chArray2 = new char[str2.Length];
                    bool flag = (str.Length%2) == 0;
                    for (num2 = 0; num2 < str.Length; num2 += 2)
                    {
                        chArray[num2/2] = str[num2];
                        if ((num2 < (str.Length - 1)) || flag)
                        {
                            chArray[(str.Length - (num2/2)) - 1] = str[num2 + 1];
                        }
                    }
                    flag = (str2.Length%2) == 0;
                    for (num2 = 0; num2 < str2.Length; num2 += 2)
                    {
                        chArray2[num2/2] = str2[num2];
                        if ((num2 < (str2.Length - 1)) || flag)
                        {
                            chArray2[(str2.Length - (num2/2)) - 1] = str2[num2 + 1];
                        }
                    }
                    flag = str.Length == str2.Length;
                    for (num2 = 0; num2 < str.Length; num2++)
                    {
                        builder.Append(chArray[num2]);
                        if ((num2 < (str.Length - 1)) || flag)
                        {
                            builder.Append(chArray2[num2]);
                        }
                    }
                    return builder.ToString();
                }

            #endregion

            if (strAlgo != Algorithm.Plain)
            {
                var transform = CreateAlgo(key, strAlgo).CreateDecryptor();
                var buf = Convert.FromBase64String(paramStr);

                return Encoding.Default.GetString(transform.TransformFinalBlock(buf, 0, buf.Length));
            }

            ThrowPhaseError("Failed to decrypt string!", 0, false);

            return "undecrypted";
        }
        private static SymmetricAlgorithm CreateAlgo(string paramStr, Algorithm strAlgo)
        {
            SymmetricAlgorithm algorithm = GetAlgo(strAlgo);
            ASCIIEncoding encoding = new ASCIIEncoding();
            algorithm.IV = encoding.GetBytes(paramStr.Substring(0, (strAlgo == Algorithm.Rijndael ? 0x10 : 8)));
            algorithm.Key = encoding.GetBytes(paramStr.Substring((strAlgo == Algorithm.Rijndael ? 0x10 : 8)));
            return algorithm;
        }
        private static SymmetricAlgorithm GetAlgo(Algorithm strAlgo)
        {
            switch (strAlgo)
            {
                case Algorithm.DES:
                    return new DESCryptoServiceProvider();

                case Algorithm.RC2:
                    return new RC2CryptoServiceProvider();

                case Algorithm.Rijndael:
                    return new RijndaelManaged();

                case Algorithm.TripleDES:
                    return new TripleDESCryptoServiceProvider();
            }

            return null;
        }

        #endregion

        public static MethodDefinition ExtractDecryptor(TypeDefinition tDef, out string key, out Algorithm strAlgo)
        {
            var cctor = tDef.GetStaticConstructor();
            var mTarget = cctor.Body.Instructions.GetOperandAt<MethodReference>(OpCodes.Call, 0).Resolve();
            var decryptor =
                mTarget.DeclaringType.Methods.FirstOrDefault(
                    m => m.Parameters.Count == 4 && m.ReturnType.ToString().Contains("String"));

            decryptor = decryptor ??
                        (cctor.Body.Instructions.FirstOfOpCode(OpCodes.Call).Operand as MethodReference).Resolve();

            key = mTarget.Body.Instructions.GetOperandAt<string>(OpCodes.Ldstr, 0);
            if (decryptor.Body.Variables.Count != 10)
                strAlgo = (Algorithm)mTarget.Body.Instructions.First(i => i.IsLdcI4()).GetLdcI4();
            else
                strAlgo = Algorithm.Plain;

            return decryptor;
        }
        public static Dictionary<FieldDefinition, string> DecryptStrings(TypeDefinition type, string key, Algorithm strAlgo)
        {
            var outDict = new Dictionary<FieldDefinition, string>();
            var curInstr = type.Methods.First(method => method.Name == ".cctor").Body.Instructions[0];

            while (curInstr.Next.OpCode != OpCodes.Ret)
            {
                if (curInstr.OpCode == OpCodes.Ldstr)
                {
                    var decryptedStr = "";

                    switch (strAlgo)
                    {
                        case Algorithm.Plain:
                            decryptedStr = DecryptString(curInstr.Operand as string, "", strAlgo);
                            outDict.Add(curInstr.Next.Next.Operand as FieldDefinition, decryptedStr);
                            break;

                        case Algorithm.RC2:
                        case Algorithm.DES:
                        case Algorithm.Rijndael:
                        case Algorithm.TripleDES:

                            decryptedStr = DecryptString(curInstr.Operand as string, key, strAlgo);
                            outDict.Add(curInstr.Next.Next.Operand as FieldDefinition, decryptedStr);

                            break;
                    }

                    Logger.VLog(string.Format(@"[Decrypt] ""{0}"" -> ""{1}""", curInstr.Operand as string, decryptedStr));
                }

                curInstr = curInstr.Next;
            }

            return outDict;
        }
        private static string FindAssociatedString(IMemberDefinition field)
        {
            var cctor = field.DeclaringType.GetStaticConstructor();

            return
                cctor.Body.Instructions.First(
                    i =>
                    i.OpCode == OpCodes.Stsfld && (i.Operand as FieldReference).SafeRefCheck(field as FieldDefinition)).
                    Previous.
                    Previous.Operand as string;
        }

        public bool BaseIsDecryptor(params object[] param)
        {
            var cctor = (param[0] as TypeDefinition).GetStaticConstructor();

            if (cctor == null)
                return false;

            if (cctor.Body.Instructions.GetOpCodeCount(OpCodes.Call) <= 0)
                return false;

            var mTarget = cctor.Body.Instructions.GetOperandAt<MethodReference>(OpCodes.Call, 0).Resolve();
            var decryptor =
                mTarget.DeclaringType.Methods.FirstOrDefault(
                    m => m.Parameters.Count == 4 && m.ReturnType.ToString().Contains("String"));

            if (decryptor == null)
                if (mTarget.Body.Instructions.GetOpCodeCount(OpCodes.Call) <= 0)
                    return false;

            return true;
        }
        public void InitializeDecryption(object param)
        {
            throw new NotImplementedException();
        }
        public void DecryptEntry(ref MancoContext entry)
        {
            entry.PlainText = DecryptString(FindAssociatedString(entry.TargetField), _key, _strAlgo);
        }
        public void ProcessEntry(MancoContext entry)
        {
            var ilProc = entry.Source.Body.GetILProcessor();
            ilProc.Replace(entry.Caller, ilProc.Create(OpCodes.Ldstr, entry.PlainText));
        }
        public IEnumerable<MancoContext> ConstructEntries(object param)
        {
            foreach(var mDef in AsmDef.FindMethods(m => m.HasBody))
                if(mDef.Body.Instructions.GetOpCodeCount(OpCodes.Ldsfld) > 0)
                {
                    foreach(var fLoad in mDef.Body.Instructions.Where(i => i.OpCode == OpCodes.Ldsfld))
                    {
                        if ((fLoad.Operand as FieldReference).Resolve().DeclaringType != _decryptor.DeclaringType)
                            continue;

                        yield return new MancoContext
                                         {
                                             Caller = fLoad,
                                             Source = mDef,
                                             TargetField = (fLoad.Operand as FieldReference).Resolve(),
                                             OrigString = FindAssociatedString((fLoad.Operand as FieldReference).Resolve())
                                         };
                    }
                }
        }
    }
}
