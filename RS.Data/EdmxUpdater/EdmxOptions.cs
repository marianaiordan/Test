using RS.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RS.Data.EdmxUpdater
{
    public class EdmxOptions
    {
        public string TextTransformFilePath { get; set; }
        public string OutputFolderName { get; set; }
        public string EFTemplatesPath { get; set; }
        public string CsprojNamespace { get; set; }
        public string CsdlNamespace { get; set; }
        public string EdmxNamespace { get; set; }
        public string EdmGenFilePath { get; set; }
        public string SsdlNamespace { get; set; }
        public string MslNamespace { get; set; }
        public string UpdateDiagram { get; set; }
        public string NamespacePrefix { get; set; }
        public Dictionary<string, string> NamesToReplace { get; set; }

        private static EdmxOptions defaultOptions = null;
        public static EdmxOptions Default { get { return defaultOptions ?? (defaultOptions = GetDefaultOptions()); } }

        private static EdmxOptions GetDefaultOptions()
        {
            string assemblyName = AssemblyHelper.GetCurrentAssemblyName();
            var options = new EdmxOptions();

            options.TextTransformFilePath = GetFilePathVersion(@"C:\Program Files (x86)\Common Files\Microsoft Shared\TextTemplating\{0}\TextTransform.exe");

            options.OutputFolderName = "EdmGeneratorOutput";
            options.EFTemplatesPath = GetFilePathVersion(@"C:\Program Files (x86)\Microsoft Visual Studio {0}\Common7\IDE\Extensions\Microsoft\Entity Framework Tools\Templates\Includes", true);

            options.CsprojNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
            options.CsdlNamespace = "http://schemas.microsoft.com/ado/2009/11/edm";
            options.EdmxNamespace = "http://schemas.microsoft.com/ado/2009/11/edmx";
            options.EdmGenFilePath = Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "edmgen.exe");
            options.SsdlNamespace = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";
            options.MslNamespace = "http://schemas.microsoft.com/ado/2009/11/mapping/cs";

            return options;
        }

        private static string GetFilePathVersion(string pattern, bool isDirectory = false)
        {
            var versions = new string[] { "14.0", "12.0" };

            foreach (var item in versions)
            {
                string path = string.Format(pattern, item);
                if (isDirectory && Directory.Exists(path))
                    return path;
                if (!isDirectory && File.Exists(path))
                    return path;
            }

            return null;
        }
    }
}