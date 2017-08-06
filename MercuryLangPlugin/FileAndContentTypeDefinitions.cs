using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace MercuryLangPlugin
{
    class FileAndContentTypeDefinitions
    {
#pragma warning disable 0169, 0649 // Supress the "not" initialized warning
        [Export]
        [Name(Tools.MercuryContentTypeName)]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition MercuryTypeDefinition;

        [Export]
        [FileExtension(Tools.MercuryLangExt)]
        [ContentType(Tools.MercuryContentTypeName)]
        internal static FileExtensionToContentTypeDefinition MercuryExtensionDefinition;
#pragma warning restore 0169, 0649
    }
}
