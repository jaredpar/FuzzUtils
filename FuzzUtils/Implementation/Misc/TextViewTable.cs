using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using EditorUtils;

namespace FuzzUtils.Implementation.Misc
{
    [Export(typeof(ITextViewTable))]
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("any")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class TextViewTable : ITextViewTable, IWpfTextViewCreationListener
    {
        private readonly List<ITextView> _textViewList = new List<ITextView>();

        internal ReadOnlyCollection<ITextView> ActiveTextViews
        {
            get { return _textViewList.ToReadOnlyCollection(); }
        }

        #region ITextViewTable

        ReadOnlyCollection<ITextView> ITextViewTable.ActiveTextViews
        {
            get { return ActiveTextViews; }
        }

        #endregion

        #region IWpfTextViewCreationListener

        void IWpfTextViewCreationListener.TextViewCreated(IWpfTextView textView)
        {
            _textViewList.Add(textView);
            textView.Closed += (sender, e) => { _textViewList.Remove(textView); };
        }

        #endregion
    }
}
