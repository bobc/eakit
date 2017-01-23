using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SExpressions;

namespace Kicad_utils.Pcb
{
    public abstract class graphic_base
    {
        public string layer;

        public abstract SExpression GetSExpression();

        public static List<SExpression> GetSExpressionList(List<graphic_base> drawings)
        {
            List<SExpression> result = new List<SExpression>();
            foreach (graphic_base drawing in drawings)
            {
                result.Add(drawing.GetSExpression());
            }
            return result;
        }


    }
}
