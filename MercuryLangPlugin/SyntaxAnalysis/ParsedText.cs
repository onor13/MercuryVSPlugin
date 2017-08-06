using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MercuryLangPlugin.SyntaxAnalysis
{
    public class ParsedText
    {      
        public MercuryToken[] Tokens { get; private set; }
        public HashSet<string> Imports { get; private set; }
        HashSet<MercuryToken> declarations; //TODO: separated func/pred declaration and type/typeclass

        public ParsedText(TextReader textReader):this(GetLines(textReader))
        {            
        }

        public ParsedText(IEnumerable<string> lines)
        {
            declarations = new HashSet<MercuryToken>();          
            var tokens = new List<MercuryToken>();

            LineContinuationInfo continuationInfo = LineContinuationInfo.None;
            int lineNumber = 0;
            foreach(string line in lines)
            {
                var lexer = new MercuryLexer(line.ToArray(), continuationInfo, lineNumber);
                continuationInfo = lexer.ContinuationInfo;
                ++lineNumber;
                tokens.AddRange(lexer.Tokens());
            }
          
            Tokens = tokens.ToArray();
            Parse();
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

        public IEnumerable<MercuryToken> LocalDeclarations
        {
            get { return declarations; }
        }

        /// <summary>
        /// fileInfo will be null, if same file
        /// </summary>
        public bool FindDeclaration(string name, out MercuryToken declarationToken, out string fileFullPath)
        {
            foreach (MercuryToken decToken in declarations)
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

        private bool FindDeclarationFromImports(string name, out MercuryToken declarationToken, out string fileFullPath)
        {
            ParsedText importParsedText;
            foreach (string import in Imports)
            {                          
                if(MercuryVSPackage.ParsedCache.GetFromImportName(import, out fileFullPath, out importParsedText))                
                {
                    foreach (MercuryToken decToken in importParsedText.declarations)
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

        private void Parse()
        {
            Imports = new HashSet<string>();
            int pos = 0;
            for(pos=0; pos < Tokens.Length; ++pos)
            {
                var currToken = Tokens[pos];
                if(currToken.Type == MercuryTokenType.Decl)
                {
                    ++pos;
                    if (pos < Tokens.Length)
                    {
                        currToken = Tokens[pos];
                        if(currToken.Type == MercuryTokenType.Keyword && currToken.Value!=null)
                        {
                            if(Keywords.GetType(currToken.Value) == Keywords.KeywordType.Import)
                            {
                                ++pos;
                                if(pos < Tokens.Length)
                                {
                                    currToken = Tokens[pos];
                                    if(currToken.Type == MercuryTokenType.Identifier && !string.IsNullOrEmpty(currToken.Value))
                                    {
                                        Imports.Add(currToken.Value);
                                    }
                                }
                            }
                            else if (Keywords.GetType(currToken.Value) == Keywords.KeywordType.Declaration)
                            {
                                ++pos;
                                if (pos < Tokens.Length)
                                {
                                    currToken = Tokens[pos];
                                    if (currToken.Type == MercuryTokenType.Identifier && !string.IsNullOrEmpty(currToken.Value) && !currToken.Value.Equals("main"))
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

        


    }
}
