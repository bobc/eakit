using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using Lexing;

namespace Kicad_utils.Symbol
{
    public class SymbolField
    {
        public const float Orientation_Horiz = 0; // aka "H"
        public const float Orientation_Vert = 90; // aka "V"

        public const string HorizAlign_Left = "L";
        public const string HorizAlign_Center = "C";
        public const string HorizAlign_Right = "R";

        public const string VertAlign_Top = "T";
        public const string VertAlign_Center = "C";
        public const string VertAlign_Bottom = "B";

        public int Index;   // 0-3 for standard fields,  >=4 for user fields
        public string UserFieldName;

        //public string layer;
        //public float font_thickness;
        //

        public TextBase Text;

        public SymbolField()
        {
            this.Text = new TextBase();
            this.Text.Value = "~";
            this.Text.Pos = new PointF(0, 0);
            this.Text.FontSize = 60;
            this.Text.Angle = Orientation_Horiz;
            this.Text.HorizAlignment = "C";
            this.Text.VertAlignment = "C";
            this.Text.Italic = false;
            this.Text.Bold = false;
            this.Text.Visible = false;
        }

        // for user defined fields
        public SymbolField(int index, string FieldName, string Value, PointF at, float font_size, bool visible,
                string orientation, string horizAlignment, string vertAlign, bool italic, bool bold)
        {
            this.Index = index;
            this.UserFieldName = FieldName;

            this.Text = new TextBase();
            this.Text.Value = Value;
            this.Text.Pos = at;
            this.Text.FontSize = font_size;
            this.Text.Angle = orientation == "H" ? Orientation_Horiz : Orientation_Vert;
            this.Text.HorizAlignment = horizAlignment;
            this.Text.VertAlignment = vertAlign;
            this.Text.Visible = visible;
            this.Text.Italic = italic;
            this.Text.Bold = bold;
        }

        // for standard fields
        public SymbolField(string value, PointF at, float font_size, bool visible,
            string orientation, string horizAlignment, string vertAlign, bool italic, bool bold)
        {
            this.Index = 0;

            this.Text = new TextBase();
            this.Text.Value = value;
            this.Text.Pos = at;
            this.Text.FontSize = font_size;
            this.Text.Angle = orientation == "H" ? Orientation_Horiz : Orientation_Vert;
            this.Text.HorizAlignment = horizAlignment;
            this.Text.VertAlignment = vertAlign;
            this.Text.Visible = visible;
            this.Text.Italic = italic;
            this.Text.Bold = bold;
        }

        public static SymbolField Parse(List<Token> tokens)
        {
            SymbolField result = new SymbolField();

            result.Text.Value = tokens[1].Value;
            result.Text.Pos.X = tokens[2].IntValue;
            result.Text.Pos.Y = tokens[3].IntValue;
            result.Text.FontSize = tokens[4].IntValue;
            result.Text.Angle = tokens[5].Value == "H" ? Orientation_Horiz : Orientation_Vert; ;
            result.Text.Visible = tokens[6].Value == "V";
            result.Text.HorizAlignment = tokens[7].Value;

            result.Text.VertAlignment = tokens[8].Value[0].ToString();
            result.Text.Italic = tokens[8].Value[1] == 'I';
            result.Text.Bold = tokens[8].Value[2] == 'B';

            if (tokens.Count > 9)
                result.UserFieldName = tokens[9].Value;

            return result;
        }

        // Legacy format
        public override string ToString()
        {
            // F0 "P" -50 -150 60 V V C CNN

            // F4 "mpn" -50 -150 60 V V C CNN "MPN"

            string result = string.Format("\"{0}\" {1} {2} {3} {4} {5} {6} {7}{8}{9}",
                Text.Value,
                (int)Text.Pos.X, (int)Text.Pos.Y,
                (int)Text.FontSize,
                Text.Angle == 0 ? "H" : "V",
                Text.Visible ? "V" : "I",
                Text.HorizAlignment,

                Text.VertAlignment,
                Text.Italic ? "I" : "N",
                Text.Bold ? "B" : "N"
                );

            if (!string.IsNullOrEmpty(UserFieldName))
                result += " " + "\"" + UserFieldName + "\"";
            return result;
        }
    }
}
