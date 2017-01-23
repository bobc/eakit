using System;
using System.Collections.Generic;
using System.Text;

namespace CadCommon
{
    public class GlPolyIndex
    {
        public int[] PointIndex;

        public Attributes Attributes;

        public int VertexCount
        {
            get
            {
                if (PointIndex != null)
                    return PointIndex.Length;
                else
                    return 0;
            }
        }

        public GlPolyIndex()
        {
            this.Attributes = new Attributes();
        }
    }
}
