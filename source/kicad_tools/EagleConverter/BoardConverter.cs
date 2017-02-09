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
    public class BoardConverter
    {
        const bool pcb_debug = false;

        EagleFile board;

        int hole_index = 1;
        int vpad_index = 1;

        ProjectConverter Parent;
        LibraryConverter libraryConverter;

        SizeF PageSize;
        string PageStr;

        NewStroke strokeFont = new NewStroke();

        PointF DrawingOffset = new PointF(10.16f, 12.7f);

        RenameMap PartMap = new RenameMap();           // All parts from <parts> element, with converted name

        //
        public BoardConverter(ProjectConverter parent)
        {
            Parent = parent;
        }

        //
        void Trace(string s)
        {
            Parent.Trace(s);
        }

        private PointF StrToPoint_Board(string x, string y)
        {
            float grid = 0.100f * 25.4f;
            // PointF result = new PointF(StrToVal_mm(x), -StrToVal_mm(y));

            float height = Common.RoundToGrid(PageSize.Height, grid);

            float y_offset = (PageSize.Height - DrawingOffset.Y);
            y_offset = Common.RoundToGrid(y_offset, grid);

            PointF result = new PointF(Common.StrToVal_mm(x) + DrawingOffset.X,
                Common.StrToVal_mm(y) + DrawingOffset.Y
                );

            //result.Y = PageSize.Height * mm_to_mil - result.Y;
            result.Y = height - result.Y;

            return result;
        }

        public k.LayerDescriptor ConvertLayer(string number)
        {
            return Parent.ConvertLayer(board.Drawing.Layers.Layer, number);
        }

        private void ConvertComponentLibraries(string OutputFolder, bool ExtractLibraries)
        {
            k.Project.FootprintTable footprintTable = new k.Project.FootprintTable();

            footprintTable = new k.Project.FootprintTable();

            foreach (Library lib in board.Drawing.Board.Libraries.Library)
            {
                if (lib.Name == "frames")
                    continue;

                libraryConverter.ConvertLibrary(lib.Name, lib, board.Drawing.Layers.Layer, OutputFolder, ExtractLibraries);

                //todo: check if any footprints were written
                footprintTable.Entries.Add(new Kicad_utils.Project.LibEntry(lib.Name, "KiCad", @"$(KIPRJMOD)\\" + lib.Name + ".pretty", "", ""));

            } // foreach library

            footprintTable.SaveToFile(Path.Combine(OutputFolder, "fp-lib-table"));
        }



        //
        private k.ModuleDef.Module OnePin(float pad_diam, float drill_diam, string name, string hole_type)
        {
            SizeF text_size = new SizeF(0.5f, 0.5f);

            k.ModuleDef.Module module = new k.ModuleDef.Module();
            module.Name = name;
            module.layer = k.LayerList.StandardLayers.GetLayerName (k.Layer.nFront_Cu);
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
            result.Pads[0]._layers.AddLayer("*.Cu");
            result.Pads[0].zone_connect = 2;
            return result;
        }

        private void SetPcbTextAttributes(k.ModuleDef.fp_text field,
            PointF element_pos, ExtRotation element_rot,
            PointF attrib_pos,  ExtRotation attrib_rot)
        {
            float angle;

            if (element_rot.Mirror)
            {
                field.position.At = new PointF(attrib_pos.X - element_pos.X, (attrib_pos.Y - element_pos.Y));
                angle = MathUtil.NormalizeAngle(attrib_rot.Rotation - element_rot.Rotation);
                field.position.At = field.position.At.Rotate(-element_rot.Rotation);

                field.position.At.X = -field.position.At.X;
            }
            else
            {
                field.position.At = new PointF(attrib_pos.X - element_pos.X, (attrib_pos.Y - element_pos.Y));
                angle = MathUtil.NormalizeAngle(attrib_rot.Rotation - element_rot.Rotation);
                field.position.At = field.position.At.Rotate(element_rot.Rotation);

                //SizeF textSize = strokeFont.GetTextSize(field.Value, field.effects);
                // get bottom left
                //PointF pivot = field.position.At + new SizeF (-textSize.Width / 2, textSize.Height / 2);
                //field.position.At = field.position.At.RotateAt (pivot, angle);
            }

            field.position.Rotation = angle;
            AdjustPos(field);
        }


        private void AdjustPos (k.ModuleDef.fp_text field)
        {
            SizeF textSize = strokeFont.GetTextSize(field.Value, field.effects);
            PointF offset = new PointF(textSize.Width / 2,  textSize.Height / 2f);

            switch ((int)field.position.Rotation % 360)
            {
                case 0:
                    field.position.At.X += offset.X;
                    field.position.At.Y -= offset.Y;
                    break;

                case 90:
                    field.position.At.X -= offset.Y;
                    field.position.At.Y -= offset.X;
                    break;

                case 180:
                    field.position.At.X -= offset.X;
                    field.position.At.Y += offset.Y;
                    break;

                case 270:
                    field.position.At.X += offset.Y;
                    field.position.At.Y += offset.X;
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

        public bool ConvertBoardFile (string SourceFilename, string OutputFolder, string ProjectName)
        {
            bool result = false;
            int net_index = 1;
            DesignRules designRules = new DesignRules();
            PartMap = new RenameMap();
            k.LayerDescriptor layer;

            Trace(string.Format("Reading board file {0}", SourceFilename));
            board = EagleFile.LoadFromXmlFile(SourceFilename);

            //
            if (board != null)
            {
                libraryConverter = new LibraryConverter(Parent);
                ConvertComponentLibraries(OutputFolder, false);

                k.Pcb.kicad_pcb k_pcb = new k.Pcb.kicad_pcb();
                k_pcb.Modules = new List<k.ModuleDef.Module>();
                k_pcb.Drawings = new List<k.Pcb.graphic_base>();

                // paper and size: get the page size
                PageStr = "A4";
                PageSize = new SizeF(297, 210);
                foreach (Element element in board.Drawing.Board.Elements.Element)
                {
                    //
                    if (element.Library == "frames")
                    {
                        //todo: 
                        //ConvertFrame(element.Package);
                        break;
                    }
                }
                k_pcb.Page = PageStr;


                // offset from bottom left
                DrawingOffset = new PointF(2 * Common.inch_to_mm, 2 * Common.inch_to_mm);
                k_pcb.Setup.grid_origin = StrToPoint_Board("0", "0");


                //testFont(k_pcb);    // ** debug


                // get list of part names
                foreach (Element element in board.Drawing.Board.Elements.Element)
                {
                    PartMap.Add(element.Name);
                }
                PartMap.Annotate();



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
                    int angle = Common.xGetAngleFlip(text.Rot, out mirror);

                    layer = ConvertLayer(text.Layer);
                    if (layer != null)
                    {
                        k.Pcb.gr_text k_text = new k.Pcb.gr_text(
                        text.mText,
                        StrToPoint_Board(text.X, text.Y),
                        layer.Name,
                        new SizeF(Common.StrToVal_mm(text.Size), Common.StrToVal_mm(text.Size)),
                        Common.GetTextThickness_mm(text),
                        angle
                        );
                        k_text.effects.horiz_align = k.TextJustify.left;

                        SizeF textSize = strokeFont.GetTextSize(text.mText, k_text.effects);
                        PointF offset = new PointF(textSize.Width / 2, textSize.Height / 2);

                        // TODO: spin

                        switch ((int)ExtRotation.Parse(text.Rot).Rotation)
                        {
                            case 0:
                                if (mirror)
                                {
                                    k_text.Position.At.Y -= offset.Y;
                                }
                                else
                                {
                                    k_text.Position.At.Y -= offset.Y;
                                }
                                break;
                            case 90:
                                if (mirror)
                                {
                                    k_text.Position.At.X += offset.Y;
                                    k_text.Position.At.Y -= textSize.Width;
                                }
                                else
                                {
                                    k_text.Position.At.X -= offset.Y;
                                }
                                break;
                            case 180:
                                if (mirror)
                                {
                                    k_text.Position.At.Y += offset.Y;
                                }
                                else
                                {
                                    k_text.Position.At.Y += textSize.Height;
                                }
                                break;
                            case 270:
                                if (mirror)
                                {
                                    k_text.Position.At.X -= offset.Y;
                                    k_text.Position.At.Y += textSize.Width;
                                }
                                else
                                {
                                    k_text.Position.At.X += offset.Y;
                                }
                                break;
                        }

                        k_pcb.Drawings.Add(k_text);
                    }
                }
                #endregion

                #region ==== Plain - lines ====
                foreach (Wire wire in board.Drawing.Board.Plain.Wire)
                {
                    layer = ConvertLayer(wire.Layer);
                    if (layer != null)
                    {
                        float width = Common.StrToVal_mm(wire.Width);

                        //todo: arcs

                        k.Pcb.gr_line k_line = new k.Pcb.gr_line(
                            StrToPoint_Board(wire.X1, wire.Y1), StrToPoint_Board(wire.X2, wire.Y2),
                            layer.Name,
                            width
                            );

                        k_pcb.Drawings.Add(k_line);
                    }
                }
                #endregion

                #region ==== Plain - rectangle ====
                // convert to unconnected zones
                foreach (EagleImport.Rectangle rect in board.Drawing.Board.Plain.Rectangle)
                {
                    layer = ConvertLayer(rect.Layer);
                    if (layer != null)
                    {
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
                }
                #endregion

                #region ==== Plain - Hole ====
                foreach (Hole hole in board.Drawing.Board.Plain.Hole)
                {
                    PointF p1 = StrToPoint_Board(hole.X, hole.Y);
                    float drill = Common.StrToVal_mm(hole.Drill);

                    k_pcb.AddModule(NonplatedHole(drill, drill), p1);
                }
                #endregion

                #region ==== plain.dimension ====
                foreach (Dimension dim in board.Drawing.Board.Plain.Dimension)
                {
                    layer = ConvertLayer(dim.Layer);
                    if (layer != null)
                    {
                        PointF p1 = StrToPoint_Board(dim.X1, dim.Y1);
                        PointF p2 = StrToPoint_Board(dim.X2, dim.Y2);
                        PointF p3 = StrToPoint_Board(dim.X3, dim.Y3);
                        float line_width = 0.15f; // default width? 
                        float text_size = Common.StrToVal_mm(dim.TextSize);
                        float text_width = Common.GetTextThickness_mm(dim.TextSize, dim.TextRatio);

                        if (!string.IsNullOrEmpty(dim.Width))
                            line_width = Common.StrToVal_mm(dim.Width);

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
                }
                #endregion

                #region ==== plain.polygon ====
                foreach (EagleImport.Polygon poly in board.Drawing.Board.Plain.Polygon)
                {
                    //todo

                    // if layer is tRestrict or bRestrict, create a keepout zone
                    if ((poly.Layer == "41") || (poly.Layer == "42"))
                    {
                        k.Pcb.Zone zone = new k.Pcb.Zone();

                        if (poly.Layer == "41")
                            zone.layer = k.LayerList.StandardLayers.GetLayerName(k.Layer.nFront_Cu);
                        else if (poly.Layer == "42")
                            zone.layer = k.LayerList.StandardLayers.GetLayerName(k.Layer.nBack_Cu);

                        zone.net = 0;
                        zone.net_name = "";
                        zone.hatch_pitch = 0.508f;
                        zone.connect_pads_clearance = 0;
                        zone.min_thickness = 10.0f;
                        zone.is_filled = false;
                        zone.fill_arc_segments = 16;
                        zone.connect_pads_mode = k.Pcb.ZonePadConnection.yes; //solid
                        zone.fill_thermal_gap = 0.508f;
                        zone.fill_thermal_bridge_width = 0.508f;

                        zone.is_keepout = true;
                        zone.outline_style = k.Pcb.ZoneOutlineStyle.none;
                        zone.keepout_allow_copper_pour = Kicad_utils.Allowed.not_allowed;

                        zone.priority = 7;

                        foreach (Vertex v in poly.Vertex)
                        {
                            zone.polygon.Add(StrToPoint_Board(v.X, v.Y));
                        }

                        k_pcb.Zones.Add(zone);
                    }
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
                    //todo: ?
                    k.Pcb.Net k_net = k_pcb.Nets.Find(x => x.Name == signal.Name);

                    foreach (Wire wire in signal.Wire)
                    {
                        layer = ConvertLayer(wire.Layer);
                        if (layer != null)
                        {
                            // todo: segment must be on a copper layer?
                            // ignore unrouted
                            if (wire.Layer != "19")
                            {
                                float width = Common.StrToVal_mm(wire.Width);
                                //todo: arcs?

                                k.Pcb.PcbSegment seg = new Kicad_utils.Pcb.PcbSegment();

                                seg.layer = layer.Name;
                                seg.net = k_net.Number;
                                seg.start = StrToPoint_Board(wire.X1, wire.Y1);
                                seg.end = StrToPoint_Board(wire.X2, wire.Y2);
                                seg.width = width;

                                k_pcb.Segments.Add(seg);

                                Contacts.Add(new PinConnection(signal.Name, seg.start, layer.Name, null, null));
                                Contacts.Add(new PinConnection(signal.Name, seg.end, layer.Name, null, null));
                            }
                        }
                    }

                    // contactref
                    foreach (Contactref con_ref in signal.Contactref)
                    {
                        Contacts.Add(new PinConnection(signal.Name, PointF.Empty, null, con_ref.Element, con_ref.Pad));
                    }

                    //<via x="6.6675" y="49.2125" extent="1-16" drill="0.3" shape="octagon"/>
                    foreach (Via via in signal.Via)
                    {
                        float drill = Common.StrToVal_mm(via.Drill);
                        PointF pos = StrToPoint_Board(via.X, via.Y);
                        float size = Common.StrToVal_mm(via.Diameter);

                        if (size == 0)
                            size = designRules.CalcViaSize(drill);

                        k.Pcb.Via k_via = new k.Pcb.Via(pos, size, drill,
                            k.LayerList.StandardLayers.GetLayerName(k.Layer.nFront_Cu),
                            k.LayerList.StandardLayers.GetLayerName(k.Layer.nBack_Cu), 
                            k_net.Number);

                        PinConnection p_conn = Contacts.Find(x => x.position.X == k_via.at.X && x.position.Y == k_via.at.Y);

                        if (via.Extent == "1-16")
                        {
                            k_via.topmost_layer = k.LayerList.StandardLayers.GetLayerName(k.Layer.nFront_Cu);
                            k_via.backmost_layer = k.LayerList.StandardLayers.GetLayerName(k.Layer.nBack_Cu);
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
                        //defaults are 6 mil
                        float width = 0.1524f;
                        float isolate = 0.1524f;
                        float spacing = 0.1524f;
                        int rank = int.Parse(poly.Rank);

                        layer = ConvertLayer(poly.Layer);
                        if (layer != null)
                        {
                            //todo: clearances etc should come from DesignRules?

                            if (!string.IsNullOrEmpty(poly.Width))
                                width = Common.StrToVal_mm(poly.Width);

                            if (!string.IsNullOrEmpty(poly.Isolate))
                                isolate = Common.StrToVal_mm(poly.Isolate);

                            if (!string.IsNullOrEmpty(poly.Spacing))
                                spacing = Common.StrToVal_mm(poly.Spacing);

                            if (k.Layer.IsCopperLayer(layer.Number) || (poly.Layer == "41") || (poly.Layer == "42"))
                            {
                                k.Pcb.Zone zone = new k.Pcb.Zone();

                                if (poly.Layer == "41")
                                    zone.layer = k.LayerList.StandardLayers.GetLayerName(k.Layer.nFront_Cu);
                                else if (poly.Layer == "42")
                                    zone.layer = k.LayerList.StandardLayers.GetLayerName(k.Layer.nBack_Cu);
                                else
                                    zone.layer = layer.Name;

                                zone.net = k_net.Number;
                                zone.net_name = k_net.Name;
                                zone.outline_style = k.Pcb.ZoneOutlineStyle.edge;
                                zone.hatch_pitch = 0.508f;
                                zone.connect_pads_clearance = 0.2032f;
                                zone.min_thickness = width; // ??
                                zone.fill_arc_segments = 32;
                                zone.fill_thermal_gap = 0.2032f;
                                zone.fill_thermal_bridge_width = 0.2032f;
                                zone.is_filled = false;

                                foreach (Vertex v in poly.Vertex)
                                {
                                    zone.polygon.Add(StrToPoint_Board(v.X, v.Y));
                                }

                                if ((poly.Pour == PolygonPour.cutout) ||
                                    !k.Layer.IsCopperLayer(layer.Number))
                                {
                                    zone.is_keepout = true;
                                    zone.outline_style = k.Pcb.ZoneOutlineStyle.none;
                                    zone.keepout_allow_copper_pour = Kicad_utils.Allowed.not_allowed;
                                }

                                if (!string.IsNullOrEmpty(poly.Isolate))
                                {
                                    zone.connect_pads_clearance = isolate;
                                }

                                if (poly.Thermals == Bool.yes)
                                {
                                    zone.connect_pads_mode = k.Pcb.ZonePadConnection.thermal_relief;
                                    zone.fill_thermal_gap = width + 0.001f; // **
                                    zone.fill_thermal_bridge_width = width + 0.001f; // **
                                }
                                else
                                    zone.connect_pads_mode = k.Pcb.ZonePadConnection.yes;

                                // priority on KiCad is opposite to rank
                                zone.priority = 6 - rank;

                                k_pcb.Zones.Add(zone);
                            }
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
                    string footprint_sid = element.Library + ":" + libraryConverter.FootprintNameMap.GetNewName(element.Package);

                    k.ModuleDef.Module k_template = libraryConverter.AllFootprints.Find(x => x.Name == footprint_sid);

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

                        if (k_mod.Value!= null)
                            k_mod.Value.Value = element.Value;

                        k_mod.layer = k.LayerList.StandardLayers.GetLayerName (k.Layer.nFront_Cu);

                        // Set position, orientation
                        ExtRotation elementRot = ExtRotation.Parse (element.Rot);
                        int element_angle = (int)elementRot.Rotation;


                        // get attributes for text

                        foreach (EagleImport.Attribute attrib in element.Attribute)
                        {
                            ExtRotation attrRot = ExtRotation.Parse(attrib.Rot);
                            bool attr_mirror = attrRot.Mirror;
                            int attr_angle = (int)attrRot.Rotation;

                            layer = ConvertLayer(attrib.Layer);
                            if (layer != null)
                            {

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
                                    field.effects.font.Size = new SizeF(Common.StrToVal_mm(attrib.Size), Common.StrToVal_mm(attrib.Size));

                                    field.layer = layer.Name;

                                    field.layer = k.Layer.MakeLayerName(k_mod.layer, field.layer);

                                    //field.effects.horiz_align = k.TextJustify.left;
                                    //field.effects.vert_align = k.VerticalAlign.bottom;

                                    SetPcbTextAttributes(field,
                                        StrToPoint_Board(element.X, element.Y), elementRot,
                                        StrToPoint_Board(attrib.X, attrib.Y), attrRot);

                                    //      AdjustPos(field);

                                    //debug
                                    if (pcb_debug)
                                    {
                                        PointF ptext = new PointF(field.position.At.X, field.position.At.Y);
                                        SizeF textSize = strokeFont.GetTextSize(field.Value, field.effects);

                                        if (elementRot.Mirror)
                                        {
                                            // get bottom right
                                            ptext.X += textSize.Width / 2;
                                            ptext.Y += textSize.Height / 2;

                                            ptext = ptext.Rotate(-elementRot.Rotation - 180);
                                            ptext.Y = -ptext.Y;
                                        }
                                        else
                                        {
                                            // get bottom left
                                            ptext.X -= textSize.Width / 2;
                                            ptext.Y += textSize.Height / 2;
                                            ptext = ptext.Rotate(-elementRot.Rotation);
                                        }


                                        ptext = k_mod.position.At.Add(ptext);

                                        //!DrawRect(k_pcb, ptext, textSize, -(elementRot.Rotation + field.position.Rotation));

                                        // 
                                        PointF p1 = new PointF(field.position.At.X, field.position.At.Y);
                                        k.Pcb.gr_line k_line;
                                        float ds = 1.27f;

                                        if (elementRot.Mirror)
                                        {
                                            p1 = p1.Rotate(-elementRot.Rotation - 180);
                                            p1.Y = -p1.Y;
                                        }
                                        else
                                            p1 = p1.Rotate(-elementRot.Rotation);

                                        //p1 = p1.Rotate(field.position.Rotation);
                                        //p1 = p1.Rotate(k_mod.position.Rotation);


                                        k_line = new k.Pcb.gr_line(
                                            new PointF(k_mod.position.At.X + p1.X - ds, k_mod.position.At.Y + p1.Y),
                                            new PointF(k_mod.position.At.X + p1.X + ds, k_mod.position.At.Y + p1.Y), "Dwgs.User", 0.01f);
                                        k_pcb.Drawings.Add(k_line);

                                        k_line = new k.Pcb.gr_line(
                                            new PointF(k_mod.position.At.X + p1.X, k_mod.position.At.Y + p1.Y - ds),
                                            new PointF(k_mod.position.At.X + p1.X, k_mod.position.At.Y + p1.Y + ds), "Dwgs.User", 0.01f);
                                        k_pcb.Drawings.Add(k_line);
                                    }
                                }
                            }
                        }

                        // Note: the Eagle "mirror" attribute reverses side and flips about Y | axis, but
                        // Kicad "flip" reverses side and flips about X -- axis.
                        // therefore Eagle mirror is equivalent to Kicad flip + rotate(180)

                        if (elementRot.Mirror)
                        {
                            k_mod.RotateBy(MathUtil.NormalizeAngle(-(element_angle + 180)));
                            k_mod.FlipX(k_mod.position.At);
                        }
                        else //if (element_angle != 0)
                        {
                            k_mod.RotateBy(element_angle);
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
            }

            return result;
        }

        //
    }
}
