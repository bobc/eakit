using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace EagleConverter.Font
{
    class StrokeChar
    {
        public List<VectorPath> Paths;

        //
        PointF MinExtent;
        PointF MaxExtent;

        const float stroke_font_scale = 1.0f / 21.0f;
        const int XOffset = 0;
        const int YOffset = -10;


        public StrokeChar()
        {
            Paths = new List<VectorPath>();
            MinExtent = new PointF(0, 0);
            MaxExtent = new PointF(0, 0);
        }

        public StrokeChar(string s)
        {
            Paths = new List<VectorPath>();
            MinExtent = new PointF(0, 0);
            MaxExtent = new PointF(0, 0);

            ParseCharacterData(s);
        }

        public float Width
        {
            get
            {
                //CalculateExtent(); 
                return MaxExtent.X - MinExtent.X;
            }
        }

        private RectangleF CalculateBoundingBox()
        {
            RectangleF result;

            if (Paths.Count == 0)
            {
                result = new RectangleF(0, 0, 0, 0);
            }
            else
            {
                PointF min, max;
                min = new PointF(float.MaxValue, float.MaxValue);
                max = new PointF(float.MinValue, float.MinValue);
                foreach (VectorPath path in Paths)
                {
                    foreach (PointF p in path.Points)
                    {
                        if (p.X < min.X) min.X = p.X;
                        if (p.X > max.X) max.X = p.X;

                        if (p.Y < min.Y) min.Y = p.Y;
                        if (p.Y > max.Y) max.Y = p.Y;
                    }
                }
                result = new RectangleF(min.X, min.Y, max.X - min.X, max.Y - min.Y);
            }
            return result;
        }

        float ConvertChar (char c, int offset)
        {
            return ((int)c - (int)'R' + offset) * stroke_font_scale;
        }

        public void ParseCharacterData(string CharData)
        {
            VectorPath path = new VectorPath();
            int index;

            MinExtent.X = ConvertChar(CharData[0], 0);
            MaxExtent.X = ConvertChar(CharData[1], 0);

            PointF p;

            for (index = 2; index < CharData.Length;)
            {
                if ((CharData[index] == ' ') && (CharData[index+1] == 'R'))
                {
                    if (path.Points.Count != 0)
                    {
                        Paths.Add(path);
                        path = new VectorPath();
                    }
                    index += 2;
                }
                else
                {
                    float x = ConvertChar(CharData[index++], XOffset) - MinExtent.X;
                    float y = ConvertChar(CharData[index++], YOffset);

                    p = new PointF(x, y);
                    path.Points.Add(p);
                }
            }
            if (path.Points.Count != 0)
            {
                Paths.Add(path);
            }


            //CalculateExtent();
        }


    }
}
