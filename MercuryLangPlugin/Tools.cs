
using System.IO;
using System.Linq;

namespace MercuryLangPlugin
{
    public static class Tools
    {
        public const string MercuryContentTypeName = "mercury";
        public const string MercuryLangExt = ".m";
        public const string BaseDefinition = "text";

        public static string FilePathFromFileName(string fileNameWithoutExtension)
        {
            return Directory.GetFiles(MercuryVSPackage.MercuryProjectDir, fileNameWithoutExtension + ".m", SearchOption.AllDirectories).FirstOrDefault();
        }
    }
}
