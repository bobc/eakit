using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;

namespace CadCommon
{
    public class TriangleIndex : GlPolyIndex
    {
        public int v1
        {
            get { return PointIndex[0]; }
            set { PointIndex[0] = value; }
        }

        public int v2
        {
            get { return PointIndex[1]; }
            set { PointIndex[1] = value; }
        }

        public int v3
        {
            get { return PointIndex[2]; }
            set { PointIndex[2] = value; }
        }


        public TriangleIndex(int v1, int v2, int v3)
        {
            PointIndex = new int[3];
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        public TriangleIndex(int v1, int v2, int v3, Color color)
        {
            PointIndex = new int[3];
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
            this.Attributes.Color = color;
        }
    }
}
