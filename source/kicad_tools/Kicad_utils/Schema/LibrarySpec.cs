using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SExpressions;

namespace Kicad_utils.Schema
{
    public class LibrarySpec
    {
        public string Name;
        public string Uri;

        public static LibrarySpec Parse(SExpression root_node)
        {
            LibrarySpec result = new LibrarySpec();

            foreach (SExpression node in root_node.Items)
            {
                switch (node.Name)
                {
                    case "logical":
                        result.Name = node.GetValue();
                        break;
                    case "uri":
                        result.Uri = node.GetValue();
                        break;
                }
            }
            return result;
        }

    }
}
