using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NETDeob.Core.Deobfuscators.Generic.Renaming
{
    public interface IMemberRenamer
    {
        int NameIdx { get; set; }
        Dictionary<int, Tuple<string, string>> StandardNames { get; set; }

        void Rename();
    }

    public abstract class IMemberRenamer<T> : IMemberRenamer
    {
        private T _member;

        protected IMemberRenamer(T member)
        {
            _member = member;
        }

        public abstract int NameIdx { get; set; }
        public abstract Dictionary<int, Tuple<string, string>> StandardNames { get; set; }
        public abstract void Rename();
    }
}
