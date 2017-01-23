using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;

namespace Kicad_utils.Pcb
{
    public class General
    {
        public int links;
        public int no_connects;
        public RectangleF area;  //??
        public float thickness;
        public int drawings;
        public int tracks;
        public int zones;
        public int modules;
        public int nets;

        public General()
        {
            links = 0;
            no_connects = 0;
            area = new RectangleF(0, 0, 0, 0);
            thickness = 1.6f;
            drawings = 0;
            tracks = 0;
            zones = 0;
            modules = 0;
            nets = 1;
        }

        public static General Parse (SNodeBase node)
        {
            General result = new General();

            if ((node is SExpression) && ((node as SExpression).Name == "fp_text"))
            {
                SExpression expr = node as SExpression;

                result.links = (expr.Items[0] as SNodeAtom).AsInt;
                result.no_connects = (expr.Items[1] as SNodeAtom).AsInt;
                result.area = (expr.Items[2] as SExpression).GetRectF();
                result.thickness = (expr.Items[3] as SNodeAtom).AsFloat;
                result.drawings = (expr.Items[4] as SNodeAtom).AsInt;
                result.tracks = (expr.Items[5] as SNodeAtom).AsInt;
                result.zones = (expr.Items[6] as SNodeAtom).AsInt;
                result.modules = (expr.Items[7] as SNodeAtom).AsInt;
                result.nets = (expr.Items[8] as SNodeAtom).AsInt;

                return result;
            }
            else
                return null;  // error

        }

        public SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "general";
            result.Items = new List<SNodeBase>();

            result.Items.Add(new SExpression("links", links));
            result.Items.Add(new SExpression("no_connects", no_connects));
            result.Items.Add(new SExpression("area", area));
            result.Items.Add(new SExpression("thickness", thickness));
            result.Items.Add(new SExpression("drawings", drawings));
            result.Items.Add(new SExpression("tracks", tracks));
            result.Items.Add(new SExpression("zones", zones));
            result.Items.Add(new SExpression("modules", modules));
            result.Items.Add(new SExpression("nets", nets));

            return result;
        }
    }
}
