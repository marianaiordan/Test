using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RS.Data.Entities
{
    public class Table
    {
        public string Name { get; set; }
        public string SchemaName { get; set; }
        public string FullName { get { return "[" + SchemaName + "].[" + Name + "]"; } }
        public List<Column> Columns { get; set; }
    }
}