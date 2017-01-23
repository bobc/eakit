using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using Lexing;

namespace Kicad_utils.Symbol
{
    public class sym_arc : sym_drawing_base
    {
        // A X Y radius start end part dmg pen fill Xstart Ystart Xend Yend

        public PointF Position;
        public float Radius;
        public float ArcStart, ArcEnd; // degrees
        public PointF Start;
        public PointF End;


        public sym_arc()
        { }

        public new static sym_arc Parse(List<Token> tokens)
        {
            sym_arc result = new sym_arc();
            //
            result.Position.X = tokens[1].IntValue;
            result.Position.Y = tokens[2].IntValue;
            result.Radius = tokens[3].IntValue;
            result.ArcStart = tokens[4].IntValue / 10f;
            result.ArcEnd = tokens[5].IntValue / 10f;
            result.Part = tokens[6].IntValue;
            result.DeMorganAlternate = tokens[7].IntValue;
            result.PenSize = (float)tokens[8].GetValueAsDouble();
            result.Fill = sym_drawing_base.GetFillType(tokens[9].Value);
            result.Start.X = tokens[10].IntValue;
            result.Start.Y = tokens[11].IntValue;
            result.End.X = tokens[12].IntValue;
            result.End.Y = tokens[13].IntValue;

            return result;
        }

        // Legacy format
        public override string ToString()
        {
            return string.Format("A {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}",
                Position.X, Position.Y,
                Radius,
                (int)(ArcStart * 10), (int)(ArcEnd * 10),
                Part,
                DeMorganAlternate,
                PenSize,
                sym_drawing_base.FillTypeToString(Fill),
                Start.X, Start.Y,
                End.X, End.Y
                );
        }
    }
}
