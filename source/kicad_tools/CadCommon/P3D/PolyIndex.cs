using System;
using System.Collections.Generic;
using System.Text;

namespace CadCommon
{
    // equivalent to AMF triangle
    // equivalent to TriangleIndexed

    public class PolyIndex
    {
        public List<int> PointIndex;

        public PolyIndex()
        {
            PointIndex = new List<int>();
        }

        public PolyIndex(int p1, int p2, int p3)
        {
            PointIndex = new List<int>();
            PointIndex.Add(p1);
            PointIndex.Add(p2);
            PointIndex.Add(p3);
        }

        public Facet GetFacet (List<Point3DF> Points)
        {
            Facet result = new Facet();
            result.Points = new Point3DF[PointIndex.Count];

            for (int j = 0; j < PointIndex.Count; j++)
                result.Points[j] = Points[PointIndex[j]];

            return result;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", PointIndex[0], PointIndex[1], PointIndex[2]);
        }
    }


}
