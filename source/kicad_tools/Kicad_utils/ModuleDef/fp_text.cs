using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;
using Cad2D;

namespace Kicad_utils.ModuleDef
{
    public class fp_text
    {
        public string Type; // reference | value | user

        public string Value;    // text

        // rotation = 0 or 90
        public Position position;

        public string layer;
        public bool visible;    // default: true
        // effects
        public TextEffects effects;

        public fp_text()
        {
            visible = true;
            position = new Position();
        }

        public fp_text(string Name, string value, PointF at, string layer, SizeF font_size, float thickness, bool visible)
        {
            this.Type = Name;
            this.Value = value;
            this.position = new Position(at);
            this.layer = layer;
            this.visible = visible;

            this.effects = new TextEffects();
            this.effects.font = new FontAttributes();
            this.effects.font.Size = font_size;
            this.effects.font.thickness = thickness;
        }

        public fp_text(string value, PointF at, string layer, SizeF font_size, float thickness, bool visible)
        {
            this.Type = "user";
            this.Value = value;
            this.position = new Position(at);
            this.layer = layer;
            this.visible = visible;

            this.effects = new TextEffects();
            this.effects.font = new FontAttributes();
            this.effects.font.Size = font_size;
            this.effects.font.thickness = thickness;
        }

        public fp_text(string Name, string value, PointF at, string layer, SizeF font_size, float thickness, float rotation, bool visible, bool italic)
        {
            this.Type = Name;
            this.Value = value;
            this.position = new Position(at);
            this.layer = layer;
            this.visible = visible;
            this.position.Rotation = rotation;

            this.effects = new TextEffects();
            this.effects.font = new FontAttributes();
            this.effects.font.Size = font_size;
            this.effects.font.thickness = thickness;
            this.effects.font.italic = italic;
        }

        public void FlipX(PointF pos)
        {
            layer = Layer.FlipLayer(layer);

            effects.mirror = !effects.mirror;

            position.At = position.At.FlipX();
            // todo : if vertical text , flip l/r justify
        }

        public void RotateBy(float angle)
        {
            float final_angle = MathUtil.NormalizeAngle(position.Rotation + angle);

            //position.At = position.At.Rotate(angle);

            switch ((int)final_angle)
            {
                case 0: position.Rotation = 0;
                    if (effects.horiz_align != TextJustify.center)
                        effects.horiz_align = TextJustify.left;
                    break;
                case 90:
                    position.Rotation = 90;
                    if (effects.horiz_align != TextJustify.center)
                        effects.horiz_align = TextJustify.left;
                    break;
                case 180:
                    position.Rotation = 0;
                    if (effects.horiz_align != TextJustify.center)
                        effects.horiz_align = TextJustify.right;
                    break;
                case 270:
                    position.Rotation = 90;
                    if (effects.horiz_align != TextJustify.center)
                        effects.horiz_align = TextJustify.right;
                    break;
            }
        }


        public SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "fp_text";
            result.Items = new List<SNodeBase>();
            result.Items.Add(new SNodeAtom(Type));
            result.Items.Add(new SNodeAtom(Value));
            result.Items.Add(position.GetSExpression());

            result.Items.Add(new SExpression("layer", layer));
            if (!visible)
                result.Items.Add(new SNodeAtom("hide"));

            if (effects != null)
                result.Items.Add(effects.GetSExpression());

            return result;
        }

        // for legacy modules
        public override string ToString()
        {
            // T0 0 4 1 1 0 0.15 N V 21 N "conn_2mm_1x2"
            return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} \"{10}\"",
                position.At.X, position.At.Y,
                effects.font.Size.Height, effects.font.Size.Width,
                (int)(position.Rotation * 10), effects.font.thickness,
                "N", // always
                visible ? "V" : "I",
                Layer.GetLayerNumber_Legacy(layer),
                effects.font.italic ? "I" : "N",
                Value);
        }


        public static fp_text Parse(SNodeBase node)
        {
            fp_text result = new fp_text();

            if ((node is SExpression) && ((node as SExpression).Name == "fp_text"))
            {
                SExpression expr = node as SExpression;

                result.Type = (expr.Items[0] as SNodeAtom).Value;
                result.Value = (expr.Items[1] as SNodeAtom).Value;

                int index = 2;

                while (index < expr.Items.Count)
                {
                    if (expr.Items[index] is SExpression)
                    {
                        SExpression sub = expr.Items[index] as SExpression;
                        if (sub.Name == "at")
                            result.position = Position.Parse(sub);
                        else if (sub.Name == "effects")
                            result.effects = TextEffects.Parse(sub);
                        else if (sub.Name == "layer")
                            result.layer = Layer.ParseLayer(sub);
                    }
                    else
                    {
                        SNodeAtom atom = expr.Items[index] as SNodeAtom;

                        if (atom.Value == "hide")
                            result.visible = false;
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
