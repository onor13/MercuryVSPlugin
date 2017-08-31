using System;
using System.ComponentModel.Composition;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using MercuryLangPlugin.SyntaxAnalysis;
using MercuryLangPlugin.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MercuryLangPlugin.Commands
{
    class EditorCommandFilter : IOleCommandTarget
    {

        private IWpfTextView m_textView;
        private IOleCommandTarget m_nextTarget;

        public EditorCommandFilter(IWpfTextView textView)
        {
            this.m_textView = textView;
        }

        internal void SetNextTarget(IOleCommandTarget nextTarget)
        {
            this.m_nextTarget = nextTarget;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VSConstants.CMDSETID.StandardCommandSet97_guid && nCmdID == (uint)VSConstants.VSStd97CmdID.GotoDefn)
            {
                string currentPredicate = GetTokenAtCurrPosition();

                if (string.IsNullOrWhiteSpace(currentPredicate))
                    return (int)VSConstants.S_OK;

                EnvDTE80.DTE2 dte = Package.GetGlobalService(typeof(DTE)) as EnvDTE80.DTE2;

                if (dte == null || dte.ActiveDocument == null || dte.ActiveDocument.Object() == null || !(dte.ActiveDocument.Object() is TextDocument))
                {
                    return (int)VSConstants.S_OK;
                }
                string fileFullPath;
                ParsedText parsedText = MercuryVSPackage.ParsedCache.GetFromFullPath(dte.ActiveDocument.FullName);
                MercuryToken declarationToken;
                if (parsedText.FindDeclaration(currentPredicate, out declarationToken, out fileFullPath))
                {
                    if (string.IsNullOrEmpty(declarationToken.Value))
                    {
                        return VSConstants.S_OK;
                    }
                    if (fileFullPath != null)
                    {
                        dte.ItemOperations.OpenFile(fileFullPath, EnvDTE.Constants.vsViewKindCode);
                    }

                    ((TextSelection)dte.ActiveDocument.Selection).MoveToLineAndOffset(AdjustLineNumber(declarationToken.LineNumber), AdjustOffset(declarationToken.EndColumn));
                    return (int)VSConstants.S_OK;
                }
                return (int)VSConstants.S_OK;
            }
            else if (pguidCmdGroup == VSConstants.CMDSETID.StandardCommandSet97_guid && nCmdID == (uint)VSConstants.VSStd97CmdID.FindReferences)
            {
                string token = GetTokenAtCurrPosition();

                if (string.IsNullOrWhiteSpace(token))
                    return (int)VSConstants.S_OK;

                FindAllRefs(token);
                return VSConstants.S_OK;

            }
            else
            {
                if (m_nextTarget != null)
                {
                    EnvDTE80.DTE2 dte = Package.GetGlobalService(typeof(DTE)) as EnvDTE80.DTE2;
                    if (dte != null && dte.ActiveDocument != null && dte.ActiveDocument.Object() != null && (dte.ActiveDocument.Object() is TextDocument))
                    {
                        MercuryVSPackage.ParsedCache.setDirty(dte.ActiveDocument.FullName);
                    }
                    return m_nextTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                }
            }

            return (int)OLECMDF.OLECMDF_INVISIBLE;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == VSConstants.CMDSETID.StandardCommandSet97_guid &&
                (prgCmds[0].cmdID == (uint)VSConstants.VSStd97CmdID.GotoDefn || prgCmds[0].cmdID == (uint)VSConstants.VSStd97CmdID.FindReferences))
            {
                string currentPredicate = GetTokenAtCurrPosition();

                if (string.IsNullOrEmpty(currentPredicate))
                    prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_INVISIBLE);
                else
                    prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);

                //INdicate we handled the QueryStatus so the shell doesn't continue looking for handlers.
                return VSConstants.S_OK;
            }
            else if (m_nextTarget != null)
            {
                return m_nextTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }

            return (int)OLECMDF.OLECMDF_INVISIBLE;
        }

        private bool IsValidCharForIdentifier(char c)
        {
            return (char.IsLetterOrDigit(c) || c == '_');
        }

        string GetTokenAtCurrPosition()
        {
            string line = m_textView.Caret.Position.BufferPosition.GetContainingLine().GetText();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("%"))
                return null;

            int lineStart = m_textView.Caret.Position.BufferPosition.GetContainingLine().Start.Position;
            int bufferPosition = m_textView.Caret.Position.BufferPosition.Position;
            int linePosition = bufferPosition - lineStart;

            int startIdx = linePosition;
            int endIdx = linePosition;

            //go left
            if (IsValidCharForIdentifier(line[startIdx]))
            {
                for (; startIdx > 0; startIdx--)
                {
                    if (!IsValidCharForIdentifier(line[startIdx]))
                    {
                        ++startIdx;
                        break;
                    }
                }
            }

            //go right
            for (; endIdx < line.Length; endIdx++)
            {
                if (!IsValidCharForIdentifier(line[endIdx]))
                {
                    break;
                }
            }
            return line.Substring(startIdx, endIdx - startIdx).Trim();
        }

        internal void FindAllRefs(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return;

            ConcurrentBag<Tuple<string, MercuryToken>> allReferences = new ConcurrentBag<Tuple<string, MercuryToken>>();
            EnvDTE80.DTE2 dte = Package.GetGlobalService(typeof(DTE)) as EnvDTE80.DTE2;

            if (dte == null || dte.ActiveDocument == null || dte.ActiveDocument.Object() == null || !(dte.ActiveDocument.Object() is TextDocument))
            {
                return;
            }

            ParsedText parsedText = MercuryVSPackage.ParsedCache.GetFromFullPath(dte.ActiveDocument.FullName);

            if (string.IsNullOrEmpty(MercuryVSPackage.MercuryProjectDir))
            {
                return;
            }
            IVsUIShellOpenDocument openDoc = Package.GetGlobalService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
            Stack<string> dirs = new Stack<string>(10);
            dirs.Push(MercuryVSPackage.MercuryProjectDir);
            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = System.IO.Directory.GetDirectories(currentDir);
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    continue;
                }

                string[] files = null;
                try
                {
                    files = System.IO.Directory.GetFiles(currentDir, "*.m");
                }

                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                catch (System.IO.DirectoryNotFoundException)
                {
                    continue;
                }
                // Perform the required action on each file here.
                // Modify this block to perform your required task.
                Parallel.ForEach(files, (file) =>
                {
                    try
                    {
                        parsedText = MercuryVSPackage.ParsedCache.GetFromFullPath(file);
                        foreach (var mercuryToken in parsedText.Tokens)
                        {
                            if (mercuryToken.Type == MercuryTokenType.Identifier && token.Equals(mercuryToken.Value))
                            {
                                allReferences.Add(new Tuple<string, MercuryToken>(file, mercuryToken));
                            }
                        }
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        // If file was deleted by a separate application
                        //  or thread since the call to TraverseTree()
                        // then just continue.
                    }
                });

                // Push the subdirectories onto the stack for traversal.
                // This could also be done before handing the files.
                foreach (string str in subDirs)
                {
                    dirs.Push(str);
                }

            }


            IVsOutputWindow output = MercuryVSPackage.GetGlobalService(typeof(IVsOutputWindow)) as IVsOutputWindow;
            Guid guidFindRefOutput = new Guid();
            output.CreatePane(ref guidFindRefOutput, "FindAllReference", 1, 1);
            output.GetPane(ref guidFindRefOutput, out IVsOutputWindowPane pane);

            pane.Clear();
            pane.SetName("Find Symbol Results");
            pane.Activate();
            pane.OutputString("Find results:\n");

            foreach (var pair in allReferences)
            {
                pane.OutputString(FormatReference(pair.Item1, AdjustLineNumber(pair.Item2.LineNumber), AdjustOffset(pair.Item2.EndColumn)));
            }

            string projectFolder = MercuryVSPackage.MercuryProjectDir ?? "<unknown>";
            pane.OutputString("\nRemark:\nSearches in " + projectFolder);
        }

        static int AdjustLineNumber(int lineNumber)
        {
            return lineNumber + 1;
        }

        static int AdjustOffset(int column)
        {
            return column + 2;
        }

        static string FormatReference(string filePath, int line, int column)
        {
            return $"{filePath}({line}, {column})\n";
        }
    }
}
