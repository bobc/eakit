using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Drawing;

using k = Kicad_utils;
using EagleImport;
using Cad2D;

using EagleConverter.Font;

namespace EagleConverter
{
    public partial class EagleUtils
    {
        const bool debug = false;

        EagleFile board;

        int hole_index = 1;
        int vpad_index = 1;

        private k.ModuleDef.Module OnePin(float pad_diam, float drill_diam, string name, string hole_type)
        {
            SizeF text_size = new SizeF(0.5f, 0.5f);

            k.ModuleDef.Module module = new k.ModuleDef.Module();
            module.Name = name;
            module.layer = k.Layer.GetLayerName (k.Layer.nFront_Cu);
            module.At = new PointF(0, 0);
            module.Reference = new k.ModuleDef.fp_text("reference", module.Name, new PointF(0, 0), k.Layer.F_SilkS, text_size, 0.1f, false);
            module.Value = new k.ModuleDef.fp_text("value", "~", new PointF(0, 0), k.Layer.F_Fab, text_size, 0.1f, false);
            module.Pads = new List<k.ModuleDef.pad>();
            module.Pads.Add(new k.ModuleDef.pad("", hole_type,
                        "circle",
                        new PointF(0, 0),
                        new SizeF(pad_diam, pad_diam),
                        drill_diam)
                        );

            return module;
        }

        private k.ModuleDef.Module NonplatedHole (float pad_diam, float drill_diam)
        {
            return OnePin(pad_diam, drill_diam, "@HOLE" + hole_index++, k.ModuleDef.pad.nonplated_hole);
        }

        private k.ModuleDef.Module ViaPad (float pad_diam, float drill_diam, k.Pcb.Net net)
        {
            k.ModuleDef.Module result = OnePin(pad_diam, drill_diam, "@V" + vpad_index++, k.ModuleDef.pad.through_hole);
            result.attr = "virtual";
            result.Pads[0].net = net;
            result.Pads[0].layers = "*.Cu";
            result.Pads[0].zone_connect = 2;
            return result;
        }

        public float GetTextThickness_mm (Text text)
        {
            return StrToVal_mm(text.Size) * int.Parse(text.Ratio) / 100f;
        }

        public float GetTextThickness_mm(string textSize, string ratio)
        {
            return StrToVal_mm(textSize) * int.Parse(ratio) / 100f;
        }

        private void SetPcbTextAttributes(k.ModuleDef.fp_text field,
            PointF element_pos, ExtRotation element_rot,
            PointF attrib_pos,  ExtRotation attrib_rot)
        {
            field.position.At = new PointF (attrib_pos.X - element_pos.X, (attrib_pos.Y - element_pos.Y));

            float angle = MathUtil.NormalizeAngle(attrib_rot.Rotation - element_rot.Rotation);

            field.position.At = field.position.At.Rotate(element_rot.Rotation);

            //field.position.Rotation = angle;
        }


        private void AdjustPos (k.ModuleDef.fp_text field)
        {
            PointF offset = new PointF(field.Value.Length * field.effects.font.Size.Width * 0.8f / 2,
                field.Value.Length * field.effects.font.Size.Height / 2f);

//            field.position.At = new PointF(field.position.At.X + offset.X, field.position.At.Y + offset.Y);

            switch ((int)field.position.Rotation)
            {
                case 0:
                    field.position.At.X += field.Value.Length * field.effects.font.Size.Width * 0.8f / 2;
                    field.position.At.Y -= field.effects.font.Size.Height / 2f;
                    break;
            }
        }

        void Test()
        {
            Font.NewStroke stroke_font = new Font.NewStroke();
            string s;
            SizeF text_size;

            k.TextEffects effects = new k.TextEffects();
            effects.font.Size = new SizeF(1f, 1f);

            s = "H";
            text_size = stroke_font.GetTextSize(s, effects);
            Trace(string.Format("text size {0} = {1}, {2}", s, text_size.Width, text_size.Height));

            s = "Hello";
            text_size = stroke_font.GetTextSize(s, effects);
            Trace(string.Format("text size {0} = {1}, {2}", s, text_size.Width, text_size.Height));
        }

        void DrawRect (k.Pcb.kicad_pcb k_pcb, PointF p1, SizeF size, float angle)
        {
            k.Pcb.gr_line k_line;
            PointF p2 = new PointF(p1.X + size.Width, p1.Y - size.Height);

            p1 = p1.RotateAt(p1, angle);
            p2 = p2.RotateAt(p1, angle);

            k_line = new k.Pcb.gr_line(new PointF(p1.X, p1.Y), new PointF(p2.X, p1.Y), "Dwgs.User", 0.01f);
            k_pcb.Drawings.Add(k_line);
            k_line = new k.Pcb.gr_line(new PointF(p1.X, p2.Y), new PointF(p2.X, p2.Y), "Dwgs.User", 0.01f);
            k_pcb.Drawings.Add(k_line);
            k_line = new k.Pcb.gr_line(new PointF(p1.X, p1.Y), new PointF(p1.X, p2.Y), "Dwgs.User", 0.011f);
            k_pcb.Drawings.Add(k_line);
            k_line = new k.Pcb.gr_line(new PointF(p2.X, p1.Y), new PointF(p2.X, p2.Y), "Dwgs.User", 0.011f);
            k_pcb.Drawings.Add(k_line);
        }

        void testFont (k.Pcb.kicad_pcb k_pcb)
        {
            Font.NewStroke stroke_font = new Font.NewStroke();
            k.TextEffects effects = new k.TextEffects();
            effects.font.Size = new SizeF(2.54f, 2.54f);
            effects.font.thickness = 0.15f;
            stroke_font.DrawText(k_pcb, new PointF(25.4f, 25.4f), "Hello", effects, k.Layer.Drawings);

            SizeF extent = stroke_font.GetTextSize("Hello", effects);

            DrawRect(k_pcb, new PointF(25.4f, 25.4f), extent, 0);
        }

        public bool ConvertBoardFile (string SourceFilename)
        {
            bool result = false;
            int net_index = 1;
            DesignRules designRules = new DesignRules();

            Trace(string.Format("Reading board file {0}", Path.ChangeExtension(SourceFilename,".brd")));
            board = EagleFile.LoadFromXmlFile(Path.ChangeExtension(SourceFilename,".brd"));

            //
            if (board != null)
            {
                // offset from bottom left
                DrawingOffset = new PointF(2 * inch_to_mm, 2 * inch_to_mm);

                k.Pcb.kicad_pcb k_pcb = new k.Pcb.kicad_pcb();

                k_pcb.Modules = new List<k.ModuleDef.Module>();
                k_pcb.Drawings = new List<k.Pcb.graphic_base>();

                k_pcb.Setup.grid_origin = StrToPoint_Board("0", "0");

                //testFont(k_pcb);    // ** debug

                // paper and size: get the page size
                PageStr = "A4";
                PageSize = new SizeF(297, 210);
                foreach (Element element in board.Drawing.Board.Elements.Element)
                {
                    //
                    if (element.Library == "frames")
                    {
                        //ConvertFrame(element.Package);
                        break;
                    }
                }
                k_pcb.Page = PageStr;

                // layers?


                #region ==== designrules ====
                foreach (Param param in board.Drawing.Board.Designrules.Param)
                {
                    designRules.Add(param.Name, param.Value);
                }

                #endregion

                #region ==== Plain - text ====
                foreach (Text text in board.Drawing.Board.Plain.Text)
                {
                    bool mirror;
                    int angle = xGetAngleFlip(text.Rot, out mirror);
                    string layer = ConvertLayer(text.Layer).Name;

                    k.Pcb.gr_text k_text = new k.Pcb.gr_text (
                        text.mText,
                        StrToPoint_Board(text.X, text.Y),
                        layer,
                        new SizeF (StrToVal_mm(text.Size), StrToVal_mm(text.Size)),
                        GetTextThickness_mm (text),
                        angle
                        );
                    k_text.horiz_align = k.TextJustify.left;

                    switch ((int)ExtRotation.Parse (text.Rot).Rotation)
                    {
                        case 0: break;
                        case 90:
                            if (mirror)
                            {
                                k_text.at.X += (int)(k_text.font.Size.Width * 1.5f);
                                k_text.at.Y -= (int)(k_text.Value.Length * k_text.font.Size.Height * 1.5f);
                            }
                            break;
                        case 180:
                            k_text.at.Y += (int)(k_text.font.Size.Height * 1.5f);
                            break;
                        case 270:
                            if (mirror)
                            {
                                //k_text.Pos.X += (int)(k_text.TextSize * 1.5f);
                                k_text.at.Y += (int)(k_text.Value.Length * k_text.font.Size.Height * 1.7f);
                            }
                            else
                                k_text.at.X += (int)(k_text.font.Size.Width * 1.5f);
                            break;
                    }

                    k_pcb.Drawings.Add(k_text);
                }
                #endregion

                #region ==== Plain - lines ====
                foreach (Wire wire in board.Drawing.Board.Plain.Wire)
                {
                    float width = StrToVal_mm(wire.Width);
                    string layer = ConvertLayer(wire.Layer).Name;

                    //todo: arcs

                    k.Pcb.gr_line k_line = new k.Pcb.gr_line (
                        StrToPoint_Board(wire.X1, wire.Y1), StrToPoint_Board(wire.X2, wire.Y2),
                        layer,
                        width
                        );

                    k_pcb.Drawings.Add(k_line);
                }
                #endregion

                #region ==== Plain - rectangle ====
                // convert to unconnected zones
                foreach (EagleImport.Rectangle rect in board.Drawing.Board.Plain.Rectangle)
                {
                    k.LayerDescriptor layer = ConvertLayer(rect.Layer);
                    PointF p1 = StrToPoint_Board(rect.X1, rect.Y1);
                    PointF p2 = StrToPoint_Board(rect.X2, rect.Y2);

                    k.Pcb.Zone zone = new k.Pcb.Zone();
                    zone.layer = layer.Name;
                    zone.net = 0;
                    zone.net_name = "";
                    zone.hatch_pitch = 0.508f;
                    zone.connect_pads_clearance = 0.508f;
                    zone.min_thickness = 0.001f;
                    zone.is_filled = false;
                    zone.fill_arc_segments = 16;
                    zone.fill_thermal_gap = 0.508f;
                    zone.fill_thermal_bridge_width = 0.508f;

                    zone.polygon.Add(new PointF(p1.X, p1.Y));
                    zone.polygon.Add(new PointF(p2.X, p1.Y));
                    zone.polygon.Add(new PointF(p2.X, p2.Y));
                    zone.polygon.Add(new PointF(p1.X, p2.Y));
                    k_pcb.Zones.Add(zone);

                    // todo : not needed?
                    //k.Pcb.gr_line k_line;
                    //k_line = new k.Pcb.gr_line(new PointF(p1.X, p1.Y), new PointF(p2.X, p1.Y), layer.Name, width);
                    //k_pcb.Drawings.Add(k_line);
                    //k_line = new k.Pcb.gr_line(new PointF(p2.X, p1.Y), new PointF(p2.X, p2.Y), layer.Name, width);
                    //k_pcb.Drawings.Add(k_line);
                    //k_line = new k.Pcb.gr_line(new PointF(p1.X, p2.Y), new PointF(p2.X, p2.Y), layer.Name, width);
                    //k_pcb.Drawings.Add(k_line);
                    //k_line = new k.Pcb.gr_line(new PointF(p1.X, p1.Y), new PointF(p1.X, p2.Y), layer.Name, width);
                    //k_pcb.Drawings.Add(k_line);
                }
                #endregion

                #region ==== Plain - Hole ====
                foreach (Hole hole in board.Drawing.Board.Plain.Hole)
                {
                    PointF p1 = StrToPoint_Board(hole.X, hole.Y);
                    float drill = StrToVal_mm(hole.Drill);

                    k_pcb.AddModule(NonplatedHole(drill, drill), p1);
                }
                #endregion

                #region ==== plain.dimension ====
                foreach (Dimension dim in board.Drawing.Board.Plain.Dimension)
                {
                    k.LayerDescriptor layer = ConvertLayer(dim.Layer);
                    PointF p1 = StrToPoint_Board(dim.X1, dim.Y1);
                    PointF p2 = StrToPoint_Board(dim.X2, dim.Y2);
                    PointF p3 = StrToPoint_Board(dim.X3, dim.Y3);
                    float line_width = 0.15f; // default width? 
                    float text_size = StrToVal_mm(dim.TextSize);
                    float text_width = GetTextThickness_mm(dim.TextSize, dim.TextRatio);

                    if (!string.IsNullOrEmpty(dim.Width))
                        line_width = StrToVal_mm(dim.Width);

                    switch (dim.Dtype)
                    {
                        case DimensionType.parallel:
                        case DimensionType.radius:
                        case DimensionType.diameter:
                            k.Pcb.Dimension k_dim = new k.Pcb.Dimension(layer.Name, line_width, p1, p2, text_size, text_width,
                                dim.Unit == GridUnit.mm, int.Parse(dim.Precision), dim.Visible == Bool.yes);
                            k_pcb.Dimensions.Add(k_dim);
                            break;
                        //todo : others?
                    }
                    
                }
                #endregion

                #region ==== plain.polygon ====
                foreach (EagleImport.Polygon poly in board.Drawing.Board.Plain.Polygon)
                {
                    //todo
                }
                #endregion

                #region ==== Signals ====
                // get net list
                foreach (Signal signal in board.Drawing.Board.Signals.Signal)
                {
                    //todo: ?
                    k_pcb.Nets.Add(new k.Pcb.Net(net_index, signal.Name));
                    net_index++;
                }

                List<PinConnection> Contacts = new List<PinConnection>();

                foreach (Signal signal in board.Drawing.Board.Signals.Signal)
                {
                    //todo:
                    k.Pcb.Net k_net = k_pcb.Nets.Find(x => x.Name == signal.Name);

                    foreach (Wire wire in signal.Wire)
                    {
                        float width = StrToVal_mm(wire.Width);
                        string layer = ConvertLayer(wire.Layer).Name;

                        //todo: arcs?

                        k.Pcb.PcbSegment seg = new Kicad_utils.Pcb.PcbSegment();

                        seg.layer = layer;
                        seg.net = k_net.Number;
                        seg.start = StrToPoint_Board(wire.X1, wire.Y1);
                        seg.end = StrToPoint_Board(wire.X2, wire.Y2);
                        seg.width = width;

                        k_pcb.Segments.Add(seg);

                        Contacts.Add(new PinConnection(signal.Name, seg.start, layer, null, null));
                        Contacts.Add(new PinConnection(signal.Name, seg.end, layer, null, null));
                    }

                    // contactref
                    foreach (Contactref con_ref in signal.Contactref)
                    {
                        Contacts.Add(new PinConnection(signal.Name, PointF.Empty, null, con_ref.Element, con_ref.Pad));
                    }

                    //<via x="6.6675" y="49.2125" extent="1-16" drill="0.3" shape="octagon"/>
                    foreach (Via via in signal.Via)
                    {
                        float drill = StrToVal_mm(via.Drill);
                        PointF pos = StrToPoint_Board(via.X, via.Y);
                        float size = StrToVal_mm(via.Diameter);

                        if (size == 0)
                            size = designRules.CalcViaSize(drill);

                        k.Pcb.Via k_via = new k.Pcb.Via(pos, size, drill,
                            k.Layer.GetLayerName(k.Layer.nFront_Cu),
                            k.Layer.GetLayerName(k.Layer.nBack_Cu), 
                            k_net.Number);

                        PinConnection p_conn = Contacts.Find(x => x.position.X == k_via.at.X && x.position.Y == k_via.at.Y);

                        if (via.Extent == "1-16")
                        {
                            k_via.topmost_layer = k.Layer.GetLayerName(k.Layer.nFront_Cu);
                            k_via.backmost_layer = k.Layer.GetLayerName(k.Layer.nBack_Cu);
                        }
                        else
                            Trace(string.Format("error : blind/buried via ? {0},{1} {2} {3}", via.X, via.Y, signal.Name, via.Extent));

                        if (p_conn == null)
                        {
                            Trace(string.Format("note : loose via converted to pad at {0},{1} net={2}", via.X, via.Y, signal.Name));
                            k.ModuleDef.Module k_pad = ViaPad(size, drill, k_net);
                            k_pcb.AddModule (k_pad, pos);
                        }
                        else
                        {
                            k_pcb.Vias.Add(k_via);
                        }
                    }

                    foreach (EagleImport.Polygon poly in signal.Polygon)
                    {
                        //<polygon width="0.2032" layer="1" spacing="0.254" isolate="0.254" rank="2">
                        float width = StrToVal_mm(poly.Width);
                        int rank = int.Parse(poly.Rank);

                        k.LayerDescriptor layer = ConvertLayer(poly.Layer);

                        if (k.Layer.IsCopperLayer(layer.Number))
                        {
                            k.Pcb.Zone zone = new k.Pcb.Zone();
                            zone.layer = layer.Name;
                            zone.net = k_net.Number;
                            zone.net_name = k_net.Name;
                            zone.hatch_pitch = 0.508f;
                            zone.connect_pads_clearance = 0.508f;
                            zone.min_thickness = width;
                            zone.is_filled = false;
                            zone.fill_arc_segments = 16;
                            zone.fill_thermal_gap = 0.508f;
                            zone.fill_thermal_bridge_width = 0.508f;

                            foreach (Vertex v in poly.Vertex)
                            {
                                zone.polygon.Add(StrToPoint_Board(v.X, v.Y));
                            }

                            if (poly.Pour == PolygonPour.cutout)
                            {
                                zone.outline_style = k.Pcb.ZoneOutlineStyle.none;
                                zone.is_keepout = true;
                                zone.keepout_allow_copper_pour = Kicad_utils.Allowed.not_allowed; 
                            }

                            if (!String.IsNullOrEmpty(poly.Spacing))
                            {
                                float spacing = StrToVal_mm(poly.Spacing);
                                zone.outline_style = k.Pcb.ZoneOutlineStyle.edge;
                                zone.hatch_pitch = spacing;
                            }

                            zone.fill_arc_segments = 32;

                            if (!String.IsNullOrEmpty(poly.Isolate))
                            {
                                float isolate = StrToVal_mm(poly.Isolate);
                                zone.connect_pads_clearance = isolate;
                            }

                            if (poly.Thermals == Bool.yes)
                            {
                                zone.connect_pads_mode = k.Pcb.ZonePadConnection.thermal_relief;
                                zone.fill_thermal_gap = width + 0.05f;
                                zone.fill_thermal_bridge_width = width + 0.05f;
                            }
                            else
                                zone.connect_pads_mode = k.Pcb.ZonePadConnection.yes;

                            zone.priority = rank;

                            k_pcb.Zones.Add(zone);
                        }
                    }

                    //
                }
                #endregion

                #region ==== Elements ====
                foreach (Element element in board.Drawing.Board.Elements.Element)
                {
                    //todo:

                    k.ModuleDef.Module k_mod;

                    // find package library : package
                    string footprint_sid = element.Library + ":" + FootprintNameMap.GetNewName(element.Package);

                    k.ModuleDef.Module k_template = AllFootprints.Find(x => x.Name == footprint_sid);

                    if (k_template == null)
                    {
                        Trace(string.Format("error: {0} not found", footprint_sid));
                    }
                    else
                    {
                        k_mod = k_template.Clone(true);
                        k_mod.Name = footprint_sid;
                        k_mod.Reference.Value = PartMap.GetNewName(element.Name);
                        k_mod.At = StrToPoint_Board(element.X, element.Y);

                        if (!string.IsNullOrEmpty(element.Value) && (k_mod.Value!= null))
                            k_mod.Value.Value = element.Value;

                        k_mod.layer = k.Layer.GetLayerName (k.Layer.nFront_Cu);

                        // Set position, orientation
                        ExtRotation elementRot = ExtRotation.Parse (element.Rot);
                        int element_angle = (int)elementRot.Rotation;


                        // todo: attributes for text

                        foreach (EagleImport.Attribute attrib in element.Attribute)
                        {
                            ExtRotation attrRot = ExtRotation.Parse(attrib.Rot);
                            bool attr_mirror = attrRot.Mirror;
                            int attr_angle = (int)attrRot.Rotation;

                            //k.Symbol.SymbolField sym_field = null;
                            k.ModuleDef.fp_text field = null;
                            switch (attrib.Name)
                            {
                                case "NAME":
                                    //sym_field = k_symbol.fReference;
                                    field = k_mod.Reference;
                                    break;
                                case "VALUE":
                                    //sym_field = k_symbol.fValue;
                                    field = k_mod.Value;
                                    break;

                                    // Part?
                                    // voltage, current
                            }

                            if (field != null)
                            {
                                field.effects.font.Size = new SizeF( StrToVal_mm(attrib.Size), StrToVal_mm(attrib.Size));

                                field.layer = ConvertLayer(attrib.Layer).Name;

                                field.layer = k.Layer.MakeLayerName(k_mod.layer, field.layer);

                                //field.effects.horiz_align = k.TextJustify.left;
                                //field.effects.vert_align = k.VerticalAlign.bottom;

                                SetPcbTextAttributes(field,
                                    StrToPoint_Board(element.X, element.Y), elementRot,
                                    StrToPoint_Board(attrib.X, attrib.Y), attrRot);

                                AdjustPos(field);

                                //debug
                                if (debug)
                                {
                                    NewStroke stroke = new NewStroke();
                                    PointF ptext = field.position.At;
                                    SizeF textSize = stroke.GetTextSize(field.Value, field.effects);
                                    ptext.X -= textSize.Width / 2;
                                    ptext.Y += textSize.Height / 2;

                                    ptext = k_mod.position.At.Add(ptext.Rotate(-elementRot.Rotation));

                                    DrawRect(k_pcb, ptext, textSize, -(elementRot.Rotation + field.position.Rotation));

                                    PointF p = field.position.At;
                                    k.Pcb.gr_line k_line;
                                    float ds = 1.27f;
                                    PointF p1 = p.Rotate(-elementRot.Rotation);

                                    k_line = new k.Pcb.gr_line(
                                        new PointF(k_mod.position.At.X + p1.X - ds, k_mod.position.At.Y + p1.Y),
                                        new PointF(k_mod.position.At.X + p1.X + ds, k_mod.position.At.Y + p1.Y), "Dwgs.User", 0.1f);
                                    k_pcb.Drawings.Add(k_line);

                                    k_line = new k.Pcb.gr_line(
                                        new PointF(k_mod.position.At.X + p1.X, k_mod.position.At.Y + p1.Y - ds),
                                        new PointF(k_mod.position.At.X + p1.X, k_mod.position.At.Y + p1.Y + ds), "Dwgs.User", 0.1f);
                                    k_pcb.Drawings.Add(k_line);
                                }
                            }
                        }

                        // Note: the Eagle "mirror" attributes reverse side and flips about Y axis, but
                        // Kicad "flip" reverses side and flips about X axis.
                        // therefore mirror is equivalent to flip + rotate(180)

                        if (!string.IsNullOrEmpty(element.Rot))
                        {
                            if (elementRot.Mirror)
                            {
                                k_mod.RotateBy(MathUtil.NormalizeAngle(-(element_angle + 180)));
                                k_mod.FlipX(k_mod.position.At);
                            }
                            else
                            {
                                k_mod.RotateBy(element_angle);
                            }
                        }


                        // fix up pads
                        foreach (k.ModuleDef.pad pad in k_mod.Pads)
                        {
                            string new_name = PartMap.GetNewName(element.Name);

                            if (pad.type != k.ModuleDef.pad.nonplated_hole)
                            {
                                PinConnection contact = Contacts.Find(x => x.PartName == element.Name && x.PinName == pad.number);

                                if (contact == null)
                                {
                                    // may actually be a non-connect
                                 //   Trace(string.Format("warning: contact {0} {1} not found", element.Name, pad.number));
                                }
                                else
                                    pad.net = k_pcb.Nets.Find(x => x.Name == contact.NetLabel);
                            }
                        }

                        //
                        k_pcb.Modules.Add(k_mod);
                    }
                }
                #endregion














                // transfer some design rules

                k_pcb.Setup.trace_min = designRules.GetValueFloat("msWidth");

                k_pcb.Setup.via_min_size = designRules.GetValueFloat("msWidth");
                k_pcb.Setup.via_min_drill = designRules.GetValueFloat("msDrill");

                k_pcb.Setup.uvia_min_size = designRules.GetValueFloat("msMicroVia");
                k_pcb.Setup.uvia_min_drill = designRules.GetValueFloat("msMicroVia"); // not right, but need layer thickness to calculate correctly

                // allow uvia
                // allow blind/buried via

                // grid

                // text and drawings

                // pad

                // pad mask clearance

                //default netclass
                k_pcb.NetClasses[0].clearance = designRules.GetValueFloat("mdPadVia"); 
                k_pcb.NetClasses[0].trace_width = designRules.GetValueFloat("msWidth");
                k_pcb.NetClasses[0].via_dia = designRules.GetValueFloat("msWidth");
                k_pcb.NetClasses[0].via_drill = designRules.GetValueFloat("msDrill");
                k_pcb.NetClasses[0].uvia_dia = designRules.GetValueFloat("msMicroVia");
                k_pcb.NetClasses[0].uvia_drill = designRules.GetValueFloat("msMicroVia"); // not right

                // write the KiCad file
                string filename = Path.Combine(OutputFolder, ProjectName + ".kicad_pcb");
                Trace(string.Format("Writing board {0}", filename));
                k_pcb.SaveToFile(filename);

                result = true;
            }
            else
            {
                result = false;

                Trace(string.Format("error opening {0}", SourceFilename));
                Trace("");
                Trace("Terminated due to error");
            }

            return result;
        }

        //
    }
}
