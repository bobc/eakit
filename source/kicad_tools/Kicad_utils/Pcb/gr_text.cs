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
    //     (effects 
    //        (font (size 1.5 1.5) 
    //              (thickness 0.3)
    //        ) 
    //        (justify mirror)
    //     )
    //  )


    public class gr_text : graphic_base
    {
        public string Value;

        public Position Position;
        //public PointF at;
        //public float rotation;

        public TextEffects effects;

        //public FontAttributes font;
        //public TextJustify horiz_align;
        //public bool mirror;

        public gr_text()
        {
            Position = new Position();
            effects = new TextEffects();
        }

        public gr_text(string value, PointF at, string layer, SizeF font_size, float thickness, float rotation)
        {
            this.layer = layer;

            this.Value = value;
            this.Position = new Position();
            this.Position.At = at;
            this.Position.Rotation = rotation;

            this.effects = new TextEffects();
            this.effects.font.Size = font_size;
            this.effects.font.thickness = thickness;

            this.effects.horiz_align = TextJustify.center;
            this.effects.mirror = false;
        }

        public gr_text(gr_text src)
        {
            this.layer = src.layer;

            this.Value = src.Value;
            this.Position = new Position();
            this.Position.At = src.Position.At;
            this.Position.Rotation = src.Position.Rotation;

            this.effects = new TextEffects();
            this.effects.font = new FontAttributes();
            this.effects.font.Size = src.effects.font.Size;
            this.effects.font.thickness = src.effects.font.thickness;
            this.effects.font.italic = src.effects.font.italic;
            this.effects.vert_align = src.effects.vert_align;
            this.effects.horiz_align = src.effects.horiz_align;
            this.effects.mirror = src.effects.mirror;
        }

        public override SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "gr_text";
            result.Items = new List<SNodeBase>();
            result.Items.Add(new SNodeAtom(Value));

            result.Items.Add(Position.GetSExpression());
            result.Items.Add(new SExpression("layer", layer));

            result.Items.Add(effects.GetSExpression());

            //SExpression effects = new SExpression();
            //{
            //    effects.Name = "effects";
            //    effects.Items = new List<SNodeBase>();

            //    effects.Items.Add(font.GetSExpression());

            //    if ((horiz_align != TextJustify.center) || mirror)
            //    {
            //        SExpression justify = new SExpression();
            //        justify.Name = "justify";
            //        justify.Items = new List<SNodeBase>();
            //        if (horiz_align == TextJustify.left)
            //            justify.Items.Add(new SNodeAtom("left"));
            //        else if (horiz_align == TextJustify.right)
            //            justify.Items.Add(new SNodeAtom("right"));

            //        if (mirror)
            //            justify.Items.Add(new SNodeAtom("mirror"));
            //        effects.Items.Add(justify);
            //    }
            //}
            //result.Items.Add(effects);

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
                result.Position = Position.Parse(sub);
            }

            // TODO:

            return result;
        }


    }
}
