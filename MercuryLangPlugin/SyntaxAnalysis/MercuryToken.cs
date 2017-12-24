
namespace MercuryLangPlugin.SyntaxAnalysis
{
    public enum MercuryTokenType
    {
        None,
        ThreeDashesArrow, // --->
        WhiteSpaces,
        NewLine,
        LeftParanthesis,
        RightParenthesis,
        TypeModeSpecifier,
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
