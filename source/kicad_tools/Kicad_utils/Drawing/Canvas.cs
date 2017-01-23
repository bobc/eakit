using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace Kicad_utils.Drawing
{
    public class Canvas
    {
        public Bitmap bitmap;

        public Graphics g;

        // real world units
        public PointF Min;
        public PointF Max;

        public PointF Scale;

        public float grid_spacing = 50f;
//        public PointF grid_min, grid_max;
        public bool ShowGrid = true;

        //
        public Color color_background = Color.White;
        public Color color_grid = Color.Gray;

        public Color color_symbol_background = Color.LightYellow;
        public Color color_symbol_drawing = Color.DarkRed;
        public Color color_symbol_text = Color.DarkCyan;

        // screen units (pixels)
        public Point S_origin;

        public Size S_size;

        // 1 means +ve Y down page (like Windows coords) (used by pcb)
        // -1 means +ve Y up page (like Windows coords) (used by eeschema?)
        public int Ydir = 1; 

        public Canvas()
        {
            Min = new PointF(-600, -600);
            Max = new PointF(600, 600);
        }

        public void Initialise ()
        {
            // assume that bitmap has already been created

            g = Graphics.FromImage(bitmap);

            S_size = new Size(bitmap.Width, bitmap.Height);

            Brush brush = new SolidBrush(color_background);
            g.FillRectangle(brush, new Rectangle(0, 0, S_size.Width, S_size.Height));

            Scale = new PointF(S_size.Width / (Max.X - Min.X), S_size.Height / (Max.Y - Min.Y));
            Scale.X = Math.Min(Scale.X, Scale.Y);
            Scale.Y = Scale.X;

            S_origin = new Point(0, 0);
            S_origin = ToScreen (new PointF(-Min.X,-Min.Y * Ydir));

            if (ShowGrid)
            {
                Pen gridpen = new Pen(color_grid);

                gridpen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                for (float x = Min.X; x <= Max.X; x += grid_spacing)
                {
                    Point sp1 = ToScreen(new PointF(x, Min.Y));
                    Point sp2 = ToScreen(new PointF(x, Max.Y));
                    g.DrawLine(gridpen, sp1, sp2);
                }

                for (float y = Min.Y; y <= Max.Y; y += grid_spacing)
                {
                    Point sp1 = ToScreen(new PointF(Min.X, y));
                    Point sp2 = ToScreen(new PointF(Max.X, y));
                    g.DrawLine(gridpen, sp1, sp2);
                }

                gridpen.Dispose();
            }
        }

        public int ToScreen(float p)
        {
            return (int)(Scale.X * p);
        }

        public Point ToScreen(PointF p)
        {
            return new Point((int)(Scale.X * p.X) + S_origin.X, (int)(Scale.Y * p.Y) * Ydir + S_origin.Y);
        }

    }
}
