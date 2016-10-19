using RS.Common;
using RS.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RS.Data.EdmxUpdater
{
    public class CodeFilesGenerator
    {
        private EdmxOptions options = null;

        public CodeFilesGenerator(EdmxOptions options)
        {
            this.options = options;
        }

        private string BuildCommand(string templateName, string namespaceToUse = null)
        {
            string command = "\"" + Path.Combine(Configurator.OutputPath, templateName) + "\"";
            command += Constants.CommandParameters.Includes + "\"" + Configurator.EFTemplatesPath + "\"";

            if (!namespaceToUse.IsNullOrWhiteSpace())
                command += " -u \"" + namespaceToUse + "\"";

            return command;
        }

        public void GenerateOutputFiles()
        {
            string templateName = Configurator.EdmxName + Constants.FilesExtensions.ContextTemplate;

            var generateContextCommand = BuildCommand(templateName);
            int result = Shell.ExecuteShellCommand(options.TextTransformFilePath, generateContextCommand, null, true, true);

            var generateModelCommand = BuildCommand(Configurator.EdmxName + Constants.FilesExtensions.Template);
            result = Shell.ExecuteShellCommand(options.TextTransformFilePath, generateModelCommand, null, true, true);
        }
    }
}