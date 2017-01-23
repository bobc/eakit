using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace RMC.VectorUtils
{
    /// <summary>
    /// A list of Point
    /// </summary>
    public class VectorPath
    {
        public List<PointF> Points;

        public VectorPath()
        {
            Points = new List<PointF>();
        }

        public void AddLine(Point p1, Point p2)
        {
            Points.Add(p1);
            Points.Add(p2);
        }

        //public PointF[] GetPoints()
        //{
        //    //
        //    PointF[] result = new PointF[Points.Count];

        //    int index = 0;
        //    foreach (PointF p in Points)
        //        result[index++] = p;

        //    return result;
        //}

        public PointF[] GetFPoints()
        {
            //
            PointF[] result = new PointF[Points.Count];

            int index = 0;
            foreach (PointF p in Points)
                result[index++] = p;

            return result;
        }
    }
}
