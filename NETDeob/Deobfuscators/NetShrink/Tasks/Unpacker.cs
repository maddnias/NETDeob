using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Mono.Cecil;

namespace NETDeob.Deobfuscators.NetShrink.Tasks
{
    class Unpacker : IDeobfuscationTask
    {
        private AssemblyDefinition _asmDef;

        public Unpacker(AssemblyDefinition asmDef)
        {
            _asmDef = asmDef;
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

        public override void PerformTask()
        {
            
        }

        public override void CleanUp()
        {
            
        }
    }
}
