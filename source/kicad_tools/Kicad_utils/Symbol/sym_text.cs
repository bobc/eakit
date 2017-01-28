using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using Lexing;

namespace Kicad_utils.Symbol
{
    public class sym_text : sym_drawing_base
    {
        // T 0     -380 -100 45        0    0   0 SCK  Normal 1    C      C
        // T angle X Y       size hidden part dmg text italic bold Halign Valign

        public TextBase Text;

        //public bool Hidden;
        //public PointF Pos;
        //public float Size;
        //public string TextValue;
        //public bool Italic;
        //public bool Bold;
        //public string HorizAlign;
        //public string VertAlign;

        public sym_text()
        {
            this.Text = new TextBase();
        }

        public sym_text(int unit, string Value, float Angle, PointF Pos, float Size, bool Hidden,
            bool Italic, bool Bold, string HorizAlign, string VertAlign)
        {
            this.Unit = unit;
            this.DeMorganAlternate = 0;

            this.Text = new TextBase();
            this.Text.Angle = Angle;
            this.Text.Pos = Pos;
            this.Text.FontSize = Size;
            this.Text.Visible = !Hidden;
            this.Text.Value = Value;
            this.Text.Italic = Italic;
            this.Text.Bold = Bold;
            this.Text.HorizAlignment = HorizAlign;
            this.Text.VertAlignment = VertAlign;
        }

        // Legacy format
        public override string ToString()
        {
            // T angle X Y       size hidden part dmg text italic bold Halign Valign
            return string.Format("T {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}",
                Text.Angle * 10.0,
                Text.Pos.X, Text.Pos.Y,
                Text.FontSize,
                Text.Visible ? "0" : "1",   // hidden
                Unit,
                DeMorganAlternate,
                "\"" + Text.Value + "\"",  // quotes, spaces
                Text.Italic ? "Italic" : "Normal",
                Text.Bold ? "1" : "0",
                Text.HorizAlignment,
                Text.VertAlignment
                );
        }

        public new static sym_text Parse(List<Token> tokens)
        {
            sym_text result = new sym_text();

            result.Text.Angle = (float)tokens[1].GetValueAsDouble() / 10.0f;
            result.Text.Pos.X = tokens[2].IntValue;
            result.Text.Pos.Y = tokens[3].IntValue;
            result.Text.FontSize = (float)tokens[4].GetValueAsDouble();
            result.Text.Visible = tokens[5].Value != "1";
            result.Unit = tokens[6].IntValue;
            result.DeMorganAlternate = tokens[7].IntValue;
            result.Text.Value = tokens[8].Value;
            result.Text.Italic = tokens[9].Value == "Italic";
            result.Text.Bold = tokens[10].Value == "1";
            result.Text.HorizAlignment = tokens[11].Value;
            result.Text.VertAlignment = tokens[12].Value;

            return result;
        }
    }
}
