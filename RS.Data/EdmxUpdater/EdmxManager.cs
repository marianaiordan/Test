using RS.Common;
using RS.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace RS.Data.EdmxUpdater
{
    public class EdmxManager
    {
        private EdmxOptions options;
        private XmlNode csdlSchemaNode;
        private XmlNode ssdlSchemaNode;
        private XmlNode mslMappingNode;

        public EdmxManager(EdmxOptions options)
        {
            this.options = options;
        }

        public void UpdateEdmx()
        {
            SetNodeContent(out csdlSchemaNode, Constants.NodesNames.SchemaNode, Constants.FilesExtensions.Csdl);
            SetNodeContent(out ssdlSchemaNode, Constants.NodesNames.SchemaNode, Constants.FilesExtensions.Ssdl);
            SetNodeContent(out mslMappingNode, Constants.NodesNames.MappingNode, Constants.FilesExtensions.Msl);

            string edmxCopyFilePath = Path.Combine(Configurator.OutputPath, Configurator.EdmxName + Constants.FilesExtensions.EDMX);
            var edmxCopyXmlDoc = new XmlDocument();
            edmxCopyXmlDoc.Load(edmxCopyFilePath);

            ReplaceNodesContent(edmxCopyXmlDoc, "ConceptualModels", csdlSchemaNode);
            ReplaceNodesContent(edmxCopyXmlDoc, "StorageModels", ssdlSchemaNode);
            ReplaceNodesContent(edmxCopyXmlDoc, "Mappings", mslMappingNode);

            RemoveFunctionsNodesFromSsdl(edmxCopyXmlDoc);

            var originalEdmx = new XmlDocument();
            originalEdmx.Load(Configurator.EdmxFilePath);

            AddFunctionsNodes(edmxCopyXmlDoc, originalEdmx, Constants.NodesNames.SsdlFunctionsNodes, Constants.NodesNames.SchemaNode, options.SsdlNamespace);
            AddFunctionsNodes(edmxCopyXmlDoc, originalEdmx, Constants.NodesNames.CsdlFunctionsNodes, Constants.NodesNames.EntityContainerNode, options.CsdlNamespace);
            AddFunctionsNodes(edmxCopyXmlDoc, originalEdmx, Constants.NodesNames.MslFunctionsNodes, Constants.NodesNames.EntityContainerMapping, options.MslNamespace);
            AddFunctionsNodes(edmxCopyXmlDoc, originalEdmx, Constants.NodesNames.ComplexType, Constants.NodesNames.SchemaNode, options.CsdlNamespace);

            UpdateEntitiesNames(edmxCopyXmlDoc, originalEdmx, ssdlSchemaNode);
            UpdateMappingProperties(edmxCopyXmlDoc, originalEdmx);
            UpdateNavigationsProperties(edmxCopyXmlDoc, originalEdmx);

            edmxCopyXmlDoc = ReplaceNames(edmxCopyXmlDoc, originalEdmx);

            edmxCopyXmlDoc.Save(edmxCopyFilePath);
        }

        private XmlDocument ReplaceNames(XmlDocument edmxCopyXmlDoc, XmlDocument originalEdmx)
        {
            if (edmxCopyXmlDoc == null || originalEdmx == null || options.NamesToReplace == null || !options.NamesToReplace.Any())
                return edmxCopyXmlDoc;

            var namespaceManager = new XmlNamespaceManager(originalEdmx.NameTable);
            namespaceManager.AddNamespace("x", options.EdmxNamespace);
            namespaceManager.AddNamespace("cx", options.CsdlNamespace);

            var conceptualModelsNode = edmxCopyXmlDoc.DocumentElement.SelectSingleNode("//x:ConceptualModels", namespaceManager);
            if (conceptualModelsNode == null)
                return edmxCopyXmlDoc;

            foreach (var item in options.NamesToReplace)
            {
                string xml = conceptualModelsNode.InnerXml;

                var nameToReplace = (options.NamespacePrefix.IsNullOrWhiteSpace() ? string.Empty : options.NamespacePrefix) + item.Key;
                var correctName = (options.NamespacePrefix.IsNullOrWhiteSpace() ? string.Empty : options.NamespacePrefix) + item.Value;

                var entitySetNode = conceptualModelsNode.SelectSingleNode("cx:Schema/cx:EntityContainer/cx:EntitySet[@EntityType='" + nameToReplace + "']", namespaceManager);
                if (entitySetNode == null)
                    continue;

                string entitySetName = entitySetNode.Attributes["Name"].Value;
                string entitySetNewName = entitySetName + item.Value;

                xml = xml.Replace("<EntitySet Name=\"" + entitySetName + "\" EntityType=\"" + nameToReplace + "\" />", "<EntitySet Name=\"" + entitySetNewName + "\" EntityType=\"" + correctName + "\" />");
                xml = xml.Replace("EntitySet=\"" + entitySetName + "\"", "EntitySet=\"" + entitySetNewName + "\"");
                xml = xml.Replace("<EntityType Name=\"" + item.Key + "\">", "<EntityType Name=\"" + item.Value + "\">");
                xml = xml.Replace("\"" + nameToReplace + "\"", "\"" + correctName + "\"");

                conceptualModelsNode.InnerXml = xml;

                xml = edmxCopyXmlDoc.InnerXml;
                xml = xml.Replace("<EntitySetMapping Name=\"" + entitySetName + "\">", "<EntitySetMapping Name=\"" + entitySetNewName + "\">");
                xml = xml.Replace("\"" + nameToReplace + "\"", "\"" + correctName + "\"");
                edmxCopyXmlDoc.InnerXml = xml;
            }

            edmxCopyXmlDoc.DocumentElement.SelectSingleNode("//x:ConceptualModels", namespaceManager).InnerXml = conceptualModelsNode.InnerXml;

            return edmxCopyXmlDoc;
        }

        private void SetNodeContent(out XmlNode newNode, string nodeName, string extension)
        {
            newNode = null;
            string filePath = new DirectoryInfo(Configurator.OutputPath).GetFiles("*" + extension).Select(el => el.FullName).FirstOrDefault();
            if (filePath.IsNullOrWhiteSpace())
                throw new HandledException("File with extension " + extension + " was not found.");

            var document = new XmlDocument();
            document.Load(filePath);

            var schemaNodes = document.GetElementsByTagName(nodeName);
            if (schemaNodes != null && schemaNodes.Count > 0)
                newNode = schemaNodes[0];
        }

        private void ReplaceNodesContent(XmlDocument edmxXmlDoc, string nodeName, XmlNode newNode)
        {
            var namespaceManager = new XmlNamespaceManager(edmxXmlDoc.NameTable);
            namespaceManager.AddNamespace("x", options.EdmxNamespace);

            if (newNode == null)
                return;

            var oldNode = edmxXmlDoc.DocumentElement.SelectSingleNode("//x:" + nodeName, namespaceManager);
            if (oldNode == null)
                return;

            var schemaNode = oldNode.FirstChild;
            if (schemaNode == null)
                return;

            schemaNode.InnerXml = newNode.InnerXml;

            var xmlns = schemaNode.Attributes["xmlns"].Value;
            var oldXmlNs = newNode.Attributes["xmlns"].Value;
            schemaNode.InnerXml = schemaNode.InnerXml.Replace(oldXmlNs, xmlns);
        }

        private void AddFunctionsNodes(XmlDocument edmxCopyXmlDocument, XmlDocument originalEdmxXmlDoc, string functionNodeName, string nodesContainer, string fileNamespace)
        {
            var edmxCopyNamespaceManager = new XmlNamespaceManager(edmxCopyXmlDocument.NameTable);
            edmxCopyNamespaceManager.AddNamespace("x", fileNamespace);
            var nodes = originalEdmxXmlDoc.GetElementsByTagName(functionNodeName);
            if (nodes == null || nodes.Count == 0)
                return;

            var entityContainer = edmxCopyXmlDocument.DocumentElement.SelectSingleNode("//x:" + nodesContainer, edmxCopyNamespaceManager);
            if (entityContainer == null)
                return;

            foreach (XmlNode node in nodes)
            {
                var newNode = entityContainer.OwnerDocument.ImportNode(node, true);
                entityContainer.AppendChild(newNode);
            }

        }

        private void RemoveFunctionsNodesFromSsdl(XmlDocument edmxXmlDoc)
        {
            var functionNodes = edmxXmlDoc.GetElementsByTagName("Function");
            if (functionNodes == null || functionNodes.Count == 0)
                return;

            while (functionNodes.Count > 0)
            {
                functionNodes[0].ParentNode.RemoveChild(functionNodes[0]);
            }
        }

        private void UpdateEntitiesNames(XmlDocument edmx, XmlDocument originalEdmx, XmlNode ssdlSchemaNode)
        {
            var namespaceManager = new XmlNamespaceManager(edmx.NameTable);
            namespaceManager.AddNamespace("mx", options.MslNamespace);
            namespaceManager.AddNamespace("cx", options.CsdlNamespace);

            var entitySetMappingNodes = originalEdmx.GetElementsByTagName("EntitySetMapping");
            if (entitySetMappingNodes == null)
                return;

            foreach (XmlNode node in entitySetMappingNodes)
            {
                var entityName = node.Attributes["Name"].Value;
                var entityTypeMappingNode = node.SelectSingleNode("descendant::mx:EntityTypeMapping", namespaceManager);
                var typeName = entityTypeMappingNode.Attributes["TypeName"].Value.ToString();
                var mappingNode = node.SelectSingleNode("descendant::mx:MappingFragment", namespaceManager);
                var storeEntitySetName = mappingNode.Attributes["StoreEntitySet"].Value;

                var oldMappingFragment = edmx.DocumentElement.SelectSingleNode("//mx:MappingFragment[@StoreEntitySet='" + storeEntitySetName + "']", namespaceManager);
                if (oldMappingFragment == null)
                    continue;

                var oldEntityTypeMapping = oldMappingFragment.ParentNode;
                var oldTypeName = oldEntityTypeMapping.Attributes["TypeName"].Value;

                var oldEntitySetMappingName = oldEntityTypeMapping.ParentNode.Attributes["Name"].Value;
                if (oldEntitySetMappingName == entityName && typeName == oldTypeName)
                    continue;

                oldEntityTypeMapping.Attributes["TypeName"].Value = typeName;
                oldEntityTypeMapping.ParentNode.Attributes["Name"].Value = entityName;

                var entitySet = edmx.DocumentElement.SelectSingleNode("//cx:EntitySet[@EntityType='" + oldTypeName + "']", namespaceManager);
                entitySet.Attributes["Name"].Value = entityName;
                entitySet.Attributes["EntityType"].Value = typeName;

                var cleanTypeName = RemoveModelName(typeName);
                var cleanOldTypeName = RemoveModelName(oldTypeName);
                var entityTypeNode = edmx.DocumentElement.SelectSingleNode("//cx:EntityType[@Name='" + cleanOldTypeName + "']", namespaceManager);
                var oldEntityTypeName = entityTypeNode.Attributes["Name"].Value;
                entityTypeNode.Attributes["Name"].Value = cleanTypeName;

                foreach (XmlNode navigationPropertyNode in entityTypeNode.SelectNodes("cx:NavigationProperty", namespaceManager))
                {
                    var relationshipName = RemoveModelName(navigationPropertyNode.Attributes["Relationship"].Value);
                    var associationNode = edmx.DocumentElement.SelectSingleNode("//cx:Association[@Name='" + relationshipName + "']", namespaceManager);
                    foreach (XmlNode endNode in associationNode.SelectNodes("cx:End[@Type='" + oldTypeName + "']", namespaceManager))
                    {
                        endNode.Attributes["Type"].Value = typeName;
                    }
                }

                var endNodes = edmx.DocumentElement.SelectNodes("//cx:AssociationSet/cx:End[@EntitySet='" + oldEntitySetMappingName + "']", namespaceManager);
                if (endNodes != null)
                {
                    foreach (XmlNode endNode in endNodes)
                    {
                        endNode.Attributes["EntitySet"].Value = entityName;
                    }
                }
            }
        }

        private string RemoveModelName(string value)
        {
            if (value.IsNullOrWhiteSpace())
                return value;

            return value.Split(new char[] { '.' }).LastOrDefault();
        }

        private void UpdateMappingProperties(XmlDocument edmx, XmlDocument originalEdmx)
        {
            var namespaceManager = new XmlNamespaceManager(edmx.NameTable);
            namespaceManager.AddNamespace("mx", options.MslNamespace);
            namespaceManager.AddNamespace("cx", options.CsdlNamespace);

            var entitySetMappingNodes = originalEdmx.GetElementsByTagName("EntitySetMapping");
            if (entitySetMappingNodes == null)
                return;

            foreach (XmlNode node in entitySetMappingNodes)
            {
                var entityTypeMappingNode = node.SelectSingleNode("descendant::mx:EntityTypeMapping", namespaceManager);
                var typeName = entityTypeMappingNode.Attributes["TypeName"].Value.ToString();
                var mappingNode = node.SelectSingleNode("descendant::mx:MappingFragment", namespaceManager);
                var storeEntitySetName = mappingNode.Attributes["StoreEntitySet"].Value;

                var oldMappingFragment = edmx.DocumentElement.SelectSingleNode("//mx:MappingFragment[@StoreEntitySet='" + storeEntitySetName + "']", namespaceManager);
                if (oldMappingFragment == null)
                    continue;

                var propertiesNodes = mappingNode.SelectNodes("mx:ScalarProperty", namespaceManager);
                if (propertiesNodes == null || propertiesNodes.Count == 0)
                    continue;

                foreach (XmlNode property in propertiesNodes)
                {
                    var columnName = property.Attributes["ColumnName"].Value.ToString();
                    var oldproperty = oldMappingFragment.SelectSingleNode("mx:ScalarProperty[@ColumnName='" + columnName + "']", namespaceManager);
                    if (oldproperty != null)
                    {
                        var originalOldPropertyName = oldproperty.Attributes["Name"].Value;
                        oldproperty.Attributes["Name"].Value = property.Attributes["Name"].Value;
                        var cleanTypeName = RemoveModelName(typeName);
                        var entityTypeNode = edmx.DocumentElement.SelectSingleNode("//cx:EntityType[@Name='" + cleanTypeName + "']", namespaceManager);
                        if (entityTypeNode != null)
                        {
                            var originalExistingEntityTypeProperty = entityTypeNode.SelectSingleNode("cx:Property[@Name='" + originalOldPropertyName.ToString() + "']", namespaceManager);
                            if (originalExistingEntityTypeProperty != null)
                                originalExistingEntityTypeProperty.Attributes["Name"].Value = columnName;

                            var existingEntityTypeProperty = entityTypeNode.SelectSingleNode("cx:Property[@Name='" + columnName + "']", namespaceManager);
                            if (existingEntityTypeProperty != null)
                            {
                                var oldPropertyName = existingEntityTypeProperty.Attributes["Name"].Value;
                                existingEntityTypeProperty.Attributes["Name"].Value = property.Attributes["Name"].Value.ToString();

                                var propertyReferences = edmx.DocumentElement.SelectNodes("//cx:Association/cx:ReferentialConstraint/cx:Dependent[@Role='" + storeEntitySetName + "']/cx:PropertyRef[@Name='" + oldPropertyName + "']", namespaceManager);
                                if (propertyReferences != null)
                                {
                                    foreach (XmlNode reference in propertyReferences)
                                    {
                                        reference.Attributes["Name"].Value = property.Attributes["Name"].Value.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateNavigationsProperties(XmlDocument edmx, XmlDocument originalEdmx)
        {
            var namespaceManager = new XmlNamespaceManager(edmx.NameTable);
            namespaceManager.AddNamespace("cx", options.CsdlNamespace);

            var conceptualModelsNode = edmx.DocumentElement.SelectSingleNode("//cx:Schema", namespaceManager);
            if (conceptualModelsNode == null)
                return;

            var namespaceName = conceptualModelsNode.Attributes["Namespace"].Value.ToString();
            var alias = conceptualModelsNode.Attributes["Alias"].Value.ToString();

            var nodes = originalEdmx.SelectNodes("//cx:EntityType", namespaceManager);
            if (nodes == null || nodes.Count == 0)
                return;


            foreach (XmlNode node in nodes)
            {
                var oldNode = edmx.DocumentElement.SelectSingleNode("//cx:EntityType[@Name='" + node.Attributes["Name"].Value.ToString() + "']", namespaceManager);
                if (oldNode == null)
                    continue;

                var navigationPropertiesNodes = node.SelectNodes("cx:NavigationProperty", namespaceManager);
                if (navigationPropertiesNodes == null || navigationPropertiesNodes.Count == 0)
                    continue;

                foreach (XmlNode navigationProperty in navigationPropertiesNodes)
                {
                    var propertyName = navigationProperty.Attributes["Name"].Value.ToString();
                    var relationshipName = navigationProperty.Attributes["Relationship"].Value.ToString();
                    var fromRoleAttribute = navigationProperty.Attributes["FromRole"].Value.ToString();
                    var toRoleAttribute = navigationProperty.Attributes["ToRole"].Value.ToString();
                    if (relationshipName.Contains(alias))
                        relationshipName = relationshipName.Replace(alias, namespaceName);

                    var oldNavigationProperty = oldNode.SelectSingleNode("cx:NavigationProperty[@Relationship='" + relationshipName + "' and @FromRole='" + fromRoleAttribute + "' and @ToRole='" + toRoleAttribute + "']", namespaceManager);
                    if (oldNavigationProperty != null)
                    {
                        oldNavigationProperty.Attributes["Name"].Value = propertyName;
                    }
                }
            }
        }
    }
}