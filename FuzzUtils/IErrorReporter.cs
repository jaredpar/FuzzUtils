using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuzzUtils
{
    public interface IErrorReporter
    {
        void Report(string summary, string message);
    }
}
