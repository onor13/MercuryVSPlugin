using MercuryLangPlugin.SyntaxAnalysis;
using MercuryLangPlugin.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace MercuryLangPlugin
{
    public class ParsedTreeCache
    {
        private ConditionalWeakTable<SourceText, ParsedText> sources =
            new ConditionalWeakTable<SourceText, ParsedText>();

        public ParsedTreeCache()
        {
            sources = new ConditionalWeakTable<SourceText, ParsedText>();
        }

        public ParsedText Get(SourceText sourceText)
        {

            ParsedText parsedText;
            if (sources.TryGetValue(sourceText, out parsedText))
            {
                return parsedText;
            }

            parsedText = new ParsedText(sourceText.TextReader);
            sources.Add(sourceText, parsedText);

            return parsedText;
        }

        public ParsedText GetFromFullPath(string filePath)
        {
            return Get(MercuryVSPackage.TextCache.GetFromFullPath(filePath));
        }

        public bool GetFromImportName(string fileName, out string fileFullPath, out ParsedText text)
        {
            SourceText source;
            if(MercuryVSPackage.TextCache.Get(fileName + ".m", out fileFullPath, out source))
            {
                text= Get(source);
                return true;
            }
            return MercuryVSPackage.Libraries.Get(fileName, out fileFullPath, out text);
        }
      
    }
}
