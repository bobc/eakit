using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EagleConverter
{
    public class RenamedItem
    {
        public string Original; // eg. "LCD"
        public string NewName;     // e.g. "LCD1"

        public RenamedItem() { }

        public RenamedItem(string orig, string newName)
        {
            Original = orig;
            NewName = newName;
        }
    }
}
