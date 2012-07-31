using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;

namespace NETDeob.Deobfuscators.Confuser.Tasks._1_7
{
    class Unpacker : UnpackingTask
    {
        public Unpacker(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        [DeobfuscationPhase(1, "Locate and load compressed resource")]
        public static bool Phase1()
        {
            string resName;

            if (!GetPackedAssembly(AsmDef, out resName)){
                ThrowPhaseError("No packer?", 0, true);
                return true;
            }

            var resStream = Assembly.LoadFile(DeobfuscatorContext.InPath).GetManifestResourceStream(resName);

            if(resStream == null){
                ThrowPhaseError("Could not load resource stream!", 1, false);
                return false;
            }

            PhaseParam = resStream;
            Logger.VSLog("Located compressed resource at " + resName);

            return true;
        }

        /// <summary>
        /// PhaseParam = resStream
        /// </summary>
        [DeobfuscationPhase(2, "Decompress resource")]
        public static bool Phase2()
        {
            var resStream = PhaseParam;
            byte[] buf;

            using (var br = new BinaryReader(resStream))
                buf = br.ReadBytes((int)resStream.Length);

            PhaseParam = buf;

            return true;
        }

        /// <summary>
        /// PhaseParam = rawBytes
        /// </summary>
        [DeobfuscationPhase(3, "Decrypt resource")]
        public static bool Phase3()
        {
            var rawBytes = PhaseParam;

            byte[] finalBuf;
            byte[] buf, buf1, buf2;

            using (var br = new BinaryReader(new DeflateStream(new MemoryStream(rawBytes), CompressionMode.Decompress))){
                buf = br.ReadBytes(br.ReadInt32());
                buf1 = br.ReadBytes(br.ReadInt32());
                buf2 = br.ReadBytes(br.ReadInt32());
            }

            for (var i = 0; i < buf2.Length; i += 4){
                buf2[i] = (byte)(buf2[i] ^ (36 & 255));
                buf2[i + 1] = (byte)(buf2[i + 1] ^ ((36 & 65280) >> 8));
                buf2[i + 2] = (byte)(buf2[i + 2] ^ (((36 & 16711680) >> 16)));
                buf2[i + 3] = (byte)(buf2[i + 3] ^ ((36 & 4278190080L) >> 24));
            }

            using (var stream2 = new CryptoStream(new MemoryStream(buf), new RijndaelManaged().CreateDecryptor(buf2, buf1), CryptoStreamMode.Read)){

                var buffer4 = new byte[4];
                stream2.Read(buffer4, 0, 4);

                var dst = new byte[BitConverter.ToUInt32(buffer4, 0)];
                Logger.VSLog(string.Format("Decompressed and decrypted resource, raw size: {0} bytes...", dst.Length));

                var buffer6 = new byte[4096];
                var length = buffer6.Length;
               
                for (var j = 0; length == buffer6.Length; j += length){
                    length = stream2.Read(buffer6, 0, buffer6.Length);
                    Buffer.BlockCopy(buffer6, 0, dst, j, length);
                }

                finalBuf = dst;
            }

            PhaseParam = finalBuf;
            return true;
        }

        /// <summary>
        /// PhaseParam = finalBuf
        /// </summary>
        [DeobfuscationPhase(4, "Write unpacked file to disk")]
        public static bool Phase4()
        {
            var finalBuf = PhaseParam;
            return true;
        }

        public static bool GetPackedAssembly(AssemblyDefinition asmDef, out string resName)
        {
            var cctor = asmDef.EntryPoint.DeclaringType.GetStaticConstructor();
            resName = cctor.Body.Instructions.GetOperandAt<string>(OpCodes.Ldstr, 0);

            if (resName == null){
                ThrowPhaseError("No packer?", 0, true);
                return true;
            }

            return true;
        }

        #region Reversed methods



        #endregion
    }
}
