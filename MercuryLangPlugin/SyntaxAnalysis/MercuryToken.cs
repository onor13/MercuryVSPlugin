
namespace MercuryLangPlugin.SyntaxAnalysis
{
    public enum MercuryTokenType
    {
        None,
        NewLine,
        Decl,
        Dot,
        Comment,
        Keyword,
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
