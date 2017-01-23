using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using Lexing;

namespace Kicad_utils.Symbol
{
    public class sym_rectangle : sym_drawing_base
    {
        public PointF P1;
        public PointF P2;

        public sym_rectangle()
        { }

        public sym_rectangle(float penSize, FillTypes fill, PointF p1, PointF p2)
        {
            this.PenSize = penSize;
            this.Fill = fill;

            this.P1 = p1;
            this.P2 = p2;
        }

        // Legacy format
        public override string ToString()
        {
            // S X1 Y1 X2 Y2 part dmg pen fill
            return string.Format("S {0} {1} {2} {3} {4} {5} {6} {7}",
                P1.X, P1.Y,
                P2.X, P2.Y,
                Part,
                DeMorganAlternate,
                PenSize,
                sym_drawing_base.FillTypeToString(Fill)
                );
        }

        public new static sym_rectangle Parse(List<Token> tokens)
        {
            sym_rectangle result = new sym_rectangle();

            result.P1.X = tokens[1].IntValue;
            result.P1.Y = tokens[2].IntValue;
            result.P2.X = tokens[3].IntValue;
            result.P2.Y = tokens[4].IntValue;

            result.Part = tokens[5].IntValue;
            result.DeMorganAlternate = tokens[6].IntValue;
            result.PenSize = (float)tokens[7].GetValueAsDouble();
            result.Fill = sym_drawing_base.GetFillType(tokens[8].Value);

            return result;
        }
    }
}
