using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RS.Data.EdmxUpdater
{
    class Constants
    {
        public class SettingsConstants
        {
            public const string Namespace = "namespace ";
        }
        public class CommandDefaultOptions
        {
            public const string Mode = "FullGeneration";
            public const string Language = "CSharp";
        }
        public class CommandParameters
        {
            public const string Mode = " /mode:";
            public const string ConnectionString = " /connectionstring:";
            public const string Project = " /project:";
            public const string EntityContainer = " /entitycontainer:";
            public const string Namespace = " /namespace:";
            public const string Language = " /language:";
            public const string OutCsdl = " /outcsdl:";
            public const string OutSsdl = " /outssdl:";
            public const string OutMsl = " /outmsl:";
            public const string OutObjectLayer = " /outobjectlayer:";
            public const string OutViews = " /outviews:";
            public const string Pluralization = " /pluralize";
            public const string Includes = " -I ";
        }
        public class TemplatesConstants
        {
            public const string TemplateNamePattern = "EF6.Utility*.*";
            public const string CleanupSetting = "CleanupBehavior";
        }
        public class DiagramConstants
        {
            public const string Width = "1.5";
            public const string PointX = "0";
            public const string PointY = "0";
        }
        public class FilesExtensions
        {
            public const string Csdl = ".csdl";
            public const string Ssdl = ".ssdl";
            public const string Msl = ".msl";
            public const string ObjectLayer = ".ObjectLayer";
            public const string Views = ".Views";
            public const string EDMX = ".edmx";
            public const string Class = ".cs";
            public const string ContextTemplate = ".Context.tt";
            public const string Template = ".tt";
            public const string Diagram = ".edmx.diagram";
        }
        public class NodesNames
        {
            public const string RootNode = "//rootnamespace";
            public const string MslMappingNode = "//mapping";
            public const string EntityContainerNode = "EntityContainer";
            public const string EnablePluralizatioNode = "//designerproperty[@name='EnablePluralization']";
            public const string SchemaNode = "Schema";
            public const string SchemaNodeFromRoot = "//schema";
            public const string SchemaNodeNamespace = "//x:Schema";
            public const string MappingNode = "Mapping";
            public const string Compile = "Compile";
            public const string DependentUpon = "DependentUpon";
            public const string CompileNode = "Compile[contains(@Include,'.cs')]";
            public const string CsdlFunctionsNodes = "FunctionImport";
            public const string SsdlFunctionsNodes = "Function";
            public const string MslFunctionsNodes = "FunctionImportMapping";
            public const string ComplexType = "ComplexType";
            public const string EntityContainerMapping = "EntityContainerMapping";
        }
    }
}