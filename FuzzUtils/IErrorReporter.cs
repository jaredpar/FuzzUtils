using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace FuzzUtils
{
    public interface IErrorReporter
    {
        void Report(ReadOnlyCollection<IFuzzTask> fuzzTasks, Exception exception);

        void Report(IFuzzTask fuzzTask, string message);
    }
}
