using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;

namespace Kicad_utils
{
    public class FontAttributes
    {
        public SizeF Size;
        public float thickness;

        public bool italic;

        public SExpression GetSExpression()
        {
            SExpression font = new SExpression();

            font.Name = "font";
            font.Items = new List<SNodeBase>();
            if (!Size.IsEmpty)
                font.Items.Add(new SExpression("size", new List<SNodeBase> { new SNodeAtom(Size.Width), new SNodeAtom(Size.Height) }));
            font.Items.Add(new SExpression("thickness", new List<SNodeBase> { new SNodeAtom(thickness) }));
            if (italic)
                font.Items.Add(new SNodeAtom("italic"));

            return font;
        }


        public static FontAttributes Parse(SExpression node)
        {
            if (node.Name == "font")
            {
                FontAttributes result = new FontAttributes();

                foreach (SNodeBase sub in node.Items)
                {
                    SExpression expr = sub as SExpression;

                    if (expr.Name == "size")
                        result.Size = expr.GetSizeF();
                    else if (expr.Name == "thickness")
                        result.thickness = expr.GetFloat();
                }

                return result;
            }
            else
                return null;  // error
        }


    }
}
