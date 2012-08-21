using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Windows.Threading;
using EditorUtils;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;

namespace FuzzUtils.Implementation.Misc
{
    [Export(typeof(IFuzzTask))]
    internal sealed class LongEditsFuzzTask : IFuzzTask
    {
        private readonly DispatcherTimer _dispatcherTimer;
        private readonly IProtectedOperations _protectedOperations;
        private readonly ITextViewTable _textViewTable;
        private readonly IErrorReporter _errorReporter;
        private readonly HashSet<ITextView> _hasEditSet = new HashSet<ITextView>();
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;

        [ImportingConstructor]
        internal LongEditsFuzzTask(
            [EditorUtilsImport] IProtectedOperations protectedOperations,
            ITextViewTable textViewTable,
            IErrorReporter errorReporter,
            ITextDocumentFactoryService textDocumentFactoryService)
        {
            _errorReporter = errorReporter;
            _protectedOperations = protectedOperations;
            _textViewTable = textViewTable;
            _textDocumentFactoryService = textDocumentFactoryService;
            _dispatcherTimer = new DispatcherTimer(
                TimeSpan.FromSeconds(1),
                DispatcherPriority.SystemIdle,
                _protectedOperations.GetProtectedEventHandler(OnTimer),
                Dispatcher.CurrentDispatcher);
        }

        private void OnTimer(object sender, EventArgs e)
        {
            CheckLongEdits();
            _hasEditSet.Clear();
            SaveCurrentEdits();
        }

        private void CheckLongEdits()
        {
            foreach (var textView in _hasEditSet)
            {
                if (textView.IsClosed || !textView.TextBuffer.EditInProgress)
                {
                    continue;
                }

                string name;
                ITextDocument textDocument;
                if (_textDocumentFactoryService.TryGetTextDocument(textView.TextBuffer, out textDocument))
                {
                    name = textDocument.FilePath;
                }
                else
                {
                    name = textView.TextSnapshot.GetLineFromLineNumber(0).GetText();
                }

                var message = String.Format(@"File: {0}
A long lived ITextEdit was detected.  While this is open no other component can edit the buffer.  This will cause very common tasks to fail", name);
                _errorReporter.Report(this, message);
            }
        }

        private void SaveCurrentEdits()
        {
            foreach (var textView in _textViewTable.ActiveTextViews)
            {
                if (textView.TextBuffer.EditInProgress)
                {
                    _hasEditSet.Add(textView);
                }
            }
        }

        #region IFuzzTask

        string IFuzzTask.Identifier
        {
            get { return "LongEdits"; }
        }

        string IFuzzTask.DisplayName
        {
            get { return "Long Edits"; }
        }

        bool IFuzzTask.IsActive
        {
            get { return false; }
        }

        #endregion 
    }
}

