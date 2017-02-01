using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;


namespace Kicad_utils.ModuleDef
{
    // for beziers?
    // not implemented yet

    public class fp_curve : fp_shape
    {
        public List<PointF> Polygon;    // 4 points

        public fp_curve()
        {
        }

        public override void FlipX(PointF pos)
        {
            throw new NotImplementedException();
        }

        public override SExpression GetSExpression()
        {
            throw new NotImplementedException();
        }
    }
}