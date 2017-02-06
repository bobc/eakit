using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cad2D
{
    public class MathUtil
    {
        public static double DegToRad(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static float  DegToRad(float degrees)
        {
            return (float)(degrees * Math.PI / 180);
        }


        public static double RadToDeg(double rad)
        {
            return rad / Math.PI * 180;
        }

        public static float RadToDeg(float rad)
        {
            return (float)(rad / Math.PI * 180);
        }

        public static float NormalizeAngle (float angle)
        {
            while (angle < 0)
                angle += 360;

            while (angle >= 360)
                angle -= 360;

            return angle;
        }
    }
}
