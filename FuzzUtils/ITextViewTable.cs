using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.ObjectModel;

namespace FuzzUtils
{
    public interface ITextViewTable
    {
        /// <summary>
        /// Returns the collection of ITextView instances which are currently alive
        /// in the program
        /// </summary>
        ReadOnlyCollection<ITextView> ActiveTextViews { get; }
    }
}
