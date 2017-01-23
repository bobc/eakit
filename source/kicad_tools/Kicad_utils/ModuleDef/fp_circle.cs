using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;

namespace Kicad_utils.ModuleDef
{
    public class fp_circle : fp_shape
    {
        public PointF center; // 
        public PointF edge;   // or "end"

        public fp_circle()
        {
        }

        public fp_circle(PointF center, float radius, string layer, float width)
        {
            this.center = center;
            this.edge = new PointF(center.X + radius, center.Y);
            this.layer = layer;
            this.width = width;
        }

        public override SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "fp_circle";
            result.Items = new List<SNodeBase>();
            result.Items.Add(new SExpression("center", new List<SNodeBase>{
                new SNodeAtom(center.X),
                new SNodeAtom(center.Y)
                }));
            result.Items.Add(new SExpression("end", new List<SNodeBase>{
                new SNodeAtom(edge.X),
                new SNodeAtom(edge.Y)
                }));
            result.Items.Add(new SExpression("layer", layer));
            result.Items.Add(new SExpression("width", width));

            return result;
        }

        public override string ToString()
        {
            // DC 2 -1 -2 -1 0.15 21
            return string.Format("DC {0} {1} {2} {3} {4} {5}",
                center.X, center.Y, edge.X, edge.Y,
                width,
                Layer.GetLayerNumber_Legacy(layer));
        }

        public static fp_circle Parse(SExpression sexpr)
        {
            fp_circle result = new fp_circle();

            if (sexpr.Name == "fp_circle")
            {
                result.center = (sexpr.Items[0] as SExpression).GetPointF();
                result.edge = (sexpr.Items[1] as SExpression).GetPointF();

                result.layer = Layer.ParseLayer(sexpr.Items[2]);
                result.width = (sexpr.Items[3] as SExpression).GetFloat();

                return result;
            }
            else
                return null;  // error
        }
    }
}
