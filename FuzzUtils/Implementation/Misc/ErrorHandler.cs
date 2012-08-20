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

            var summary = BuildSummary(all, exception);
            var message = exception.ToString();
            _errorReporter.Report(summary, message);
        }

        private string BuildSummary(ReadOnlyCollection<IFuzzTask> fuzzTasks, Exception exception)
        {
            string summary;
            if (fuzzTasks.Count == 1)
            {
                var fuzzTask = fuzzTasks[0];
                summary = String.Format("Fuzzing Task '{0}' appears to have caused an error: {1}", fuzzTask.Name, exception.GetType().Name);
            }
            else
            {
                var builder = new StringBuilder();
                foreach (var fuzzTask in fuzzTasks)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(fuzzTask.Name);
                }
                summary = String.Format("Fuzzing Tasks '{0}' appears to have caused an error: {1}", builder.ToString(), exception.GetType().Name);
            }

            return summary;
        }

        #region IExtensionErrorHandler

        void IExtensionErrorHandler.HandleError(object sender, Exception exception)
        {
            HandleError(exception);
        }

        #endregion
    }
}
