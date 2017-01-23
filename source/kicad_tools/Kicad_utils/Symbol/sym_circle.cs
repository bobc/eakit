using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using Lexing;

namespace Kicad_utils.Symbol
{
    public class sym_circle : sym_drawing_base
    {
        public PointF Position;
        public float Radius;

        public sym_circle()
        { }

        public new static sym_circle Parse(List<Token> tokens)
        {
            sym_circle result = new sym_circle();

            result.Position.X = tokens[1].IntValue;
            result.Position.Y = tokens[2].IntValue;
            result.Radius = tokens[3].IntValue;
            result.Part = tokens[4].IntValue;
            result.DeMorganAlternate = tokens[5].IntValue;
            result.PenSize = (float)tokens[6].GetValueAsDouble();
            result.Fill = sym_drawing_base.GetFillType(tokens[7].Value);

            return result;
        }

        // Legacy format
        public override string ToString()
        {
            return string.Format("C {0} {1} {2} {3} {4} {5} {6}",
                Position.X, Position.Y,
                Radius,
                Part,
                DeMorganAlternate,
                PenSize,
                sym_drawing_base.FillTypeToString(Fill)
                );
        }
    }
}
