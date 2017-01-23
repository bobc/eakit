using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace CadCommon
{
    // compatible with:
    //  AMF color
    //  VRML color (alpha not used)

    // NB alpha = 1 means opaque
    // 
    public class ColorF
    {
        [XmlElement("r")]
        public double R;

        [XmlElement("g")]
        public double G;

        [XmlElement("b")]
        public double B;

        [XmlElement("a")]
        public double A;

        public ColorF()
        {
        }

        public ColorF(double r, double g, double b, double a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public ColorF(ColorF source)
        {
            R = source.R;
            G = source.G;
            B = source.B;
            A = source.A;
        }

        public override bool Equals(object obj)
        {
            if (obj is ColorF)
            {
                ColorF o2 = obj as ColorF;

                return (R == o2.R) && (G == o2.G) && (B == o2.B) && (A == o2.A);
            }
            else if (obj is Color)
            {
                ColorF o2 = ColorF.FromColor((Color)obj);

                return (R == o2.R) && (G == o2.G) && (B == o2.B) && (A == o2.A);
            }
            else
                return base.Equals(obj);
        }

        public bool IsEqual (Color color)
        {
            ColorF o2 = ColorF.FromColor(color);

            return (R == o2.R) && (G == o2.G) && (B == o2.B) && (A == o2.A);
        }

        public bool IsEqual(ColorF color)
        {
            return (R == color.R) && (G == color.G) && (B == color.B) && (A == color.A);
        }

        public static byte ByteRange(double f)
        {
            if (f < 0)
                return 0;
            else if (f >= 1.0)
                return 255;
            else
                return (byte)(255.0 * f);
        }

        public Color ToRGBColor()
        {            
            int r, g, b;

            r = ByteRange(this.R);
            g = ByteRange(this.G);
            b = ByteRange(this.B);

            return Color.FromArgb (r,g,b);
        }

        public static ColorF FromColor(Color color)
        {
            return new ColorF(color.R / 255.0, color.G / 255.0, color.B / 255.0, color.A / 255.0);
        }

        public override string ToString()
        {
            return string.Format ("[RGBA={0}, {1}, {2}, {3}]", R, G, B, A);
        }

        public static ColorF Blend (ColorF a, ColorF b, float alpha)
        {
            if ((a == null) && (b == null))
            {
                return null;
            }
            else if (a==null)
            {
                return b;
            }
            else if (b==null)
            {
                return a;
            }
            else
                return new ColorF(a.R * alpha + b.R * (1 - alpha), a.G * alpha + b.G * (1 - alpha), a.B * alpha + b.B * (1 - alpha), 1);
        }
    }
}
