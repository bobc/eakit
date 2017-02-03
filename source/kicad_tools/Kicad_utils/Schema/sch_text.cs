using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace Kicad_utils.Schema
{
    public class sch_text : sch_item_base
    {
        public string Type; // Label Notes GLabel HLabel
        public Point Pos;
        // 0 = right = Align left, 
        // 1 = up    = align bottom
        // 2 = left  = align right
        // 3 = down  = align top
        public int Orientation;  
        public int TextSize;

        // Type="Notes":
        // "~" normal
        // "Italic"

        // Type = GLabel
        // UnSpc
        // 3State
        // UnSpc
        // Output
        // Input
        public string Shape;

        // Type="Notes":
        // 0  = normal
        // 12 = bold
        public int Param; 

        public string Value;

        public sch_text ()
        {
            name = "Text";
        }

        public static sch_text CreateText (string type, string value, PointF pos, float textsize, int orientation, string shape)
        {
            sch_text result = new sch_text();

            result.Type = type;
            result.Pos = new Point((int)pos.X, (int)pos.Y);
            result.Orientation = orientation / 90;
            result.TextSize = (int)textsize;
            result.Shape = shape;
            result.Value = value;
            return result;
        }

        public static sch_text CreateNote(string value, PointF pos, float textsize, int orientation, bool italic, bool bold)
        {
            sch_text result= CreateText("Notes", value, pos, textsize, orientation, "~");

            if (italic)
                result.Shape = "Italic";
            else
                result.Shape = "~";

            if (bold)
                result.Param = 12;
            else
                result.Param = 0;
            return result;
        }

        public static sch_text CreateLocalLabel(string value, PointF pos, float textsize, int orientation)
        {
            return CreateText("Label", value, pos, textsize, orientation, "~");
        }

        public static sch_text CreateHierarchicalLabel(string value, PointF pos, float textsize, int orientation, string shape)
        {
            return CreateText("HLabel", value, pos, textsize, orientation, shape);
        }

        public static sch_text CreateGlobalLabel(string value, PointF pos, float textsize, int orientation, string shape)
        {
            return CreateText("GLabel", value, pos, textsize, orientation, shape);
        }

        public override void Write(List<string> data)
        {
            data.Add(string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", name, Type, Pos.X, Pos.Y, Orientation, TextSize, Shape, Param));
            data.Add(string.Format("{0}", Value));
        }

    }
}
