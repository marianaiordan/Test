using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RS.Common.Extensions;

namespace RS.Data.EdmxUpdater
{
    public class EdmxUpdateEngine
    {
        private EdmxOptions options;

        public EdmxUpdateEngine(EdmxOptions options = null)
        {
            this.options = options ?? EdmxOptions.Default;
        }
        public Action<string> Output { get; set; }

        public void Initialize(string projectPath, string entitiesNamespace, string connectionString, string edmxRelativePath = null)
        {
            Configurator.Initialize(projectPath, entitiesNamespace, connectionString, options, edmxRelativePath);

            SendMessage("Generating .csdl, .ssdl, .msl files...");
            new EdmxGenerator(options).GenerateEdmx(s => SendMessage(s));

            new OutputFilesManager(options).CopyFilesToOutputDirectory();
            new EFTemplatesCleaner().SetTemplatesContent();

            SendMessage("Generating .csdl, .ssdl, .msl files...");
            new EdmxManager(options).UpdateEdmx();

            if (Convert.ToBoolean(options.UpdateDiagram))
            {
                SendMessage("Updating diagram...");
                new DiagramManager(options).UpdateDiagram();
            }

            SendMessage("Generating code files...");
            new CodeFilesGenerator(options).GenerateOutputFiles();
            SendMessage("Generating output files...");
            new OutputFilesManager(options).CopyGeneratedFilesToProject();

            SendMessage("Updating project file...");
            new CsprojManager(options).UpdateProject();
        }

        private void SendMessage(string message)
        {
            if (message.IsNullOrWhiteSpace() || Output == null)
                return;

            Output(message);
        }
    }
}