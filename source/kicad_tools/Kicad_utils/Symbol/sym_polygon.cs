using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using Lexing;

namespace Kicad_utils.Symbol
{
    public class sym_polygon : sym_drawing_base
    {
        public int NumVertex;
        public List<PointF> Vertex;

        public sym_polygon()
        { }

        public sym_polygon(int unit, float penSize, FillTypes fill, List<PointF> vertex)
        {
            this.Unit = unit;
            this.PenSize = penSize;
            this.Fill = fill;

            this.Vertex = vertex;
            this.NumVertex = vertex.Count;
        }

        // P count part dmg pen X Y … fill
        public new static sym_polygon Parse(List<Token> tokens)
        {
            sym_polygon result = new sym_polygon();
            result.NumVertex = tokens[1].IntValue;
            result.Unit = tokens[2].IntValue;
            result.DeMorganAlternate = tokens[3].IntValue;
            result.PenSize = (float)tokens[4].GetValueAsDouble();

            int pos = 5;
            int count = result.NumVertex;
            result.Vertex = new List<PointF>();

            while (count > 0)
            {
                result.Vertex.Add(new PointF((float)tokens[pos].GetValueAsDouble(), (float)tokens[pos + 1].GetValueAsDouble()));
                count--;
                pos += 2;
            }
            result.Fill = sym_drawing_base.GetFillType(tokens[pos].Value);

            return result;
        }

        // Legacy format
        public override string ToString()
        {
            // P count part dmg pen X Y … fill
            string result = string.Format("P {0} {1} {2} {3}",
                NumVertex,
                Unit,
                DeMorganAlternate,
                PenSize);
            for (int j = 0; j < NumVertex; j++)
                result += string.Format(" {0} {1}", Vertex[j].X, Vertex[j].Y);

            result += string.Format(" {0}", sym_drawing_base.FillTypeToString(Fill));
            return result;
        }

    }
}
