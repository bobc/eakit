using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;

namespace Kicad_utils.Pcb
{
    public class Setup
    {
        public float last_trace_width;
        public float trace_clearance;
        public float zone_clearance;
        public bool zone_45_only; // yes, no
        public float trace_min;
        public float segment_width;
        public float edge_width;
        public float via_size;
        public float via_drill;
        public float via_min_size;
        public float via_min_drill;
        public float uvia_size;
        public float uvia_drill;
        public bool uvias_allowed; // no;
        public float uvia_min_size;
        public float uvia_min_drill;
        public float pcb_text_width;
        public SizeF pcb_text_size;
        public float mod_edge_width;
        public SizeF mod_text_size;
        public float mod_text_width;
        public SizeF pad_size;
        public float pad_drill;
        public float pad_to_mask_clearance;
        public PointF aux_axis_origin;
        public uint visible_elements; // hex bit mask FFFFFFBF;

        public PcbPlotParams pcb_plot_params;

        public Setup()
        {
            last_trace_width = 0.254f;
            trace_clearance = 0.254f;
            zone_clearance = 0.508f;
            zone_45_only = false; //no;
            trace_min = 0.254f;
            segment_width = 0.2f;
            edge_width = 0.15f;
            via_size = 0.889f;
            via_drill = 0.635f;
            via_min_size = 0.889f;
            via_min_drill = 0.508f;
            uvia_size = 0.508f;
            uvia_drill = 0.127f;
            uvias_allowed = false; //no;
            uvia_min_size = 0.508f;
            uvia_min_drill = 0.127f;
            pcb_text_width = 0.3f;
            pcb_text_size = new SizeF(1.5f, 1.5f);
            mod_edge_width = 0.15f;
            mod_text_size = new SizeF(1.5f, 1.5f);
            mod_text_width = 0.15f;
            pad_size = new SizeF(1.524f, 1.524f);
            pad_drill = 0.762f;
            pad_to_mask_clearance = 0.2f;
            aux_axis_origin = new PointF(50, 150);  // depends on page size
            visible_elements = 0xFFFFFFFF;

            pcb_plot_params = new PcbPlotParams();
        }


        public SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "setup";
            result.Items = new List<SNodeBase>();

            result.Items.Add(new SExpression("last_trace_width", last_trace_width));
            result.Items.Add(new SExpression("trace_clearance", trace_clearance));
            result.Items.Add(new SExpression("zone_clearance", zone_clearance));
            result.Items.Add(new SExpression("zone_45_only", YesNo(zone_45_only)));
            result.Items.Add(new SExpression("trace_min", trace_min));
            result.Items.Add(new SExpression("segment_width", segment_width));
            result.Items.Add(new SExpression("edge_width", edge_width));
            result.Items.Add(new SExpression("via_size", via_size));
            result.Items.Add(new SExpression("via_drill", via_drill));
            result.Items.Add(new SExpression("via_min_size", via_min_size));
            result.Items.Add(new SExpression("via_min_drill", via_min_drill));
            result.Items.Add(new SExpression("uvia_size", uvia_size));
            result.Items.Add(new SExpression("uvia_drill", uvia_drill));
            result.Items.Add(new SExpression("uvias_allowed", YesNo(zone_45_only)));
            result.Items.Add(new SExpression("uvia_min_size", uvia_min_size));
            result.Items.Add(new SExpression("uvia_min_drill", uvia_min_drill));
            result.Items.Add(new SExpression("pcb_text_width", pcb_text_width));
            result.Items.Add(new SExpression("pcb_text_size", pcb_text_size));
            result.Items.Add(new SExpression("mod_edge_width", mod_edge_width));
            result.Items.Add(new SExpression("mod_text_size", mod_text_size));
            result.Items.Add(new SExpression("mod_text_width", mod_text_width));
            result.Items.Add(new SExpression("pad_size", pad_size));
            result.Items.Add(new SExpression("pad_drill", pad_drill));
            result.Items.Add(new SExpression("pad_to_mask_clearance", pad_to_mask_clearance));
            result.Items.Add(new SExpression("aux_axis_origin", aux_axis_origin));
            result.Items.Add(new SExpression("visible_elements", visible_elements.ToString("X")));

            result.Items.Add(pcb_plot_params.GetSExpression());

            return result;
        }

        public static string YesNo(bool value)
        {
            if (value)
                return "yes";
            else
                return "no";
        }
    }
}
