using System.Configuration;
using System.IO;
using System.Reflection;

namespace RS.Data.EdmxUpdater
{
    public class Configurator
    {
        public static string OutputFolderName { get; set; }
        public static string ProjectPath { get; set; }
        public static string EFTemplatesPath { get; set; }
        public static string OutputPath { get; set; }
        public static string EdmxFilePath { get; set; }
        public static string EdmxRelativePath { get; set; }
        public static string EdmxName { get; set; }
        public static string EntitiesNamespace { get; set; }
        public static string ConnectionString { get; set; }

        public static void Initialize(string projectPath, string entitiesNamespace, string connectionString, EdmxOptions options, string edmxRelativePath = null)
        {
            InitializeFolders(projectPath, edmxRelativePath,options);

            Configurator.EdmxName = Path.GetFileNameWithoutExtension(Configurator.EdmxFilePath);
            Configurator.EntitiesNamespace = entitiesNamespace;
            Configurator.ConnectionString = connectionString;
            Configurator.EdmxRelativePath = edmxRelativePath;
        }

        private static void InitializeFolders(string projectPath, string edmxPath, EdmxOptions options)
        {
            Configurator.ProjectPath = projectPath;
            if (!Path.IsPathRooted(Configurator.ProjectPath))
                Configurator.ProjectPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), Configurator.ProjectPath));

            var edmxDirectoryPath = new FileInfo(Configurator.ProjectPath).Directory.FullName;
            var edmxRelativePath = edmxPath;
            if (!string.IsNullOrEmpty(edmxRelativePath))
                edmxDirectoryPath = Path.Combine(edmxDirectoryPath, edmxRelativePath);

            var directory = new DirectoryInfo(edmxDirectoryPath);
            var edmxFile = directory.GetFiles("*" + Constants.FilesExtensions.EDMX);
            if (edmxFile.Length > 0)
                Configurator.EdmxFilePath = edmxFile[0].FullName;

            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Configurator.OutputFolderName = options.OutputFolderName;
            Configurator.OutputPath = Path.Combine(basePath, Configurator.OutputFolderName);
            if (Directory.Exists(Configurator.OutputPath))
                Directory.Delete(Configurator.OutputPath, true);

            if (!Directory.Exists(Configurator.OutputPath))
                Directory.CreateDirectory(Configurator.OutputPath);

            var templatesDirectory = new DirectoryInfo(options.EFTemplatesPath);
            var destDirectoryPath = Path.Combine(Configurator.OutputPath, templatesDirectory.Name);
            if (Directory.Exists(destDirectoryPath))
                Directory.Delete(destDirectoryPath, true);

            if (Directory.Exists(destDirectoryPath))
                Configurator.EFTemplatesPath = new DirectoryInfo(destDirectoryPath).FullName;
            else Configurator.EFTemplatesPath = Directory.CreateDirectory(destDirectoryPath).FullName;
        }
    }
}