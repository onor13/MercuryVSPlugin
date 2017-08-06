using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using MercuryLangPlugin.SyntaxAnalysis;

namespace MercuryLangPlugin.Highlight
{
    public class MercuryClassifier : IClassifier
    {
        //   [ImportMany]
        //   IEnumerable<EditorFormatDefinition> EditorFormats { get; set; }

        [Import]
        internal IEditorFormatMapService FormatMapService { get; set; }

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistryService { get; set; }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        private readonly List<LineContinuationInfo> lineStates = new List<LineContinuationInfo>();
        private readonly ITextBuffer textBuffer;

        private int? firstDirtyLine;
        private int? lastDirtyLine;

        private int? firstChangedLine;
        private int? lastChangedLine;
        private IClassificationTypeRegistryService classificationTypeRegistry;
        private EnvDTE.DTE dte;

        public MercuryClassifier(ITextBuffer textBuffer, IClassificationTypeRegistryService classificationTypeRegistryService, EnvDTE.DTE dte)
        {
            this.textBuffer = textBuffer;
            lineStates.AddRange(Enumerable.Repeat(LineContinuationInfo.None, textBuffer.CurrentSnapshot.LineCount));
            SubscribeEvents();
            classificationTypeRegistry = classificationTypeRegistryService;
            this.dte = dte;
        }       

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var classificationSpans = new List<ClassificationSpan>();
            int startLineNb = span.Snapshot.GetLineNumberFromPosition(span.Start.Position);
            int endLine = span.Snapshot.GetLineNumberFromPosition(span.End.Position);
            if (endLine > startLineNb)
            {
                --endLine; // Why?
            }
            MercuryLexer lexer = null;
            var savedLastLineContinuationInfo = lineStates[endLine];
            for (int currentLineNb = startLineNb; currentLineNb <= endLine; currentLineNb++)
            {
                var tmpLine = span.Snapshot.GetLineFromLineNumber(currentLineNb).GetText();
                var previousLineContinuation = currentLineNb > 0 ? lineStates[currentLineNb - 1] : LineContinuationInfo.None;


                var line = span.Snapshot.GetLineFromPosition(span.Start.Position);
                int lineNumber = line.LineNumber;
                int columnIndex = span.Start.Position - line.Start.Position;
                lexer = new MercuryLexer(tmpLine, previousLineContinuation, lineNumber);

                foreach (var colorableToken in lexer.ColorableItems())
                {
                    //var tokenSpan = new SnapshotSpan(span.Snapshot, new Span(colorableToken.Start, Math.Min(span.Snapshot.Length - colorableToken.Start, colorableToken.Length)));
                    var tokenSpan = new SnapshotSpan(span.Snapshot, span.Start.Position + colorableToken.StartColumn, colorableToken.EndColumn - colorableToken.StartColumn + 1);
                    var tmpSpan = new ClassificationSpan(tokenSpan, GetClassificationType(colorableToken.Type));
                    classificationSpans.Add(tmpSpan);
                }

                lineStates[currentLineNb] = lexer.ContinuationInfo;

            }
            if (lexer != null && lexer.ContinuationInfo != savedLastLineContinuationInfo && (endLine + 1) < lineStates.Count)
            {
                MyForceReclassifyLine(endLine + 1);
            }
          
            return classificationSpans;
        }

        public void ForceReclassifyLines(int startLine, int endLine)
        {
            firstDirtyLine = firstDirtyLine.HasValue ? Math.Min(firstDirtyLine.Value, startLine) : startLine;
            lastDirtyLine = lastDirtyLine.HasValue ? Math.Max(lastDirtyLine.Value, endLine) : endLine;

            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            int start = snapshot.GetLineFromLineNumber(startLine).Start;
            int end = snapshot.GetLineFromLineNumber(endLine).EndIncludingLineBreak;
            var e = new ClassificationChangedEventArgs(new SnapshotSpan(textBuffer.CurrentSnapshot, Span.FromBounds(start, end)));
            OnClassificationChanged(e);
        }

        private void Invalidate(SnapshotSpan span)
        {
            ClassificationChanged?.Invoke(this, new ClassificationChangedEventArgs(span));
        }

        private void OnClassificationChanged(ClassificationChangedEventArgs e)
        {
            ClassificationChanged?.Invoke(this, e);
        }

        private void SubscribeEvents()
        {
            textBuffer.ChangedLowPriority += HandleTextBufferChangedLowPriority;
            textBuffer.ChangedHighPriority += HandleTextBufferChangedHighPriority;/**/
        }

        private void UnsubscribeEvents()
        {
            textBuffer.ChangedLowPriority -= HandleTextBufferChangedLowPriority;
            textBuffer.ChangedHighPriority -= HandleTextBufferChangedHighPriority;
        }

        private void HandleTextBufferChangedLowPriority(object sender, TextContentChangedEventArgs e)
        {
            if (e.After == textBuffer.CurrentSnapshot)
            {
                if (firstChangedLine.HasValue && lastChangedLine.HasValue)
                {
                    int startLine = firstChangedLine.Value;
                    int endLine = Math.Min(lastChangedLine.Value, e.After.LineCount - 1);

                    firstChangedLine = null;
                    lastChangedLine = null;
                    ForceReclassifyLines(startLine, endLine);
                }
            }
        }

        private void HandleTextBufferChangedHighPriority(object sender, TextContentChangedEventArgs e)
        {
            foreach (ITextChange change in e.Changes)
            {
                int lineNumberFromPosition = e.After.GetLineNumberFromPosition(change.NewPosition);
                int num2 = e.After.GetLineNumberFromPosition(change.NewEnd);
                if (change.LineCountDelta < 0)
                {
                    lineStates.RemoveRange(lineNumberFromPosition, Math.Abs(change.LineCountDelta));
                }
                else if (change.LineCountDelta > 0)
                {
                    lineStates.InsertRange(lineNumberFromPosition, Enumerable.Repeat(LineContinuationInfo.None, change.LineCountDelta));
                }

                if (lastDirtyLine.HasValue && lastDirtyLine.Value > lineNumberFromPosition)
                {
                    lastDirtyLine += change.LineCountDelta;
                }

                if (lastChangedLine.HasValue && lastChangedLine.Value > lineNumberFromPosition)
                {
                    lastChangedLine += change.LineCountDelta;
                }

                firstChangedLine = firstChangedLine.HasValue ? Math.Min(firstChangedLine.Value, lineNumberFromPosition) : lineNumberFromPosition;
                lastChangedLine = lastChangedLine.HasValue ? Math.Max(lastChangedLine.Value, num2) : num2;
            }
        }

        public void MyForceReclassifyLine(int line)
        {
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            ITextSnapshotLine snapshotLine = snapshot.GetLineFromLineNumber(line);
            var start = snapshotLine.Start;
            int length = snapshotLine.Length;
            var snapshotToUpdate = new SnapshotSpan(textBuffer.CurrentSnapshot, start, length);
            var e = new ClassificationChangedEventArgs(snapshotToUpdate);
            OnClassificationChanged(e);
        }

        private IClassificationType GetClassificationType(MercuryTokenType type)
        {
            if (type == MercuryTokenType.Comment)
            {
                return classificationTypeRegistry.GetClassificationType("COMMENT");
            }
            if (type == MercuryTokenType.StringLiteral)
            {
                return classificationTypeRegistry.GetClassificationType("STRING");
            }
            if (type == MercuryTokenType.Variable)
            {
                return classificationTypeRegistry.GetClassificationType("cppMacro");
            }
            if (type == MercuryTokenType.Keyword)
            {
                return classificationTypeRegistry.GetClassificationType("keyword");
            }
            return classificationTypeRegistry.GetClassificationType("OTHER");
        }
    }
}
