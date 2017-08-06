using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace MercuryLangPlugin.Commands
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(Tools.MercuryContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    class CommandFilterProvider : IVsTextViewCreationListener
    {
        [Import(typeof(IVsEditorAdaptersFactoryService))]
        internal IVsEditorAdaptersFactoryService editorFactory = null;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView textView = editorFactory.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            AddCommandFilter(textViewAdapter, new EditorCommandFilter(textView));
        }

        void AddCommandFilter(IVsTextView viewAdapter, EditorCommandFilter commandFilter)
        {
            //get the view adapter from the editor factory
            IOleCommandTarget next;
            int hr = viewAdapter.AddCommandFilter(commandFilter, out next);

            if (hr == VSConstants.S_OK)
            {
                if (next != null)
                    commandFilter.SetNextTarget(next);
            }
        }
    }
}
