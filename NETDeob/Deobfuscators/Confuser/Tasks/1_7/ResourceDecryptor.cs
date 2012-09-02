using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation;

namespace NETDeob.Deobfuscators.Confuser.Tasks._1_7
{
    public class ResourceDecryptor : AssemblyDeobfuscationTask
    {
        public ResourceDecryptor(AssemblyDefinition asmDef)
            : base(asmDef)
        {
            RoutineDescription = "Decrypt and replace resources";
        }

        [DeobfuscationPhase(1, "Locate resource resolver")]
        public static bool Phase1()
        {
            var globalType =
                AsmDef.MainModule.Types.First(t => t.Name == "<Module>").GetStaticConstructor();

            MethodDefinition resolver = null;

            foreach (var instr in globalType.Body.Instructions.Where(i => i.OpCode == OpCodes.Ldftn))
                if (instr.Operand.ToString().Contains("System.Object,System.ResolveEventArgs"))
                    resolver = (instr.Operand as MethodReference).Resolve();

            if(resolver == null){
                ThrowPhaseError("No resource encryption?", 0, true);
                return true;
            }

            var cctor = resolver.DeclaringType.Methods.First(m => m.IsConstructor && m.Name == ".cctor");

            MarkMember(cctor);
            MarkMember(resolver);

            Logger.VSLog("Found resolver method at " + resolver.Name);
            PhaseParam = resolver;

            return true;
        }

        /// <summary>
        /// Phaseparam = resolver
        /// </summary>
        [DeobfuscationPhase(2, "Decompress and load resource")]
        public static bool Phase2()
        {
            var resolver = PhaseParam as MethodDefinition;
            var resName = resolver.Body.Instructions.GetOperandAt<string>(OpCodes.Ldstr, 0);
            var modifier = resolver.Body.Instructions.GetOperandAt<dynamic>(instr => instr.IsLdcI4(), 6);

            if(resName == null && modifier == null)
            {
                resName = (AsmDef.MainModule.Resources[0] as EmbeddedResource).Name;
                modifier = resolver.Body.Instructions.GetOperandAt<dynamic>(instr => instr.IsLdcI4(), 1);
            }

            if(resName == null){
                ThrowPhaseError("Could not locate compressed resource!", 1, false);
                return false;
            }

            MarkMember(AsmDef.MainModule.Resources.First(r => r.Name == resName));
            var streamRes = Assembly.LoadFile(Globals.DeobContext.InPath).GetManifestResourceStream(resName);

            using(var br = new BinaryReader(new DeflateStream(streamRes, CompressionMode.Decompress)))
            {
                byte[] buffer = br.ReadBytes(br.ReadInt32());
                byte[] buffer2 = new byte[buffer.Length / 2];

                for (int i = 0; i < buffer.Length; i += 2)
                    buffer2[i / 2] = (byte)(((buffer[i + 1] ^ modifier) * modifier) + (buffer[i] ^ modifier));

                using(var br2 = new BinaryReader(new DeflateStream(new MemoryStream(buffer2), CompressionMode.Decompress)))
                    PhaseParam = Assembly.Load(br2.ReadBytes(br2.ReadInt32()));
            }

            return true;
        }

        /// <summary>
        /// PhaseParam = asm
        /// </summary>
        [DeobfuscationPhase(3, "Extract resources and replace old")]
        public static bool Phase4()
        {
            var asm = PhaseParam;

            foreach (var res in asm.GetManifestResourceNames())
            {
                var tmpStream = asm.GetManifestResourceStream(res);
                Globals.DeobContext.ResStreams.Add(new ResEx {Name = res, ResStream = tmpStream});
                var buf = new byte[tmpStream.Length]; tmpStream.Read(buf, 0, buf.Length);
                
                AsmDef.MainModule.Resources.Add(new EmbeddedResource(res,
                                                 ManifestResourceAttributes.Private, buf));

                Logger.VSLog(string.Format(@"Injected original resource ""{0}""",
                                           AsmDef.MainModule.Resources[AsmDef.MainModule.Resources.Count -1].Name));
            }

            Globals.DeobContext.ResStreams.Clear();

            // For compability with constant decryptor
            AsmDef.Write(Globals.DeobContext.InPath + "_resdump.exe");
            Globals.DeobContext.InPath = Globals.DeobContext.InPath + "_resdump.exe";

            return true;
        }
    }
}
