using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Kicad_utils.Schema
{
    public class sch_Junction: sch_item_base
    {
        public string dummy;
        public Point Pos;

        public sch_Junction()
        {
            name = "Connection";
            dummy = "~";
        }

        public sch_Junction(PointF pos)
        {
            name = "Connection";
            dummy = "~";
            Pos = new Point((int)pos.X, (int)pos.Y);
        }

        public override void Write(List<string> data)
        {
            data.Add(string.Format("{0} {1} {2} {3}", name, dummy, Pos.X, Pos.Y));
        }

    }
}
