using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;

namespace Kicad_utils.Pcb
{
    //   (gr_line (start 20 40) (end 20 20) (angle 90) (layer Edge.Cuts) (width 0.15))
    public class gr_line : graphic_base
    {
        public float width;

        public PointF start;
        public PointF end;
        public float angle;

        public gr_line()
        {
        }

        public gr_line(PointF start, PointF end, string layer, float width)
        {
            this.layer = layer;
            this.width = width;
            this.start = start;
            this.end = end;
            this.angle = 0;
        }

        public override SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "gr_line";
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

        //  (gr_line (start 77.47 39.37) (end 77.47 62.23) (angle 90) (layer Dwgs.User) (width 0.14986))
        public static gr_line Parse(SExpression root_node)
        {
            gr_line result = new gr_line();

            foreach (SExpression node in root_node.Items)
            {

                if (node.Name == "start")
                {
                    float x = float.Parse((node.Items[0] as SNodeAtom).Value);
                    float y = float.Parse((node.Items[1] as SNodeAtom).Value);

                    result.start = new PointF(x, y);
                }
                else if (node.Name == "end")
                {
                    float x = float.Parse((node.Items[0] as SNodeAtom).Value);
                    float y = float.Parse((node.Items[1] as SNodeAtom).Value);

                    result.end = new PointF(x, y);
                }
                else if (node.Name == "layer")
                {
                    result.layer = (node.Items[0] as SNodeAtom).Value;
                }
                else if (node.Name == "width")
                {
                    result.width = (node.Items[0] as SNodeAtom).AsFloat;
                }
            }
            return result;
        }

    }
}
