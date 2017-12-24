using System.Collections.Generic;
using System.IO;
using System.Linq;
using static MercuryLangPlugin.Keywords;

namespace MercuryLangPlugin.SyntaxAnalysis
{
    public class ParsedText
    {
        public MercuryToken[] Tokens { get; private set; }
        public HashSet<string> Imports { get; private set; }
        HashSet<MercuryToken> interfaceDeclarations; //TODO: separated func/pred declaration and type/typeclass
        HashSet<MercuryToken> implementationDeclarations;
        HashSet<MercuryToken> allDeclarations;
        MercuryParser parser;

        public ParsedText(string filePath) : this(new StreamReader(filePath))
        {
        }

        public ParsedText(TextReader textReader) : this(GetLines(textReader))
        {
        }

        public ParsedText(IEnumerable<string> lines)
        {
            interfaceDeclarations = new HashSet<MercuryToken>();
            var tokens = new List<MercuryToken>();

            LineContinuationInfo continuationInfo = LineContinuationInfo.None;
            int lineNumber = 0;
            foreach (string line in lines)
            {
                var lexer = new MercuryLexer(line.ToArray(), continuationInfo, lineNumber);
                continuationInfo = lexer.ContinuationInfo;
                ++lineNumber;
                tokens.AddRange(lexer.Tokens());
            }

            Tokens = tokens.ToArray();
            parser = new MercuryParser(Tokens);
            Imports = new HashSet<string>(parser.InterfaceImports);
            foreach (var import in parser.ImplementationImports)
            {
                Imports.Add(import);
            }

            interfaceDeclarations = parser.InterfaceDeclarations;
            implementationDeclarations = parser.ImplementationDeclarations;
            allDeclarations = new HashSet<MercuryToken>(interfaceDeclarations);
            foreach (var decl in parser.ImplementationDeclarations)
            {
                allDeclarations.Add(decl);
            }

        }

        private static IEnumerable<string> GetLines(TextReader reader)
        {
            List<string> lines = new List<string>();
            string line = reader.ReadLine();
            while (line != null)
            {
                lines.Add(line);
                line = reader.ReadLine();
            }
            return lines;
        }

        private HashSet<string> declarationsAvailableFromOutside;
        public HashSet<string> DeclarationsAvailableFromOutside
        {
            get
            {
                if (declarationsAvailableFromOutside != null)
                {
                    return declarationsAvailableFromOutside;

                }
                declarationsAvailableFromOutside = GetCompletions(interfaceDeclarations);
                foreach (string import in Imports)
                {
                    declarationsAvailableFromOutside.Add(import);
                }
                return declarationsAvailableFromOutside;
            }
        }

        private HashSet<string> declarationsAvailableFromInside;
        public IEnumerable<string> DeclarationsAvailableFromInside
        {
            get
            {
                if (declarationsAvailableFromInside == null)
                {
                    declarationsAvailableFromInside = DeclarationsAvailableFromOutside;
                    foreach (string completion in GetCompletions(implementationDeclarations))
                    {
                        declarationsAvailableFromInside.Add(completion);
                    }
                }
                return declarationsAvailableFromInside;
            }
        }

        /// <summary>
        /// fileInfo will be null, if same file
        /// </summary>
        public bool FindDeclaration(string name, out MercuryToken declarationToken, out string fileFullPath)
        {
            foreach (MercuryToken decToken in allDeclarations)
            {
                if (decToken.Value.Equals(name))
                {
                    declarationToken = decToken;
                    fileFullPath = null;
                    return true;
                }
            }
            return FindDeclarationFromImports(name, out declarationToken, out fileFullPath);
        }

        public static bool EndOfStmt(int pos, MercuryToken[] tokens)
        {
            if (pos < tokens.Length && tokens[pos].Type == MercuryTokenType.Dot)
            {
                ++pos;
                if (pos < tokens.Length)
                {
                    return (tokens[pos].Type == MercuryTokenType.WhiteSpaces || tokens[pos].Type == MercuryTokenType.NewLine) ? true : false;
                }
                return true;
            }
            return false;
        }

        private bool FindDeclarationFromImports(string name, out MercuryToken declarationToken, out string fileFullPath)
        {
            ParsedText importParsedText;
            foreach (string import in Imports)
            {
                if (MercuryVSPackage.ParsedCache.GetFromImportName(import, out fileFullPath, out importParsedText))
                {
                    foreach (MercuryToken decToken in importParsedText.interfaceDeclarations)
                    {
                        if (decToken.Value.Equals(name))
                        {
                            declarationToken = decToken;
                            return true;
                        }
                    }
                }
            }
            declarationToken = new MercuryToken();
            fileFullPath = null;
            return false;
        }

        private HashSet<string> GetCompletions(HashSet<MercuryToken> tokens)
        {
            List<Microsoft.VisualStudio.Language.Intellisense.Completion> completions = new List<Microsoft.VisualStudio.Language.Intellisense.Completion>();
            HashSet<string> strCompletions = new HashSet<string>();
            foreach (var token in tokens)
            {
                if (token.EndColumn > token.StartColumn && !string.IsNullOrWhiteSpace(token.Value))
                {
                    strCompletions.Add(token.Value);
                }
            }
            return strCompletions;
        }

        private class MercuryParser
        {  //TODO: separated func/pred declaration and type/typeclass
            HashSet<string> interfaceImports;
            HashSet<MercuryToken> interfaceDeclarations;

            HashSet<string> implementationImports;
            HashSet<MercuryToken> implementationDeclarations;

            public HashSet<string> InterfaceImports => interfaceImports;
            public HashSet<string> ImplementationImports => implementationImports;

            public HashSet<MercuryToken> InterfaceDeclarations => interfaceDeclarations;
            public HashSet<MercuryToken> ImplementationDeclarations => implementationDeclarations;
            string moduleName;

            private System.Predicate<MercuryToken> foundImplementationCondition =
                (t => t.Type == MercuryTokenType.Keyword && t.Value != null && Keywords.GetType(t.Value).Equals(KeywordType.Implementation));

            public MercuryParser(MercuryToken[] tokens)
            {
                int index = 0;
                while (index < tokens.Length &&
                    !(tokens[index].Value != null && Keywords.GetType(tokens[index].Value).Equals(KeywordType.Implementation)))
                {
                    ++index;
                }
                var interfaceTokens = new MercuryToken[index];
                System.Array.Copy(tokens, 0, interfaceTokens, 0, index);
                Parse(interfaceTokens, out interfaceImports, out interfaceDeclarations);

                int implementationTokensSize = tokens.Length - index;
                var implementationTokens = new MercuryToken[implementationTokensSize];
                System.Array.Copy(tokens, index, implementationTokens, 0, implementationTokensSize);
                Parse(implementationTokens, out implementationImports, out implementationDeclarations);
            }

            private void Parse(MercuryToken[] tokens, out HashSet<string> imports, out HashSet<MercuryToken> declarations)
            {
                imports = new HashSet<string>();
                declarations = new HashSet<MercuryToken>();
                int pos = 0;
                for (pos = 0; pos < tokens.Length; ++pos)
                {
                    var currToken = tokens[pos];
                    if (currToken.Type == MercuryTokenType.Decl)
                    {
                        pos = NextRelevantPosition(pos, tokens);
                        if (pos >= tokens.Length)
                        {
                            return;
                        }

                        currToken = tokens[pos];
                        if (currToken.Type == MercuryTokenType.Keyword && currToken.Value != null)
                        {
                            if (Keywords.GetType(currToken.Value) == KeywordType.Import)
                            {
                                pos = NextRelevantPosition(pos, tokens);
                                if (pos < tokens.Length)
                                {
                                    currToken = tokens[pos];
                                    if (currToken.Type == MercuryTokenType.Identifier && !string.IsNullOrEmpty(currToken.Value))
                                    {
                                        imports.Add(currToken.Value);
                                    }
                                }
                            }
                            else if (Keywords.GetType(currToken.Value) == KeywordType.Declaration)
                            {
                                if (currToken.Value.Equals("module"))
                                {
                                    ++pos;
                                    if (pos < tokens.Length)
                                    {
                                        moduleName = tokens[pos].Value;
                                    }
                                }
                                else if (currToken.Value.Equals("type"))
                                {
                                    pos = NextRelevantPosition(pos, tokens);
                                    if (pos >= tokens.Length)
                                    {
                                        return;
                                    }
                                    currToken = tokens[pos];

                                    if (currToken.Type == MercuryTokenType.Identifier && !string.IsNullOrEmpty(currToken.Value))
                                    {
                                        declarations.Add(currToken);
                                        pos = NextRelevantPosition(pos, tokens);
                                        if (pos + 2 < tokens.Length && tokens[pos].Type == MercuryTokenType.ThreeDashesArrow)
                                        {
                                            pos = NextRelevantPosition(pos, tokens);
                                            if (pos >= tokens.Length)
                                            {
                                                return;
                                            }
                                            currToken = tokens[pos];
                                            if (currToken.Type == MercuryTokenType.Identifier && !string.IsNullOrEmpty(currToken.Value))
                                            {
                                                declarations.Add(currToken);
                                                pos = NextRelevantPosition(pos, tokens);
                                                while (pos < tokens.Length && !EndOfStmt(pos, tokens))
                                                {
                                                    if (tokens[pos].Type == MercuryTokenType.Semicolon)
                                                    {
                                                        pos = NextRelevantPosition(pos, tokens);
                                                        if (pos < tokens.Length && tokens[pos].Type == MercuryTokenType.Identifier &&
                                                            !string.IsNullOrEmpty(tokens[pos].Value))
                                                        {
                                                            declarations.Add(tokens[pos]);
                                                        }
                                                    }
                                                    pos = NextRelevantPosition(pos, tokens);
                                                    //TODO support cases like
                                                    //:- type employee
                                                    //       --->employee(
                                                    //               name:: string,
                                                    //               age:: int,
                                                    //               department:: string).
                                                    //Requires extending declaration to more than a simple Token.
                                                }
                                            }
                                        }
                                    }

                                }
                                else if (currToken.Value.Equals("typeclass"))
                                {
                                    pos = NextRelevantPosition(pos, tokens);
                                    if (pos >= tokens.Length)
                                    {
                                        return;
                                    }
                                    declarations.Add(tokens[pos]);
                                    pos = NextRelevantPosition(pos, tokens);
                                    while (pos < tokens.Length && !EndOfStmt(pos, tokens))
                                    {
                                        if (tokens[pos].Type == MercuryTokenType.Keyword &&
                                            ("pred".Equals(tokens[pos].Value) || "func".Equals(tokens[pos].Value)))
                                        {
                                            pos = NextRelevantPosition(pos, tokens);
                                            if (pos < tokens.Length && tokens[pos].Type == MercuryTokenType.Identifier &&
                                                            !string.IsNullOrEmpty(tokens[pos].Value))
                                            {
                                                declarations.Add(tokens[pos]);
                                            }
                                        }
                                        pos = NextRelevantPosition(pos, tokens);
                                    }
                                }
                                else
                                {
                                    pos = NextRelevantPosition(pos, tokens);
                                    if (pos >= tokens.Length)
                                    {
                                        return;
                                    }
                                    currToken = tokens[pos];
                                    if (currToken.Type == MercuryTokenType.Identifier && !string.IsNullOrEmpty(currToken.Value) && !currToken.Value.Equals("main"))
                                    {
                                        if (moduleName != null && currToken.Value == moduleName)
                                        {
                                            ++pos;
                                            currToken = tokens[pos];
                                            if ((pos + 1 < tokens.Length) && tokens[pos].Type == MercuryTokenType.Dot)
                                            {
                                                pos = NextRelevantPosition(pos, tokens);
                                                if (pos < tokens.Length)
                                                {
                                                    currToken = tokens[pos];
                                                    if (currToken.Type == MercuryTokenType.Identifier && !string.IsNullOrEmpty(currToken.Value))
                                                    {
                                                        declarations.Add(currToken);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            declarations.Add(currToken);
                                        }
                                    }
                                }

                            }

                        }
                    }
                }

            }

            private int SkipNotRelevantTokens(int pos, MercuryToken[] tokens)
            {
                while (pos < tokens.Length &&
                    (tokens[pos].Type == MercuryTokenType.Comment ||
                    tokens[pos].Type == MercuryTokenType.NewLine ||
                    tokens[pos].Type == MercuryTokenType.WhiteSpaces))
                {
                    ++pos;
                }
                return pos;
            }

            private int NextRelevantPosition(int pos, MercuryToken[] tokens)
            {
                ++pos;
                return SkipNotRelevantTokens(pos, tokens);
            }

        }

    }
}
