using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace MercuryLangPlugin.Text
{
    public class SourceText
    {
        public SourceText(string text)
        {
            this.text = text;
            this.Lines = this.GetLines();
        }

        private string text;

        public ImmutableArray<SourceTextLine> Lines { get; }

        public TextReader TextReader => new StringReader(this.text);

        public int? GetLineNumberFromIndex(int index)
        {
            for (int i = 0; i < this.Lines.Length; ++i)
            {
                if (index >= this.Lines[i].Start && index < this.Lines[i].End)
                {
                    return i;
                }
            }

            return null;
        }

        private ImmutableArray<SourceTextLine> GetLines()
        {
            List<SourceTextLine> lines = new List<SourceTextLine>();

            StringBuilder currentLine = new StringBuilder();

            int start = 0;

            bool addLine = false;

            for (int i = 0; i < this.text.Length; ++i)
            {
                if (addLine)
                {
                    addLine = false;
                    lines.Add(new SourceTextLine(currentLine.ToString(), start, i - 1 - start));
                    currentLine.Clear();
                    start = i - 1;
                }

                currentLine.Append(this.text[i]);

                if (this.text[i] == '\r')
                {
                    addLine = true;

                    if (i < this.text.Length - 1 || this.text[i + 1] == '\n')
                    {
                        i++;
                        currentLine.Append(this.text[i]);
                        continue;
                    }

                    continue;
                }

                if (this.text[i] == '\n')
                {
                    addLine = true;
                }
            }

            if (currentLine.Length > 0)
            {
                lines.Add(new SourceTextLine(currentLine.ToString(), start, this.text.Length - start));
            }

            return lines.ToImmutableArray();
        }

        internal static ITextBuffer GetTextBuffer(string fileFullName)
        {
            IVsUIShellOpenDocument openDoc = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
            IVsWindowFrame frame;
            Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp;
            IVsUIHierarchy hier;
            Guid logicalView = Microsoft.VisualStudio.VSConstants.LOGVIEWID_Code;
            uint itemid;
            if (ErrorHandler.Failed(openDoc.OpenDocumentViaProject(fileFullName, ref logicalView, out sp,
                                    out hier, out itemid, out frame)) || frame == null)
            {
                return null;
            }
            object docData;
            frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out docData);
            VsTextBuffer buffer = docData as VsTextBuffer;
            var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
            IVsEditorAdaptersFactoryService adapterFactoryService = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            return adapterFactoryService.GetDocumentBuffer(buffer);
        }
    }
}
