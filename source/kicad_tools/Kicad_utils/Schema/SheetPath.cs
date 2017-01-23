using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kicad_utils.Schema
{
    public class SheetPath
    {
        public string Names;
        public string Tstamps;

        public SheetPath() { }

        public SheetPath(string names, string tstamps)
        {
            Names = names;
            Tstamps = tstamps;
        }
    }
}
