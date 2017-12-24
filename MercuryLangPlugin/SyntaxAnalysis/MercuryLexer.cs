using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MercuryLangPlugin.SyntaxAnalysis
{
    public enum LineContinuationInfo
    {
        Unknown,
        None,
        StringLiteral,
        Statement
    }

    public class MercuryLexer
    {
        private int lineNb = 0;
        private int currentPosition = 0;

        private LineContinuationInfo PreviousLine { get; set; }

        public LineContinuationInfo ContinuationInfo { get; private set; }
        public char[] Line { get; private set; }
        public int LineLength { get; private set; }


        public MercuryLexer(char[] line, LineContinuationInfo previousLine, int lineNumber)
        {
            PreviousLine = previousLine;
            ContinuationInfo = LineContinuationInfo.None;
            lineNb = lineNumber;
            Line = line;
            LineLength = line.Length;
        }

        public MercuryLexer(TextReader text)
        {
            PreviousLine = LineContinuationInfo.None;
            ContinuationInfo = LineContinuationInfo.None;
            this.Line = text.ReadToEnd().ToArray<char>();
            LineLength = Line.Length;
        }

        public MercuryLexer(string line, LineContinuationInfo previousLine, int lineNumber) :
            this(line.ToCharArray(), previousLine, lineNumber)
        {
        }

        public IEnumerable<MercuryToken> ColorableItems()
        {
            foreach (var token in Tokens())
            {
                if (token.Type == MercuryTokenType.Comment ||
                   token.Type == MercuryTokenType.Keyword ||
                   token.Type == MercuryTokenType.StringLiteral ||
                   token.Type == MercuryTokenType.Variable)
                {
                    yield return token;
                }
            }
            yield break;
        }


        public IEnumerable<MercuryToken> Tokens()
        {
            ContinuationInfo = LineContinuationInfo.None;
            while (currentPosition < LineLength)
            {
                if (PreviousLine == LineContinuationInfo.StringLiteral)
                {
                    yield return HandleStringLiteral(currentPosition);
                }
                else
                {
                    if (char.IsWhiteSpace(Line[currentPosition]))
                    {
                        int savedPos = currentPosition;
                        ++currentPosition;
                        while (currentPosition < Line.Length && char.IsWhiteSpace(Line[currentPosition]))
                        {
                            ++currentPosition;
                        }
                        --currentPosition;
                        yield return new MercuryToken()
                        {
                            Type = MercuryTokenType.WhiteSpaces,
                            LineNumber = lineNb,
                            StartColumn = savedPos,
                            EndColumn = currentPosition
                        };
                    }
                    else if (Line[currentPosition] == '%')
                    {
                        yield return new MercuryToken()
                        {
                            Type = MercuryTokenType.Comment,
                            LineNumber = lineNb,
                            StartColumn = currentPosition,
                            EndColumn = LineLength
                        };
                        break;
                    }
                    else if (Line[currentPosition] == '\r' || Line[currentPosition] == '\n')
                    {
                        int savedStartPos = currentPosition;
                        SkipNewLine();
                        yield return new MercuryToken()
                        {
                            Type = MercuryTokenType.NewLine,
                            StartColumn = savedStartPos,
                            LineNumber = lineNb,
                            EndColumn = currentPosition
                        };
                    }
                    else if (Line[currentPosition] == '"')
                    {
                        int savedStartPos = currentPosition;
                        ++currentPosition;
                        yield return HandleStringLiteral(currentPosition - 1);
                    }
                    else if (Line[currentPosition] == '.')
                    {
                        yield return new MercuryToken()
                        {
                            Type = MercuryTokenType.Dot,
                            LineNumber = lineNb,
                            StartColumn = currentPosition,
                            EndColumn = currentPosition,
                            Value = "."
                        };
                    }
                    else if (Line[currentPosition] == ';')
                    {
                        yield return new MercuryToken()
                        {
                            Type = MercuryTokenType.Semicolon,
                            LineNumber = lineNb,
                            StartColumn = currentPosition,
                            EndColumn = currentPosition
                        };
                    }
                    else if (Line[currentPosition] == '(')
                    {
                        yield return new MercuryToken()
                        {
                            Type = MercuryTokenType.LeftParanthesis,
                            LineNumber = lineNb,
                            StartColumn = currentPosition,
                            EndColumn = currentPosition
                        };
                    }
                    else if (Line[currentPosition] == ')')
                    {
                        yield return new MercuryToken()
                        {
                            Type = MercuryTokenType.RightParenthesis,
                            LineNumber = lineNb,
                            StartColumn = currentPosition,
                            EndColumn = currentPosition
                        };
                    }
                    else if (currentPosition + 1 < LineLength && Line[currentPosition] == ':' && Line[currentPosition + 1] == ':')
                    {
                        yield return new MercuryToken()
                        {
                            Type = MercuryTokenType.TypeModeSpecifier,
                            LineNumber = lineNb,
                            StartColumn = currentPosition,
                            EndColumn = currentPosition + 1
                        };
                        ++currentPosition;
                    }
                    else if (currentPosition + 3 < LineLength &&
                        Line[currentPosition] == '-' && Line[currentPosition + 1] == '-' &&
                        Line[currentPosition + 2] == '-' && Line[currentPosition + 3] == '>')
                    {
                        yield return new MercuryToken()
                        {
                            Type = MercuryTokenType.ThreeDashesArrow,
                            LineNumber = lineNb,
                            StartColumn = currentPosition,
                            EndColumn = currentPosition + 3
                        };
                        currentPosition = currentPosition + 3;
                    }
                    else if (currentPosition + 1 < LineLength && Line[currentPosition] == ':' && Line[currentPosition + 1] == '-')
                    {
                        yield return new MercuryToken()
                        {
                            Type = MercuryTokenType.Decl,
                            LineNumber = lineNb,
                            StartColumn = currentPosition,
                            EndColumn = currentPosition + 1
                        };
                        ++currentPosition;
                    }
                    else if (char.IsUpper(Line[currentPosition]) || Line[currentPosition] == '_')
                    {
                        int savedStartPos = currentPosition;
                        ++currentPosition;
                        AdvanceToTheEndOfdentifier();
                        MercuryToken token = new MercuryToken()
                        {
                            Type = MercuryTokenType.Variable,
                            LineNumber = lineNb,
                            StartColumn = savedStartPos,
                            EndColumn = currentPosition
                        };
                        char[] identifier = new char[token.EndColumn - token.StartColumn + 1];
                        Array.Copy(Line, savedStartPos, identifier, 0, identifier.Length);
                        yield return new MercuryToken()
                        {
                            Type = MercuryTokenType.Variable,
                            LineNumber = lineNb,
                            StartColumn = savedStartPos,
                            EndColumn = currentPosition,
                            Value = new string(identifier)
                        };
                    }
                    else if (char.IsLower(Line[currentPosition]))
                    {
                        int savedStartPos = currentPosition;
                        ++currentPosition;
                        AdvanceToTheEndOfdentifier();
                        int identifierLength = currentPosition - savedStartPos + 1;
                        MercuryToken token = new MercuryToken();
                        token.LineNumber = lineNb;
                        token.StartColumn = savedStartPos;
                        token.EndColumn = currentPosition;

                        token.Value = new String(Line, savedStartPos, identifierLength);
                        if (Keywords.IsMercuryKeyword(token.Value))
                        {
                            token.Type = MercuryTokenType.Keyword;
                            yield return token;
                        }
                        else
                        {
                            token.Type = MercuryTokenType.Identifier;
                            yield return token;
                        }

                    }
                }
                ++currentPosition;
            }

            yield return new MercuryToken()
            {
                Type = MercuryTokenType.NewLine,
                LineNumber = lineNb,
                StartColumn = currentPosition,
                EndColumn = currentPosition
            };
            yield break;
        }

        private bool NotEndOfLiteralString()
        {
            if (currentPosition < LineLength)
            {
                if (Line[currentPosition] != '"')
                {
                    return true;
                }
                else
                {
                    int nextPos = currentPosition + 1;
                    if (nextPos < LineLength && Line[nextPos] == '"')
                    {
                        ++currentPosition;
                        return true;
                    }
                }
            }
            return false;
        }

        private MercuryToken HandleStringLiteral(int localStartPosition)
        {
            while (NotEndOfLiteralString())
            {
                ++currentPosition;
            }
            if (PreviousLine == LineContinuationInfo.StringLiteral)
            {
                PreviousLine = LineContinuationInfo.None;
            }
            if (currentPosition < LineLength)
            {
                return new MercuryToken()
                {
                    Type = MercuryTokenType.StringLiteral,
                    LineNumber = lineNb,
                    StartColumn = localStartPosition,
                    EndColumn = currentPosition
                };
            }
            else
            {
                ContinuationInfo = LineContinuationInfo.StringLiteral;
                return new MercuryToken()
                {
                    Type = MercuryTokenType.StringLiteral,
                    LineNumber = lineNb,
                    StartColumn = localStartPosition,
                    EndColumn = LineLength
                };
            }
        }

        public static bool IsMultiLine(LineContinuationInfo continuationInfo)
        {
            return continuationInfo != LineContinuationInfo.Unknown && continuationInfo != LineContinuationInfo.None;
        }

        private void AdvanceToTheEndOfdentifier()
        {
            while (currentPosition < LineLength && (char.IsLetterOrDigit(Line[currentPosition]) || Line[currentPosition] == '_'))
            {
                ++currentPosition;
            }
            --currentPosition;

        }

        private void SkipNewLine()
        {
            while (currentPosition < LineLength && Line[currentPosition] == '\r' || Line[currentPosition] == '\n')
            {
                ++currentPosition;
            }
            --currentPosition;
        }
    }
}
