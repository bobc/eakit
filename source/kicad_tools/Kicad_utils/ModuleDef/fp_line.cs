using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;
using Cad2D;

namespace Kicad_utils.ModuleDef
{
    public class fp_line : fp_shape
    {
        public PointF start;
        public PointF end;

        public fp_line()
        {
        }

        public fp_line(PointF start, PointF end, string layer, float width)
        {
            this.start = start;
            this.end = end;
            this.layer = layer;
            this.width = width;
        }

        public override void FlipX(PointF pos)
        {
            base.FlipX(pos);

            start = PointFExt.FlipX(start);
            end = PointFExt.FlipX(end);
        }


        public override SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "fp_line";
            result.Items = new List<SNodeBase>();
            result.Items.Add(new SExpression("start", new List<SNodeBase>{
                new SNodeAtom(start.X),
                new SNodeAtom(start.Y)
                }));
            result.Items.Add(new SExpression("end", new List<SNodeBase>{
                new SNodeAtom(end.X),
                new SNodeAtom(end.Y)
                }));
            result.Items.Add(new SExpression("layer", layer));
            result.Items.Add(new SExpression("width", width));
            return result;
        }

        public override string ToString()
        {
            // DS 2 -1 -2 -1 0.15 21
            return string.Format("DS {0} {1} {2} {3} {4} {5}",
                start.X, start.Y, end.X, end.Y,
                width,
                Layer.GetLayerNumber_Legacy(layer));
        }

        public static fp_line Parse(SNodeBase node)
        {
            fp_line result = new fp_line();

            if ((node is SExpression) && ((node as SExpression).Name == "fp_line"))
            {
                SExpression expr = node as SExpression;

                result.start = (expr.Items[0] as SExpression).GetPointF();
                result.end = (expr.Items[1] as SExpression).GetPointF();

                result.layer = Layer.ParseLayer(expr.Items[2]);
                result.width = (expr.Items[3] as SExpression).GetFloat();

                return result;
            }
            else
                return null;  // error
        }

    }
}
