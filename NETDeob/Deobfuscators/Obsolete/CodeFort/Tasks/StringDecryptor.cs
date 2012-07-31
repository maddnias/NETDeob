using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Engine.Utils.Extensions;
using NETDeob.Core.Misc;
using NETDeob.Misc;

namespace NETDeob.Deobfuscators.CodeFort.Tasks
{
    class StringDecryptor : AssemblyDeobfuscationTask
    {
        public StringDecryptor(AssemblyDefinition asmDef)
            : base(asmDef)
        {
        }

        public bool Phase1()
        {
           // FindDecryptor();
           // Method21(12);
            return true;
        }

        #region Reversed methods

//        internal void Method21(int param19)

//        {
//            int metadataToken = param19 + 0x2000000;
//            var typeDef = AsmDef.MainModule.Types.First(type => type.MetadataToken.ToInt32() == metadataToken);

//            foreach (var fieldDef in typeDef.Fields)
//            {
//                string name = fieldDef.Name;
//                StringBuilder builder = new StringBuilder();
//                foreach (char t in name)
//                {
//                    builder.Append((char) (((byte) t) + 0x2f));
//                }
//                int num3 = int.Parse(builder.ToString(), NumberStyles.HexNumber);
//                int num4 = num3 & 0xffffff;
//// ReSharper disable UnusedVariable
//                int num5 = num3 >> 0x18;
//// ReSharper restore UnusedVariable
//                int num6 = 0xa000000 + num4;
//// ReSharper disable UnusedVariable
//                var info2 = AsmDef.FindMethod(mDef => mDef.Resolve().MetadataToken.ToInt32() == num6);
//// ReSharper restore UnusedVariable
//                // info.SetValue(null, Method_22(type, info2, num5));
//            }
//        }

        #endregion

        public MethodDefinition FindDecryptor()
        {
            var signature = new ILSignature
                                {
                                    StartIndex = 0,
                                    StartOpCode = OpCodes.Nop,
                                    Instructions = new List<OpCode>
                                                       {
                                                           OpCodes.Ldsfld,
                                                           OpCodes.Call,
                                                           OpCodes.Newarr,
                                                           OpCodes.Stloc_0,
                                                           OpCodes.Ldc_R8,
                                                           OpCodes.Ldsfld,
                                                           OpCodes.Call
                                                       }
                                };

            return AsmDef.FindMethod(mDef => SignatureFinder.IsMatch(mDef, signature));
        }
    }
}
