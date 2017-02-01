using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using Kicad_utils;
using Kicad_utils.ModuleDef;
using Kicad_utils.Pcb;

using Cad2D;

namespace EagleConverter.Font
{
    public partial class NewStroke
    {
        const double interline_pitch_ratio = 1.5;
        const double overbar_position_factor = 1.22;
        const double bold_factor = 1.3;
        const double italic_tilt = 1.0 / 8;

        public SizeF GetTextSize (string text, TextEffects effects)
        {
            float width = 0;
            float height = effects.font.Size.Height;

            foreach (char c in text)
            {
                StrokeChar ch = new StrokeChar(newstroke_font[(int)c - 32]);
                width += ch.Width * effects.font.Size.Width;
            }
            return new SizeF (width, height);
        }

        // todo: bold, italic, mirror, (flip?)
        // align horiz/vert
        // overbar (~)
        // rotation

        public void DrawText(kicad_pcb k_pcb, PointF pos, string text, TextEffects effects, string layer)
        {
            foreach (char c in text)
            {
                StrokeChar ch = new StrokeChar(newstroke_font[(int)c - 32]);

                foreach (VectorPath path in ch.Paths)
                {
                    for (int j = 0; j < path.Points.Count - 1; j++)
                    {
                        PointF p1 = pos.Add (path.Points[j].Scale (effects.font.Size));
                        PointF p2 = pos.Add (path.Points[j+1].Scale (effects.font.Size));

                        gr_line line = new gr_line(p1, p2, layer, effects.font.thickness);
                        k_pcb.Drawings.Add(line);
                    }
                }

                pos.X += ch.Width * effects.font.Size.Width;
            }
        }

        // 

    }

}
