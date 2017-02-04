using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;

namespace Kicad_utils.ModuleDef
{

    // abstract base
    public abstract class fp_shape
    {
        public string layer;
        public float width;

        public abstract SExpression GetSExpression();

        /// <summary>
        /// Performs the KiCad "flip" function.
        /// </summary>
        /// <param name="pos"></param>
        public virtual void FlipX(PointF pos)
        {
            layer = Layer.FlipLayer(layer);
        }

        public abstract void RotateBy(float angle);
        
        

    }
}
