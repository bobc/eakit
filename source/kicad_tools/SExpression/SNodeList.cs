using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SExpressions
{
    public class SNodeList : SNodeBase
    {
        public List<SNodeBase> Items;

        public override void Output(List<string> lines, int depth)
        {
            string line = new String(' ', depth) + "(";

            // sub
            if (Items != null)
            {
                foreach (SNodeBase item in Items)
                {
                    if (item is SExpression)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            lines.Add(line);
                        line = new String(' ', depth);
                        item.Output(lines, depth + 1);
                    }
                    else if (item is SNodeList)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            lines.Add(line);
                        line = new String(' ', depth);
                        item.Output(lines, depth + 1);
                    }
                    else
                    {
                        line = line + " " + item.ToString();
                    }
                }

                //if (!string.IsNullOrEmpty(line))
                //{
                //    line += ")";
                //    //lines.Add(line);
                //}
            }

            if (line == "") line = new String(' ', depth);
            line += ")";

            lines.Add(line);

        }

        public Types Type { get { return Types.List; } }
    }
}
