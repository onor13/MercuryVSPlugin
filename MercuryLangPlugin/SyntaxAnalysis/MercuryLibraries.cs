using MercuryLangPlugin.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;

namespace MercuryLangPlugin.SyntaxAnalysis
{
    public class MercuryLibraries
    {
        private Dictionary<string, Tuple<string,ParsedText>> libraries = new Dictionary<string, Tuple<string,ParsedText>>();

        private Uri path= null;
        public MercuryLibraries()
        {
            path=null;
        }
        public MercuryLibraries(string directoryPath)
        {
            init(directoryPath);
        } 

        public bool IsInLibrary(string fileFullPath)
        {         
            if(path == null || fileFullPath == null)
            {
                return false;
            }
            return new Uri(fileFullPath).AbsolutePath.StartsWith(path.AbsolutePath);
        }

        public bool Get(string name, out string fileFullPath, out ParsedText text)
        {
            Tuple<string, ParsedText> result;
            libraries.TryGetValue(name, out result);
            if(result == null)
            {
                text = null;
                fileFullPath = null;
                return false;
            }
            text = result.Item2;
            fileFullPath = result.Item1;
            return text !=null;
        }

        private void init(string directoryPath)
        {
            if(directoryPath == null || !System.IO.Directory.Exists(directoryPath))
            {
                return;
            }
            path = new Uri(directoryPath);
            IVsUIShellOpenDocument openDoc = Package.GetGlobalService(typeof(IVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
            Stack<string> dirs = new Stack<string>(10);
            dirs.Push(directoryPath);
            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = System.IO.Directory.GetDirectories(currentDir);
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    continue;
                }

                string[] files = null;
                try
                {
                    files = System.IO.Directory.GetFiles(currentDir, "*.m");
                }

                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                catch (System.IO.DirectoryNotFoundException)
                {
                    continue;
                }
                // Perform the required action on each file here.
                // Modify this block to perform your required task.
                foreach (string file in files)
                {
                    try
                    {
                        // Perform whatever action is required in your scenario.  
                        var textBuffer = SourceText.GetTextBuffer(file);
                        var snapshot = textBuffer.CurrentSnapshot;
                        var lines = new List<string>();
                        foreach(var line in snapshot.Lines)
                        {
                            lines.Add(line.GetText());
                        }
                        libraries.Add(System.IO.Path.GetFileNameWithoutExtension(file), new Tuple<string, ParsedText>(file, new ParsedText(lines)));
                    
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        // If file was deleted by a separate application
                        //  or thread since the call to TraverseTree()
                        // then just continue.
                        continue;
                    }
                }

                // Push the subdirectories onto the stack for traversal.
                // This could also be done before handing the files.
                foreach (string str in subDirs)
                {
                    dirs.Push(str);
                }

            }

        }


    }
}
