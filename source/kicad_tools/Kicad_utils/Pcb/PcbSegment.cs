using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Drawing;

using SExpressions;
using Kicad_utils.ModuleDef;

namespace Kicad_utils.Pcb
{
    public class PcbSegment
    {
        public PointF start;
        public PointF end;
        public float width;
        public string layer;
        public int net;
        public uint tstamp;

        public PcbSegment()
        {
        }


        public SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "segment";
            result.Items = new List<SNodeBase>();
            result.Items.Add(new SExpression("start", new List<SNodeBase>{
                new SNodeAtom(start.X),
                new SNodeAtom(start.Y)
            }));
            result.Items.Add(new SExpression("end", new List<SNodeBase>{
                new SNodeAtom(end.X),
                new SNodeAtom(end.Y)
            }));

            result.Items.Add(new SExpression("width", width));
            result.Items.Add(new SExpression("layer", layer));
            result.Items.Add(new SExpression("net", net));

            result.Items.Add(new SExpression("tstamp", tstamp.ToString("X8")));

            return result;
        }

        public static List<SExpression> GetSExpressionList(List<PcbSegment> Segments)
        {
            List<SExpression> result = new List<SExpression>();
            foreach (PcbSegment segment in Segments)
            {
                result.Add(segment.GetSExpression());
            }
            return result;
        }


        public static PcbSegment Parse(SExpression root_node)
        {
            PcbSegment result;

            if ((root_node is SExpression) && ((root_node as SExpression).Name == "segment"))
            {
                result = new PcbSegment();
                int index = 0;

                //
                while (index < root_node.Items.Count)
                {
                    SNodeBase node = root_node.Items[index];
                    SExpression sub = node as SExpression;

                    switch (sub.Name)
                    {
                        case "start":
                            {
                                float x = float.Parse((sub.Items[0] as SNodeAtom).Value);
                                float y = float.Parse((sub.Items[1] as SNodeAtom).Value);
                                result.start = new PointF(x, y);
                            }
                            break;
                        case "end":
                            {
                                float x = float.Parse((sub.Items[0] as SNodeAtom).Value);
                                float y = float.Parse((sub.Items[1] as SNodeAtom).Value);
                                result.end = new PointF(x, y);
                            }
                            break;
                        case "width":
                            result.width = sub.GetFloat();
                            break;
                        case "layer":
                            result.layer = sub.GetString();
                            break;
                        case "net":
                            result.net = sub.GetInt();
                            break;
                        case "tstamp":
                            result.tstamp = sub.GetUintHex();
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
