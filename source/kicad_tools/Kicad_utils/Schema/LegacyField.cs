using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace Kicad_utils.Schema
{
    public class LegacyField
    {
        //  Reference = 0
        //  Value = 1
        //  Pcb FootPrint = 2
        //  User Doc Link = 3

        public const int FieldReference = 0;
        public const int FieldValue = 1;
        public const int FieldPcbFootprint = 2;
        public const int FieldUserDocLink = 3;

        public int Number;  // coded "name"
        public string Value;

        // todo: replace with Position
        public PointF Pos;
        public string Orientation; // H (horizontal) or V (vertical).

        // todo: replace with TextEffects
        public int Size;

        //private string Flags;    // 0000 = visible or 0001 = invisible
        public bool Hidden;

        // e.g. C CNN
        public string HorizJustify; // (C)enter (L)eft, (R)ight
        public string VertJustify;  // (C)enter (B)ottom (T)op
        public string Italic;       // Y or N or I
        public string Bold;         // Y or N or B

        public string UserName;
       

        public LegacyField()
        {
            Pos = new PointF(0, 0);
            Size = 50;
            Orientation = "H";

            HorizJustify = "C";
            VertJustify = "C";
            Italic = "N";
            Bold = "N";
        }

        public LegacyField(int number)
        {
            Number = number;
            Pos = new PointF(0, 0);
            Size = 50;
            Orientation = "H";

            HorizJustify = "C";
            VertJustify = "C";
            Italic = "N";
            Bold = "N";
        }

        public LegacyField(string name, int size)
        {
            Number = 0;
            Pos = new PointF(0, 0);
            Size = size;
            Orientation = "H";

            HorizJustify = "C";
            VertJustify = "C";
            Italic = "N";
            Bold = "N";

            Value = name;
        }

        public override string ToString()
        {
            // F 0 "#PWR04" H 1100 2750 50  0001 C CNN

            string flags = "";

            if (Hidden)
                flags = "0001";
            else
                flags = "0000";

            string result = string.Format("F {0} \"{1}\" {2} {3} {4} {5} {6} {7} {8}{9}{10}",
                Number,
                Value,
                Orientation,
                (int)Pos.X, (int)Pos.Y,
                Size,
                flags,
                HorizJustify,

                VertJustify,
                Italic,
                Bold
                );

            if (!string.IsNullOrEmpty(UserName))
                result += " " + "\"" + UserName + "\"";

            return result;
        }

    }
}
