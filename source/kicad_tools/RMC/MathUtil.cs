using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RMC
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
    }
}
