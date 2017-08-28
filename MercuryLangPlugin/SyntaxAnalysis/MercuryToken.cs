
namespace MercuryLangPlugin.SyntaxAnalysis
{
    public enum MercuryTokenType
    {
        None,
        ThreeDashesArrow, // --->
        NewLine,
        Decl,
        Dot,
        Comment,
        Keyword,
        Semicolon,// ;
        StringLiteral,
        Variable,
        Identifier
    }
    public struct MercuryToken
    {
        public int LineNumber;
        public int StartColumn;
        public int EndColumn;
        public string Value;
        public MercuryTokenType Type;
    }
}
