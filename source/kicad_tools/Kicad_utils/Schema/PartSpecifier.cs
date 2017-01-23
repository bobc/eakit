using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SExpressions;

namespace Kicad_utils.Schema
{
    public class PartSpecifier
    {
        public string LibName;  // lib
        public string PartName; // part

        public PartSpecifier() { }

        public PartSpecifier(string Name)
        {
            this.PartName = Name;
        }

        public PartSpecifier(string Lib, string Name)
        {
            this.LibName = Lib;
            this.PartName = Name;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(LibName))
                return LibName + ":" + PartName;
            else
                return PartName;
        }

        public static PartSpecifier Parse(SExpression root_node)
        {
            PartSpecifier result = new PartSpecifier();

            foreach (SExpression node in root_node.Items)
            {
                switch (node.Name)
                {
                    case "lib":
                        result.LibName = node.GetValue();
                        break;
                    case "part":
                        result.PartName = node.GetValue();
                        break;
                }
            }
            return result;
        }

    }
}
