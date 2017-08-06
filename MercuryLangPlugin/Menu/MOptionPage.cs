using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace MercuryLangPlugin.Menu
{
    public class MOptionPage : DialogPage
    {
        [Category("Sources")]
        [DisplayName("Project path")]
        [Description("Path to the project folder, used for intellisense")]
        public string ProjectDir { get; set; }

        [Category("Sources")]
        [DisplayName("Mercury libraries path")]
        [Description("Path to the mercury libraries directory, something like .../mercury-master/library")]
        public string LibrariesFolder { get; set; }

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);  
            MercuryVSPackage.Libraries = new SyntaxAnalysis.MercuryLibraries(LibrariesFolder);
            MercuryVSPackage.MercuryProjectDir = ProjectDir;
            MercuryVSPackage.TextCache = new Text.SourceTextCache();
            MercuryVSPackage.ParsedCache = new ParsedTreeCache();
        }

    }
}