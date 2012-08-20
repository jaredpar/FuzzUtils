using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Threading;
using EditorUtils;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace FuzzUtils.Implementation.Indent
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class Listener : IWpfTextViewCreationListener
    {
        private readonly DispatcherTimer _dispatcherTimer;
        private readonly IProtectedOperations _protectedOperations;
        private readonly List<ITextView> _textViewList = new List<ITextView>();

        [ImportingConstructor]
        internal Listener([EditorUtilsImport] IProtectedOperations protectedOperations)
        {
            _protectedOperations = protectedOperations;
            _dispatcherTimer = new DispatcherTimer(
                TimeSpan.FromSeconds(1),
                DispatcherPriority.SystemIdle,
                _protectedOperations.GetProtectedEventHandler(OnTimer),
                Dispatcher.CurrentDispatcher);
        }

        private void OnTimer(object sender, EventArgs e)
        {
            foreach (var textView in _textViewList)
            {
                MaybeInstallSmartIndent(textView);
            }
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
            ISmartIndent smartindent;
            if (!TryGetSmartIndent(properties, out key, out smartindent))
            {
                return;
            }

            properties[key] = new SmartIndent(smartindent);
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

        #region IWpfTextViewCreationListener

        void IWpfTextViewCreationListener.TextViewCreated(IWpfTextView textView)
        {
            _textViewList.Add(textView);
            textView.Closed += (sender, e) => { _textViewList.Remove(textView); };
        }

        #endregion

    }
}
