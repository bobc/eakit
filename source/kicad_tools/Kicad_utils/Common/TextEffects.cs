﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SExpressions;

namespace Kicad_utils
{
    public enum TextJustify { left, center, right };

    public class TextEffects
    {
        public FontAttributes font;

        public TextJustify horiz_align;
        public bool mirror;

        public TextEffects()
        {
            font = new FontAttributes();

            horiz_align = TextJustify.center;
            mirror = false;
        }

        public SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "effects";
            result.Items = new List<SNodeBase>();
            result.Items.Add(font.GetSExpression());

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

                result.Items.Add(justify);
            }
            
            return result;
        }


        //
        public static TextEffects Parse(SNodeBase node)
        {
            if ((node is SExpression) && ((node as SExpression).Name == "effects"))
            {
                SExpression expr = node as SExpression;
                TextEffects result = new TextEffects();

                int index = 0;
                while (index < expr.Items.Count)
                {
                    SExpression sexpr = expr.Items[index] as SExpression;

                    // justify: horiz_align, mirror
                    switch (sexpr.Name)
                    {
                        case "font":
                            result.font = FontAttributes.Parse(expr.Items[0] as SExpression);
                            break;

                        case "justify":
                            foreach (SNodeBase s2 in sexpr.Items)
                                if (s2 is SNodeAtom)
                                {
                                    SNodeAtom atom = s2 as SNodeAtom;
                                    switch (atom.Value)
                                    {
                                        case "mirror":
                                            result.mirror = true;
                                            break;
                                        case "left":
                                            result.horiz_align = TextJustify.left;
                                            break;
                                        case "right":
                                            result.horiz_align = TextJustify.right;
                                            break;
                                        case "center":
                                            result.horiz_align = TextJustify.center;
                                            break;
                                    }
                                }
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