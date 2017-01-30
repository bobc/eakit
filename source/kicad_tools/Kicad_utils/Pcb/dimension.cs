using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

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

        public Dimension(string layer, float width, PointF start, PointF end, bool is_mm)
        {


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
