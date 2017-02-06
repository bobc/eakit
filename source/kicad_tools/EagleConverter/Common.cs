using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cad2D;
using EagleImport;
using System.Drawing;
using RMC;

namespace EagleConverter
{
    public class Common
    {
        public const float mm_to_mil = 1000.0f / 25.4f;
        public const float inch_to_mm = 25.4f;

        // convert strings in mm to inches (actually mil)
        public static float StrToInch(string s)
        {
            return (float)(StringUtils.StringToDouble(s) * mm_to_mil);
        }

        public static PointF StrToPointInch(string x, string y)
        {
            return new PointF(StrToInch(x), StrToInch(y));
        }

        public static SizeF StrToSizeInch(string dx, string dy)
        {
            return new SizeF(StrToInch(dx), StrToInch(dy));
        }


        // convert string to mm
        public static float StrToVal_mm(string s)
        {
            return (float)(StringUtils.StringToDouble(s));
        }

        public static PointF StrToPoint_mm(string x, string y)
        {
            return new PointF(StrToVal_mm(x), StrToVal_mm(y));
        }

        public static SizeF StrToSize_mm(string dx, string dy)
        {
            return new SizeF(StrToVal_mm(dx), StrToVal_mm(dy));
        }

        // For footprint files
        public static PointF StrToPointFlip_mm(string x, string y)
        {
            PointF result = new PointF(StrToVal_mm(x), -StrToVal_mm(y));
            return result;
        }

        //NB works in mm
        public static float RoundToGrid(float x, float align)
        {
            int j = (int)(x / align + align / 2f);

            return j * align;
        }


        public static RectangleF ConvertRect_mm(string x1, string y1, string x2, string y2, string rot)
        {
            PointF p1 = StrToPoint_mm(x1, y1);
            PointF p2 = StrToPoint_mm(x2, y2);
                        ExtRotation rotation = ExtRotation.Parse(rot);

            PointF mid = new PointF((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);

            p1 = p1.RotateAt(mid, rotation.Rotation);
            p2 = p2.RotateAt(mid, rotation.Rotation);

            p1 = p1.FlipX();
            p2 = p2.FlipX();

            RectangleF rect = new RectangleF(
                Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y),
                Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));

            return rect;
        }

        public static List<PointF> RectToPoly (RectangleF rect)
        {
            List<PointF> result = new List<PointF>();
            result.Add(new PointF(rect.X, rect.Y));
            result.Add(new PointF(rect.X + rect.Width, rect.Y));
            result.Add(new PointF(rect.X + rect.Width, rect.Y + rect.Height));
            result.Add(new PointF(rect.X, rect.Y + rect.Height));

            return result;
        }




        //
        public static int xGetAngleFlip(string rot, out bool mirror)
        {
            ExtRotation extRot = ExtRotation.Parse(rot);

            int result = (int)extRot.Rotation;

            if (extRot.Mirror)
                result = (result + 180) % 360;

            mirror = extRot.Mirror;
            return result;
        }


        /// Convert an Eagle curve end to a KiCad center for S_ARC
        public static PointF kicad_arc_center(PointF start, PointF end, double angle, out float radius, out float arc_start, out float arc_end)
        {
            // Eagle give us start and end.
            // S_ARC wants start to give the center, and end to give the start.
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;

            PointF mid = new PointF((float)(start.X + dx / 2), (float)(start.Y + dy / 2));

            double dlen = Math.Sqrt(dx * dx + dy * dy);
            double dist = dlen / (2 * Math.Tan(MathUtil.DegToRad(angle) / 2));

            PointF center = new PointF(
                (float)(mid.X + dist * (dy / dlen)),
                (float)(mid.Y - dist * (dx / dlen))
            );

            radius = (float)Math.Sqrt(dist * dist + (dlen / 2) * (dlen / 2));
            arc_start = (float)Math.Atan2(start.Y - center.Y, start.X - center.X);
            arc_start = MathUtil.RadToDeg(arc_start);
            arc_end = (float)Math.Atan2(end.Y - center.Y, end.X - center.X);
            arc_end = MathUtil.RadToDeg(arc_end);

            return center;
        }


        // for pin, label names
        public static string ConvertName(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                //todo: other chars?
                // special chars not at start?
                s = s.Replace("\"", "_");   //todo: mapping
                s = s.Replace("!", "~");   //todo: what if ~ already there?
                return s;
            }
            return s;
        }

        // Replace illegal filename chracters with underscore (_)
        public static string CleanFootprintName(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                //todo: other characters?
                // Replace \ and / with underscore (_)
                s = s.Replace("/", "_");
                s = s.Replace(@"\", "_");
                s = s.Replace(":", "_");
            }
            return s;
        }

        /// <summary>
        /// Remove HTML tags from string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string CleanTags(string orig)
        {
            if (!string.IsNullOrEmpty(orig))
            {
                string s = orig;

                s = s.Replace("<b>", "");
                s = s.Replace("</b>", "");
                s = s.Replace("<B>", "");
                s = s.Replace("</B>", "");

                s = s.Replace("<p>\n", "; ");
                s = s.Replace("<P>\n", "; ");

                s = s.Replace("<p>", "; ");
                s = s.Replace("<P>", "; ");

                s = s.Replace("<br>\n", "; ");
                s = s.Replace("<BR>\n", "; ");

                s = s.Replace("\n", "; ");

                s = s.Trim();
                while ((s.Length != 0) && s.StartsWith(";"))
                {
                    s = s.Substring(1);
                    s = s.Trim();
                }

                return s;
            }
            else
                return orig;
        }


        // [MSR]0 to 359.9
        public static int GetAngle(string rot)
        {
            int result = (int)ExtRotation.Parse(rot).Rotation;
            return result;
        }

        public static float GetTextThickness_mm(Text text)
        {
            return StrToVal_mm(text.Size) * int.Parse(text.Ratio) / 100f;
        }

        public static float GetTextThickness_mm(string textSize, string ratio = "8")
        {
            return StrToVal_mm(textSize) * int.Parse(ratio) / 100f;
        }


    }
}
