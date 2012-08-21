using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Windows;

namespace FuzzUtils.Implementation.ErrorReporter
{
    internal sealed class BannerMargin : IWpfTextViewMargin
    {
        internal const string MarginName = "BannerMargin";

        private readonly ITextView _textView;
        private readonly BannerControl _bannerControl;

        internal event EventHandler IgnoreAllClicked;

        internal BannerMargin(ITextView textView)
        {
            _textView = textView;
            _bannerControl = new BannerControl();
            _bannerControl.Visibility = Visibility.Collapsed;
            _bannerControl.IgnoreError += OnIgnoreError;
            _bannerControl.IgnoreAllErrors += OnIgnoreAllErrors;
        }

        internal void Report(Uri uri, string summary, string message)
        {
            _bannerControl.HeaderText = summary;
            _bannerControl.ErrorText = message;
            _bannerControl.ErrorTextVisibility = Visibility.Collapsed;
            _bannerControl.FuzzDocumentationUri = uri;
            _bannerControl.Visibility = Visibility.Visible;
        }

        private void OnIgnoreError(object sender, EventArgs e)
        {
            _bannerControl.Visibility = Visibility.Collapsed;
        }

        private void OnIgnoreAllErrors(object sender, EventArgs e)
        {
            _bannerControl.Visibility = Visibility.Collapsed;
            if (IgnoreAllClicked != null)
            {
                IgnoreAllClicked(this, EventArgs.Empty);
            }
        }

        #region IWpfTextViewMargin

        FrameworkElement IWpfTextViewMargin.VisualElement
        {
            get { return _bannerControl; }
        }

        bool ITextViewMargin.Enabled
        {
            get { return true; }
        }

        ITextViewMargin ITextViewMargin.GetTextViewMargin(string marginName)
        {
            return marginName == MarginName ? this : null;
        }

        double ITextViewMargin.MarginSize
        {
            get { return 25; }
        }

        void IDisposable.Dispose()
        {

        }

        #endregion
    }

}
