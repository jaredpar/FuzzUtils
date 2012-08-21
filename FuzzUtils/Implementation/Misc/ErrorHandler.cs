using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using EditorUtils;

namespace FuzzUtils.Implementation.Misc
{
    [Export(typeof(IExtensionErrorHandler))]
    internal sealed class ErrorHandler : IExtensionErrorHandler
    {
        private readonly ReadOnlyCollection<IFuzzTask> _fuzzTasks;
        private readonly IErrorReporter _errorReporter;

        [ImportingConstructor]
        internal ErrorHandler(IErrorReporter errorReporter, [ImportMany] IEnumerable<IFuzzTask> fuzzTasks)
        {
            _errorReporter = errorReporter;
            _fuzzTasks = new ReadOnlyCollection<IFuzzTask>(fuzzTasks.ToList());
        }

        private void HandleError(Exception exception)
        {
            // First find the set of IFuzzTasks which are likely to have caused this problem
            var all = _fuzzTasks.Where(x => x.IsActive).ToReadOnlyCollection();
            if (all.Count == 0)
            {
                return;
            }

            _errorReporter.Report(all, exception);
        }

        #region IExtensionErrorHandler

        void IExtensionErrorHandler.HandleError(object sender, Exception exception)
        {
            HandleError(exception);
        }

        #endregion
    }
}
