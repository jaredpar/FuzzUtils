using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace FuzzUtils.Implementation.ErrorReporter
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(BannerMargin.MarginName)]
    [MarginContainer(PredefinedMarginNames.Top)]
    [ContentType("any")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    [Export(typeof(IErrorReporter))]
    internal sealed class ErrorReporter : IErrorReporter, IWpfTextViewMarginProvider
    {
        private readonly object _key = new object();
        private readonly List<ITextView> _textViewList = new List<ITextView>();

        private void Report(string summary, string message)
        {
            foreach (var textView in _textViewList)
            {
                if (textView.HasAggregateFocus)
                {
                    BannerMargin bannerMargin;
                    if (textView.Properties.TryGetProperty(_key, out bannerMargin))
                    {
                        bannerMargin.Report(summary, message);
                        break;
                    }
                }
            }
        }

        private IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            var textView = wpfTextViewHost.TextView;
            var bannerMargin = new BannerMargin(textView);

            EventHandler onIgnoreAll = null;
            onIgnoreAll =
                (sender, e) =>
                {
                    bannerMargin.IgnoreAllClicked -= onIgnoreAll;
                };
            bannerMargin.IgnoreAllClicked += onIgnoreAll;

            EventHandler onClosed = null;
            onClosed =
                (sender, e) =>
                {
                    _textViewList.Remove(textView);
                    textView.Properties.RemoveProperty(_key);
                };

            wpfTextViewHost.TextView.Properties[_key] = bannerMargin;
            _textViewList.Add(textView);

            return bannerMargin;
        }

        #region IErrorReporter

        void IErrorReporter.Report(string summary, string message)
        {
            Report(summary, message);
        }

        #endregion

        #region IWpfTextViewMarginProvider

        IWpfTextViewMargin IWpfTextViewMarginProvider.CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            return CreateMargin(wpfTextViewHost, marginContainer);
        }

        #endregion
    }
}
