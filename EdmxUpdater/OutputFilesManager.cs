using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RS.Data.EdmxUpdater
{
    public class OutputFilesManager
    {
        private EdmxOptions options;

        public OutputFilesManager(EdmxOptions options)
        {
            this.options = options;
        }
        public void CopyGeneratedFilesToProject()
        {
            var outputDirectory = new DirectoryInfo(Configurator.OutputPath);
            var edmxDirectory = new DirectoryInfo(new FileInfo(Configurator.EdmxFilePath).Directory.FullName);

            var resultedEdmxFile = outputDirectory.GetFiles("*" + Constants.FilesExtensions.EDMX);
            if (resultedEdmxFile != null && resultedEdmxFile.Length > 0)
            {
                string resultedEdmxFilePath = resultedEdmxFile[0].FullName;
                var edmxCopyPath = Path.Combine(edmxDirectory.FullName, Path.GetFileName(resultedEdmxFilePath));
                File.Copy(resultedEdmxFilePath, edmxCopyPath, true);
            }

            var resultedDiagramFile = outputDirectory.GetFiles("*" + Constants.FilesExtensions.Diagram);
            if (resultedDiagramFile != null && resultedDiagramFile.Length > 0)
            {
                string resultedDiagramFilePath = resultedDiagramFile[0].FullName;
                var diagramCopyPath = Path.Combine(edmxDirectory.FullName, Path.GetFileName(resultedDiagramFilePath));
                File.Copy(resultedDiagramFilePath, diagramCopyPath, true);
            }

            var codeFiles = outputDirectory.GetFiles("*" + Constants.FilesExtensions.Class);
            foreach (var file in codeFiles)
            {
                var content = File.ReadAllText(file.FullName);
                content = Constants.SettingsConstants.Namespace + Configurator.EntitiesNamespace + "\n{\n" + content + "\n}\n";
                File.WriteAllText(file.FullName, content);

                File.Copy(file.FullName, Path.Combine(edmxDirectory.FullName, Path.GetFileName(file.FullName)), true);
            }
        }

        public void CopyFilesToOutputDirectory()
        {
            var directory = new DirectoryInfo(new FileInfo(Configurator.EdmxFilePath).Directory.FullName);
            File.Copy(Configurator.EdmxFilePath, Path.Combine(Configurator.OutputPath, Configurator.EdmxName + Constants.FilesExtensions.EDMX), true);
            File.Copy(Path.Combine(directory.FullName, Configurator.EdmxName + Constants.FilesExtensions.Diagram), Path.Combine(Configurator.OutputPath, Configurator.EdmxName + Constants.FilesExtensions.Diagram), true);
            File.Copy(Path.Combine(directory.FullName, Configurator.EdmxName + Constants.FilesExtensions.Template), Path.Combine(Configurator.OutputPath, Configurator.EdmxName + Constants.FilesExtensions.Template), true);
            File.Copy(Path.Combine(directory.FullName, Configurator.EdmxName + Constants.FilesExtensions.ContextTemplate), Path.Combine(Configurator.OutputPath, Configurator.EdmxName + Constants.FilesExtensions.ContextTemplate), true);

            var templatesDirectory = new DirectoryInfo(options.EFTemplatesPath);
            var templates = templatesDirectory.GetFiles();
            foreach (var file in templates)
                File.Copy(file.FullName, Path.Combine(Configurator.EFTemplatesPath, Path.GetFileName(file.FullName)), true);
        }
    }
}