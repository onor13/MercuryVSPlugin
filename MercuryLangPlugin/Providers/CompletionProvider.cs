using MercuryLangPlugin.Completion;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace MercuryLangPlugin.Providers
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType(Tools.MercuryContentTypeName)]
    [Name("token completion")]
    internal class CompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }
        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new MercuryCompletion(this, textBuffer);
        }
    }
}
