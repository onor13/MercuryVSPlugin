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

        /// <summary>
        ///Dictionary<fileName, Tuple<filePath, ParsedText>>
        /// </summary>
        private Dictionary<string, Tuple<string, ParsedText>> deepFastCache = new Dictionary<string, Tuple<string, ParsedText>>();

        private HashSet<string> modulesToUpdate = new HashSet<string>();

        public ParsedTreeCache()
        {
            sources = new ConditionalWeakTable<SourceText, ParsedText>();
            deepFastCache = new Dictionary<string, Tuple<string, ParsedText>>();
            modulesToUpdate = new HashSet<string>();
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
            string moduleName = Path.GetFileNameWithoutExtension(filePath);
            if (modulesToUpdate.Contains(moduleName))
            {
                modulesToUpdate.Remove(moduleName);
                var result = new ParsedText(filePath);
                deepFastCache[moduleName] = new Tuple<string, ParsedText>(filePath, result);
                return result;
            }
            Tuple<string, ParsedText> tuple;
            deepFastCache.TryGetValue(moduleName, out tuple);
            if(tuple != null)
            {
                return tuple.Item2;
            }

            var parsedText = new ParsedText(filePath);

            tuple = new Tuple<string, ParsedText>(filePath, parsedText);
            deepFastCache[moduleName] = tuple;
            return parsedText;

        }

        public bool GetFromImportName(string fileName, out string fileFullPath, out ParsedText text)
        {
            Tuple<string, ParsedText> tuple;
            if (modulesToUpdate.Contains(fileName))
            {
                modulesToUpdate.Remove(fileName);
                deepFastCache.TryGetValue(fileName, out tuple);
                if(tuple != null)
                {
                    fileFullPath = tuple.Item1;
                }
                else
                {
                    fileFullPath = Tools.FilePathFromFileName(fileName);
                    if(fileFullPath == null)
                    {
                        text = null;
                        return false;
                    }
                }
                text = new ParsedText(fileFullPath);
                deepFastCache[fileName] = new Tuple<string, ParsedText>(fileFullPath, text);
                return true;
            }
            deepFastCache.TryGetValue(fileName, out tuple);
            if(tuple != null)
            {
                fileFullPath = tuple.Item1;
                text = tuple.Item2;
                return true;
            }

            if(MercuryVSPackage.Libraries.Get(fileName, out fileFullPath, out text))
            {
                return true;
            }
        
            fileFullPath = Tools.FilePathFromFileName(fileName);
            if(fileFullPath == null)
            {
                return false;
            }

            text = new ParsedText(fileFullPath);
            deepFastCache[fileName] = new Tuple<string, ParsedText>(fileFullPath, text);
            return text != null;

    
           
         /*   if(MercuryVSPackage.TextCache.Get(fileName + ".m", out fileFullPath, out source))
            {
                text= Get(source);
                return true;
            }
            return MercuryVSPackage.Libraries.Get(fileName, out fileFullPath, out text);*/
        }

        public void setDirty(string filePath)
        {
            modulesToUpdate.Add(Path.GetFileNameWithoutExtension(filePath));
        }
    }
}
