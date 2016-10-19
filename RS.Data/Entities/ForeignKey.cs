using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RS.Data.Entities
{
    public class ForeignKey
    {
        public string Name { get; set; }
        public string FK_Schema { get; set; }
        public string FK_Table { get; set; }
        public string FK_Column { get; set; }
        public string PK_Schema { get; set; }
        public string PK_Table { get; set; }
        public string PK_Column { get; set; }
    }
}
