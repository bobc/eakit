using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace EagleConverter.Font
{
    /// <summary>
    /// A list of PointF
    /// </summary>
    public class VectorPath
    {
        public List<PointF> Points;

        public VectorPath()
        {
            Points = new List<PointF>();
        }

        public void AddLine(PointF p1, PointF p2)
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
