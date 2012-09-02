using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;
using NETDeob.Core.Engine.Utils.Extensions;

namespace NETDeob.Deobfuscators.Confuser.Tasks._1_7
{
    public class Confuser1_7Entry : DecryptionContext
    {
        public MethodDefinition Caller;

        public int Id;
        public uint MDToken;

        public IEnumerable<Instruction> BadInstructions;

        public override string ToString()
        {
            return string.Format(@"[Decrypt] ""({0})"" -> ""{1}""", Id, PlainText);
        }
    }

    class ConstantsDecryptor : AssemblyDeobfuscationTask, IStringDecryptor<Confuser1_7Entry>
    {
        public static int[] Modifiers;

        private const int BufSize = 4096;
        private static byte[] _rawData;

        public ConstantsDecryptor(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "Decrypt and replace encrypted constants";
        }

        [DeobfuscationPhase(1, "Locate decryption method")]
        public bool Phase1()
        {
            var decryptor = AsmDef.FindMethod(m => BaseIsDecryptor(m));

            if(decryptor == null){
                ThrowPhaseError("No string encryption?", 0, true);
                return true;
            }

            var resName = decryptor.Body.Instructions.GetOperandAt<string>(OpCodes.Ldstr, 1);

            MarkMember(decryptor);
            MarkMember(decryptor.DeclaringType.Methods.FirstOrDefault(m => m.IsConstructor));

            if(resName == null){
                ThrowPhaseError("Could not locate string resource!", 1, false);
                return false;
            }

            if (Globals.DeobContext.ResStreams.Count == 0)
            {
                PhaseParam = new object[]
                                 {
                                     Assembly.LoadFile(Globals.DeobContext.InPath).GetManifestResourceStream(resName),
                                     decryptor
                                 };

                // Resource decryption compability
                Globals.DeobContext.InPath = Globals.DeobContext.InPath.Replace("_resdump.exe", null);
            }
            else
                PhaseParam = new object[] { Globals.DeobContext.ResStreams.First(res => res.Name == resName), decryptor };

            if (PhaseParam[0] == null){
                ThrowPhaseError("Null stream!", 1, false);
                return false;
            }

            MarkMember(AsmDef.MainModule.Resources.First(r => r.Name == resName));
            Logger.VSLog("Found decryptor method at " + decryptor.Name);

            return true;
        }

        /// <summary>
        /// PhaseParam = { assembly, decryptor }
        /// </summary>
        [DeobfuscationPhase(2, "Decompress resource data")]
        public static bool Phase2()
        {
            var inStream = PhaseParam[0];
            var outStream = new MemoryStream();

            using(var deflater = new DeflateStream(inStream, CompressionMode.Decompress))
            {
                var buf = new byte[BufSize];
                var count = deflater.Read(buf, 0, BufSize);

                while(count != 0)
                {
                    outStream.Write(buf, 0, count);
                    count = deflater.Read(buf, 0, BufSize);
                }
            }

            _rawData = outStream.ToArray();

            if(_rawData.Length == 0)
            {
                ThrowPhaseError("Failed to decompress resource!", 1, false);
                return false;
            }

            Logger.VSLog(string.Format("Decompressed constants resource ({0} bytes)...", _rawData.Length));

            return true;
        }

        /// <summary>
        /// PhaseParam = { assembly, decryptor }
        /// </summary>
        [DeobfuscationPhase(3, "Construct decryption entries")]
        public bool Phase3()
        {
            var decEntries = ConstructEntries(null).ToList();
            PhaseParam = new object[] {decEntries, PhaseParam[1]};

            Logger.VSLog(string.Format("{0} decryption entries constructed...", decEntries.Count));
            return true;
        }

        /// <summary>
        /// PhaseParam = { decEntries, decryptor }
        /// </summary>
        [DeobfuscationPhase(4, "Decrypt constants")]
        public bool Phase4()
        {
            var decEntries = PhaseParam[0];
            InitializeDecryption(PhaseParam[1]);

            foreach (var entry in decEntries)
            {
                var e = entry as Confuser1_7Entry;
                DecryptEntry(ref e);

                Logger.VLog(entry.ToString());
            }

            Logger.VSLog(string.Format("{0} constants was decrypted...", decEntries.Count));

            PhaseParam = decEntries;
            return true;
        }

        /// <summary>
        /// PhaseParam = decEntries
        /// </summary>
        [DeobfuscationPhase(5, "Replace constants")]
        public bool Phase5()
        {
            var decEntries = PhaseParam as List<Confuser1_7Entry>;

            foreach (var entry in decEntries)
                ProcessEntry(entry);

            Logger.VSLog("All encrypted constants was replaced with plaintext...");

            return true;
        }

        public bool BaseIsDecryptor(params object[] param)
        {
            var mDef = param[0] as MethodDefinition;

            if (!mDef.HasBody)
                return false;

            if (!mDef.HasParameters || !mDef.Body.HasVariables)
                return false;

            if (mDef.Body.Variables.Count != 20)
                return false;

            if (mDef.Parameters.Count != 1)
                return false;

            return true;
        }

        public void InitializeDecryption(object param)
        {
            var mDef = param as MethodDefinition;

            Modifiers = new int[4];

            Modifiers[0] = mDef.Body.Instructions.GetOperandAt<int>(OpCodes.Ldc_I4, 3);
            Modifiers[1] = mDef.Body.Instructions.GetOperandAt<int>(OpCodes.Ldc_I4, 4);
            Modifiers[2] = mDef.Body.Instructions.GetOperandAt<int>(OpCodes.Ldc_I4, 5);
            Modifiers[3] = mDef.Body.Instructions.GetOperandAt<int>(OpCodes.Ldc_I4, 12);
        }

        public void DecryptEntry(ref Confuser1_7Entry entry)
        {
            uint num3 = (uint)Modifiers[0] ^ entry.MDToken;
            uint num4 = (uint)Modifiers[1];
            uint num5 = (uint)Modifiers[2];

            for (uint i = 1; i <= 64; i++)
            {
                num3 = (uint)(((num3 & 16777215) << 8) | ((num3 & -16777216) >> 24));
                uint num7 = (num3 & 255) % 64;
                if ((num7 >= 0) && (num7 < 16))
                {
                    num4 |= (((num3 & 65280) >> 8) & ((num3 & 16711680) >> 16)) ^ (~num3 & 255);
                    num5 ^= ((num3 * i) + 1) % 16;
                    num3 += (num4 | num5) ^ (uint)Modifiers[3];
                }
                else if ((num7 >= 16) && (num7 < 32))
                {
                    num4 ^= ((num3 & 16711935) << 8) ^ (((num3 & 16776960) >> 8) | (~num3 & 65535));
                    num5 += (num3 * i) % 32;
                    num3 |= (num4 + ~num5) & (uint)Modifiers[3];
                }
                else if ((num7 >= 32) && (num7 < 48))
                {
                    num4 += ((num3 & 255) | ((num3 & 16711680) >> 16)) + (~num3 & 255);
                    num5 -= ~(num3 + num7) % 48;
                    num3 ^= (num4 % num5) | (uint)Modifiers[3];
                }
                else if ((num7 >= 48) && (num7 < 64))
                {
                    num4 ^= (((num3 & 16711680) >> 16) | ~(num3 & 255)) * (~num3 & 16711680);
                    num5 += (num3 ^ (i - 1)) % num7;
                    num3 -= ~(num4 ^ num5) + (uint)Modifiers[3];
                }
            }

            uint key = num3 ^ (uint)entry.Id;

            using (BinaryReader reader = new BinaryReader(new MemoryStream(_rawData)))
            {
                reader.BaseStream.Seek(key, SeekOrigin.Begin);

                reader.ReadByte();
                var bytes = reader.ReadBytes(reader.ReadInt32());

                var random = new Random(Modifiers[3] ^ ((int)key));
                var buffer3 = new byte[bytes.Length];

                random.NextBytes(buffer3);

                BitArray array = new BitArray(bytes);
                array.Xor(new BitArray(buffer3));
                array.CopyTo(bytes, 0);

                entry.PlainText = Encoding.UTF8.GetString(bytes);
            }
        }

        public void ProcessEntry(Confuser1_7Entry entry)
        {
            var ilProc = entry.Caller.Body.GetILProcessor();
            var badInstructions = entry.BadInstructions.ToArray();

            ilProc.InsertAfter(badInstructions[2], ilProc.Create(OpCodes.Ldstr, entry.PlainText));

            foreach (var instr in badInstructions)
                MarkMember(instr, entry.Caller);
        }

        public IEnumerable<Confuser1_7Entry> ConstructEntries(object param)
        {
            foreach (var mDef in AsmDef.FindMethods(m => true).Where(mDef => !BaseIsDecryptor(mDef)))
            {
                if (!mDef.HasBody)
                    continue;

                for (var i = 0; i < mDef.Body.Instructions.Count; i++)
                {
                    var instr = mDef.Body.Instructions[i];

                    if (instr.Previous == null)
                        continue;

                    if (instr.OpCode != OpCodes.Call || instr.Previous.OpCode != OpCodes.Ldc_I4 ||
                        !BaseIsDecryptor((instr.Operand as MethodReference).Resolve())) continue;

                    var entry = new Confuser1_7Entry
                    {
                        Id = (int)instr.Previous.Operand,
                        Caller = mDef,
                        MDToken = mDef.MetadataToken.ToUInt32(),
                        BadInstructions = mDef.Body.Instructions.GetInstructionBlock(i - 1, 3),
                    };

                    yield return entry;
                }
            }
        }
    }
}
