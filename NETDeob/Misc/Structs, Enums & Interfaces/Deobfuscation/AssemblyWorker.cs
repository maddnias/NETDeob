using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using NETDeob.Core.Deobfuscators.Generic;
using NETDeob.Core.Engine.Utils;
using NETDeob.Core.Misc;
using NETDeob.Deobfuscators;
using NETDeob.Deobfuscators.Confuser;
using NETDeob.Deobfuscators.Generic;
using NETDeob.Misc.Structs__Enums___Interfaces.Tasks.Base;
using de4dot.PE;

namespace NETDeob.Misc.Structs__Enums___Interfaces
{
    public abstract class AssemblyWorker
    {
        public AssemblyDefinition AsmDef;
        public List<Task > TaskQueue = new List<Task>();
        public bool IsSave = true;

        private DateTime _startTime;

        protected AssemblyWorker(AssemblyDefinition asmDef)
        {
            AsmDef = asmDef;

            CreateTaskQueue();
        }

        public abstract void CreateTaskQueue();

        public void Deobfuscate()
        {
            TaskQueue.Add(new AssemblyStripper(AsmDef));
            _startTime = DateTime.Now;

            foreach (var task in TaskQueue)
            {
                Logger.VSLog("\nPerforming routine: " + task.RoutineDescription);
                task.PerformTask();
            }

            Logger.VSLog(string.Format("\n----------------------------\nSuccessfully deobfuscated assembly: {0} in {1} milliseconds", AsmDef.Name.ToString().Split(',')[0], (DateTime.Now - _startTime).Milliseconds));
            Logger.VSLog(string.Format("\nDeobfuscated assembly saved at: {0}", DeobfuscatorContext.OutPath));
            
            if(IsSave)
                Save(DeobfuscatorContext.OutPath);
        }

        public void Save(string path)
        {
            try
            {
                AsmDef.Write(path);
            }
            catch
            {
                var ms = new MemoryStream();
                AsmDef.MainModule.Write(ms);
            }
        }
    }
}
