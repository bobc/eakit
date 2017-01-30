using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EagleConverter
{
    public class PrefixItem
    {
        public string Prefix; // eg. "R"
        public int Index;

        public PrefixItem() { }

        public PrefixItem(string p)
        {
            Prefix = p;
            Index = 1;
        }
    }
}
