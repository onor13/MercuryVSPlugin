using Microsoft.VisualStudio.Text.Tagging;

namespace MercuryLangPlugin
{
    public class PredefinedTextMarkerTags
    {
        public static readonly TextMarkerTag AddLine = new TextMarkerTag(PredefinedMarkerFormats.AddLine);
        public static readonly TextMarkerTag AddWord = new TextMarkerTag(PredefinedMarkerFormats.AddWord);
        public static readonly TextMarkerTag Blue = new TextMarkerTag(PredefinedMarkerFormats.Blue);
        public static readonly TextMarkerTag Bookmark = new TextMarkerTag(PredefinedMarkerFormats.Bookmark);
        public static readonly TextMarkerTag BraceMatching = new TextMarkerTag(PredefinedMarkerFormats.BraceMatching);
        public static readonly TextMarkerTag Breakpoint = new TextMarkerTag(PredefinedMarkerFormats.Breakpoint);
        public static readonly TextMarkerTag CurrentStatement = new TextMarkerTag(PredefinedMarkerFormats.CurrentStatement);
        public static readonly TextMarkerTag RemoveLine = new TextMarkerTag(PredefinedMarkerFormats.RemoveLine);
        public static readonly TextMarkerTag RemoveWord = new TextMarkerTag(PredefinedMarkerFormats.RemoveWord);
        public static readonly TextMarkerTag ReturnStatement = new TextMarkerTag(PredefinedMarkerFormats.ReturnStatement);
        public static readonly TextMarkerTag StepBackCurrentStatement = new TextMarkerTag(PredefinedMarkerFormats.StepBackCurrentStatement);
        public static readonly TextMarkerTag Vivid = new TextMarkerTag(PredefinedMarkerFormats.Vivid);
    }

    public static class PredefinedMarkerFormats
    {
        public const string AddLine = "add line";
        public const string AddWord = "add word";
        public const string Blue = "blue";
        public const string Bookmark = "bookmark";
        public const string BraceMatching = "brace matching";
        public const string Breakpoint = "breakpoint";
        public const string CurrentStatement = "currentstatement";
        public const string RemoveLine = "remove line";
        public const string RemoveWord = "remove word";
        public const string ReturnStatement = "returnstatement";
        public const string StepBackCurrentStatement = "stepbackcurrentstatement";
        public const string Vivid = "vivid";
    }
}
