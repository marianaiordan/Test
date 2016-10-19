using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace RS.Data.Entities
{
    public class Column
    {
        public string Name { get; set; }
        public string Default { get; set; }
        public bool IsNullable { get; set; }
        public SqlDbType DataType { get; set; }
        public int? MaximumLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public string TableName { get; set; }
        public string TableSchemaName { get; set; }
        public int OrdinalPosition { get; set; }
        public bool IsComputed { get; set; }
        public ForeignKey ForeignKey { get; set; }
    }
}