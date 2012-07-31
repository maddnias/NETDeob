using System.IO;
using Mono.Cecil;
using NETDeob.Deobfuscators;

namespace NETDeob.Core.Unpackers.NetShrink.Tasks
{
    class Unpacker : UnpackingTask
    {
        public Unpacker(AssemblyDefinition asmDef) : base(asmDef)
        {
    
        }

        #region Reversed methods

        public long Decrypt1(Stream strIn)
        {
            long num = 0L;
            for (int i = 0; i < 8; i++)
            {
                int num3 = strIn.ReadByte();
                num |= ((byte) num3) << (8*i);
            }
            return num;
        }

        #endregion
    }
}
