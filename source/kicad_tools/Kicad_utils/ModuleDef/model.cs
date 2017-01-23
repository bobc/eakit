using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SExpressions;
using CadCommon;

namespace Kicad_utils.ModuleDef
{
    public class model
    {
        public string path;
        Point3DF at;            // (xyz x y z)
        Point3DF scale;
        Point3DF rotate;

        public model()
        {
        }

        public model(string path)
        {
            this.path = path;
            this.at = new Point3DF(0, 0, 0);
            this.scale = new Point3DF(1, 1, 1);
            this.rotate = new Point3DF(0, 0, 0);
        }

        public SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "model";
            result.Items = new List<SNodeBase>();
            result.Items.Add(new SNodeAtom(path));

            SExpression sub1 = new SExpression("xyz", new List<SNodeBase> { new SNodeAtom(at.X), new SNodeAtom(at.Y), new SNodeAtom(at.Z) });
            result.Items.Add (new SExpression("at", new List<SNodeBase> { sub1 } ));

            SExpression sub2 = new SExpression("xyz", new List<SNodeBase> { new SNodeAtom(scale.X), new SNodeAtom(scale.Y), new SNodeAtom(scale.Z) });
            result.Items.Add(new SExpression("scale", new List<SNodeBase> { sub2 }));

            SExpression sub3 = new SExpression("xyz", new List<SNodeBase> { new SNodeAtom(rotate.X), new SNodeAtom(rotate.Y), new SNodeAtom(rotate.Z) });
            result.Items.Add(new SExpression("rotate", new List<SNodeBase> { sub3 }));

            return result;
        }

        public static Point3DF GetXyz (SExpression node)
        {
            SExpression sub = node.Items[0] as SExpression;
            return sub.GetPoint3DF();
        }

        public static model Parse(SExpression sexpr)
        {
            model result = new model();

            if (sexpr.Name == "model")
            {
                result.path = (sexpr.Items[0] as SNodeAtom).Value;

                for (int index = 1; index < sexpr.Items.Count; index++)
                {
                    SExpression sub = sexpr.Items[index] as SExpression;

                    if (sub.Name == "at")
                        result.at = GetXyz(sub);
                    else if (sub.Name == "scale")
                        result.scale = GetXyz(sub);
                    else if (sub.Name == "rotate")
                        result.rotate = GetXyz(sub);
                }

                return result;
            }
            else
                return null;  // error
        }

    }
}
