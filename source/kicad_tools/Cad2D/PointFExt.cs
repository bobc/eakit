using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace Cad2D
{
    public static class PointFExt
    {

        public static PointF Rotate(this PointF point, double angleDegree)
        {
            return Rotate(point, new PointF(0, 0), angleDegree);
        }

        public static PointF RotateP(PointF point, double angleDegree)
        {
            return Rotate(point, new PointF(0, 0), angleDegree);
        }

        public static PointF Rotate(PointF point, PointF pivot, double angleDegree)
        {
            double angle = MathUtil.DegToRad(angleDegree);
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            double dx = point.X - pivot.X;
            double dy = point.Y - pivot.Y;
            double x = cos * dx - sin * dy + pivot.X;
            double y = sin * dx + cos * dy + pivot.Y;

            PointF rotated = new PointF((float)x, (float)y);
            return rotated;
        }

        public static PointF RotateAt(this PointF point, PointF pivot, double angleDegree)
        {
            double angle = MathUtil.DegToRad(angleDegree);
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            double dx = point.X - pivot.X;
            double dy = point.Y - pivot.Y;
            double x = cos * dx - sin * dy + pivot.X;
            double y = sin * dx + cos * dy + pivot.Y;

            PointF rotated = new PointF((float)x, (float)y);
            return rotated;
        }

        /// <summary>
        /// Flip (mirror) about X axis   
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>

        //   ^
        // ------
        //   v

        public static PointF FlipX(this PointF point)
        {
            return new PointF(point.X, -point.Y);
        }

        public static float DistanceBetweenPoints(PointF a, PointF b)
        {
            return (float)Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
        }


        public static PointF Add (this PointF a, PointF b)
        {
            return new PointF (a.X + b.X, a.Y + b.Y);
        }

        public static PointF Sub(this PointF a, PointF b)
        {
            return new PointF(a.X - b.X, a.Y - b.Y);
        }

        //
        public static PointF Add(this PointF a, SizeF b)
        {
            return new PointF(a.X + b.Width, a.Y + b.Height);
        }

        public static PointF Scale(this PointF a, SizeF b)
        {
            return new PointF(a.X * b.Width, a.Y * b.Height);
        }

    }
}