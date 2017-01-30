using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Drawing;

using SExpressions;
using Kicad_utils.ModuleDef;

namespace Kicad_utils.Pcb
{
    public class Zone
    {
        public int net;         // 0
        public string net_name; // "" = keepout
        public string layer;
        public uint tstamp;

        // hatch
        public ZoneOutlineStyle outline_style;
        public float hatch_pitch;

        public int priority; // aka rank

        public ZonePadConnection connect_pads_mode;
        public float connect_pads_clearance;

        public float min_thickness;

        public bool is_filled;  // has been filled (has filled_polygon field) - default = no
        public ZoneFillMode fill_mode;  // default polygon
        public int fill_arc_segments; // 16 or 32
        public float fill_thermal_gap;
        public float fill_thermal_bridge_width;
        public ZoneCornerSmoothing fill_smoothing;
        public float fill_radius; // corner radius

        public List<PointF> polygon;

        // new ver >4?
        public bool is_keepout;
        public Allowed keepout_allow_tracks;
        public Allowed keepout_allow_vias;
        public Allowed keepout_allow_copper_pour;


        public Zone()
        {
            priority = 0;
            is_filled = false;
            is_keepout = false;
            hatch_pitch = 0.508f;
            fill_radius = 0;
            outline_style = ZoneOutlineStyle.none;
            connect_pads_mode = ZonePadConnection.thermal_relief;

            polygon = new List<PointF>();
        }

        /*
          (zone 
  	        (net 0) 
	        (net_name "") 
	        (layer Top) 
	        (tstamp 71ED150) 
	        (hatch edge|full|none 0.508)
            (priority 2)
            (connect_pads [yes|no|thru_hole_only]
                (clearance 0.508))
            (min_thickness 0.254)
            (fill   [yes ]
                    [(mode segment)]    
                    (arc_segments 16) 
                    (thermal_gap 0.508) 
                    (thermal_bridge_width 0.508))
                    [(smoothing chamfer|fillet)]
                    [(radius 1)]
            (polygon
              (pts
                (xy 154.2211 115.5486) (xy 159.3011 115.5486) (xy 159.3011 113.0086) (xy 154.2211 113.0086)
              )
            )
        */

        public static List<SExpression> GetSExpressionList(List<Zone> Zones)
        {
            List<SExpression> result = new List<SExpression>();
            foreach (Zone zone in Zones)
            {
                result.Add(zone.GetSExpression());
            }
            return result;
        }

        public SExpression GetSExpression()
        {
            SExpression result = new SExpression();
            List<SNodeBase> list;

            result.Name = "zone";
            result.Items = new List<SNodeBase>();
            result.Items.Add(new SExpression("net", net));
            result.Items.Add(new SExpression("net_name", net_name));
            result.Items.Add(new SExpression("layer", layer));
            result.Items.Add(new SExpression("tstamp", tstamp));

            result.Items.Add(new SExpression("hatch", new List<SNodeBase>{
                new SNodeAtom(outline_style.ToString()),
                new SNodeAtom(hatch_pitch)
            }));

            if (priority!=0)
                result.Items.Add(new SExpression("priority", priority));

            list = new List<SNodeBase>();
            if (connect_pads_mode != ZonePadConnection.thermal_relief)
                list.Add(new SNodeAtom(connect_pads_mode.ToString()));
            list.Add(new SExpression("clearance", connect_pads_clearance));

            result.Items.Add(new SExpression("connect_pads", list));
                
            result.Items.Add(new SExpression("min_thickness", min_thickness));

            list = new List<SNodeBase>();
            if (is_filled)
                list.Add(new SNodeAtom("yes"));
            if (fill_mode != ZoneFillMode.polygon)
                list.Add(new SExpression("mode", "segment"));
            if (fill_radius!=0)
                list.Add(new SExpression("radius", fill_radius));
            list.Add(new SExpression("arc_segments", fill_arc_segments));
            list.Add(new SExpression("thermal_gap", fill_thermal_gap));
            list.Add(new SExpression("thermal_bridge_width", fill_thermal_bridge_width));
            result.Items.Add(new SExpression("fill", list));

            list = new List<SNodeBase>();
            list.Add(fp_polygon.GetPointList(polygon));
            result.Items.Add(new SExpression("polygon", list));

            return result;
        }

        public static Zone Parse(SExpression root_node)
        {
            Zone result;

            if ((root_node is SExpression) && ((root_node as SExpression).Name == "zone"))
            {
                result = new Zone();
                int index = 0;

                //
                while (index < root_node.Items.Count)
                {
                    SNodeBase node = root_node.Items[index];
                    SExpression sub = node as SExpression;

                    switch (sub.Name)
                    {
                        case "net": result.net = sub.GetInt();
                            break;
                        case "net_name":
                            result.net_name = sub.GetString();
                            break;
                        case "layer":
                            result.layer = sub.GetString();
                            break;
                        case "tstamp":
                            result.tstamp = sub.GetUintHex();
                            break;
                        case "hatch":
                            {
                                switch (sub.GetString())
                                {
                                    case "none": result.outline_style = ZoneOutlineStyle.none;
                                        break;
                                    case "edge":result.outline_style = ZoneOutlineStyle.edge;
                                        break;
                                    case "full":
                                        result.outline_style = ZoneOutlineStyle.full;
                                        break;
                                }
                                result.hatch_pitch = sub.GetFloat(1);
                            }
                            break;
                        case "connect_pads":
                            //todo
                            break;
                        case "min_thickness":
                            result.min_thickness = sub.GetFloat();
                            break;
                        case "fill":
                            //todo
                            break;
                        case "polygon":
                            //todo
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
