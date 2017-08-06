using EnvDTE;
using MercuryLangPlugin.Highlight;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;

namespace MercuryLangPlugin.Providers
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(Tools.MercuryContentTypeName)]
    internal sealed class MClassifierProvider : IClassifierProvider
    {
#pragma warning disable 0169, 0649 // Supress the "not" initialized warning
        [Import]
        private IClassificationTypeRegistryService classificationRegistry;

        [Import]
        internal SVsServiceProvider ServiceProvider = null;

#pragma warning restore 0169, 0649

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            if (textBuffer == null)
                return null;
            DTE dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            //dte.ActiveDocument.FullName
            return new MercuryClassifier(textBuffer, classificationRegistry, dte);
        }
    }
}
