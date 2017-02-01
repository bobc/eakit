using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using Cad2D;
using SExpressions;
using Kicad_utils.ModuleDef;

namespace Kicad_utils.Pcb
{
    public class Dimension
    {
        public string layer;
        public float length;
        public float width;
        public gr_text caption;

        public PointF Start;
        public PointF End;

        public List<DimensionFeature> Features;

        public Dimension()
        {
            Features = new List<DimensionFeature>();
        }

        /*
          (dimension 
                81.28 
                (width 0.3) 
                (layer Dwgs.User)
                (gr_text "81.280 mm" 
                    (at 39.37 -77.55) 
                    (layer Dwgs.User)
                    (effects (font (size 1.5 1.5) (thickness 0.3)))
                )
                (feature1 (pts (xy 80.01 -74.93) (xy 80.01 -78.9)))
                (feature2 (pts (xy -1.27 -74.93) (xy -1.27 -78.9)))
                (crossbar (pts (xy -1.27 -76.2) (xy 80.01 -76.2)))
                (arrow1a  (pts (xy 80.01 -76.2) (xy 78.883496 -75.613579)))
                (arrow1b  (pts (xy 80.01 -76.2) (xy 78.883496 -76.786421)))
                (arrow2a  (pts (xy -1.27 -76.2) (xy -0.143496 -75.613579)))
                (arrow2b  (pts (xy -1.27 -76.2) (xy -0.143496 -76.786421)))
          )
        */

        public Dimension(string layer, float width, PointF start, PointF end, 
            float text_size, float text_width,
            bool is_mm, int precision, bool show_units)
        {
            this.layer = layer;
            this.width = width;
            this.Start = start;
            this.End = end;
            this.length = PointFExt.DistanceBetweenPoints(start, end);

            float height = -2 * text_size;
            SizeF arrow_head = new SizeF(1f, 0.2f);
            PointF normalised_end = new PointF(start.X + length, start.Y);
            double angle = MathUtil.RadToDeg (Math.Atan2(end.Y - start.Y, end.X - start.X));

            // create the dimension line assuming angle = 0

            Features = new List<DimensionFeature>();
            Features.Add(new DimensionFeature(DimensionFeature.Crossbar, 
                new PointF(start.X, start.Y + height / 2),
                new PointF(normalised_end.X, start.Y + height / 2)
                ));

            Features.Add(new DimensionFeature(DimensionFeature.Feature1,
                new PointF(start.X, start.Y ),
                new PointF(start.X, start.Y + height)
                ));

            Features.Add(new DimensionFeature(DimensionFeature.Feature2,
                new PointF(normalised_end.X, start.Y),
                new PointF(normalised_end.X, start.Y + height)
                ));

            Features.Add(new DimensionFeature(DimensionFeature.Arrow1a,
                new PointF(start.X, start.Y + height / 2),
                new PointF(start.X + arrow_head.Width, start.Y + height / 2 + arrow_head.Height)
                ));
            Features.Add(new DimensionFeature(DimensionFeature.Arrow1b,
                new PointF(start.X, start.Y + height / 2),
                new PointF(start.X + arrow_head.Width, start.Y + height / 2 - arrow_head.Height)
                ));

            Features.Add(new DimensionFeature(DimensionFeature.Arrow2a,
                new PointF(normalised_end.X, start.Y + height / 2),
                new PointF(normalised_end.X - arrow_head.Width, start.Y + height / 2 + arrow_head.Height)
                ));

            Features.Add(new DimensionFeature(DimensionFeature.Arrow2b,
                new PointF(normalised_end.X, start.Y + height / 2),
                new PointF(normalised_end.X - arrow_head.Width, start.Y + height / 2 - arrow_head.Height)
                ));

            string text;
            if (is_mm)
            {
                text = length.ToString ("f"+precision);
                if (show_units)
                    text += "mm";
            }
            else
            {
                text = (length / 25.4).ToString("f" + precision);
                if (show_units)
                    text += "in";
            }

            caption = new gr_text(text, new PointF(start.X + length / 2, start.Y + height / 2 - text_size / 2 - text_size*0.1f), layer, new SizeF(text_size, text_size), text_width, 0);

            // rotate about start point
            RotateBy((float)angle);
        }

        public void RotateBy (float angle)
        {
            caption.at = caption.at.RotateAt(Start, angle);
            caption.rotation = MathUtil.NormalizeAngle (caption.rotation - angle);

            foreach (DimensionFeature feature in Features)
                for (int j= 0; j < feature.Polygon.Count; j++)
                    feature.Polygon[j] = feature.Polygon[j].RotateAt(Start, angle);

        }

        public SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "dimension";
            result.Items = new List<SNodeBase>();
            result.Items.Add(new SNodeAtom(length));
            result.Items.Add(new SExpression("width", width));
            result.Items.Add(new SExpression("layer", layer));
            // text
            result.Items.Add(caption.GetSExpression());

            foreach (DimensionFeature feature in Features)
                result.Items.Add(feature.GetSExpression());

            return result;
        }

        public static List<SExpression> GetSExpressionList(List<Dimension> list)
        {
            List<SExpression> result = new List<SExpression>();
            foreach (Dimension item in list)
            {
                result.Add(item.GetSExpression());
            }
            return result;
        }

        public static Dimension Parse(SExpression root_node)
        {
            Dimension result;

            if ((root_node is SExpression) && ((root_node as SExpression).Name == "dimension"))
            {
                result = new Dimension();
                int index = 0;

                //
                while (index < root_node.Items.Count)
                {
                    SNodeBase node = root_node.Items[index];
                    SExpression sub = node as SExpression;

                    switch (sub.Name)
                    {
                        case "at": //todo: get all the things
                            break;
                    }
                    index++;
                }

                return result;
            }
            else
                return null;  // error
        }

    }
}
