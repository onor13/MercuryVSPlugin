using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercuryLangPlugin.Commands
{
    public static class CommentCommand
    {
        public static bool CommentSelection(ITextSelection textSelection)
        {
            return CommentUncomment(textSelection, true);
        }

        public static bool UncommentSelection(ITextSelection textSelection)
        {
            return CommentUncomment(textSelection, false);
        }

        private static bool CommentUncomment(ITextSelection textSelection, bool doComment)
        {
            var selectionStartLine = textSelection.Start.Position.GetContainingLine();
            var selectionEndLine = GetSelectionEndLine(selectionStartLine, textSelection);
            if (doComment)
            {
                CommentIt(selectionStartLine, selectionEndLine);                
            }
            else
            {
                UncommentIt(selectionStartLine, selectionEndLine);
            }

            // Select the entirety of the lines that were just commented or uncommented, have to update start/end lines due to snapshot changes
            selectionStartLine = textSelection.Start.Position.GetContainingLine();
            selectionEndLine = GetSelectionEndLine(selectionStartLine, textSelection);
            textSelection.Select(new SnapshotSpan(selectionStartLine.Start, selectionEndLine.End), false);

            return true;
        }

        private static void UncommentIt(ITextSnapshotLine startLine, ITextSnapshotLine endLine)
        {
            using (var textEdit = startLine.Snapshot.TextBuffer.CreateEdit())
            {
                for (int i = startLine.LineNumber; i <= endLine.LineNumber; i++)
                {
                    var curLine = startLine.Snapshot.GetLineFromLineNumber(i);
                    string curLineText = curLine.GetTextIncludingLineBreak();
                    if (IsComment(curLineText))
                    {
                        int commentCharPosition = curLineText.IndexOf('%');
                        textEdit.Delete(curLine.Start.Position + commentCharPosition, 1);
                    }
                }

                textEdit.Apply();
            }
        }

        static bool IsComment(string text)
        {
            return text.TrimStart().StartsWith("%");
        }

        static bool IsCommented(ITextSnapshotLine line)
        {
            return line.GetText().TrimStart().StartsWith("%");
        }

        private static ITextSnapshotLine GetSelectionEndLine(ITextSnapshotLine selectionStartLine, ITextSelection textSelection)
        {
            var selectionEndLine = textSelection.End.Position.GetContainingLine();
            if (selectionStartLine.LineNumber != selectionEndLine.LineNumber && selectionEndLine.Start.Equals(textSelection.End.Position))
            {
                selectionEndLine = selectionEndLine.Snapshot.GetLineFromLineNumber(selectionEndLine.LineNumber - 1);
            }
            return selectionEndLine;
        }


        private static void CommentIt(ITextSnapshotLine startLine, ITextSnapshotLine endLine)
        {
            List<ITextSnapshotLine> lines = new List<ITextSnapshotLine>();
            int commentCharPosition = int.MaxValue;

            // Build up the line collection and determine the position that the comment char will be inserted into
            for (int i = startLine.LineNumber; i <= endLine.LineNumber; i++)
            {
                var curLine = startLine.Snapshot.GetLineFromLineNumber(i);
                string curLineText = curLine.GetText();
                int firstCharPosition = curLineText.Length - curLineText.TrimStart().Length;
                if (firstCharPosition < commentCharPosition)
                {
                    commentCharPosition = firstCharPosition;
                }

                lines.Add(curLine);
            }

            // Add the comment char to each line at commentCharPosition
            using (var textEdit = startLine.Snapshot.TextBuffer.CreateEdit())
            {
                foreach (var line in lines)
                {
                    textEdit.Insert(line.Start.Position + commentCharPosition, "%");
                }
                textEdit.Apply();
            }
        }
    }
}
