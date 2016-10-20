using RS.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RS.Data.EdmxUpdater
{
    public class EdmxGenerator
    {
        public EdmxOptions options;
        private string projectName;
        private string namespaceName;
        private string entityContainerName;
        private bool pluralizeOption;

        public EdmxGenerator(EdmxOptions options)
        {
            this.options = options;
        }

        private string BuildGenerateEdmxCommand()
        {
            SetGenerateEdmxCommandParameters();

            string command = Constants.CommandParameters.Mode + Constants.CommandDefaultOptions.Mode;
            command += Constants.CommandParameters.ConnectionString + "\"" + Configurator.ConnectionString + "\"";
            command += Constants.CommandParameters.Project + this.projectName;
            command += Constants.CommandParameters.EntityContainer + this.entityContainerName;
            command += Constants.CommandParameters.Namespace + this.namespaceName;
            command += Constants.CommandParameters.Language + Constants.CommandDefaultOptions.Language;
            command += Constants.CommandParameters.OutCsdl + "\"" + Configurator.OutputPath + "\\" + this.projectName + Constants.FilesExtensions.Csdl + "\"";
            command += Constants.CommandParameters.OutSsdl + "\"" + Configurator.OutputPath + "\\" + this.projectName + Constants.FilesExtensions.Ssdl + "\"";
            command += Constants.CommandParameters.OutMsl + "\"" + Configurator.OutputPath + "\\" + this.projectName + Constants.FilesExtensions.Msl + "\"";
            command += Constants.CommandParameters.OutObjectLayer + "\"" + Configurator.OutputPath + "\\" + this.projectName + Constants.FilesExtensions.ObjectLayer + "\"";
            command += Constants.CommandParameters.OutViews + "\"" + Configurator.OutputPath + "\\" + this.projectName + Constants.FilesExtensions.Views + "\"";
            if (pluralizeOption)
                command = command + Constants.CommandParameters.Pluralization;

            return command;
        }

        private void SetGenerateEdmxCommandParameters()
        {
            string csprojFileContent = File.ReadAllText(Configurator.ProjectPath);
            HtmlAgilityPack.HtmlDocument csprojHtmlDoc = new HtmlAgilityPack.HtmlDocument();
            csprojHtmlDoc.LoadHtml(csprojFileContent);
            HtmlAgilityPack.HtmlNode root = null;
            if (csprojHtmlDoc.DocumentNode != null)
            {
                root = csprojHtmlDoc.DocumentNode.SelectSingleNode(Constants.NodesNames.RootNode);
                if (root != null)
                    this.projectName = root.InnerText.ToString();
            }

            string edmxFileContent = File.ReadAllText(Configurator.EdmxFilePath);
            HtmlAgilityPack.HtmlDocument edmxHtmlDoc = new HtmlAgilityPack.HtmlDocument();
            edmxHtmlDoc.LoadHtml(edmxFileContent);
            if (edmxHtmlDoc.DocumentNode != null)
            {
                HtmlAgilityPack.HtmlNode conceptualModelsNode = edmxHtmlDoc.DocumentNode.SelectSingleNode(Constants.NodesNames.SchemaNodeFromRoot + "[@xmlns='" + options.CsdlNamespace + "']");
                if (conceptualModelsNode != null)
                {
                    this.namespaceName = conceptualModelsNode.Attributes["Namespace"].Value.ToString();
                    this.entityContainerName = conceptualModelsNode.SelectSingleNode(Constants.NodesNames.EntityContainerNode.ToLower()).Attributes["Name"].Value.ToString();
                }

                HtmlAgilityPack.HtmlNode designerNode = edmxHtmlDoc.DocumentNode.SelectSingleNode(Constants.NodesNames.EnablePluralizatioNode);
                if (designerNode != null)
                    this.pluralizeOption = Convert.ToBoolean(designerNode.Attributes["Value"].Value);
            }
        }

        public void GenerateEdmx(Action<string> logger)
        {
            var command = BuildGenerateEdmxCommand();
            Shell.ExecuteShellCommand(options.EdmGenFilePath, command, (output) =>
            {
                logger(output);
            });
        }
    }
}