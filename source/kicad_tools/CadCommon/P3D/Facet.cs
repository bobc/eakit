using System;
using System.Collections.Generic;
using System.Text;

namespace CadCommon
{
    public class Facet
    {
        // typically 3 points (STL)
        // similar to Triangle

        public Point3DF[] Points;

        public Facet()
        {
        }

        public Facet(Point3DF p1, Point3DF p2, Point3DF p3)
        {
            Points = new Point3DF[3] { p1, p2, p3 };

        }

        public Facet(Point3DF[] points)
        {
            this.Points = new Point3DF[points.Length];
            for (int j = 0; j < points.Length; j++)
                this.Points[j] = points[j];

        }

        public override string ToString()
        {
            string result = "";

            if (Points.Length == 3)
                result = Points[0].ToString() + ", " + Points[1].ToString() + ", " + Points[2].ToString();
            return result;
        }
    }
}
