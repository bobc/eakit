using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kicad_utils.Project
{
    public class Item
    {
        public string KeyName;
        public List<string> Values;

        public Item()
        {
            Values = new List<string>();
        }

        public Item(string key, string value)
        {
            Values = new List<string>();
            KeyName = key;
            Values.Add(value);
        }

    }
}
