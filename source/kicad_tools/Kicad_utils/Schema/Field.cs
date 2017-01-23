using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kicad_utils.Schema
{
    public class Field
    {
        public string Name;
        public string Value;

        public Field() { }

        public Field(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
