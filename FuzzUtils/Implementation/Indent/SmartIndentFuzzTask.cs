using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Threading;
using EditorUtils;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace FuzzUtils.Implementation.Indent
{
    [Export(typeof(IFuzzTask))]
    [Export(typeof(ISmartIndentProvider))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class SmartIndentFuzzTask : IFuzzTask, ISmartIndentProvider
    {
        private readonly DispatcherTimer _dispatcherTimer;
        private readonly IProtectedOperations _protectedOperations;
        private readonly ITextViewTable _textViewTable;
        private bool _isActive;

        [ImportingConstructor]
        internal SmartIndentFuzzTask([EditorUtilsImport] IProtectedOperations protectedOperations, ITextViewTable textViewTable)
        {
            _textViewTable = textViewTable;
            _protectedOperations = protectedOperations;
            _dispatcherTimer = new DispatcherTimer(
                TimeSpan.FromSeconds(1),
                DispatcherPriority.SystemIdle,
                _protectedOperations.GetProtectedEventHandler(OnTimer),
                Dispatcher.CurrentDispatcher);
        }

        private void OnTimer(object sender, EventArgs e)
        {
            foreach (var textView in _textViewTable.ActiveTextViews)
            {
                MaybeInstallSmartIndent(textView);
            }

            // The _isActive check stays true from the point of fuzzing until the next idle.  If it 
            // does cause an exception in the caller code it will happen after the fuzz.  We stay
            // active until the next idle to help diagnostics pick smart indent as the cause
            _isActive = false;
        }

        /// <summary>
        /// The ISmartIndentationService will install a cached ISmartIndent instance into the
        /// ITextView property bag.  When it does this we will wrap it with the fuzzing version
        /// of ISmartIndent
        /// </summary>
        private void MaybeInstallSmartIndent(ITextView textView)
        {
            var properties = textView.Properties;
            object key;
            ISmartIndent smartIndent;
            if (!TryGetSmartIndent(properties, out key, out smartIndent))
            {
                return;
            }

            properties[key] = CreateSmartIndent(textView, smartIndent);
        }

        private bool TryGetSmartIndent(PropertyCollection properties, out object key, out ISmartIndent smartIndent)
        {
            key = null;
            smartIndent = null;
            foreach (var pair in properties.PropertyList)
            {
                smartIndent = pair.Value as ISmartIndent;
                if (smartIndent == null)
                {
                    continue;
                }

                // Don't replace ourself
                if (smartIndent is SmartIndent)
                {
                    return false;
                }

                key = pair.Key;
                return true;
            }

            return false;
        }

        private SmartIndent CreateSmartIndent(ITextView textView, ISmartIndent optionalSmartIndent = null)
        {
            var smartIndent = new SmartIndent(optionalSmartIndent);
            EventHandler onFuzzed = null;
            EventHandler onDisposed = null;

            onFuzzed = (sender, e) => _isActive = true;
            onDisposed = 
                (sender, e) =>
                {
                    smartIndent.Fuzzed -= onFuzzed;
                    smartIndent.Disposed -= onDisposed;
                };
            smartIndent.Disposed += onDisposed;
            smartIndent.Fuzzed += onFuzzed;

            return smartIndent;
        }

        #region IFuzzTask

        string IFuzzTask.Identifier
        {
            get { return "SmartIndent"; }
        }

        string IFuzzTask.DisplayName
        {
            get { return "Smart Indent"; }
        }

        bool IFuzzTask.IsActive
        {
            get { return _isActive; }
        }

        #endregion

        #region ISmartIndentProvider

        ISmartIndent ISmartIndentProvider.CreateSmartIndent(ITextView textView)
        {
            return CreateSmartIndent(textView);
        }

        #endregion
    }
}
