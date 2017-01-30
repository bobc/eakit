using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace Cad2D
{
    public static class PointFExt
    {

        public static PointF RotatePoint(PointF point, double angleDegree)
        {
            return RotatePoint(point, new PointF(0, 0), angleDegree);
        }

        public static PointF RotatePoint(PointF point, PointF pivot, double angleDegree)
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
    }
}
