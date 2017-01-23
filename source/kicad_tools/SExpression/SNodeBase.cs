using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SExpressions
{
    public class SNodeBase
    {
        public Types Type
        {
            get { return Types.None; }
        }

        public virtual void Output(List<string> lines, int depth)
        {
        }
    }
}
