using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;

namespace MercuryLangPlugin.Providers
{
    internal class MercuryQuickInfoController: IIntellisenseController
    {
        ITextView view;
        IList<ITextBuffer> textBuffers;
        MercuryQuickInfoControllerProvider provider;
        private IQuickInfoSession session;
        internal MercuryQuickInfoController(
            ITextView textView,
            IList<ITextBuffer> subjectBuffers,
            MercuryQuickInfoControllerProvider mercuryQuickInfoControllerProvider)
        {
            view = textView;
            textBuffers = subjectBuffers;
            provider = mercuryQuickInfoControllerProvider;

            view.MouseHover += this.OnTextViewMouseHover;
        }

        private void OnTextViewMouseHover(object sender, MouseHoverEventArgs e)
        {
            //find the mouse position by mapping down to the subject buffer
            SnapshotPoint? point = view.BufferGraph.MapDownToFirstMatch
                 (new SnapshotPoint(view.TextSnapshot, e.Position),
                PointTrackingMode.Positive,
                snapshot => textBuffers.Contains(snapshot.TextBuffer),
                PositionAffinity.Predecessor);

            if (point != null)
            {
                ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position,
                PointTrackingMode.Positive);

                if (!provider.QuickInfoBroker.IsQuickInfoActive(view))
                {
                    session = provider.QuickInfoBroker.TriggerQuickInfo(view, triggerPoint, true);
                }
            }
        }

        public void Detach(ITextView textView)
        {
            if (view == textView)
            {
                view.MouseHover -= this.OnTextViewMouseHover;
                view = null;
            }
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }
    }
}
