using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.Collections.ObjectModel;

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

        private void ReportCore(string summary, string message)
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

        private void Report(ReadOnlyCollection<IFuzzTask> fuzzTasks, Exception exception)
        {
            var summary = BuildSummary(fuzzTasks, exception);
            var message = exception.ToString();
            ReportCore(summary, message);
        }

        private void Report(IFuzzTask fuzzTask, string message)
        {
            var summary = BuildSummary(fuzzTask, "Failed");
            ReportCore(summary, message);
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

        private string BuildSummary(IFuzzTask fuzzTask, string summary)
        {
            return String.Format("Fuzzing Task '{0}' appears to have caused an error: {1}", fuzzTask.Name, summary);
        }

        private string BuildSummary(ReadOnlyCollection<IFuzzTask> fuzzTasks, Exception exception)
        {
            string summary;
            if (fuzzTasks.Count == 1)
            {
                return BuildSummary(fuzzTasks[0], exception.GetType().Name);
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

        #region IErrorReporter

        void IErrorReporter.Report(ReadOnlyCollection<IFuzzTask> fuzzTasks, Exception exception)
        {
            Report(fuzzTasks, exception);
        }

        void IErrorReporter.Report(IFuzzTask fuzzTask, string message)
        {
            Report(fuzzTask, message);
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
