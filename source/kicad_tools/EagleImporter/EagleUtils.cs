using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Drawing;

using RMC;
using EagleImport.Schematic;

using k = Kicad_utils;
using Kicad_utils.Schema;

namespace EagleImporter
{
    public class EagleUtils
    {
        const float mm_to_mil = 1000.0f / 25.4f;
        const float inch_to_mm = 25.4f;

        public delegate void TraceHandler(string s);

        public event TraceHandler OnTrace;

        //
        private string ProjectName;
        private EagleSchematic schematic;
        private string OutputFolder;

        private List<string> LibNames = new List<string>();
        private List<Device> AllDevices = new List<Device>();
        private List<k.Symbol.Symbol> AllSymbols = new List<k.Symbol.Symbol>();
        private List<ComponentBase> AllComponents = new List<ComponentBase>();
        k.Project.FootprintTable footprintTable = new k.Project.FootprintTable();
        RenameMap PartMap = new RenameMap();
        RenameMap FootprintNameMap = new RenameMap();

        // A4, landscape
//        float PageWidth = 271.78f;   // = 10.7 inches
//        float PageHeight = 185.42f;  // = 7.3 inches

        SizeF PageSize;
        string PageStr;

        PointF DrawingOffset = new PointF(10.16f, 12.7f);
        //PointF DrawingOffset = new PointF(0, 0);

        private void Trace(string s)
        {
            if (OnTrace != null)
                OnTrace(s);
        }

        // convert strings in mm to inches
        public float StrToInch(string s)
        {
            return (float)(StringUtils.StringToDouble(s) * mm_to_mil);
        }

        public PointF StrToPointInch(string x, string y)
        {
            return new PointF(StrToInch(x), StrToInch(y));
        }

        public SizeF StrToSizeInch(string dx, string dy)
        {
            return new SizeF(StrToInch(dx), StrToInch(dy));
        }

        // For schematic page
        public PointF StrToPointInchFlip(string x, string y)
        {
            float height = (int)(PageSize.Height * mm_to_mil / 25) * 25;

            float y_offset = (PageSize.Height - DrawingOffset.Y * mm_to_mil);
            y_offset = (int)(y_offset / 25) * 25;

            PointF result = new PointF(StrToInch(x) + DrawingOffset.X * mm_to_mil, 
                StrToInch(y) + DrawingOffset.Y * mm_to_mil
                );

            //result.Y = PageSize.Height * mm_to_mil - result.Y;
            result.Y = height - result.Y;

            return result;
        }

        // convert string to mm
        public float StrToVal_mm(string s)
        {
            return (float)(StringUtils.StringToDouble(s));
        }

        public PointF StrToPoint_mm(string x, string y)
        {
            return new PointF(StrToVal_mm(x), StrToVal_mm(y));
        }

        public SizeF StrToSize_mm(string dx, string dy)
        {
            return new SizeF(StrToVal_mm(dx), StrToVal_mm(dy));
        }

        public PointF StrToPointFlip_mm(string x, string y)
        {
            PointF result = new PointF(StrToVal_mm(x), -StrToVal_mm(y));
            return result;
        }

        //


        public void ConvertFrame (string s)
        {
            switch (s)
            {
                case "A0P-LOC":
                    PageStr = "A0";
                    PageSize = new SizeF(841, 1189);
                    break;

                case "A0L-LOC":
                    PageStr = "A0";
                    PageSize = new SizeF(1189, 841);
                    break;

                case "A1P-LOC":
                    PageStr = "A1";
                    PageSize = new SizeF(594, 841);
                    break;

                case "A1L-LOC":
                    PageStr = "A1";
                    PageSize = new SizeF(841, 594);
                    break;

                case "A2P-LOC":
                    PageStr = "A2";
                    PageSize = new SizeF(420, 594);
                    break;

                case "A2L-LOC":
                    PageStr = "A2";
                    PageSize = new SizeF(594, 420);
                    break;

                case "A3L-LOC":
                case "DINA3_L":
                case "FRAMEA3L":
                case "TABL_L":
                case "RAHMEN3Q":
                case "RAHMENA3Q":
                    PageStr = "A3";
                    PageSize = new SizeF(420, 297);
                    break;

                case "A3P-LOC":
                case "DINA3_P":
                case "TABL_P":
                    PageStr = "A3";
                    PageSize = new SizeF(297, 420);
                    break;

                case "A4L-LOC":
                case "DINA4_L":
                    PageStr = "A4";
                    PageSize = new SizeF(297, 210);
                    break;

                case "A4-35SC":
                case "A4-35SCP":
                case "A4P-LOC":
                case "DINA4_P":
                case "DOCSMAL":
                case "A4-SMALL-DOCFIELD":
                    PageStr = "A4";
                    PageSize = new SizeF(210, 297);
                    break;

                case "A5L-LOC":
                case "DINA5_L":
                    PageStr = "User";
                    PageSize = new SizeF(210, 148);
                    break;

                case "A5P-LOC":
                case "DINA5_P":
                    PageStr = "User";
                    PageSize = new SizeF(148, 210);
                    break;

                case "LETTER_P":
                    PageStr = "USLetter";
                    PageSize = new SizeF((float)8.5 * inch_to_mm, (float)(11 * inch_to_mm));
                    break;

                case "LETTER_L":
                    PageStr = "USLetter";
                    PageSize = new SizeF(11 * inch_to_mm, (float)(8.5 * inch_to_mm));
                    break;

                case "DINA-DOC": break; // n/a
                case "DOCFIELD": break; // n/a

                case "FRAME_A_L":
                    PageStr = "A";
                    PageSize = new SizeF(11 * inch_to_mm, (float)(8.5 * inch_to_mm) );
                    break;
                case "FRAME_B_L":
                    PageStr = "B";
                    PageSize = new SizeF(17 * inch_to_mm, (float)(11 * inch_to_mm));
                    break;
                case "FRAME_C_L":
                    PageStr = "C";
                    PageSize = new SizeF(22 * inch_to_mm, (float)(17 * inch_to_mm));
                    break;
                case "FRAME_D_L":
                    PageStr = "D";
                    PageSize = new SizeF(34 * inch_to_mm, (float)(22 * inch_to_mm));
                    break;
                case "FRAME_E_L":
                    PageStr = "E";
                    PageSize = new SizeF(44 * inch_to_mm, (float)(34 * inch_to_mm));
                    break;
            }
        }



        // for pin, label names
        public string FixName(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                //todo: other chars?
                // special chars not at start?
                if (s.StartsWith("!"))
                    return "~" + s.Substring(1);
                else
                    return s;
            }
            return s;
        }

        // Replace illegal filename chracters with underscore (_)
        public string CleanFootprintName(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                //todo: other characters?
                // Replace \ and / with underscore (_)
                s = s.Replace("/", "_");
                s = s.Replace(@"\", "_");
            }
            return s;
        }

        /// <summary>
        /// Remove HTML tags from string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string CleanTags(string orig)
        {
            if (!string.IsNullOrEmpty(orig))
            {
                string s = orig;

                s = s.Replace("<b>", "");
                s = s.Replace("</b>", "");
                s = s.Replace("<B>", "");
                s = s.Replace("</B>", "");

                s = s.Replace("<p>\n", "; ");
                s = s.Replace("<P>\n", "; ");

                s = s.Replace("<p>", "; ");
                s = s.Replace("<P>", "; ");

                s = s.Replace("<br>\n", "; ");
                s = s.Replace("<BR>\n", "; ");

                s = s.Replace("\n", "; ");

                s = s.Trim();
                while ((s.Length != 0) && s.StartsWith(";"))
                {
                    s = s.Substring(1);
                    s = s.Trim();
                }

                return s;
            }
            else
                return orig;
        }

        public static double DegToRad(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static PointF RotatePoint(PointF point, double angleDegree)
        {
            return RotatePoint(point, new PointF(0, 0), angleDegree);
        }

        public static PointF RotatePoint(PointF point, PointF pivot, double angleDegree)
        {
            double angle = DegToRad(angleDegree);
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            double dx = point.X - pivot.X;
            double dy = point.Y - pivot.Y;
            double x = cos * dx - sin * dy + pivot.X;
            double y = sin * dx + cos * dy + pivot.Y;

            PointF rotated = new PointF((float)x, (float)y);
            return rotated;
        }

        public int GetAngle(string rot)
        {
            int result = 0;

            if (!string.IsNullOrEmpty(rot))
            {
                if (rot.StartsWith("M"))
                {
                    rot = rot.Substring(1);
                }
                switch (rot)
                {
                    case "R0":
                        result = 0;
                        break;
                    case "R90":
                        result = 90;
                        break;
                    case "R180":
                        result = 180;
                        break;
                    case "R270":
                        result = 270;
                        break;
                    default:
                        result = 0;
                        break;
                }
            }

            return result;
        }

        public int GetAngle(string rot, out bool mirror)
        {
            int result = GetAngle(rot);

            mirror = false;
            if (!string.IsNullOrEmpty(rot))
            {
                if (rot.StartsWith("M"))
                {
                    mirror = true;
                }
                else
                    mirror = false;
            }

            if (mirror)
                result = (result + 180) % 360;

            return result;
        }

        public float DistanceBetweenPoints(Point a, Point b)
        {
            return (float)Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
        }

        public Point FindSnapPoint(List<Point> points, Point p)
        {
            float min_distance;
            int min_point;

            if (points.Count > 0)
            {
                min_point = 0;
                min_distance = DistanceBetweenPoints(p, points[0]);

                for (int index = 1; index < points.Count; index++)
                {
                    float dist = DistanceBetweenPoints(p, points[index]);
                    if (dist < min_distance)
                    {
                        min_point = index;
                        min_distance = dist;
                    }
                }
                return points[min_point];
            }
            else
                return p;
        }

        public Part FindPart(string PartName)
        {
            foreach (Part p in schematic.Drawing.Schematic.Parts.Part)
            {
                if (p.Name == PartName)
                    return p;
            }
            return null;
        }

        public k.Symbol.Symbol FindSymbol(string Name)
        {
            k.Symbol.Symbol result = AllSymbols.Find(x => x.Name == Name);
            return result;
        }

        public string GetUniqueTimeStamp()
        {
            ulong seconds = Kicad_utils.Utils.GetUnixSeconds(DateTime.Now);
            string tstamp = seconds.ToString("X");

            while (AllComponents.Find(x => x.Timestamp == tstamp) != null)
            {
                seconds++;
                tstamp = seconds.ToString("X");
            }
            return tstamp;
        }

        public string GetUniqueTimeStamp(List<string> Timestamps)
        {
            ulong seconds = Kicad_utils.Utils.GetUnixSeconds(DateTime.Now);
            string tstamp = seconds.ToString("X");

            if (Timestamps == null)
            {
                Timestamps = new List<string>();
            }
            else
            {
                while (Timestamps.IndexOf(tstamp) != -1)
                {
                    seconds++;
                    tstamp = seconds.ToString("X");
                }
            }
            Timestamps.Add(tstamp);
            return tstamp;
        }

        public string GetLayer(string s)
        {
            Layer layer = schematic.Drawing.Layers.Layer.Find(x => x.Number == s);

            if (layer == null)
            {
                Trace("layer not found: " + s);
                return "Cmts.User";
            }

            switch (layer.Name)
            {
                //1
                case "Top":
                case "tCopper":
                    return "F.Cu";              // or Top
                //(16)
                case "Bottom":
                case "bCopper":
                    return "B.Cu";           // or Bottom

                // (20)
                case "Dimension": return "Dwgs.User";   // or edge?

                // (21)
                case "tPlace": return "F.SilkS";
                // (22)
                case "bPlace": return "B.SilkS";

                // (25)
                case "tNames": return "F.SilkS";
                // (26)
                case "bNames": return "B.SilkS";

                // (27)
                case "tValues": return "F.Fab";
                // (28)
                case "bValues": return "B.Fab";

                // 29
                case "tStop": return "F.Mask";
                // 30
                case "bStop": return "B.Mask";

                // 31
                case "tCream": return "F.Paste";
                // 32
                case "bCream": return "B.Paste";

                // (35)
                case "tGlue": return "F.Adhes";
                // (36)
                case "bGlue": return "B.Adhes";

                // (39)
                case "tKeepout": return "F.CrtYd"; 
                // (40)
                case "bKeepout": return "B.CrtYd";

                // (51)
                case "tDocu": return "Dwgs.User";
                // (52)
                case "bDocu": return "Dwgs.User";


                // -> clearance
                // 41
                case "tRestrict": return "Dwgs.User";
                // 42
                case "bRestrict": return "Dwgs.User";
                // 43
                case "vRestrict": return "Dwgs.User";

                case "Milling": return "Dwgs.User"; // edge?
            }

            Trace(layer.Name);
            return "Cmts.User";
        }

        public void GetSymbol(Library lib, string SymbolName, k.Symbol.Symbol k_sym)
        {
            Symbol sym = null;
            foreach (Symbol i_sym in lib.Symbols.Symbol)
            {
                if (i_sym.Name == SymbolName)
                {
                    sym = i_sym;
                    break;
                }
            }

            if (sym == null)
            {
                Trace("symbol not found: " + SymbolName);
                return;
            }

            if (lib.Name.StartsWith("supply"))
            {
                k_sym.PowerSymbol = true;
                k_sym.Reference = "#PWR";
                k_sym.fReference.Text.Value = "#PWR";
                k_sym.fReference.Text.Visible = false;
            }

            foreach (Wire wire in sym.Wire)
            {
                k_sym.Drawings.Add(new k.Symbol.sym_polygon(StrToInch(wire.Width), k.Symbol.FillTypes.None,
                    new List<PointF>() { StrToPointInch(wire.X1, wire.Y1), StrToPointInch(wire.X2, wire.Y2) }));
            }

            foreach (EagleImport.Schematic.Rectangle rect in sym.Rectangle)
            {
                k_sym.Drawings.Add(new k.Symbol.sym_rectangle(1f, k.Symbol.FillTypes.PenColor, StrToPointInch(rect.X1, rect.Y1), StrToPointInch(rect.X2, rect.Y2)));
            }

            foreach (Pin pin in sym.Pin)
            {
                k.Symbol.sym_pin k_pin = new k.Symbol.sym_pin(FixName(pin.Name), pin.Name, StrToPointInch(pin.X, pin.Y),
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
                    case "point": k_pin.Length = 0; break;
                    case "short": k_pin.Length = 100; break;
                    case "middle": k_pin.Length = 200; break;
                    case "long": k_pin.Length = 300; break;
                    default:
                        k_pin.Length = 300; break;
                }
                switch (pin.Visible)
                {
                    case "off":
                        k_sym.ShowPinName = false;
                        k_sym.ShowPinNumber = false;
                        break;
                    case "pad":
                        k_sym.ShowPinName = false;
                        k_sym.ShowPinNumber = true;
                        break;
                    case "pin":
                        k_sym.ShowPinName = true;
                        k_sym.ShowPinNumber = false;
                        break;
                    case "both":
                        k_sym.ShowPinName = true;
                        k_sym.ShowPinNumber = true;
                        break;
                }

                switch (pin.Direction)
                {
                    case "nc": k_pin.Type = "N"; break;
                    case "in": k_pin.Type = "I"; break;
                    case "out": k_pin.Type = "O"; break;
                    case "io": k_pin.Type = "B"; break;
                    case "oc": k_pin.Type = "C"; break;
                    case "hiz": k_pin.Type = "T"; break;
                    case "pas": k_pin.Type = "P"; break;
                    case "pwr": k_pin.Type = "W"; break;
                    case "sup": k_pin.Type = "w"; break;
                    default:
                        k_pin.Type = "B";
                        break;
                }

                switch (pin.Direction)
                {
                    case "none": k_pin.Shape = ""; break;
                    case "dot": k_pin.Shape = "I"; break;
                    case "clk": k_pin.Shape = "C"; break;
                    case "dotclk": k_pin.Shape = "CI"; break;
                }

                //
                k_sym.Drawings.Add(k_pin);
            }

            // check for name, value

            foreach (Text text in sym.Text)
            {
                k.Symbol.sym_text k_text = new k.Symbol.sym_text(text.mText, 0, StrToPointInch(text.X, text.Y), StrToInch(text.Size),
                    false, false, false, k.Symbol.SymbolField.HorizAlign_Left, k.Symbol.SymbolField.VertAlign_Bottom);

                bool mirror;
                k_text.Text.Angle = GetAngle(text.Rot, out mirror);

                if (text.mText.Contains(">NAME") || text.mText.Contains(">VALUE"))
                {
                    k.Symbol.SymbolField sym_text;
                    if (text.mText.Contains(">NAME"))
                        sym_text = k_sym.fReference;
                    else
                        sym_text = k_sym.fValue;

                    sym_text.Text.Pos = k_text.Text.Pos;
                    sym_text.Text.FontSize = k_text.Text.FontSize;
                    if ((k_text.Text.Angle == 0) || (k_text.Text.Angle == 180))
                        sym_text.Text.Angle = 0;
                    else
                        sym_text.Text.Angle = 90;
                }
                else
                {
                    k_sym.Drawings.Add(k_text);
                }
            }
        }

        public void ConvertComponentLibraries()
        {
            string lib_filename;

            footprintTable = new k.Project.FootprintTable();

            foreach (Library lib in schematic.Drawing.Schematic.Libraries.Library)
            {
                if (lib.Name == "frames")
                    continue;

                Trace("Library: " + lib.Name);

                // Packages
                k.ModuleDef.LibModule k_footprint_lib = new Kicad_utils.ModuleDef.LibModule();
                k_footprint_lib.Name = lib.Name;

                foreach (Package package in lib.Packages.Package)
                {
                    k.ModuleDef.Module k_module = new Kicad_utils.ModuleDef.Module();

                    k_module.Name = CleanFootprintName(package.Name);

                    FootprintNameMap.Add(package.Name, k_module.Name);
                    if (package.Name != k_module.Name)
                        Trace(String.Format("{0} => {1}", package.Name, k_module.Name));


                    k_module.description = CleanTags(package.Description);
                    k_module.position = new k.ModuleDef.Position(0, 0, 0);

                    k_module.layer = "F.Cu"; // todo: back ???

                    foreach (Wire wire in package.Wire)
                    {
                        k.ModuleDef.fp_line k_line = new Kicad_utils.ModuleDef.fp_line(
                            StrToPointFlip_mm(wire.X1, wire.Y1),
                            StrToPointFlip_mm(wire.X2, wire.Y2),
                            GetLayer(wire.Layer),
                            StrToVal_mm(wire.Width));
                        k_module.Borders.Add(k_line);
                    }

                    foreach (Smd smd in package.Smd)
                    {
                        k.ModuleDef.pad k_pad = new k.ModuleDef.pad(smd.Name, "smd", "rect", StrToPointFlip_mm(smd.X, smd.Y), StrToSize_mm(smd.Dx, smd.Dy), 0);
                        k_module.Pads.Add(k_pad);

                        string layer = GetLayer(smd.Layer);
                    }

                    foreach (Pad pad in package.Pad)
                    {
                        //TODO:
                        // default pad size is half grid??
                        float pad_size = 1.5f;
                        k.ModuleDef.pad k_pad = new k.ModuleDef.pad(pad.Name, "thru_hole", "circle",
                            StrToPointFlip_mm(pad.X, pad.Y), new SizeF(pad_size, pad_size), StrToVal_mm(pad.Drill));

                        if (pad.Shape == "long")
                        {
                            k_pad.shape = "oval";
                            if (GetAngle(pad.Rot) % 180 == 0)
                                k_pad.size = new SizeF(pad_size * 2, pad_size);
                            else
                                k_pad.size = new SizeF(pad_size, pad_size * 2);
                        }
                        k_module.Pads.Add(k_pad);
                    }

                    foreach (Text text in package.Text)
                    {
                        k.ModuleDef.fp_text k_text = new k.ModuleDef.fp_text("ref", text.mText,
                            StrToPointFlip_mm(text.X, text.Y),
                            GetLayer(text.Layer),
                            new SizeF(StrToVal_mm(text.Size), StrToVal_mm(text.Size)),
                            0.12f,
                            true);

                        if (text.mText.Contains("NAME"))
                        {
                            k_text.Type = "reference";
                            k_module.Reference = k_text;
                        }
                        else if (text.mText.Contains("VALUE"))
                        {
                            k_text.Type = "value";
                            k_module.Value = k_text;
                        }
                        else
                        {
                            k_text.Type = "user";
                            k_module.UserText.Add(k_text);
                        }
                    }

                    foreach (EagleImport.Schematic.Rectangle rect in package.Rectangle)
                    {
                        k.ModuleDef.fp_polygon k_poly = new Kicad_utils.ModuleDef.fp_polygon(
                            StrToPointFlip_mm(rect.X1, rect.Y1), StrToPointFlip_mm(rect.X2, rect.Y2),
                            GetLayer(rect.Layer),
                            0.12f
                            );
                        k_module.Borders.Add(k_poly);
                    }

                    foreach (Circle circle in package.Circle)
                    {
                        k.ModuleDef.fp_circle k_circle = new Kicad_utils.ModuleDef.fp_circle(
                            StrToPointFlip_mm(circle.X, circle.Y),
                            StrToVal_mm(circle.Radius),
                            GetLayer(circle.Layer),
                            StrToVal_mm(circle.Width)
                            );
                        k_module.Borders.Add(k_circle);
                    }

                    foreach (Hole hole in package.Hole)
                    {
                        k.ModuleDef.pad k_hole = new Kicad_utils.ModuleDef.pad("", "np_thru_hole",
                            "circle",
                            StrToPointFlip_mm(hole.X, hole.Y),
                            new SizeF(StrToVal_mm(hole.Drill), StrToVal_mm(hole.Drill)),
                            StrToVal_mm(hole.Drill),
                            ""
                            );
                        k_module.Pads.Add(k_hole);
                    }

                    foreach (EagleImport.Schematic.Polygon poly in package.Polygon)
                    {
                        Cad2D.Polygon poly_2d = new Cad2D.Polygon();

                        foreach (Vertex v in poly.Vertex)
                        {
                            PointF p = StrToPointFlip_mm(v.X, v.Y);
                            poly_2d.AddVertex(p.X, p.Y);
                        }

                        k.ModuleDef.fp_polygon k_poly = new Kicad_utils.ModuleDef.fp_polygon(poly_2d, GetLayer(poly.Layer), 0.12f);
                        k_module.Borders.Add(k_poly);
                    }

                    //
                    k_footprint_lib.Modules.Add(k_module);
                }

                if (k_footprint_lib.Modules.Count > 0)
                {
                    lib_filename = Path.Combine(OutputFolder);
                    k_footprint_lib.WriteLibrary(lib_filename);

                    footprintTable.Entries.Add(new Kicad_utils.Project.LibEntry(lib.Name, "KiCad", @"$(KIPRJMOD)\\" + k_footprint_lib.Name + ".pretty", "", ""));
                }


                // Symbols
                k.Symbol.LibSymbolLegacy kicad_lib = new k.Symbol.LibSymbolLegacy();
                kicad_lib.Name = lib.Name;
                kicad_lib.Symbols = new List<k.Symbol.Symbol>();

                foreach (Deviceset devset in lib.Devicesets.Deviceset)
                {
                    string prefix;
                    if (string.IsNullOrEmpty(devset.Prefix))
                        prefix = "U";
                    else
                        prefix = devset.Prefix;

                    k.Symbol.Symbol k_sym = new k.Symbol.Symbol(devset.Name, true, prefix, 20, true, true, 1, false, false);

                    k_sym.Description = CleanTags(devset.Description);

                    // prefix placeholder for reference     =  >NAME   or >PART if multi-part?
                    // symbol name is placeholder for value =  >VALUE
                    k_sym.fReference = new k.Symbol.SymbolField(prefix, new PointF(-50, 0), 50, true, "H", "L", "B", false, false);
                    k_sym.fValue = new k.Symbol.SymbolField(k_sym.Name, new PointF(50, 0), 50, true, "H", "L", "B", false, false);

                    k_sym.Drawings = new List<k.Symbol.sym_drawing_base>();
                    k_sym.UserFields = new List<k.Symbol.SymbolField>();

                    string symbol_name = devset.Gates.Gate[0].Symbol;
                    GetSymbol(lib, symbol_name, k_sym);

                    AllSymbols.Add(k_sym);

                    if ((devset.Devices.Device.Count == 1) && (devset.Devices.Device[0].Package == null))
                    {
                        //symbol only
                        kicad_lib.Symbols.Add(k_sym);
                    }
                    else
                    {
                        foreach (Device device in devset.Devices.Device)
                        {
                            string name;
                            if (device.Name == "")
                                name = devset.Name;
                            else
                                name = devset.Name + device.Name;

                            k.Symbol.Symbol k_sym_device = k_sym.Clone();
                            k_sym_device.Name = name;
                            k_sym_device.fValue.Text.Value = name;

                            // place next to below value
                            PointF pos;
                            if (k_sym_device.fValue.Text.Angle == 0)
                                pos = new PointF(k_sym_device.fValue.Text.Pos.X, k_sym_device.fValue.Text.Pos.Y - 50);
                            else
                                pos = new PointF(k_sym_device.fValue.Text.Pos.X + 50, k_sym_device.fValue.Text.Pos.Y);

                            k_sym_device.fPcbFootprint = new k.Symbol.SymbolField(kicad_lib.Name + ":" + device.Package,
                                pos,
                                50, true, k_sym_device.fValue.Text.Angle == 0 ? "H" : "V",
                                "L", "B", false, false);

                            //todo : pin mapping

                            kicad_lib.Symbols.Add(k_sym_device);

                            AllDevices.Add(new Device(name, device.Package));
                        }
                    }
                }

                LibNames.Add(kicad_lib.Name);
                //
                lib_filename = Path.Combine(OutputFolder, kicad_lib.Name + ".lib");
                kicad_lib.WriteToFile(lib_filename);

            } // foreach library

            footprintTable.SaveToFile(Path.Combine(OutputFolder, "fp-lib-table"));
        }

        public void WriteProjectFile()
        {
            k.Project.KicadProject k_project = new k.Project.KicadProject();

            k.Project.Section k_section = new k.Project.Section();
            k_project.Sections.Add(k_section);
            k_section.AddItem("update", k.Project.KicadProject.FormatDateTime(DateTime.Now));
            k_section.AddItem("version", 1);
            k_section.AddItem("last_client", "kicad");

            k_section = new k.Project.Section("general");
            k_project.Sections.Add(k_section);
            k_section.AddItem("version", 1);
            k_section.AddItem("RootSch", "");
            k_section.AddItem("BoardNm", "");

            k_section = new k.Project.Section("pcbnew");
            k_project.Sections.Add(k_section);
            k_section.AddItem("version", 1);
            k_section.AddItem("LastNetListRead", "");
            k_section.AddItem("UseCmpFile", 1);
            k_section.AddItem("PadDrill", 0.6f);
            k_section.AddItem("PadDrillOvalY", 0.6f);
            k_section.AddItem("PadSizeH", 1.5f);
            k_section.AddItem("PadSizeV", 1.5f);
            k_section.AddItem("PcbTextSizeV", 1.5f);
            k_section.AddItem("PcbTextSizeH", 1.5f);
            k_section.AddItem("PcbTextThickness", 0.3f);
            k_section.AddItem("ModuleTextSizeV", 1.0f);
            k_section.AddItem("ModuleTextSizeH", 1.0f);
            k_section.AddItem("ModuleTextSizeThickness", 0.15f);
            k_section.AddItem("SolderMaskClearance", 0.0f);
            k_section.AddItem("SolderMaskMinWidth", 0.0f);
            k_section.AddItem("DrawSegmentWidth", 0.2f);
            k_section.AddItem("BoardOutlineThickness", 0.1f);
            k_section.AddItem("ModuleOutlineThickness", 0.15f);

            k_section = new k.Project.Section("cvpcb");
            k_project.Sections.Add(k_section);
            k_section.AddItem("version", 1);
            k_section.AddItem("NetIExt", "net");

            k_section = new k.Project.Section("eeschema");
            k_project.Sections.Add(k_section);
            k_section.AddItem("version", 1);
            k_section.AddItem("LibDir", "");

            k_section = new k.Project.Section("eeschema/libraries");
            k_project.Sections.Add(k_section);
            int index = 1;
            foreach (string lib in LibNames)
            {
                k_section.AddItem("LibName" + index, lib);
                index++;
            }

            //
            /*
            [schematic_editor]
            version=1
            PageLayoutDescrFile=custom.kicad_wks
            PlotDirectoryName=
            SubpartIdSeparator=0
            SubpartFirstId=65
            NetFmtName=
            SpiceForceRefPrefix=0
            SpiceUseNetNumbers=0
            LabSize=60
            */


            //
            k_project.SaveToFile(Path.Combine(OutputFolder, ProjectName));
        }

        private void ConvertSheet(k.Schema.SchematicLegacy k_schematic, int EagleSheetNumber, string DestName, bool IsMain, int SheetNumber, int NumSheets)
        {
            Trace(string.Format("Sheet: {0}", EagleSheetNumber + 1));

            Sheet source_sheet = schematic.Drawing.Schematic.Sheets.Sheet[EagleSheetNumber];
            SheetLegacy k_sheet = new SheetLegacy();
            k_sheet.Filename = DestName;
            k_sheet.LibNames = LibNames;
            k_sheet.SheetNumber = SheetNumber;
            k_sheet.SheetCount = NumSheets;

            k_schematic.Sheets.Add(k_sheet);

            if (IsMain)
                k_schematic.MainSheet = k_sheet;

            // first get the page size
            foreach (Instance instance in source_sheet.Instances.Instance)
            {
                // find part -> 
                Part part = FindPart(instance.Part);
                if (part == null)
                {
                    continue;
                }

                //
                if (part.Library == "frames")
                {
                    PageStr = "A4";
                    PageSize = new SizeF(297, 210);

                    ConvertFrame(part.Deviceset);

                    k_sheet.PaperName = PageStr;
                    k_sheet.PageSize = new SizeF(PageSize.Width * mm_to_mil, PageSize.Height * mm_to_mil);
                    break;
                }
            }
            
            // text items
            foreach (Text text in source_sheet.Plain.Text)
            {
                bool mirror;

                k.Schema.sch_text k_text = sch_text.CreateNote(
                    text.mText,
                    StrToPointInchFlip(text.X, text.Y),
                    StrToInch(text.Size),
                    GetAngle(text.Rot, out mirror),
                    false, false);

                k_sheet.Items.Add(k_text);
            }

            // components
            foreach (Instance instance in source_sheet.Instances.Instance)
            {
                // find part -> 
                Part part = FindPart(instance.Part);

                if (part == null)
                {
                    Trace("Part not found: " + instance.Part);
                    continue;
                }

                if (part.Library == "frames")
                    continue;

                k.Symbol.Symbol k_symbol = FindSymbol(part.Deviceset);

                LegacyComponent k_comp = new LegacyComponent();

                k_comp.Timestamp = GetUniqueTimeStamp();

                // need to flip Y coord
                k_comp.Position = StrToPointInchFlip(instance.X, instance.Y);
                k_comp.Symbol = new PartSpecifier(part.Deviceset + part.Device);

                // set Reference field
                if (!k_symbol.PowerSymbol)
                {
                    k_comp.Reference = PartMap.GetNewName(instance.Part);
                    if (instance.Part != k_comp.Reference)
                        Trace(String.Format("{0} => {1}", instance.Part, k_comp.Reference));
                }
                else
                {
                    k_comp.Reference = k_symbol.Reference;
                    k_comp.fReference.Hidden = !k_symbol.fReference.Text.Visible;
                    k_comp.fReference.Size = (int)k_symbol.fReference.Text.FontSize;
                }

                k_comp.fReference.Pos = new PointF(k_comp.Position.X, k_comp.Position.Y);
                k_comp.fReference.HorizJustify = "L";
                k_comp.fReference.VertJustify = "B";

                // Set Value field
                if (!string.IsNullOrEmpty(part.Value))
                    k_comp.Value = part.Value;
                else
                    k_comp.Value = k_symbol.fValue.Text.Value;

                k_comp.fValue.Pos = new PointF(k_comp.Position.X, k_comp.Position.Y);
                k_comp.fValue.HorizJustify = "L";
                k_comp.fValue.VertJustify = "B";

                // Set Footprint field
                Device device = AllDevices.Find(x => x.Name == part.Deviceset + part.Device);
                if (device != null)
                    k_comp.Footprint = part.Library + ":" + device.Package;
                k_comp.fPcbFootprint.Pos = new PointF(k_comp.Position.X, k_comp.Position.Y);
                k_comp.fPcbFootprint.Hidden = true;

                // User doc field (not used)
                k_comp.fUserDocLink.Pos = new PointF(k_comp.Position.X, k_comp.Position.Y);

                // Set position, orientation
                if (!string.IsNullOrEmpty(instance.Rot))
                {
                    k_comp.Rotation = GetAngle(instance.Rot, out k_comp.Mirror);
                }

                foreach (EagleImport.Schematic.Attribute attrib in instance.Attribute)
                {
                    bool mirror;
                    int angle = GetAngle(attrib.Rot, out mirror) + k_comp.Rotation;
                    //int angle = GetAngle(attrib.Rot);
                    angle %= 360;
                    string orientation = (angle == 0) || (angle == 180) ? "H" : "V";

                    LegacyField field = null;
                    switch (attrib.Name)
                    {
                        case "NAME":
                            field = k_comp.fReference;
                            field.Pos = new PointF(k_comp.Position.X + k_symbol.fReference.Text.Pos.X, k_comp.Position.Y + k_symbol.fReference.Text.Pos.Y);
                            break;
                        case "VALUE":
                            field = k_comp.fValue;
                            field.Pos = new PointF(k_comp.Position.X + k_symbol.fValue.Text.Pos.X, k_comp.Position.Y + k_symbol.fValue.Text.Pos.Y);
                            break;

                            //Part?
                            // voltage, current
                    }

                    if (field != null)
                    {
                        //field.Pos = StrToPointFlip(attrib.X, attrib.Y);

                        field.Size = (int)StrToInch(attrib.Size);
                        //!field.Orientation = orientation;

                        //PointF offset = new PointF(field.Pos.X - k_comp.Position.X, field.Pos.Y - k_comp.Position.Y );
                        //offset = RotatePoint(offset, angle);
                        //field.Pos = RotatePoint(field.Pos, k_comp.Position, k_comp.Rotation);

                        //switch (angle)
                        //{
                        //    case 0:
                        //        field.HorizJustify = "R";
                        //        field.VertJustify = "B";
                        //        break;
                        //    case 180:
                        //        field.HorizJustify = "L";
                        //        field.VertJustify = "T";
                        //        break;
                        //    case 90:
                        //        field.HorizJustify = "L";
                        //        field.VertJustify = "B";
                        //        break;
                        //    case 270:
                        //        field.HorizJustify = "L";
                        //        field.VertJustify = "B";
                        //        break;
                        //}

                        // show text anchor points
                        //PointF p = StrToPointFlip(attrib.X, attrib.Y);
                        //k.Schema.sch_wire k_line = sch_wire.CreateLine(p, new PointF(p.X + 25, p.Y));
                        //k_sheet.Items.Add(k_line);
                        //k_line = sch_wire.CreateLine(p, new PointF(p.X, p.Y+25));
                        //k_sheet.Items.Add(k_line);
                    }
                }

                //
                AllComponents.Add(k_comp);

                k_sheet.Components.Add(k_comp);
            }

            // look for wires, junctions, labels
            foreach (Net net in source_sheet.Nets.Net)
            {
                foreach (Segment segment in net.Segment)
                {
                    List<Point> snap_points = new List<Point>();

                    foreach (Wire wire in segment.Wire)
                    {
                        sch_wire k_wire = sch_wire.CreateWire(StrToPointInchFlip(wire.X1, wire.Y1), StrToPointInchFlip(wire.X2, wire.Y2));
                        k_sheet.Items.Add(k_wire);

                        snap_points.Add(k_wire.start);
                        snap_points.Add(k_wire.end);
                    }

                    foreach (Junction junction in segment.Junction)
                    {
                        sch_Junction k_junction = new sch_Junction(StrToPointInchFlip(junction.X, junction.Y));
                        k_sheet.Items.Add(k_junction);

                        snap_points.Add(k_junction.Pos);
                    }

                    //todo: add gate positions to snap_points

                    foreach (Label label in segment.Label)
                    {
                        bool mirror;
                        sch_text k_text = sch_text.CreateLocalLabel(net.Name,
                            StrToPointInchFlip(label.X, label.Y),
                            StrToInch(label.Size),
                            GetAngle(label.Rot, out mirror), false, false);

                        // find nearest point
                        k_text.Pos = FindSnapPoint(snap_points, k_text.Pos);

                        k_sheet.Items.Add(k_text);
                    }
                }
            }

            // !

        }

        private void CreateMainSheet(SchematicLegacy k_schematic, int NumSheets)
        {
            List<string> Timestamps = new List<string>() ;

            SheetLegacy k_sheet = new SheetLegacy();
            k_sheet.Filename = ProjectName;
            k_sheet.LibNames = LibNames;
            k_sheet.SheetNumber = 1;
            k_sheet.SheetCount = NumSheets;

            k_sheet.SubSheets = new List<SheetSpecLegacy>();

            PointF cur_pos = new PointF(1000, 1000);
            for (int sheet_number = 0; sheet_number < schematic.Drawing.Schematic.Sheets.Sheet.Count; sheet_number++)
            {
                k.Schema.SheetSpecLegacy sheet_spec = new SheetSpecLegacy();

                sheet_spec.Name = new LegacyField ("sheet" + (sheet_number + 1).ToString(), 50);
                sheet_spec.Filename = new LegacyField(sheet_spec.Name.Value+".sch", 50);
                sheet_spec.Position = cur_pos;
                sheet_spec.Size = new PointF (1600, 1000);
                sheet_spec.Timestamp = GetUniqueTimeStamp(Timestamps);

                k_sheet.SubSheets.Add(sheet_spec);

                cur_pos.X += 2000;
                if (cur_pos.X + 1600 > k_sheet.PageSize.Width * mm_to_mil)
                {
                    cur_pos.X = 1000;
                    cur_pos.Y += 1500;
                }
            }
            k_schematic.Sheets.Add(k_sheet);
            k_schematic.MainSheet = k_sheet;
        }

        public bool ImportFromEagle(string SourceFilename, string DestFolder)
        {
            schematic = EagleSchematic.LoadFromXmlFile(SourceFilename);
            ProjectName = Path.GetFileNameWithoutExtension(SourceFilename);

            OutputFolder = DestFolder;

            if (schematic != null)
            {
                ConvertComponentLibraries();

                //
                foreach (Part part in schematic.Drawing.Schematic.Parts.Part)
                {
                    k.Symbol.Symbol k_symbol = FindSymbol(part.Deviceset);

                    if ( (k_symbol != null) && !k_symbol.PowerSymbol)
                        PartMap.Add(part.Name);
                }
                PartMap.Annotate();

                //
                k.Schema.SchematicLegacy k_schematic = new SchematicLegacy();

                if (schematic.Drawing.Schematic.Sheets.Sheet.Count == 1)
                {
                    // single sheet is also top level sheet
                    ConvertSheet(k_schematic, 0, ProjectName, true, 1, 1);
                }
                else
                {
                    // create top level
                    CreateMainSheet(k_schematic, schematic.Drawing.Schematic.Sheets.Sheet.Count+1);

                    for (int sheet_number = 0; sheet_number < schematic.Drawing.Schematic.Sheets.Sheet.Count; sheet_number++)
                    {
                        ConvertSheet(k_schematic, sheet_number, "sheet"+(sheet_number+1).ToString(), false, sheet_number + 2, schematic.Drawing.Schematic.Sheets.Sheet.Count+1);
                    }
                }

                string filename = Path.Combine(OutputFolder, ProjectName + ".sch");
                k_schematic.SaveToFile(filename);


                // todo: pcb

                WriteProjectFile();

                //string fmt = "{0,-8} {1,-12} {2,-12} {3,-12} {4}";
                //Trace(string.Format(fmt, "Name", "Lib", "DevSet", "dev", "value"));
                //foreach (Part part in schematic.Drawing.Schematic.Parts.Part)
                //{
                //    Trace(string.Format(fmt, part.Name, 
                //        part.Library,
                //        part.Deviceset,
                //        part.Device,
                //        part.Value==null?"" : part.Value));
                //}

                return true;
            }
            else
                return false;
        }
    }


    public class PrefixItem
    {
        public string Prefix; // eg. "R"
        public int Index;

        public PrefixItem() { }

        public PrefixItem(string p)
        {
            Prefix = p;
            Index = 1;
        }
    }

    public class RenamedItem
    {
        public string Original; // eg. "LCD"
        public string NewName;     // e.g. "LCD1"

        public RenamedItem() { }

        public RenamedItem(string orig, string newName)
        {
            Original = orig;
            NewName = newName;
        }
    }

    public class RenameMap
    {
        public List<PrefixItem> PrefixItems;
        public List<RenamedItem> Renames;

        public RenameMap()
        {
            Renames = new List<RenamedItem>();
            PrefixItems = new List<PrefixItem>();
        }

        public void Add (string orig, string cleanName)
        {
            if (Renames.Find (x => x.Original == orig) == null)
                Renames.Add(new RenamedItem(orig, cleanName));
        }

        public void Add(string orig)
        {
            if (Renames.Find(x => x.Original == orig) == null)
                Renames.Add(new RenamedItem(orig, null));
        }

        public string GetNewName(string orig)
        {
            RenamedItem item = Renames.Find(x => x.Original == orig);

            if (item != null)
                return item.NewName;
            else
                return orig;
        }

        public string GetPrefix (string name)
        {
            while (char.IsDigit (name[name.Length-1]))
            {
                name = name.Substring(0, name.Length - 1);
            }
            return name;
        }

        public int GetNumber(string name)
        {
            int result = 0;
            string t="";
            int index = name.Length - 1;

            while ( (index < name.Length) && char.IsDigit(name[index]) )
            {
                t = name[index] + t;
                index--;
            }

            int.TryParse(t, out result);

            return result;
        }

        public void Annotate ()
        {
            foreach (RenamedItem item in Renames)
            {
                string base_name = GetPrefix(item.Original);
                int num = GetNumber(item.Original);

                if (num != 0)
                {
                    item.NewName = item.Original;
                }
                else
                {
                    num = 1;
                    string new_name = base_name + num;
                    RenamedItem test = Renames.Find(x => x.Original == new_name);
                    while (test!= null)
                    {
                        num++; new_name = base_name + num;
                        test = Renames.Find(x => x.Original == new_name);
                    }

                    item.NewName = new_name;
                }
            }
        }

    } 

}
