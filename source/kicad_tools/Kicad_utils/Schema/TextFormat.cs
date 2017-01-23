using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kicad_utils.Schema
{
    public class TextFormat
    {
        public bool Visible;
        public string Orientation;
        public int Size;
        public string HorizJustify; // C = center
        public string VertJustify;  // C = center
        public bool Italic;       // Y or N
        public bool Bold;         // Y or N

        public TextFormat()
        {
            Visible = true;
            Size = 50;
            Orientation = "H";
            HorizJustify = "C";
            VertJustify = "C";
            Italic = false;
            Bold = false;
        }
    }
}
