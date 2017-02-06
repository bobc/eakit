using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.IO;

using Cad2D;
using EagleImport;
using EagleConverter.Font;
using RMC;
using k = Kicad_utils;

namespace EagleConverter
{
    class LibraryConverter
    {
        ProjectConverter Parent;

        DesignRules designRules = new DesignRules();
        NewStroke strokeFont = new NewStroke(); 

        bool debug = false;

        //Library library;

        public List<string> LibNames = new List<string>();
        public List<Device> AllDevices = new List<Device>();
        public RenameMap FootprintNameMap = new RenameMap();  // Footprint names from AllFootprints with converted name
        public List<k.Symbol.Symbol> AllSymbols = new List<k.Symbol.Symbol>(); // all symbols from all libraries
        public List<k.ModuleDef.Module> AllFootprints = new List<k.ModuleDef.Module>(); // all package/module/footprints from all libraries

        List<Layer> Layers;

        public LibraryConverter(ProjectConverter parent)
        {
            Parent = parent;
            LibNames = new List<string>();
            AllDevices = new List<Device>();
            AllSymbols = new List<k.Symbol.Symbol>();
            AllFootprints = new List<k.ModuleDef.Module>();
            FootprintNameMap = new RenameMap();
        }

        void Trace(string s)
        {
            if (!debug && (s.StartsWith("debug")))
                return;

            Parent.Trace(s);
        }

        private k.LayerDescriptor ConvertLayer(string number, string message)
        {
            //
            return Parent.ConvertLayer (Layers, number, message);
        }

        private void GetSymbol(Library lib, Deviceset devset, k.Symbol.Symbol k_sym)
        {
            bool is_multi_part = false;
            bool interchangeable_units = false;
            string symbol_name = devset.Gates.Gate[0].Symbol;

            if (devset.Gates.Gate.Count == 1)
                is_multi_part = false;
            else
            {
                is_multi_part = true;
                interchangeable_units = true;
                foreach (Gate gate in devset.Gates.Gate)
                    if (symbol_name != gate.Symbol)
                    {
                        interchangeable_units = false;
                        break;
                    }

                k_sym.NumUnits = devset.Gates.Gate.Count;
                if (interchangeable_units)
                    k_sym.Locked = false;
                else
                    k_sym.Locked = true;
            }

            int gate_index = 0;

            k_sym.ShowPinName = true;
            k_sym.ShowPinNumber = true;

            foreach (Gate gate in devset.Gates.Gate)
            {
                Symbol sym = lib.Symbols.Symbol.Find(x => x.Name == gate.Symbol);
                if (sym == null)
                {
                    Trace("error: symbol not found: " + gate.Symbol);
                    return;
                }

                if ( (lib.Name!=null) && lib.Name.StartsWith("supply"))
                {
                    k_sym.PowerSymbol = true;
                    k_sym.Reference = "#PWR";
                    k_sym.fReference.Text.Value = "#PWR";
                    k_sym.fReference.Text.Visible = false;
                }

                int unit = 0;
                //int convert = 1;

                if (is_multi_part && !interchangeable_units)
                {
                    unit = gate_index + 1;
                }

                if ((gate_index == 0) || (is_multi_part && !interchangeable_units))
                {
                    // graphic lines, arcs
                    foreach (Wire wire in sym.Wire)
                    {
                        float curve = (float)StringUtils.StringToDouble(wire.Curve);

                        if (curve == 0)
                        {
                            k_sym.Drawings.Add(new k.Symbol.sym_polygon(unit, Common.StrToInch(wire.Width), k.Symbol.FillTypes.None,
                                new List<PointF>() { Common.StrToPointInch(wire.X1, wire.Y1), Common.StrToPointInch(wire.X2, wire.Y2) }));
                        }
                        else
                        {
                            PointF start = Common.StrToPointInch(wire.X1, wire.Y1);
                            PointF end = Common.StrToPointInch(wire.X2, wire.Y2);
                            float arc_start, arc_end, radius;
                            PointF center = Common.kicad_arc_center(start, end, -curve, out radius, out arc_start, out arc_end);

                            k_sym.Drawings.Add(new k.Symbol.sym_arc(unit, Common.StrToInch(wire.Width),
                                center, radius, arc_start, arc_end,
                                start, end));
                        }
                    }

                    // graphic : circles
                    foreach (Circle circle in sym.Circle)
                    {
                        float width = 1;
                        if (!string.IsNullOrEmpty(circle.Width))
                            width = Math.Max(Common.StrToInch(circle.Width), 1);

                        k_sym.Drawings.Add(new k.Symbol.sym_circle(unit, width, k.Symbol.FillTypes.None, 
                            Common.StrToPointInch(circle.X, circle.Y), 
                            Common.StrToInch(circle.Radius)));
                    }

                    // graphic : rectangles
                    foreach (EagleImport.Rectangle rect in sym.Rectangle)
                    {
                        k_sym.Drawings.Add(new k.Symbol.sym_rectangle(unit, 1f, k.Symbol.FillTypes.PenColor, Common.StrToPointInch(rect.X1, rect.Y1), Common.StrToPointInch(rect.X2, rect.Y2)));
                    }

                    // graphic : texts
                    // check for name, value
                    foreach (Text text in sym.Text)
                    {
                        k.Symbol.sym_text k_text = new k.Symbol.sym_text(unit, text.mText, 0, Common.StrToPointInch(text.X, text.Y), Common.StrToInch(text.Size),
                            false, false, false, k.Symbol.SymbolField.HorizAlign_Left, k.Symbol.SymbolField.VertAlign_Bottom);

                        //ExtRotation extRot = ExtRotation.Parse(text.Rot);
                        k_text.Text.xAngle = Common.GetAngle(text.Rot);

                        k_text.Text.Pos.Rotation = Common.xGetAngleFlip(text.Rot, out k_text.Text.xMirror);

                        SizeF textSize = strokeFont.GetTextSize(text.mText, new Kicad_utils.TextEffects(k_text.Text.FontSize));

                        string t = text.mText.ToUpperInvariant();
                        if (t.Contains(">NAME") || t.Contains(">PART") ||
                            t.Contains(">VALUE"))
                        {
                            k.Symbol.SymbolField sym_text;
                            if (t.Contains(">VALUE"))
                                sym_text = k_sym.fValue;
                            else
                                sym_text = k_sym.fReference;

                            sym_text.Text.Pos = k_text.Text.Pos;
                            sym_text.Text.FontSize = k_text.Text.FontSize;
                            if ((k_text.Text.Pos.Rotation == 0) || (k_text.Text.Pos.Rotation == 180))
                                sym_text.Text.Pos.Rotation = 0;
                            else
                                sym_text.Text.Pos.Rotation = 90;
                        }
                        else
                        {
                            // unless Spin is set?
                            if (k_text.Text.Pos.Rotation == 180)
                            {
                                k_text.Text.Pos.At.X -= textSize.Width;
                                k_text.Text.Pos.At.Y -= textSize.Height;
                                k_text.Text.Pos.Rotation = 0;
                            }

                            //
                            k_sym.Drawings.Add(k_text);
                        }
                    }
                }


                // Pins
                foreach (Pin pin in sym.Pin)
                {
                    k.Symbol.sym_pin k_pin = new k.Symbol.sym_pin(unit, Common.ConvertName(pin.Name), "~", Common.StrToPointInch(pin.X, pin.Y),
                        250,
                        "L",
                        50f, 50f, "P", "", true);
                    switch (pin.Rot)
                    {
                        case "R0": k_pin.Orientation = "R"; break;
                        case "R90": k_pin.Orientation = "U"; break;
                        case "R180": k_pin.Orientation = "L"; break;
                        case "R270": k_pin.Orientation = "D"; break;
                        default:
                            k_pin.Orientation = "R"; break;
                    }
                    switch (pin.Length)
                    {
                        case PinLength.point: k_pin.Length = 0; break;
                        case PinLength.@short: k_pin.Length = 100; break;
                        case PinLength.middle: k_pin.Length = 200; break;
                        case PinLength.@long: k_pin.Length = 300; break;
                        default:
                            k_pin.Length = 300; break;
                    }

                    switch (pin.Visible)
                    {
                        case PinVisible.off:
                            k_pin.SizeName = 0;
                            k_pin.SizeNum = 0;
                            break;
                        case PinVisible.pad:
                            k_pin.SizeName = 0;
                            break;
                        case PinVisible.pin:
                            k_pin.SizeNum = 0;
                            break;
                        case PinVisible.both:
                            break;
                    }

                    switch (pin.Direction)
                    {
                        case PinDirection.nc: k_pin.Type = "N"; break;
                        case PinDirection.@in: k_pin.Type = "I"; break;
                        case PinDirection.@out: k_pin.Type = "O"; break;
                        case PinDirection.io: k_pin.Type = "B"; break;
                        case PinDirection.oc: k_pin.Type = "C"; break;
                        case PinDirection.hiz: k_pin.Type = "T"; break;
                        case PinDirection.pas: k_pin.Type = "P"; break;
                        case PinDirection.pwr: k_pin.Type = "W"; break;
                        case PinDirection.sup: k_pin.Type = "w"; break;
                        default:
                            k_pin.Type = "B";
                            break;
                    }

                    if (k_sym.PowerSymbol)
                    {
                        k_pin.Type = k.Symbol.sym_pin.dir_power_in;
                        k_pin.PinNumber = "~";
                        k_pin.Visible = false;
                        // todo add line...

                        if (k_pin.Length != 0)
                        {
                            PointF p1 = k_pin.Pos;
                            PointF p2 = new PointF(k_pin.Pos.X + k_pin.Length, k_pin.Pos.Y);

                            p2 = PointFExt.Rotate(p2, k_pin.Pos, Common.GetAngle(pin.Rot));

                            k_sym.Drawings.Add(new k.Symbol.sym_polygon(unit, 6,
                                k.Symbol.FillTypes.None,
                                new List<PointF>() { p1, p2 }));
                        }
                    }

                    switch (pin.Function)
                    {
                        case PinFunction.none: k_pin.Shape = ""; break;
                        case PinFunction.dot: k_pin.Shape = "I"; break;
                        case PinFunction.clk: k_pin.Shape = "C"; break;
                        case PinFunction.dotclk: k_pin.Shape = "CI"; break;
                    }

                    //
                    k_sym.Drawings.Add(k_pin);
                }

                gate_index++;
            } // foreach gate
        }



        public bool ConvertLibrary (string LibName, Library lib, List<Layer> layers, string OutputFolder, bool WriteLibFile)
        {
            string lib_filename;

            Trace("Processing Library: " + LibName);

            Layers = layers;

            // Packages
            k.ModuleDef.LibModule k_footprint_lib = new Kicad_utils.ModuleDef.LibModule();
            k_footprint_lib.Name = LibName;

            foreach (Package package in lib.Packages.Package)
            {
                k.ModuleDef.Module k_module = new Kicad_utils.ModuleDef.Module();

                k_module.Name = Common.CleanFootprintName(package.Name);

                FootprintNameMap.Add(package.Name, k_module.Name);
                if (package.Name != k_module.Name)
                    Trace(String.Format("note: {0} is renamed to {1}", package.Name, k_module.Name));

                if (package.Description != null)
                    k_module.description = Common.CleanTags(package.Description.Text);
                k_module.position = new k.Position(0, 0, 0);

                k_module.layer = "F.Cu"; // todo: back ???

                foreach (Wire wire in package.Wire)
                {
                    //
                    float curve = (float)StringUtils.StringToDouble(wire.Curve);
                    if (curve == 0)
                    {
                        k.ModuleDef.fp_line k_line = new Kicad_utils.ModuleDef.fp_line(
                            Common.StrToPointFlip_mm(wire.X1, wire.Y1),
                            Common.StrToPointFlip_mm(wire.X2, wire.Y2),
                            ConvertLayer(wire.Layer, package.Name).Name,
                            Common.StrToVal_mm(wire.Width));
                        k_module.Borders.Add(k_line);
                    }
                    else
                    {
                        PointF start = Common.StrToPointFlip_mm(wire.X1, wire.Y1);
                        PointF end = Common.StrToPointFlip_mm(wire.X2, wire.Y2);
                        float arc_start, arc_end, radius;
                        PointF center = Common.kicad_arc_center(start, end, curve, out radius, out arc_start, out arc_end);

                        k.ModuleDef.fp_arc k_arc = new k.ModuleDef.fp_arc(
                            center, start,
                            -curve,
                            ConvertLayer(wire.Layer, package.Name).Name,
                            Common.StrToVal_mm(wire.Width));

                        k_module.Borders.Add(k_arc);
                    }
                }

                foreach (Smd smd in package.Smd)
                {
                    k.ModuleDef.pad k_pad = new k.ModuleDef.pad(smd.Name, "smd", "rect", Common.StrToPointFlip_mm(smd.X, smd.Y), Common.StrToSize_mm(smd.Dx, smd.Dy), 0);

                    if (Common.GetAngle(smd.Rot) % 180 == 90)
                        k_pad.size = Common.StrToSize_mm(smd.Dy, smd.Dx);

                    if (smd.Stop == Bool.no)
                    {
                        k_pad._layers.RemoveLayer("F.Mask");
                    }
                    if (smd.Cream == Bool.no)
                    {
                        k_pad._layers.RemoveLayer("F.Paste");
                    }

                    k_module.Pads.Add(k_pad);
                }

                foreach (Pad pad in package.Pad)
                {
                    float pad_size = Common.StrToVal_mm(pad.Diameter);
                    if (pad_size == 0)
                        pad_size = designRules.CalcPadSize(Common.StrToVal_mm(pad.Drill));

                    k.ModuleDef.pad k_pad = new k.ModuleDef.pad(pad.Name, "thru_hole", "circle",
                        Common.StrToPointFlip_mm(pad.X, pad.Y), new SizeF(pad_size, pad_size), Common.StrToVal_mm(pad.Drill));

                    if (pad.Stop == Bool.no)
                    {
                        k_pad._layers.RemoveLayer("F.Mask");
                        k_pad._layers.RemoveLayer("B.Mask");
                    }

                    if (pad.Thermals == Bool.no)
                        k_pad.thermal_gap = 0;
                    
                    if (pad.Shape == PadShape.@long)
                    {
                        k_pad.shape = "oval";
                        if (Common.GetAngle(pad.Rot) % 180 == 0)
                            k_pad.size = new SizeF(pad_size * 2, pad_size);
                        else
                            k_pad.size = new SizeF(pad_size, pad_size * 2);
                    }
                    k_module.Pads.Add(k_pad);
                }

                foreach (Text text in package.Text)
                {
                    k.ModuleDef.fp_text k_text = new k.ModuleDef.fp_text("ref", text.mText,
                        Common.StrToPointFlip_mm(text.X, text.Y),
                        ConvertLayer(text.Layer, package.Name).Name,
                        new SizeF(Common.StrToVal_mm(text.Size), Common.StrToVal_mm(text.Size)),
                        Common.GetTextThickness_mm(text),
                        true);

                    ExtRotation rot = ExtRotation.Parse(text.Rot);
                    //k_text.RotateBy(rot.Rotation);
                    k_text.position.Rotation = rot.Rotation;

                    // adjust position for center, center alignment

                    SizeF textSize = strokeFont.GetTextSize(text.mText, k_text.effects);

                    if (rot.Rotation % 180 == 0)
                    {
                        k_text.position.At.X += textSize.Width / 2;
                        k_text.position.At.Y -= textSize.Height / 2;
                    }
                    else
                    {
                        k_text.position.At.X -= textSize.Height / 2;
                        k_text.position.At.Y -= textSize.Width / 2;
                    }


                    //k_text.effects.horiz_align = k.TextJustify.left;

                    if (text.mText.StartsWith(">"))
                    {
                        string t = text.mText.ToUpperInvariant();

                        if (t.Contains("NAME") || t.Contains("PART"))
                        {
                            k_text.Type = "reference";
                            k_module.Reference = k_text;
                        }
                        else if (t.Contains("VALUE"))
                        {
                            k_text.Type = "value";
                            k_module.Value = k_text;
                        }
                        // user field ?
                    }
                    else
                    {
                        k_text.Type = "user";
                        k_module.UserText.Add(k_text);
                    }
                }

                foreach (EagleImport.Rectangle rect in package.Rectangle)
                {
                    RectangleF r = Common.ConvertRect_mm(rect.X1, rect.Y1, rect.X2, rect.Y2, rect.Rot);
                    List<PointF> poly = Common.RectToPoly(r);
                    
                    k.ModuleDef.fp_polygon k_poly = new Kicad_utils.ModuleDef.fp_polygon(
                        poly,
                        ConvertLayer(rect.Layer, package.Name).Name,
                        0.01f   // width?
                        );
                    k_module.Borders.Add(k_poly);
                }

                foreach (Circle circle in package.Circle)
                {
                    k.ModuleDef.fp_circle k_circle = new Kicad_utils.ModuleDef.fp_circle(
                        Common.StrToPointFlip_mm(circle.X, circle.Y),
                        Common.StrToVal_mm(circle.Radius),
                        ConvertLayer(circle.Layer, package.Name).Name,
                        Common.StrToVal_mm(circle.Width)
                        );
                    k_module.Borders.Add(k_circle);
                }

                foreach (Hole hole in package.Hole)
                {
                    k.ModuleDef.pad k_hole = new Kicad_utils.ModuleDef.pad("", "np_thru_hole",
                        "circle",
                        Common.StrToPointFlip_mm(hole.X, hole.Y),
                        new SizeF(Common.StrToVal_mm(hole.Drill), Common.StrToVal_mm(hole.Drill)),
                        Common.StrToVal_mm(hole.Drill)
                        );
                    k_module.Pads.Add(k_hole);
                }

                foreach (EagleImport.Polygon poly in package.Polygon)
                {
                    Cad2D.Polygon poly_2d = new Cad2D.Polygon();

                    foreach (Vertex v in poly.Vertex)
                    {
                        PointF p = Common.StrToPointFlip_mm(v.X, v.Y);
                        poly_2d.AddVertex(p.X, p.Y);
                    }

                    k.ModuleDef.fp_polygon k_poly = new Kicad_utils.ModuleDef.fp_polygon(poly_2d, ConvertLayer(poly.Layer, package.Name).Name, 0.12f);
                    k_module.Borders.Add(k_poly);
                }

                //
                k_footprint_lib.Modules.Add(k_module);

                //
                k.ModuleDef.Module k_generic = k_module.Clone();
                k_generic.Name = LibName + ":" + k_generic.Name;
                AllFootprints.Add(k_generic);
            }

            if (WriteLibFile & (k_footprint_lib.Modules.Count > 0))
            {
                lib_filename = Path.Combine(OutputFolder);
                k_footprint_lib.WriteLibrary(lib_filename);

//!                footprintTable.Entries.Add(new Kicad_utils.Project.LibEntry(LibName, "KiCad", @"$(KIPRJMOD)\\" + k_footprint_lib.Name + ".pretty", "", ""));
            }


            if (lib.Devicesets != null)
            {
                // Symbols
                k.Symbol.LibSymbolLegacy kicad_lib = new k.Symbol.LibSymbolLegacy();
                kicad_lib.Name = LibName;
                kicad_lib.Symbols = new List<k.Symbol.Symbol>();

                foreach (Deviceset devset in lib.Devicesets.Deviceset)
                {
                    string prefix;
                    if (string.IsNullOrEmpty(devset.Prefix))
                        prefix = "U";
                    else
                        prefix = devset.Prefix;

                    Trace(string.Format("debug: {0}", devset.Name));

                    k.Symbol.Symbol k_sym = new k.Symbol.Symbol(devset.Name, true, prefix, 20, true, true, 1, false, false);

                    if (devset.Description != null)
                        k_sym.Description = Common.CleanTags(devset.Description.Text);

                    // prefix placeholder for reference     =  >NAME   or >PART if multi-part?
                    // symbol name is placeholder for value =  >VALUE
                    k_sym.fReference = new k.Symbol.SymbolField(prefix, new PointF(-50, 0), 50, true, "H", "L", "B", false, false);
                    k_sym.fValue = new k.Symbol.SymbolField(k_sym.Name, new PointF(50, 0), 50, true, "H", "L", "B", false, false);

                    k_sym.Drawings = new List<k.Symbol.sym_drawing_base>();
                    k_sym.UserFields = new List<k.Symbol.SymbolField>();

                    //
                    GetSymbol(lib, devset, k_sym);
                    AllSymbols.Add(k_sym);

                    //
                    if ((devset.Devices.Device.Count == 1) && (devset.Devices.Device[0].Package == null))
                    {
                        // symbol only
                        Trace(string.Format("debug: symbol only {0}", devset.Name));
                        kicad_lib.Symbols.Add(k_sym);
                    }
                    else
                    {
                        foreach (Device device in devset.Devices.Device)
                        {
                            // foreach technology

                            string name;
                            if (device.Name == "")
                                name = devset.Name;
                            else
                                name = devset.Name + device.Name;

                            k.Symbol.Symbol k_sym_device = k_sym.Clone();

                            k_sym_device.Name = name;
                            k_sym_device.fValue.Text.Value = name;

                            // place below value
                            PointF pos;
                            if (k_sym_device.fValue.Text.Pos.Rotation == 0)
                                pos = new PointF(k_sym_device.fValue.Text.Pos.At.X, k_sym_device.fValue.Text.Pos.At.Y - 100);
                            else
                                pos = new PointF(k_sym_device.fValue.Text.Pos.At.X + 100, k_sym_device.fValue.Text.Pos.At.Y);

                            k_sym_device.fPcbFootprint = new k.Symbol.SymbolField(kicad_lib.Name + ":" + device.Package,
                                pos,
                                50, true, k_sym_device.fValue.Text.Pos.Rotation == 0 ? "H" : "V",
                                "L", "B", false, false);

                            Trace(string.Format("debug: device {0} {1}", name, k_sym_device.fPcbFootprint.Text.Value));


                            // pin mapping
                            if (device.Connects != null)
                            {
                                foreach (Connect connect in device.Connects.Connect)
                                {
                                    int unit;
                                    if (k_sym_device.NumUnits == 1)
                                        unit = 0;
                                    else
                                    {
                                        unit = 1;
                                        foreach (Gate gate in devset.Gates.Gate)
                                            if (gate.Name == connect.Gate)
                                                break;
                                            else
                                                unit++;
                                    }

                                    k.Symbol.sym_pin k_pin = k_sym_device.FindPin(unit, Common.ConvertName(connect.Pin));
                                    if (k_pin == null)
                                        Trace(string.Format("error: pin not found {0} {1}", k_sym_device.Name, connect.Pin));
                                    else
                                    {
                                        string[] pads;
                                        pads = connect.Pad.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                        int index = 0;
                                        foreach (string s in pads)
                                        {
                                            // check length
                                            if (s.Length > 4)
                                                Trace(string.Format("error: pad name too long {0} {1}", k_sym_device.Name, connect.Pad));

                                            if (index==0)
                                                k_pin.PinNumber = s;
                                            else
                                            {
                                                k.Symbol.sym_pin k_dup_pin = k.Symbol.sym_pin.Clone(k_pin);
                                                k_dup_pin.Visible = false;
                                                k_dup_pin.PinNumber = s;
                                                k_sym_device.Drawings.Add(k_dup_pin);
                                            }
                                            index++;
                                        }
                                    }
                                }
                            }

                            // k_sym_device
                            bool first = true;
                            foreach (Technology tech in device.Technologies.Technology)
                            {
                                if (tech.Name == "")
                                {
                                    if (device.Name == "")
                                        name = devset.Name.Replace("*", "");
                                    else
                                        name = devset.Name.Replace("*", "") + device.Name;

                                    k_sym_device.Name = name;
                                    k_sym_device.fValue.Text.Value = name;

                                    kicad_lib.Symbols.Add(k_sym_device);
                                    AllDevices.Add(new Device(name, device.Package));
                                }
                                else
                                {
                                    if (first)
                                    {
                                        if (device.Name == "")
                                            name = devset.Name.Replace("*", tech.Name);
                                        else
                                            name = devset.Name.Replace("*", tech.Name) + device.Name;

                                        k_sym_device.Name = name;
                                        k_sym_device.fValue.Text.Value = name;

                                        kicad_lib.Symbols.Add(k_sym_device);
                                        AllDevices.Add(new Device(name, device.Package));
                                    }
                                    else
                                    {
                                        // create alias
                                        k_sym_device.Alias.Add(devset.Name.Replace("*", tech.Name) + device.Name); // ?
                                    }
                                }

                                first = false;
                            }

                        }
                    }
                }

                LibNames.Add(kicad_lib.Name);
                //

                if (WriteLibFile)
                {
                    lib_filename = Path.Combine(OutputFolder, kicad_lib.Name + ".lib");
                    kicad_lib.WriteToFile(lib_filename);
                }

            }

            return true;
        }


        public bool ConvertLibraryFile (string SourceFilename, string DestFolder)
        {
            bool result = false;
            EagleFile libraryFile;

            //
            Trace(string.Format("Reading library file {0}", SourceFilename));
            libraryFile = EagleFile.LoadFromXmlFile(SourceFilename);

            string name = Path.GetFileNameWithoutExtension(SourceFilename);

            if (libraryFile != null)
            {
                ConvertLibrary(name, libraryFile.Drawing.Library, libraryFile.Drawing.Layers.Layer, DestFolder, true);

                result = true;
            }
            else
            {
                result = false;
                Trace(string.Format("error opening {0}", SourceFilename));
            }

            return result;
        }

    }
}
