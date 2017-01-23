using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;

namespace Kicad_utils.ModuleDef
{
    public class fp_polygon : fp_shape
    {
        public List<PointF> Polygon;

        public fp_polygon()
        {
            Polygon = new List<PointF>();
        }

        public fp_polygon(Cad2D.Polygon poly, string layer, float width)
        {
            this.layer = layer;
            this.width = width;

            Polygon = new List<PointF>();
            for (int j = 0; j < poly.Vertices.Count; j++)
                Polygon.Add(poly.GetPoint(j));
        }

        public fp_polygon(PointF p1, PointF p2, string layer, float width)
        {
            this.layer = layer;
            this.width = width;

            Polygon = new List<PointF>();

            Polygon.Add(new PointF(p1.X, p1.Y));
            Polygon.Add(new PointF(p1.X, p2.Y));
            Polygon.Add(new PointF(p2.X, p2.Y));
            Polygon.Add(new PointF(p2.X, p1.Y));
        }

        public fp_polygon (RectangleF rect, string layer, float width)
        {
            this.layer = layer;
            this.width = width;

            Polygon = new List<PointF>();

            Polygon.Add(new PointF(rect.Left, rect.Bottom));
            Polygon.Add(new PointF(rect.Left, rect.Top));
            Polygon.Add(new PointF(rect.Right, rect.Top));
            Polygon.Add(new PointF(rect.Right, rect.Bottom));
        }

        public override SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "fp_poly";
            result.Items = new List<SNodeBase>();
            result.Items.Add(GetPointList(Polygon));
            result.Items.Add(new SExpression("layer", layer));
            result.Items.Add(new SExpression("width", width));

            return result;
        }

        public static SExpression GetPointList(List<PointF> polygon)
        {
            SExpression result = new SExpression();

            result.Name = "pts";
            result.Items = new List<SNodeBase>();

            // close poly?
            foreach (PointF p in polygon)
            {
                result.Items.Add(new SExpression("xy", new List<SNodeBase> { new SNodeAtom(p.X), new SNodeAtom(p.Y) }));
            }
            return result;
        }

        public override string ToString()
        {
            // DP 0 0 0 0 <count> 0.15 21
            return string.Format("DS 0 0 0 0 {0} {1} {2}",
                Polygon.Count,
                width,
                Layer.GetLayerNumber_Legacy(layer));

            // Dl 
        }

        public static fp_polygon Parse(SExpression sexpr)
        {
            fp_polygon result = new fp_polygon();

            if ((sexpr.Name == "fp_polygon") || // ??
                 (sexpr.Name == "fp_poly")
                 )
            {
                //
                result.Polygon = new List<PointF>();

                // [0] = pts
                SExpression pts = (sexpr.Items[0] as SExpression);
                foreach (SExpression sub in pts.Items)
                    result.Polygon.Add(sub.GetPointF());

                result.layer = Layer.ParseLayer(sexpr.Items[1]);
                result.width = (sexpr.Items[2] as SExpression).GetFloat();

                return result;
            }
            else
                return null;  // error
        }
    }
}
