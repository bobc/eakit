using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;
using Kicad_utils.Pcb;

namespace Kicad_utils.ModuleDef
{
    public class pad
    {
        public static string connect = "connect";
        public static string smd = "smd";
        public static string through_hole = "thru_hole";
        public static string nonplated_hole = "np_thru_hole";

        public string number; // pin number or name

        // thru_hole
        // smd
        // connect
        // np_thru_hole
        public string type;

        // circle 
        // rect 
        // oval
        // trapezoid
        public string shape;

        public Position position;
        //public PointF at;
        //public float rotation;

        public SizeF size;

        public PointF rect_delta;   // trapezoid

        public Drill drill;
        public SizeF oval_drill_size;

        public string layers; //    List/string , a wildcard mask PSV names e.g "F.Cu|F.Paste|F.Mask"
        public Net net;       // net <number> <string>    // use by pcb file  

        public float die_length;
        public float solder_mask_margin;
        public float clearance;
        public float solder_paste_margin;
        public int solder_paste_ratio;
        // 0 = parent
        // 2 = solid
        public int zone_connect;
        public float thermal_width;
        public float thermal_gap;

        public pad()
        {
        }

        public pad(string number, string type, string shape, PointF at, SizeF size, float drill)
        {
            this.number = number;
            this.type = type;
            this.shape = shape;
            this.position = new Position (at);
            this.size = size;
            this.drill = new Drill(drill);

            set_layers();
        }

        public pad(string number, string type, string shape, PointF at, SizeF size, float drill, string layers)
        {
            this.number = number;
            this.type = type;
            this.shape = shape;
            this.position = new Position(at);
            this.size = size;
            this.drill = new Drill(drill);

            this.layers = layers;
        }

        private void set_layers ()
        {
            switch (type)
            {
                case "thru_hole":
                    this.layers = "*.Cu|B.Mask|F.Mask"; break;
                case "smd":
                    this.layers = "F.Cu|F.Mask|F.Paste"; break;
                case "connect":
                    this.layers = "F.Cu|F.Mask"; break;
                case "np_thru_hole":
                    this.layers = "*.Cu"; break;
            }
        }

        public SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "pad";
            result.Items = new List<SNodeBase>();
            result.Items.Add(new SNodeAtom(number));
            result.Items.Add(new SNodeAtom(type));
            result.Items.Add(new SNodeAtom(shape));

            result.Items.Add(position.GetSExpression());

            result.Items.Add(new SExpression("size", new List<SNodeBase>{
                new SNodeAtom(size.Width),
                new SNodeAtom(size.Height)
            }));

            if (drill != null)
                result.Items.Add(drill.GetSExpression());

            if (layers != null)
                result.Items.Add(new SExpression("layers", Layer.GetLayerList(layers)));

            if (net != null)
                result.Items.Add(net.GetSExpression());

            //todo:
            //public float die_length;
            //public float solder_paste_margin;
            //public int solder_paste_ratio;
            //public float thermal_width;
            //public float thermal_gap;

            if (zone_connect != 0)
                result.Items.Add(new SExpression("zone_connect", zone_connect));

            if (solder_mask_margin != 0)
                result.Items.Add(new SExpression("solder_mask_margin", solder_mask_margin));

            if (clearance != 0)
                result.Items.Add(new SExpression("clearance", clearance));

            return result;
        }

        //
        public void WriteText(List<string> text)
        {
            text.Add("$PAD");
            //Sh "1" C 1 1 0 0 0
            //Dr 0.6 0 0
            //At STD N 00E0FFFF
            //Ne 0 ""
            //Po -1 0

            text.Add(string.Format("Sh \"{0}\" {1} {2} {3} {4} {5} {6}",
                number, GetShape(), size.Width, size.Height, 0, 0, 0));
            text.Add(string.Format("Dr {0} {1} {2}", drill, 0, 0));
            text.Add(string.Format("At {0} {1} {2}", GetType(), "N", Layer.GetLayerBitmaskString_Legacy(layers)));  // layer mask "00E0FFFF"
            text.Add(string.Format("Ne 0 \"\""));
            text.Add(string.Format("Po {0} {1}", position.At.X, position.At.Y));

            text.Add("$EndPAD");
        }


        // "STD" = "thru_hole"
        // SMD = smd
        // CONN = connect
        // HOLE = np_thru_hole
        private string GetType()
        {
            switch (type)
            {
                default:
                case "thru_hole": return "STD";
                case "smd": return "SMD";
                case "connect": return "CONN";
                case "np_thru_hole": return "HOLE";
            }
        }

        // "C" = circle 
        // "R" = rect 
        // O = oval
        // T = trapezoid
        private string GetShape()
        {
            switch (shape)
            {
                default:
                case "circle": return "C";
                case "rect": return "R";
                case "oval": return "O";
                case "trapezoid": return "T";
            }
        }

        public static pad Parse(SNodeBase node)
        {
            pad result = new pad();

            if ((node is SExpression) && ((node as SExpression).Name == "pad"))
            {
                SExpression expr = node as SExpression;

                int index = 0;

                result.number = (expr.Items[index++] as SNodeAtom).Value;
                result.type = (expr.Items[index++] as SNodeAtom).Value;
                result.shape = (expr.Items[index++] as SNodeAtom).Value;

                //
                while (index < expr.Items.Count)
                {
                    SExpression sub = expr.Items[index] as SExpression;

                    switch (sub.Name)
                    {
                        case "at":
                            result.position = Position.Parse(sub);
                            break;
                        case "size":
                            result.size = sub.GetSizeF();
                            break;
                        case "drill":
                            result.drill = Drill.Parse(sub);
                            break;
                        case "layers":
                            result.layers = Layer.ParseLayers(sub);
                            break;
                        case "net":
                            result.net = Net.Parse(sub);
                            break;
                        case "die_length":
                            result.die_length = sub.GetFloat();
                            break;
                        case "solder_mask_margin":
                            result.solder_mask_margin = sub.GetFloat();
                            break;
                        case "clearance":
                            result.clearance = sub.GetFloat();
                            break;
                        case "solder_paste_margin":
                            result.solder_paste_margin = sub.GetFloat();
                            break;
                        case "solder_paste_ratio":
                            result.solder_paste_ratio = sub.GetInt();
                            break;
                        case "zone_connect":
                            result.zone_connect = sub.GetInt();
                            break;
                        case "thermal_width":
                            result.thermal_width = sub.GetFloat();
                            break;
                        case "thermal_gap":
                            result.thermal_gap = sub.GetFloat();
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
