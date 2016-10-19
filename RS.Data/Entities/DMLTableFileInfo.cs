using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RS.Data.Entities
{
    public class DMLTableFileInfo
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string DataHash { get; set; }
        public bool Generated { get; set; }
    }
}