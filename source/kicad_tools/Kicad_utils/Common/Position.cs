using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using SExpressions;

namespace Kicad_utils
{
    public class Position
    {
        public PointF At;
        public float Rotation;

        public Position() { }

        public Position(PointF p)
        {
            At = new PointF(p.X, p.Y);
            Rotation = 0f;
        }

        public Position(float x, float y)
        {
            At = new PointF(x, y);
            Rotation = 0f;
        }

        public Position(float x, float y, float rotation)
        {
            At = new PointF(x, y);
            Rotation = rotation;
        }

        public static Position Parse(SExpression node)
        {
            Position result;

            if ((node.Items != null) && (node.Items.Count >= 2) && (node.Items[0] is SNodeAtom))
            {
                result = new Position();
                float x = 0;
                float.TryParse((node.Items[0] as SNodeAtom).Value, out x);

                float y = 0;
                float.TryParse((node.Items[1] as SNodeAtom).Value, out y);

                result.At = new PointF(x, y);

                if (node.Items.Count > 2)
                {
                    float.TryParse((node.Items[2] as SNodeAtom).Value, out result.Rotation);
                }

                return result;
            }
            else
                return null;  // error
        }

        public SExpression GetSExpression()
        {
            return new SExpression("at", new List<SNodeBase>{
                new SNodeAtom(At.X),
                new SNodeAtom(At.Y),
                new SNodeAtom(Rotation)
                });

        }

    }
}
