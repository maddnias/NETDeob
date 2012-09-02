using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NETDeob.Core.Engine.Utils;

namespace NETDeob.Core.Misc.Structs__Enums___Interfaces
{
    internal class NETDeobExceptionFormatter : IExceptionFormatter
    {
        public string Format(Exception exception)
        {
            var errorInformation = new StringBuilder();

            errorInformation.Append("Error message:\r\n\t" + exception.Message);
            errorInformation.Append("\r\n\r\nTarget site:\r\n\t" + exception.TargetSite);
            errorInformation.Append("\r\n\r\nInner Exception:\r\n\t" + exception.InnerException);
            errorInformation.Append("\r\n\r\nStack trace:\r\n\r\n");

            foreach (var obj in exception.StackTrace)
                errorInformation.Append(obj.ToString());

            return errorInformation.ToString();
        }
    }
}
