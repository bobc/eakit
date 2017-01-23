using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using SExpressions;


namespace Kicad_utils.ModuleDef
{
    public class Drill
    {
        public string style;        // "oval" default round
        public float diameter;      // round hole

        public SizeF size;          // oval hole

        public PointF offset;       // for hole?


        public Drill() { }

        public Drill(float size)
        {
            diameter = size;
            offset = Point.Empty;
        }

        public static Drill Parse(SExpression node)
        {
            Drill result = new Drill();

            int index = 0;
            if ((node.Items[0] as SNodeAtom).Value == "oval")
            {
                result.style = "oval";
                index++;
                float w = (node.Items[index++] as SNodeAtom).AsFloat;
                float h = (node.Items[index++] as SNodeAtom).AsFloat;
                result.size = new SizeF(w, h);
            }
            else
            {
                result.diameter = (node.Items[0] as SNodeAtom).AsFloat;
                index++;
            }

            for (int j = index; j < node.Items.Count; j++)
            {
                SExpression sub = node.Items[j] as SExpression;

                if (sub.Name == "offset")
                {
                    result.offset = sub.GetPointF();
                }
            }

            return result;
        }

        public SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "drill";
            result.Items = new List<SNodeBase>();

            if (style == "oval")
            {
                result.Items.Add(new SNodeAtom("oval"));
                result.Items.Add(new SNodeAtom(size.Width));
                result.Items.Add(new SNodeAtom(size.Height));
            }
            else {
                result.Items.Add(new SNodeAtom(diameter));
            }

            if (!offset.IsEmpty)
                result.Items.Add(new SExpression("offset", offset));

            return result;
        }
    }
}
