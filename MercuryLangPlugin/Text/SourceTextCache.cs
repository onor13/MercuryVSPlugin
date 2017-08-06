using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Text;
using System.IO;
using System.Linq;

namespace MercuryLangPlugin.Text
{
    public class SourceTextCache
    {
        public SourceTextCache()
        {
            sources = new ConditionalWeakTable<ITextSnapshot, SourceText>();
        }
        public  SourceText Get(ITextSnapshot textSnapshot)
        {
            SourceText sourceText = null;
            if (sources.TryGetValue(textSnapshot, out sourceText))
            {
                return sourceText;
            }

            sourceText = new SourceText(textSnapshot.GetText());
            sources.Add(textSnapshot, sourceText);

            return sourceText;
        }

        public bool Get(string fileName, out string fileFullName, out SourceText text)
        {
            fileFullName = Directory.GetFiles(MercuryVSPackage.MercuryProjectDir, fileName, SearchOption.AllDirectories).FirstOrDefault();
            if(fileFullName == null)
            {
                text = null;
                return false;
            }
            text = GetFromFullPath(fileFullName);
            return text != null;
           
        }


        public SourceText GetFromFullPath(string path)
        {
            if (path == null)
            {
                return null;
            }
            var textBuffer = SourceText.GetTextBuffer(path);
            return Get(textBuffer.CurrentSnapshot);
        }

        private  ConditionalWeakTable<ITextSnapshot, SourceText> sources =
            new ConditionalWeakTable<ITextSnapshot, SourceText>();
    }
}
