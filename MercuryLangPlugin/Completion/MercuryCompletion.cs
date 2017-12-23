using MercuryLangPlugin.Providers;
using MercuryLangPlugin.SyntaxAnalysis;
using MercuryLangPlugin.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace MercuryLangPlugin.Completion
{
    internal class MercuryCompletion : ICompletionSource
    {
        private CompletionSourceProvider sourceProvider;
        private ITextBuffer textBuffer;


        public MercuryCompletion(CompletionSourceProvider provider, ITextBuffer textBuffer)
        {
            this.textBuffer = textBuffer;
            sourceProvider = provider;
        }

        void ICompletionSource.AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            var parsedText = MercuryVSPackage.ParsedCache.Get(MercuryVSPackage.TextCache.Get(textBuffer.CurrentSnapshot));
            var triggerPoint = (SnapshotPoint)session.GetTriggerPoint(textBuffer.CurrentSnapshot);
            var strList = new SortedSet<string>(Keywords.GetMercuryKeywords());

            MercuryToken currentToken = new MercuryToken()
            {
                Type = MercuryTokenType.None
            };
            int currentTokenIdx = 0;
            int position = triggerPoint.Position;
            var line = triggerPoint.GetContainingLine();

            int lineNumber = line.LineNumber;
            int columnIndex = position - line.Start.Position - 1;
            for (int i = 0; i < parsedText.Tokens.Length; ++i)
            {
                MercuryToken token = parsedText.Tokens[i];
                if (token.LineNumber == lineNumber && token.StartColumn <= columnIndex && token.EndColumn >= columnIndex)
                {
                    currentToken = token;
                    currentTokenIdx = i;
                    break;
                }
            }
            //Loop till the end of current stmt
            for (int i = currentTokenIdx; i < parsedText.Tokens.Length && parsedText.Tokens[i].Type != MercuryTokenType.Dot; ++i)
            {
                MercuryToken token = parsedText.Tokens[i];
                if (token.EndColumn > token.StartColumn && token.Type == MercuryTokenType.Variable && !string.IsNullOrWhiteSpace(token.Value))
                {
                    strList.Add(token.Value);
                }
            }
            //loop till the end of the previous statement
            for (int i = currentTokenIdx; i >= 0 && parsedText.Tokens[i].Type != MercuryTokenType.Dot; --i)
            {
                MercuryToken token = parsedText.Tokens[i];
                if (token.EndColumn > token.StartColumn && token.Type == MercuryTokenType.Variable && !string.IsNullOrWhiteSpace(token.Value))
                {
                    strList.Add(token.Value);
                }
            }

            string fileFullPath;
            HashSet<string> completions = new HashSet<string>(strList);
            ParsedText importParsedText;
            if (currentToken.Type == MercuryTokenType.Dot && currentTokenIdx > 0)
            {
                MercuryToken beforeDot = parsedText.Tokens[currentTokenIdx - 1];
                if (!string.IsNullOrWhiteSpace(beforeDot.Value))
                {
                    foreach (string import in parsedText.Imports)
                    {
                        if (import.Equals(beforeDot.Value))
                        {
                            if (MercuryVSPackage.ParsedCache.GetFromImportName(beforeDot.Value, out fileFullPath, out importParsedText))
                            {
                                foreach (string c in importParsedText.DeclarationsAvailableFromOutside)
                                {
                                    completions.Add(c);
                                }
                            }
                            break;
                        }
                    }
                    completionSets.Add(new CompletionSet(
                        "ModuleDeclarations",
                        "ModuleDeclarations",
                        this.FindTokenSpanAtPosition(session.GetTriggerPoint(this.textBuffer),
                            session),
                        Convert(completions),
                        null));
                    return;
                }
            }
            else
            {
                foreach (string import in parsedText.Imports)
                {
                    if (MercuryVSPackage.ParsedCache.GetFromImportName(import, out fileFullPath, out importParsedText))
                    {
                        foreach (string c in importParsedText.DeclarationsAvailableFromOutside)
                        {
                            completions.Add(c);
                        }
                    }
                }
            }
            foreach (string c in parsedText.DeclarationsAvailableFromInside)
            {
                completions.Add(c);
            }

            completionSets.Add(new CompletionSet(
                "Tokens",    // the non-localized title of the tab 
                "Tokens",    // the display title of the tab
                this.FindTokenSpanAtPosition(session.GetTriggerPoint(this.textBuffer),
                    session),
                Convert(completions),
                null));
        }

        private List<Microsoft.VisualStudio.Language.Intellisense.Completion> Convert(HashSet<string> completions)
        {
            var resultList = new List<Microsoft.VisualStudio.Language.Intellisense.Completion>();
            foreach (string completion in completions)
            {
                resultList.Add(new Microsoft.VisualStudio.Language.Intellisense.Completion(completion, completion, completion, null, null));
            }
            return resultList;
        }

        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession session)
        {
            SnapshotPoint currentPoint = session.TextView.Caret.Position.BufferPosition - 1;
            var navigator = sourceProvider.NavigatorService.GetTextStructureNavigator(textBuffer);
            var extent = navigator.GetExtentOfWord(currentPoint);
            var text = extent.Span.GetText();
            System.Diagnostics.Debug.WriteLine("t: " + text);
            if (!string.IsNullOrEmpty(text.Trim().Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty)))
            {
                return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
            }

            return currentPoint.Snapshot.CreateTrackingSpan(
                currentPoint.Position,
                0,
                SpanTrackingMode.EdgeInclusive);
        }

        private bool m_isDisposed;
        public void Dispose()
        {
            if (!m_isDisposed)
            {
                GC.SuppressFinalize(this);
                m_isDisposed = true;
            }
        }
    }
}
