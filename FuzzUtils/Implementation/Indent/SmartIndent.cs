using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace FuzzUtils.Implementation.Indent
{
    /// <summary>
    /// Simulate the quirk of the Razor editor here where by it will perform an 
    /// edit in the request for indentation
    /// </summary>
    internal sealed class SmartIndent : ISmartIndent
    {
        private ISmartIndent _optionalOriginalSmartIndent;

        internal SmartIndent(ISmartIndent optionalOriginalSmartIndent = null)
        {
            _optionalOriginalSmartIndent = optionalOriginalSmartIndent;
        }

        private int? GetDesiredIndentation(ITextSnapshotLine snapshotLine)
        {
            var indent = _optionalOriginalSmartIndent != null
                ? _optionalOriginalSmartIndent.GetDesiredIndentation(snapshotLine)
                : 0;
            PerformNoopEdit(snapshotLine.Snapshot.TextBuffer, snapshotLine.Start.Position);
            return indent;
        }

        private void Dispose()
        {
            try
            {
                if (_optionalOriginalSmartIndent != null)
                {
                    _optionalOriginalSmartIndent.Dispose();
                }
            }
            finally
            {
                _optionalOriginalSmartIndent = null;
            }
        }

        private void PerformNoopEdit(ITextBuffer textBuffer, int position)
        {
            var text = Environment.NewLine;
            textBuffer.Insert(position, text);
            textBuffer.Delete(new Span(position, text.Length));
        }

        #region ISmartIndent

        int? ISmartIndent.GetDesiredIndentation(ITextSnapshotLine line)
        {
            return GetDesiredIndentation(line);
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }

        #endregion
    }
}
