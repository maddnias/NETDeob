using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETDeob._Console.Bases
{
    public abstract class Command
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string RawCommand { get; set; }
        public string UserInput { get; set; }

        public List<Command> Incompabilities { get; set; }

        public abstract void Display(dynamic param);
    }
}
