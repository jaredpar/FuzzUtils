using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace FuzzUtils.Implementation.ErrorReporter
{
    [Export(typeof(IErrorReporter))]
    internal sealed class ErrorReporter : IErrorReporter
    {
        private void Report(string summary, string message)
        {
            throw new NotImplementedException();
        }

        #region IErrorReporter

        void IErrorReporter.Report(string summary, string message)
        {
            Report(summary, message);
        }

        #endregion
    }
}
