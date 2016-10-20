using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RS.Data.Entities
{
    public class Schema
    {
        public string Name { get; set; }
        public List<Table> Tables { get; set; }
    }
}