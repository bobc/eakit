using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using Cad2D;
using SExpressions;
using Kicad_utils.ModuleDef;

namespace Kicad_utils.Pcb
{
    public class DimensionFeature
    {
        public static string Feature1 = "feature1";
        public static string Feature2 = "feature2";
        public static string Crossbar = "crossbar";
        public static string Arrow1a = "arrow1a";
        public static string Arrow1b = "arrow1b";
        public static string Arrow2a = "arrow2a";
        public static string Arrow2b = "arrow2b";

        public string Name;
        public List<PointF> Polygon;

        public DimensionFeature()
        {
            Polygon = new List<PointF>();
        }

        public DimensionFeature (string name, PointF p1, PointF p2)
        {
            Name = name;
            Polygon = new List<PointF>();
            Polygon.Add(p1);
            Polygon.Add(p2);
        }


        public SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = Name;
            result.Items = new List<SNodeBase>();
            result.Items.Add(fp_polygon.GetPointList(Polygon));

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

        public void RotateBy(float angle)
        {
            //todo: ???
        }

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
