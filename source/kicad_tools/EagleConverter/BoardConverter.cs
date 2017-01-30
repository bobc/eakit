using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Drawing;

using k = Kicad_utils;
using EagleImport;

namespace EagleConverter
{
    public partial class EagleUtils
    {
        EagleFile board;

        int hole_index = 1;

        private k.ModuleDef.Module OnePin(float pad_diam, float drill_diam)
        {
            SizeF text_size = new SizeF(2.0f, 2.0f);

            k.ModuleDef.Module module = new k.ModuleDef.Module();
            module.Name = "@HOLE" + hole_index++;
            module.layer = k.Layer.GetLayerName (k.Layer.nFront_Cu);
            module.At = new PointF(0, 0);
            module.Reference = new k.ModuleDef.fp_text("reference", module.Name, new PointF(0, 2.54f), k.Layer.F_SilkS, new SizeF(1.5f, 1.5f), 0.15f, false);
            module.Value = new k.ModuleDef.fp_text("value", "~", new PointF(0, -2.54f), k.Layer.F_SilkS, new SizeF(1.5f, 1.5f), 0.15f, false);
            module.Pads = new List<k.ModuleDef.pad>();
            module.Pads.Add(new k.ModuleDef.pad("", k.ModuleDef.pad.nonplated_hole,
                        "circle",
                        new PointF(0, 0),
                        new SizeF(pad_diam, pad_diam),
                        drill_diam)
                        );

            return module;
        }

        public float GetTextThickness_mm (Text text)
        {
            return StrToVal_mm(text.Size) * int.Parse(text.Ratio) / 100f;
        }

        public bool ConvertBoardFile (string SourceFilename)
        {
            bool result = false;
            int net_index = 1;
            DesignRules designRules = new DesignRules();

            Trace(string.Format("Reading board file {0}", Path.ChangeExtension(SourceFilename,".brd")));
            board = EagleFile.LoadFromXmlFile(Path.ChangeExtension(SourceFilename,".brd"));

            if (board != null)
            {
                // offset from bottom left
                DrawingOffset = new PointF(2 * inch_to_mm, 2 * inch_to_mm);

                k.Pcb.kicad_pcb k_pcb = new k.Pcb.kicad_pcb();

                k_pcb.Modules = new List<k.ModuleDef.Module>();
                k_pcb.Drawings = new List<k.Pcb.graphic_base>();

                k_pcb.Setup.grid_origin = StrToPoint_Board("0", "0");

                // paper and size
                // first get the page size
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

                    switch (GetAngle(text.Rot))
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

                    k_pcb.AddModule(OnePin(drill, drill), p1);
                }
                #endregion

                #region ==== plain.dimension ====
                foreach (Dimension dim in board.Drawing.Board.Plain.Dimension)
                {
                    //todo:

                }
                #endregion

                #region ==== plain.polygon ====
                foreach (Polygon poly in board.Drawing.Board.Plain.Polygon)
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
                            Trace(string.Format("error : loose via {0},{1} {2}", via.X, via.Y, signal.Name));
                        }
                        
                        k_pcb.Vias.Add(k_via);
                    }

                    foreach (Polygon poly in signal.Polygon)
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
                        bool instance_mirror;
                        int instance_angle = GetAngle2(element.Rot, out instance_mirror);

                        if (!string.IsNullOrEmpty(element.Rot))
                        {
                            if (instance_mirror)
                                k_mod.position.Rotation = (instance_angle + 180) % 360;
                            else
                                k_mod.position.Rotation = instance_angle;

                            if (instance_mirror)
                            {
                                k_mod.Flip(k_mod.position.At); //todo
                            }
                        }

                        //todo: pad connections
                        //attributes for text

                        foreach (k.ModuleDef.pad pad in k_mod.Pads)
                        {
                            string new_name = PartMap.GetNewName(element.Name);

                            if (pad.type != k.ModuleDef.pad.nonplated_hole)
                            {
                                PinConnection contact = Contacts.Find(x => x.PartName == element.Name && x.PinName == pad.number);

                                if (contact == null)
                                    Trace(string.Format("error: contact {0} {1} not found", element.Name, pad.number));
                                else
                                    pad.net = k_pcb.Nets.Find(x => x.Name == contact.NetLabel);
                            }
                        }

                        //
                        k_pcb.Modules.Add(k_mod);
                    }
                }
                #endregion


















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
