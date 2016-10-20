using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace RS.Data.EdmxUpdater
{
    public class DiagramManager
    {
        EdmxOptions options;
        public DiagramManager(EdmxOptions options)
        {
            this.options = options;
        }
        public void UpdateDiagram()
        {
            var edmxFile = new DirectoryInfo(Configurator.OutputPath).GetFiles("*" + Constants.FilesExtensions.EDMX);
            if (edmxFile == null || edmxFile.Length == 0)
                return;

            var edmxFilePath = edmxFile[0].FullName;
            string edmxFileContent = File.ReadAllText(edmxFilePath);
            var document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(edmxFileContent);
            if (document.DocumentNode == null)
                return;

            HtmlAgilityPack.HtmlNode csdlSchemaNode = document.DocumentNode.SelectSingleNode(Constants.NodesNames.SchemaNodeFromRoot + "[@xmlns='" + options.CsdlNamespace + "']");
            if (csdlSchemaNode == null)
                return;

            var directory = new DirectoryInfo(Configurator.OutputPath);
            var diagramFile = directory.GetFiles("*.edmx.diagram").FirstOrDefault();
            if (diagramFile == null)
                return;

            var diagramFilePath = diagramFile.FullName;
            var doc = new XmlDocument();
            doc.Load(diagramFilePath);
            XmlNode root = doc.DocumentElement;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("x", options.EdmxNamespace);
            XmlNode diagramNode = root.SelectSingleNode("//x:Diagram", nsmgr);
            if (diagramNode == null)
                return;

            while (diagramNode.ChildNodes.Count > 0)
                diagramNode.RemoveChild(diagramNode.ChildNodes[0]);

            var entityContainerNode = csdlSchemaNode.SelectSingleNode("entitycontainer");
            if (entityContainerNode == null)
                return;

            var entitiesNodes = entityContainerNode.SelectNodes("entityset");
            var associations = entityContainerNode.SelectNodes("associationset");

            foreach (var node in entitiesNodes)
            {
                var newNode = CreateEntityTypeShapeNode(doc, node.Attributes["EntityType"].Value.ToString());
                diagramNode.AppendChild(newNode);
            }

            foreach (var association in associations)
            {
                var newAssociation = CreateAssociationNode(doc, association.Attributes["Association"].Value.ToString());
                diagramNode.AppendChild(newAssociation);
            }

            doc.Save(diagramFilePath);

            Console.WriteLine("Successfully set diagram.");
        }

        private XmlNode CreateEntityTypeShapeNode(XmlDocument document, string entityTypeName)
        {
            XmlNode entityShapeNode = document.CreateNode(XmlNodeType.Element, "EntityTypeShape", document.DocumentElement.NamespaceURI);

            XmlAttribute entityTypeAttribute = document.CreateAttribute("EntityType");
            entityTypeAttribute.Value = entityTypeName;
            entityShapeNode.Attributes.Append(entityTypeAttribute);

            XmlAttribute widthAttribute = document.CreateAttribute("Width");
            widthAttribute.Value = Constants.DiagramConstants.Width;
            entityShapeNode.Attributes.Append(widthAttribute);

            XmlAttribute pointXAttribute = document.CreateAttribute("PointX");
            pointXAttribute.Value = Constants.DiagramConstants.PointX;
            entityShapeNode.Attributes.Append(pointXAttribute);

            XmlAttribute pointYAttribute = document.CreateAttribute("PointY");
            pointYAttribute.Value = Constants.DiagramConstants.PointY;
            entityShapeNode.Attributes.Append(pointYAttribute);

            return entityShapeNode;

        }

        private XmlNode CreateAssociationNode(XmlDocument document, string associationName)
        {
            XmlNode associationNode = document.CreateNode(XmlNodeType.Element, "AssociationConnector", document.DocumentElement.NamespaceURI);

            XmlAttribute associationAttribute = document.CreateAttribute("Association");
            associationAttribute.Value = associationName;
            associationNode.Attributes.Append(associationAttribute);

            return associationNode;
        }
    }
}