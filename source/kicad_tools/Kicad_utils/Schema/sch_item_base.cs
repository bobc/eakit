using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kicad_utils.Schema
{
    public class sch_item_base
    {
        public string name; // Text Wire Connection Entry NoConn

        public virtual void Write(List<string> data)
        { }
    }
}
