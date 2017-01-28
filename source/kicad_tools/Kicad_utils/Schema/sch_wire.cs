using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace Kicad_utils.Schema
{
    public class sch_wire : sch_item_base
    {
        //public string WireOrEntry; // Wire Entry

        public string type;  // Wire Bus Notes
        public string type2; // Line Bus

        public Point start;
        public Point end;

        // Wire Wire Line       - regular wires (green)
        // Wire Bus Line        - bus lines (blue)
        // Wire Notes Line      - documentary lines (blue, dsashed)

        // Wire Wire Bus
        // Wire Bus Bus         

        // Entry Wire Bus
        // Entry Bus Bus

        public static sch_wire CreateBus (PointF start, PointF end)
        {
            sch_wire result = new sch_wire();

            result.name = "Wire";
            result.type = "Bus";
            result.type2 = "Line";
            result.start = new Point((int)start.X, (int)start.Y);
            result.end = new Point((int)end.X, (int)end.Y);
            return result;
        }

        public static sch_wire CreateWire (PointF start, PointF end)
        {
            sch_wire result = new sch_wire();

            result.name = "Wire";
            result.type = "Wire";
            result.type2 = "Line";
            result.start = new Point((int)start.X, (int)start.Y);
            result.end = new Point((int)end.X, (int)end.Y);
            return result;
        }

        public static sch_wire CreateLine(PointF start, PointF end)
        {
            sch_wire result = new sch_wire();

            result.name = "Wire";
            result.type = "Notes";
            result.type2 = "Line";
            result.start = new Point((int)start.X, (int)start.Y);
            result.end = new Point((int)end.X, (int)end.Y);
            return result;
        }

        public static sch_wire CreateWireToBusEntry(PointF start, PointF end)
        {
            sch_wire result = new sch_wire();

            result.name = "Entry";
            result.type = "Wire";
            result.type2 = "Line";
            result.start = new Point((int)start.X, (int)start.Y);
            result.end = new Point((int)end.X, (int)end.Y);
            return result;
        }


        public override void Write(List<string> data)
        {
            data.Add(string.Format("{0} {1} {2}", name, type, type2));
            data.Add(string.Format("\t{0} {1} {2} {3}", start.X, start.Y, end.X, end.Y));
        }

    }
}
