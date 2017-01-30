using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;
using Kicad_utils.ModuleDef;

namespace Kicad_utils.Pcb
{
    public class DimensionFeature
    {
        public string Name;
        public List<PointF> polygon;

        public DimensionFeature()
        {
            polygon = new List<PointF>();
        }

        public SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = Name;
            result.Items = new List<SNodeBase>();
            result.Items.Add(fp_polygon.GetPointList(polygon));

            return result;
        }

        private static List<string> feature_names = new List<string> {
            "feature1",
            "feature2",
            "crossbar",
            "arrow1a",
            "arrow1b",
            "arrow2a",
            "arrow2b"
        };

        public static DimensionFeature Parse(SExpression root_node)
        {
            DimensionFeature result;

            if ((root_node is SExpression) && (feature_names.Find (x => x == (root_node as SExpression).Name) != null) )
            {
                result = new DimensionFeature();
                int index = 0;

                result.Name = (root_node as SExpression).Name;

                //
                while (index < root_node.Items.Count)
                {
                    SNodeBase node = root_node.Items[index];
                    SExpression sub = node as SExpression;

                    switch (sub.Name)
                    {
                        case "pts": //TODO: get points
                            break;
                    }
                    index++;
                }

                return result;
            }
            else
                return null;  // error
        }

    }
}
