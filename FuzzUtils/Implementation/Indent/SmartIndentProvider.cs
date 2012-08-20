using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace FuzzUtils.Implementation.Indent
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType("text")]
    internal sealed class SmartIndentProvider : ISmartIndentProvider
    {
        #region ISmartIndentProvider

        ISmartIndent ISmartIndentProvider.CreateSmartIndent(ITextView textView)
        {
            return new SmartIndent();
        }

        #endregion
    }
}
