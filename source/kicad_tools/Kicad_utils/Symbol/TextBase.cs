using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace Kicad_utils.Symbol
{
    public class TextBase
    {
        public string Value;

        public Position Pos;
        //public PointF Pos;
        //public float Angle; // degrees H = 0, V = 90
        //public string Orientation; // H / V

        // todo: public TextEffects effects;

        public float FontSize;

        public string HorizAlignment;   // C L R
        public string VertAlignment;    // T C B

        public bool Visible;            // I or V
        public bool Italic; // I or N
        public bool Bold; // B or N

        public bool xMirror;
        public float xAngle;

        public TextBase()
        {
            Pos = new Position();

            Visible = true;
            HorizAlignment = SymbolField.HorizAlign_Left;
            VertAlignment = SymbolField.VertAlign_Bottom;
        }

    }
}
