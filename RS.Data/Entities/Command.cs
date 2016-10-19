using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RS.Data.Entities
{
    public class Command
    {
        public CommandTypes Type { get; set; }
        public string Text { get; set; }
        public Table Table { get; set; }

        public Command(CommandTypes type, string text, Table table = null)
        {
            Type = type;
            Text = text;
            Table = table;
        }
    }
}