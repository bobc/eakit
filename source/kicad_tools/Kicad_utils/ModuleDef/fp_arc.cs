using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;

namespace Kicad_utils.ModuleDef
{
    public class fp_arc : fp_shape
    {
        public PointF start; // was center;
        public PointF end;   // was start;

        public float angle;

        public fp_arc()
        {
        }

        public fp_arc(PointF start, PointF end, float angle, string layer, float width)
        {
            this.start = start;
            this.end = end;
            this.angle = angle;
            this.layer = layer;
            this.width = width;
        }

        public override void FlipX(PointF pos)
        {
            throw new NotImplementedException();
        }

        public override void RotateBy(float angle)
        {
            throw new NotImplementedException();
        }

        public override SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "fp_arc";
            result.Items = new List<SNodeBase>();
            result.Items.Add(new SExpression("start", new List<SNodeBase>{
                new SNodeAtom(start.X),
                new SNodeAtom(start.Y)
                }));
            result.Items.Add(new SExpression("end", new List<SNodeBase>{
                new SNodeAtom(end.X),
                new SNodeAtom(end.Y)
                }));

            result.Items.Add(new SExpression("angle", angle));
            result.Items.Add(new SExpression("layer", layer));
            result.Items.Add(new SExpression("width", width));

            return result;
        }

        public override string ToString()
        {
            // DA 2 -1 -2 -1 0.15 21
            return string.Format("DA {0} {1} {2} {3} {4} {5} {6}",
                start.X, start.Y,   // center
                end.X, end.Y,       // start
                angle * 10f,
                width,
                Layer.GetLayerNumber_Legacy(layer));
        }

        public static fp_arc Parse(SExpression sexpr)
        {
            fp_arc result = new fp_arc();

            if (sexpr.Name == "fp_arc")
            {
                foreach (SNodeBase sub in sexpr.Items)
                {
                    if (sub is SExpression)
                    {
                        SExpression node = sub as SExpression;

                        switch (node.Name)
                        {
                            case "start":
                                result.start = node.GetPointF();
                                break;
                            case "end":
                                result.end = node.GetPointF();
                                break;
                            case "angle":
                                result.angle = node.GetFloat();
                                break;
                            case "layer":
                                result.layer = Layer.ParseLayer(node);
                                break;
                            case "width":
                                result.width = node.GetFloat();
                                break;
                        }
                    }
                }

                return result;
            }
            else
                return null;  // error
        }

    }
}
