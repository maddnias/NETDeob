using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETDeob.Misc.Structs__Enums___Interfaces.Deobfuscation
{
    public abstract class DecryptionContext
    {
        public string PlainText;

        public new abstract string ToString();
    }

    public interface IStringDecryptor<T> where T : DecryptionContext
    {
        bool BaseIsDecryptor(params object[] param);
        void InitializeDecryption(object param);
        void DecryptEntry(ref T entry);
        void ProcessEntry(T entry);
        IEnumerable<T> ConstructEntries(object param);
    }

    public interface IStringDecryptor
    {
        void InitializeDecryption(object param);
        void DecryptEntry(ref DecryptionContext entry);
        void ProcessEntry(DecryptionContext entry);
        IEnumerable<T> ConstructEntries<T>(object param) where T : DecryptionContext;
    }
}
