using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace RS.Data.EdmxUpdater
{
    public class CsprojManager
    {
        private List<string> classesToRemoveNames = new List<string>();
        public EdmxOptions options;

        public CsprojManager(EdmxOptions options)
        {
            this.options = options;
        }
        public void UpdateProject()
        {
            SetCsprojContent();
            RemoveClasses();
        }

        private List<string> GetEntitiesClassesNames()
        {
            List<string> entitiesClassesNames = new List<string>();
            var templates = new DirectoryInfo(Configurator.OutputPath).GetFiles("*" + Constants.FilesExtensions.Class);
            foreach (var file in templates)
            {
                if (!file.Name.Contains(Path.GetFileNameWithoutExtension(Configurator.EdmxFilePath)))
                    entitiesClassesNames.Add(file.Name);
            }

            return entitiesClassesNames;
        }

        private void SetCsprojContent()
        {
            var entitiesClassesName = GetEntitiesClassesNames();
            var edmxRelativePath = Configurator.EdmxRelativePath;
            var document = new XmlDocument();
            document.Load(Configurator.ProjectPath);

            var namespaceManager = new XmlNamespaceManager(document.NameTable);
            namespaceManager.AddNamespace("x", options.CsprojNamespace);
            XmlNodeList nodes = document.DocumentElement.SelectNodes("//x:" + Constants.NodesNames.CompileNode, namespaceManager);
            if (nodes == null || nodes.Count == 0)
                return;

            var itemGroupNode = nodes[0].ParentNode;
            foreach (XmlNode node in nodes)
            {
                if (node.ChildNodes.Count != 1)
                    continue;

                XmlNode child = node.ChildNodes[0];
                if (child.Name != Constants.NodesNames.DependentUpon || !child.InnerText.Contains(Constants.FilesExtensions.Template))
                    continue;

                var entityName = node.Attributes["Include"].Value;
                entityName = entityName.Substring(entityName.LastIndexOf("\\") + 1);

                if (!entitiesClassesName.Contains(entityName))
                {
                    classesToRemoveNames.Add(entityName);
                    itemGroupNode.RemoveChild(node);
                }
                else entitiesClassesName.Remove(entityName);

            }

            for (int i = 0; i < entitiesClassesName.Count; i++)
            {
                var entity = entitiesClassesName[i];
                if (!string.IsNullOrEmpty(edmxRelativePath))
                    entity = Path.Combine(edmxRelativePath, entity);
                var node = CreateEntityClassNode(document, entity);
                itemGroupNode.AppendChild(node);
            }

            document.Save(Configurator.ProjectPath);
        }

        private XmlNode CreateEntityClassNode(XmlDocument document, string entityName)
        {
            var modelName = Configurator.EdmxName + Constants.FilesExtensions.Template;
            XmlNode compileNode = document.CreateNode(XmlNodeType.Element, Constants.NodesNames.Compile, document.DocumentElement.NamespaceURI);
            XmlAttribute includeAttribute = document.CreateAttribute("Include");
            includeAttribute.Value = entityName;
            compileNode.Attributes.Append(includeAttribute);
            XmlNode dependentUponNode = document.CreateNode(XmlNodeType.Element, Constants.NodesNames.DependentUpon, document.DocumentElement.NamespaceURI);
            dependentUponNode.InnerText = modelName;
            compileNode.AppendChild(dependentUponNode);

            return compileNode;
        }

        private void RemoveClasses()
        {
            if (classesToRemoveNames.Count == 0)
                return;

            var directory = new DirectoryInfo(new FileInfo(Configurator.EdmxFilePath).Directory.FullName);
            foreach (var file in classesToRemoveNames)
            {
                var fileToRemove = directory.GetFiles(file);
                if (fileToRemove.Length > 0)
                    fileToRemove[0].Delete();
            }
        }
    }
}