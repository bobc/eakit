using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using OpenTK;

namespace Cad2D
{
    public static class Vector2Ext
    {
        public static float Distance( Vector2 v1, Vector2 v2)
        {
            return (float)Math.Sqrt(Math.Pow(v1.X - v2.X, 2) + Math.Pow(v1.Y - v2.Y, 2));
        }

        public static Vector2 ToVector2 (Point a)
        {
            return new Vector2(a.X, a.Y);
        }

        public static Vector2 ToVector2(PointF a)
        {
            return new Vector2(a.X, a.Y);
        }

        public static PointF ToPointF(this Vector2 a)
        {
            return new PointF(a.X, a.Y);
        }

        public static Point ToPoint(this Vector2 a)
        {
            return new Point((int)a.X, (int)a.Y);
        }
    }
}
