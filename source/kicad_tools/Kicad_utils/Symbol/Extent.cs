using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;



namespace Kicad_utils.Symbol
{
    public class Extent
    {
        public PointF Min;
        public PointF Max;

        public Extent()
        {
            Min = new PointF(0, 0);
            Max = new PointF(0, 0);
        }
    }
}
