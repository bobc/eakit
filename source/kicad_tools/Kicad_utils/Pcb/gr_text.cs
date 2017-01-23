using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;

namespace Kicad_utils.Pcb
{
    //  (gr_text 
    //     Test 
    //     (at 196.7564 126.9798) 
    //     (layer B.SilkS)
    //     (effects ( 
    //                 font (size 1.5 1.5) 
    //                      (thickness 0.3)
    //              ) 
    //     (justify mirror))
    //  )


    public class gr_text : graphic_base
    {
        public string Value;
        public PointF at;
        public float rotation;
        public FontAttributes font;
        public TextJustify horiz_align;
        public bool mirror;

        public gr_text()
        {
        }

        public gr_text(string value, PointF at, string layer, SizeF font_size, float thickness, float rotation)
        {
            this.Value = value;
            this.at = at;
            this.layer = layer;
            this.rotation = rotation;

            this.font = new FontAttributes();
            this.font.Size = font_size;
            this.font.thickness = thickness;

            horiz_align = TextJustify.center;
            mirror = false;
        }

        public gr_text(gr_text src)
        {
            this.layer = src.layer;

            this.Value = src.Value;
            this.at = src.at;
            this.rotation = src.rotation;

            this.font = new FontAttributes();
            this.font.Size = src.font.Size;
            this.font.thickness = src.font.thickness;
            this.font.italic = src.font.italic;

            this.horiz_align = src.horiz_align;
            this.mirror = src.mirror;
        }

        public override SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "gr_text";
            result.Items = new List<SNodeBase>();
            result.Items.Add(new SNodeAtom(Value));
            result.Items.Add(new SExpression("at", new List<SNodeBase>{
                new SNodeAtom(at.X), 
                new SNodeAtom(at.Y),
                new SNodeAtom(rotation)
            }));
            result.Items.Add(new SExpression("layer", layer));

            SExpression effects = new SExpression();
            {
                effects.Name = "effects";
                effects.Items = new List<SNodeBase>();

                effects.Items.Add(font.GetSExpression());

                if ((horiz_align != TextJustify.center) || mirror)
                {
                    SExpression justify = new SExpression();
                    justify.Name = "justify";
                    justify.Items = new List<SNodeBase>();
                    if (horiz_align == TextJustify.left)
                        justify.Items.Add(new SNodeAtom("left"));
                    else if (horiz_align == TextJustify.right)
                        justify.Items.Add(new SNodeAtom("right"));

                    if (mirror)
                        justify.Items.Add(new SNodeAtom("mirror"));
                    effects.Items.Add(justify);
                }
            }
            result.Items.Add(effects);

            return result;
        }

        public static gr_text Parse(SExpression root_node)
        {
            gr_text result = new gr_text();
            SNodeBase node;

            node = root_node.Items[0];
            result.Value = (node as SNodeAtom).Value;

            // at
            node = root_node.Items[1];
            SExpression sub = node as SExpression;

            if (sub.Name == "at")
            {
                float x = float.Parse((sub.Items[0] as SNodeAtom).Value);
                float y = float.Parse((sub.Items[1] as SNodeAtom).Value);

                result.at = new PointF(x, y);
            }

            return result;
        }


    }
}
