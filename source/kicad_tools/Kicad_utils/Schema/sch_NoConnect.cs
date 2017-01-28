using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Kicad_utils.Schema
{
    public class sch_NoConnect : sch_item_base
    {
        public string dummy = "~";
        public Point pos;

        public sch_NoConnect()
        {
            name = "NoConn";
        }

        public sch_NoConnect(PointF _pos)
        {
            name = "NoConn";
            pos = new Point((int)_pos.X, (int)_pos.Y);
        }

        public override void Write(List<string> data)
        {
            data.Add(string.Format("{0} {1} {2} {3}", name, dummy, pos.X, pos.Y));
        }

    }
}
