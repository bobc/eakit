using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Drawing;

using OpenTK;
using Cad2D;
using RMC;
using EagleImport;

using k = Kicad_utils;
using Kicad_utils.Schema;

namespace EagleConverter
{
    public partial class EagleUtils
    {
        const float mm_to_mil = 1000.0f / 25.4f;
        const float inch_to_mm = 25.4f;

        public delegate void TraceHandler(string s);

        public event TraceHandler OnTrace;

        /// <summary>
        /// Place no-connect flags on pins that are not in any net.
        /// Reduces "pin not connected" errors in DRC.
        /// </summary>
        public bool option_add_no_connects = true;

        //
        string ProjectName;
        string OutputFolder;
        EagleSchematic schematic;
        DesignRules designRules;

        StreamWriter reportFile = null;

        List<string> LibNames = new List<string>();
        List<Device> AllDevices = new List<Device>();

        List<k.Symbol.Symbol> AllSymbols = new List<k.Symbol.Symbol>(); // all symbols from all libraries in sch file
        List<k.ModuleDef.Module> AllFootprints = new List<k.ModuleDef.Module>(); // all package/module/foorptints from all libraries in sch file

        List<ComponentBase> AllComponents = new List<ComponentBase>(); // all component instances in all schematic sheets
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

            if (reportFile!= null)
            {
                reportFile.WriteLine(s);
            }
        }

        // convert strings in mm to inches
        private float StrToInch(string s)
        {
            return (float)(StringUtils.StringToDouble(s) * mm_to_mil);
        }

        private PointF StrToPointInch(string x, string y)
        {
            return new PointF(StrToInch(x), StrToInch(y));
        }

        private SizeF StrToSizeInch(string dx, string dy)
        {
            return new SizeF(StrToInch(dx), StrToInch(dy));
        }

        // For schematic page
        private PointF StrToPointInchFlip(string x, string y)
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
        private float StrToVal_mm(string s)
        {
            return (float)(StringUtils.StringToDouble(s));
        }

        private PointF StrToPoint_mm(string x, string y)
        {
            return new PointF(StrToVal_mm(x), StrToVal_mm(y));
        }

        private SizeF StrToSize_mm(string dx, string dy)
        {
            return new SizeF(StrToVal_mm(dx), StrToVal_mm(dy));
        }

        // For footprint files
        private PointF StrToPointFlip_mm(string x, string y)
        {
            PointF result = new PointF(StrToVal_mm(x), -StrToVal_mm(y));
            return result;
        }

        //NB works in mm
        private float RoundToGrid (float x, float align)
        {
            int j = (int)(x / align + align/2f);

            return j * align;

        }

        private PointF StrToPoint_Board(string x, string y)
        {
            float grid = 0.100f * 25.4f;
            // PointF result = new PointF(StrToVal_mm(x), -StrToVal_mm(y));

            float height = RoundToGrid(PageSize.Height, grid);

            float y_offset = (PageSize.Height - DrawingOffset.Y);
            y_offset = RoundToGrid (y_offset, grid);

            PointF result = new PointF(StrToVal_mm(x) + DrawingOffset.X,
                StrToVal_mm(y) + DrawingOffset.Y
                );

            //result.Y = PageSize.Height * mm_to_mil - result.Y;
            result.Y = height - result.Y;

            return result;
        }

        //
        /// Convert an Eagle curve end to a KiCad center for S_ARC
        private PointF kicad_arc_center(PointF start, PointF end, double angle, out float radius, out float arc_start, out float arc_end)
        {
            // Eagle give us start and end.
            // S_ARC wants start to give the center, and end to give the start.
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;

            PointF mid = new PointF((float)(start.X + dx / 2), (float)(start.Y + dy / 2));

            double dlen = Math.Sqrt(dx * dx + dy * dy);
            double dist = dlen / (2 * Math.Tan(MathUtil.DegToRad(angle) / 2));

            PointF center = new PointF(
                (float)(mid.X + dist * (dy / dlen)),
                (float)(mid.Y - dist * (dx / dlen))
            );

            radius = (float)Math.Sqrt(dist * dist + (dlen / 2) * (dlen / 2));
            arc_start = (float)Math.Atan2(start.Y - center.Y, start.X - center.X);
            arc_start = MathUtil.RadToDeg(arc_start);
            arc_end = (float)Math.Atan2(end.Y - center.Y, end.X - center.X);
            arc_end = MathUtil.RadToDeg(arc_end);

            return center;
        }


        private void ConvertFrame(string s)
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
                    PageSize = new SizeF(11 * inch_to_mm, (float)(8.5 * inch_to_mm));
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
        private string ConvertName(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                //todo: other chars?
                // special chars not at start?
                s = s.Replace("\"", "_");   //todo: mapping
                s = s.Replace("!", "~");   //todo: what if ~ already there?
                return s;
            }
            return s;
        }

        // Replace illegal filename chracters with underscore (_)
        private string CleanFootprintName(string s)
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
        private string CleanTags(string orig)
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


        // [MSR]0 to 359.9
        private int GetAngle(string rot)
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

        private int xGetAngleFlip(string rot, out bool mirror)
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

        private int GetAngle2(string rot, out bool mirror)
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

            return result;
        }

        private float DistanceBetweenPoints(Point a, Point b)
        {
            return (float)Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
        }

        private Point FindSnapPoint(List<Point> points, Point p)
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

        private Part FindPart(string PartName)
        {
            foreach (Part p in schematic.Drawing.Schematic.Parts.Part)
            {
                if (p.Name == PartName)
                    return p;
            }
            return null;
        }

        private Symbol FindSymbol(Part part, string gateId)
        {
            Library lib = schematic.Drawing.Schematic.Libraries.Library.Find(x => x.Name == part.Library);

            Deviceset devset = lib.Devicesets.Deviceset.Find(x => x.Name == part.Deviceset);

            Gate gate = devset.Gates.Gate.Find(x => x.Name == gateId);
            Symbol symbol = lib.Symbols.Symbol.Find(x => x.Name == gate.Symbol);

            return symbol;
        }

        private k.Symbol.Symbol FindSymbol(string Name)
        {
            k.Symbol.Symbol result = AllSymbols.Find(x => x.Name == Name);
            return result;
        }

        private string GetUniqueTimeStamp()
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

        private string GetUniqueTimeStamp(List<string> Timestamps)
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

        private k.LayerDescriptor ConvertLayer(string number)
        {
            k.LayerDescriptor result = new Kicad_utils.LayerDescriptor();

            //todo: for copper layers use number?
            //todo: use layer names from Eagle? (cu layers only)

            Layer layer = schematic.Drawing.Layers.Layer.Find(x => x.Number == number);

            if (layer == null)
            {
                switch (number)
                {
                    case "160": result.Name = "Eco1.User"; break;
                    case "161": result.Name = "Eco2.User"; break;
                }

                Trace(string.Format("warning: layer not found: {0}", number));
                result.Name = "Cmts.User";
            }
            else
            {
                switch (layer.Name)
                {
                    // 1
                    case "Top":
                    case "tCopper":
                        result.Name = "F.Cu";              // or Top
                        break;
                    // 16
                    case "Bottom":
                    case "bCopper":
                        result.Name = "B.Cu";           // or Bottom
                        break;

                    // 20
                    case "Dimension": result.Name = "Edge.Cuts"; break;  // or edge?

                    // 21
                    case "tPlace": result.Name = "F.SilkS"; break;
                    // 22
                    case "bPlace": result.Name = "B.SilkS"; break;

                    // 25
                    case "tNames": result.Name = "F.SilkS"; break;
                    // 26
                    case "bNames": result.Name = "B.SilkS"; break;

                    // 27
                    case "tValues": result.Name = "F.SilkS"; break;
                    // 28
                    case "bValues": result.Name = "B.SilkS"; break;

                    // 29
                    case "tStop": result.Name = "F.Mask"; break;
                    // 30
                    case "bStop": result.Name = "B.Mask"; break;

                    // 31
                    case "tCream": result.Name = "F.Paste"; break;
                    // 32
                    case "bCream": result.Name = "B.Paste"; break;

                    // 33
                    case "tFinish": result.Name = "F.Mask"; break;
                    // 34
                    case "bFinish": result.Name = "B.Mask"; break;

                    // 35
                    case "tGlue": result.Name = "F.Adhes"; break;
                    // 36
                    case "bGlue": result.Name = "B.Adhes"; break;

                    // 39
                    case "tKeepout": result.Name = "F.CrtYd"; break;
                    // 40
                    case "bKeepout": result.Name = "B.CrtYd"; break;

                    // -> clearance?
                    // 41
                    case "tRestrict": result.Name = "Dwgs.User"; break;
                    // 42
                    case "bRestrict": result.Name = "Dwgs.User"; break;
                    // 43
                    case "vRestrict": result.Name = "Dwgs.User"; break;

                    // 46
                    case "Milling": result.Name = "Dwgs.User"; break; // edge?

                    // 49
                    case "ReferenceLC": result.Name = "Cmts.User"; break;
                    // 50
                    case "ReferenceLS": result.Name = "Cmts.User"; break;

                    // 51
                    case "tDocu": result.Name = "Dwgs.User"; break;
                    // 52
                    case "bDocu": result.Name = "Dwgs.User"; break;

                    default:
                        Trace(string.Format("warning: layer not found: {0}", number, layer.Name));
                        result.Name = "Cmts.User";
                        break;
                }
            }

            result.Number = k.Layer.GetLayerNumber(result.Name);
            return result;
        }

        private void GetSymbol(Library lib, Deviceset devset, k.Symbol.Symbol k_sym)
        {
            bool is_multi_part = false;
            bool interchangeable_units = true;
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

            foreach (Gate gate in devset.Gates.Gate)
            {
                Symbol sym = lib.Symbols.Symbol.Find (x => x.Name == gate.Symbol);
                if (sym == null)
                {
                    Trace("error: symbol not found: " + gate.Symbol);
                    return;
                }

                if (lib.Name.StartsWith("supply"))
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

                if ((gate_index == 0) || (is_multi_part && !interchangeable_units) )
                {
                    // graphic lines, arcs
                    foreach (Wire wire in sym.Wire)
                    {
                        float curve = (float)StringUtils.StringToDouble(wire.Curve);

                        if (curve == 0)
                        {
                            k_sym.Drawings.Add(new k.Symbol.sym_polygon(unit, StrToInch(wire.Width), k.Symbol.FillTypes.None,
                                new List<PointF>() { StrToPointInch(wire.X1, wire.Y1), StrToPointInch(wire.X2, wire.Y2) }));
                        }
                        else
                        {
                            PointF start = StrToPointInch(wire.X1, wire.Y1);
                            PointF end = StrToPointInch(wire.X2, wire.Y2);
                            float arc_start, arc_end, radius;
                            PointF center = kicad_arc_center(start, end, -curve, out radius, out arc_start, out arc_end);

                            k_sym.Drawings.Add(new k.Symbol.sym_arc(unit, StrToInch(wire.Width),
                                center, radius, arc_start, arc_end,
                                start, end));
                        }
                    }

                    // graphic : rectangles
                    foreach (EagleImport.Rectangle rect in sym.Rectangle)
                    {
                        k_sym.Drawings.Add(new k.Symbol.sym_rectangle(unit, 1f, k.Symbol.FillTypes.PenColor, StrToPointInch(rect.X1, rect.Y1), StrToPointInch(rect.X2, rect.Y2)));
                    }

                    // graphic : texts
                    // check for name, value
                    foreach (Text text in sym.Text)
                    {
                        k.Symbol.sym_text k_text = new k.Symbol.sym_text(unit, text.mText, 0, StrToPointInch(text.X, text.Y), StrToInch(text.Size),
                            false, false, false, k.Symbol.SymbolField.HorizAlign_Left, k.Symbol.SymbolField.VertAlign_Bottom);

                        k_text.Text.xAngle = GetAngle2(text.Rot, out k_text.Text.xMirror);
                        k_text.Text.Angle = xGetAngleFlip(text.Rot, out k_text.Text.xMirror);

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


                // Pins
                foreach (Pin pin in sym.Pin)
                {
                    k.Symbol.sym_pin k_pin = new k.Symbol.sym_pin(unit, ConvertName(pin.Name), "~", StrToPointInch(pin.X, pin.Y),
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
                            k_sym.ShowPinName = false;
                            k_sym.ShowPinNumber = false;
                            break;
                        case PinVisible.pad:
                            k_sym.ShowPinName = false;
                            k_sym.ShowPinNumber = true;
                            break;
                        case PinVisible.pin:
                            k_sym.ShowPinName = true;
                            k_sym.ShowPinNumber = false;
                            break;
                        case PinVisible.both:
                            k_sym.ShowPinName = true;
                            k_sym.ShowPinNumber = true;
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

        private int GetUnitNumber (Part part, string gate_name)
        {
            Library lib = schematic.Drawing.Schematic.Libraries.Library.Find(x => x.Name == part.Library);
            Deviceset devset = lib.Devicesets.Deviceset.Find(x => x.Name == part.Deviceset);

            int unit = 1;
            foreach (Gate gate in devset.Gates.Gate)
                if (gate.Name == gate_name)
                    return unit;
                else
                    unit++;
            return 0;
        }

        private void ConvertComponentLibraries()
        {
            string lib_filename;

            footprintTable = new k.Project.FootprintTable();

            foreach (Library lib in schematic.Drawing.Schematic.Libraries.Library)
            {
                if (lib.Name == "frames")
                    continue;

                Trace("Processing Library: " + lib.Name);

                // Packages
                k.ModuleDef.LibModule k_footprint_lib = new Kicad_utils.ModuleDef.LibModule();
                k_footprint_lib.Name = lib.Name;

                foreach (Package package in lib.Packages.Package)
                {
                    k.ModuleDef.Module k_module = new Kicad_utils.ModuleDef.Module();

                    k_module.Name = CleanFootprintName(package.Name);

                    FootprintNameMap.Add(package.Name, k_module.Name);
                    if (package.Name != k_module.Name)
                        Trace(String.Format("note: {0} is renamed to {1}", package.Name, k_module.Name));

                    if (package.Description!=null)
                        k_module.description = CleanTags(package.Description.Text);
                    k_module.position = new k.ModuleDef.Position(0, 0, 0);

                    k_module.layer = "F.Cu"; // todo: back ???

                    foreach (Wire wire in package.Wire)
                    {
                        //
                        float curve = (float)StringUtils.StringToDouble(wire.Curve);
                        if (curve == 0)
                        {
                            k.ModuleDef.fp_line k_line = new Kicad_utils.ModuleDef.fp_line(
                                StrToPointFlip_mm(wire.X1, wire.Y1),
                                StrToPointFlip_mm(wire.X2, wire.Y2),
                                ConvertLayer(wire.Layer).Name,
                                StrToVal_mm(wire.Width));
                            k_module.Borders.Add(k_line);
                        }
                        else
                        {
                            PointF start = StrToPointFlip_mm(wire.X1, wire.Y1);
                            PointF end = StrToPointFlip_mm(wire.X2, wire.Y2);
                            float arc_start, arc_end, radius;
                            PointF center = kicad_arc_center(start, end, curve, out radius, out arc_start, out arc_end);

                            k.ModuleDef.fp_arc k_arc = new k.ModuleDef.fp_arc (
                                center, start,
                                -curve,
                                ConvertLayer(wire.Layer).Name,
                                StrToVal_mm(wire.Width));

                            k_module.Borders.Add(k_arc);
                        }
                    }

                    foreach (Smd smd in package.Smd)
                    {
                        k.ModuleDef.pad k_pad = new k.ModuleDef.pad(smd.Name, "smd", "rect", StrToPointFlip_mm(smd.X, smd.Y), StrToSize_mm(smd.Dx, smd.Dy), 0);
                        if (GetAngle(smd.Rot) % 180 == 90)
                            k_pad.size = StrToSize_mm(smd.Dy, smd.Dx);
                        k_module.Pads.Add(k_pad);

                        string layer = ConvertLayer(smd.Layer).Name;
                    }

                    foreach (Pad pad in package.Pad)
                    {
                        
                        float pad_size = designRules.CalcPadSize (StrToVal_mm(pad.Drill));
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
                            ConvertLayer(text.Layer).Name,
                            new SizeF(StrToVal_mm(text.Size), StrToVal_mm(text.Size)),
                            0.12f,
                            true);

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
                        k.ModuleDef.fp_polygon k_poly = new Kicad_utils.ModuleDef.fp_polygon(
                            StrToPointFlip_mm(rect.X1, rect.Y1), StrToPointFlip_mm(rect.X2, rect.Y2),
                            ConvertLayer(rect.Layer).Name,
                            0.12f
                            );
                        k_module.Borders.Add(k_poly);
                    }

                    foreach (Circle circle in package.Circle)
                    {
                        k.ModuleDef.fp_circle k_circle = new Kicad_utils.ModuleDef.fp_circle(
                            StrToPointFlip_mm(circle.X, circle.Y),
                            StrToVal_mm(circle.Radius),
                            ConvertLayer(circle.Layer).Name,
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

                    foreach (EagleImport.Polygon poly in package.Polygon)
                    {
                        Cad2D.Polygon poly_2d = new Cad2D.Polygon();

                        foreach (Vertex v in poly.Vertex)
                        {
                            PointF p = StrToPointFlip_mm(v.X, v.Y);
                            poly_2d.AddVertex(p.X, p.Y);
                        }

                        k.ModuleDef.fp_polygon k_poly = new Kicad_utils.ModuleDef.fp_polygon(poly_2d, ConvertLayer(poly.Layer).Name, 0.12f);
                        k_module.Borders.Add(k_poly);
                    }

                    //
                    k_footprint_lib.Modules.Add(k_module);

                    //
                    k.ModuleDef.Module k_generic = k_module.Clone();
                    k_generic.Name = lib.Name + ":" + k_generic.Name;
                    AllFootprints.Add(k_generic);
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

                    if (devset.Description!=null)
                        k_sym.Description = CleanTags(devset.Description.Text);

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

                            // place below value
                            PointF pos;
                            if (k_sym_device.fValue.Text.Angle == 0)
                                pos = new PointF(k_sym_device.fValue.Text.Pos.X, k_sym_device.fValue.Text.Pos.Y - 100);
                            else
                                pos = new PointF(k_sym_device.fValue.Text.Pos.X + 100, k_sym_device.fValue.Text.Pos.Y);

                            k_sym_device.fPcbFootprint = new k.Symbol.SymbolField(kicad_lib.Name + ":" + device.Package,
                                pos,
                                50, true, k_sym_device.fValue.Text.Angle == 0 ? "H" : "V",
                                "L", "B", false, false);

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

                                    k.Symbol.sym_pin k_pin = k_sym_device.FindPin(unit, ConvertName(connect.Pin));
                                    if (k_pin == null)
                                        Trace(string.Format("error: pin not found {0} {1}", k_sym_device, connect.Pin));
                                    else
                                    {
                                        //todo: check length
                                        k_pin.PinNumber = connect.Pad;

                                        if (k_pin.PinNumber.Length > 4)
                                            Trace(string.Format("error: pad name too long {0} {1}", k_sym_device.Name, connect.Pad));
                                    }
                                }
                            }

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

        private Point FindNearestPoint(List<Vector2> points, List<LineSegment> snap_lines, Vector2 p)
        {
            float min_distance = 1e9f;
            int min_point = -1;

            if (points.Count > 0)
            {
                min_point = 0;
                min_distance = Vector2Ext.Distance(p, points[0]);

                for (int index = 1; index < points.Count; index++)
                {
                    float dist = Vector2Ext.Distance(p, points[index]);
                    if (dist < min_distance)
                    {
                        min_point = index;
                        min_distance = dist;
                    }
                }
                //return points[min_point];
            }

            int min_line = -1;

            if (snap_lines.Count > 0)
            {
                for (int index=0; index < snap_lines.Count; index++)
                {
                    float dist = snap_lines[index].Distance(p);
                    if (dist < min_distance)
                    {
                        min_line = index;
                        min_distance = dist;
                        min_point = -1;
                    }
                }
            }

            if (min_point==-1)
            {
                if (min_line==-1)
                {
                    return p.ToPoint();
                }
                else
                {
                    Vector2 nearestPoint= GeometryHelper.GetNearestPointInSegmentToPoint(
                        snap_lines[min_line].StartPos, snap_lines[min_line].EndPos, p);
                    return nearestPoint.ToPoint();
                }
            }
            else
            {
                return points[min_point].ToPoint();
            }
        }

        private void WriteProjectFile()
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
            //PageLayoutDescrFile=C:/git_bobc/WebRadio/hardware/test_main - Copy/custom.kicad_wks
            k_section.AddItem("LastNetListRead", "");
            //k_section.AddItem("UseCmpFile", 1);
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
            k_section.AddItem("SolderMaskClearance", 0.2f);
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
            string filename = Path.Combine(OutputFolder, ProjectName);
            Trace(string.Format ("Writing project file {0}", Path.ChangeExtension(filename,".pro")));
            
            k_project.SaveToFile(filename);
        }


        // this method was derived from:
        // https://github.com/lachlanA/eagle-to-kicad/blob/master/eagle6xx-sch-to-kicad-sch.ulp#L1181
        private void SetFieldAttributes(LegacyField field,
            PointF text_pos, 
            float instance_angle, bool instance_mirror, 
            PointF comp_pos, int attr_angle, bool attr_mirror)
        {
            int x = (int)text_pos.X;
            int y = (int)text_pos.Y;

            int i_angle = (int)instance_angle;

            char orient = 'H';
            char hAlign = 'L';
            char vAlign = 'B';

            float rotation = instance_angle ;//+ 180; // - 90;

            // calculate text position from origin of component
            int diffX = (int)comp_pos.X - x;
            int diffY = (int)comp_pos.Y - y;

            //if (mirror)
            {
            //    diffX = -diffX;
            }

            //if (!attr_mirror)
            //{
            //    diffY = -diffY;
            //}

            int px, py;
            if (!instance_mirror)
            {
                //diffX = -diffX;
                diffY = -diffY;
                rotation = rotation+180;

                // rotate offset position about origin by rotation degrees (anti-clockwise)
                //px = (int)(Math.Cos(MathUtil.DegToRad(rotation)) * diffX - Math.Sin(MathUtil.DegToRad(rotation)) * diffY);
                //py = (int)(Math.Sin(MathUtil.DegToRad(rotation)) * diffX + Math.Cos(MathUtil.DegToRad(rotation)) * diffY);

                // rotate offset position about origin by rotation degrees (clockwise)
                px = (int)(Math.Cos(MathUtil.DegToRad(rotation)) * diffX + Math.Sin(MathUtil.DegToRad(rotation)) * diffY);
                py = (int)(-Math.Sin(MathUtil.DegToRad(rotation)) * diffX + Math.Cos(MathUtil.DegToRad(rotation)) * diffY);

                x = (int)(comp_pos.X + px);
                y = (int)(comp_pos.Y + py);
            }
            else
            {
                // rotate offset position about origin by rotation degrees (clockwise)
                px = (int)(Math.Cos(MathUtil.DegToRad(rotation)) * diffX + Math.Sin(MathUtil.DegToRad(rotation)) * diffY);
                py = (int)(-Math.Sin(MathUtil.DegToRad(rotation)) * diffX + Math.Cos(MathUtil.DegToRad(rotation)) * diffY);

                x = (int)(comp_pos.X + px);
                y = (int)(comp_pos.Y + py);
            }

            // calculate difference in coordinate so we can properly flip this coordinate system
            //x = (int)(comp_pos.X + py);
            //y = (int)(comp_pos.Y + px);


            switch (i_angle)
            {
                case 0: // angle
                    switch (attr_angle)
                    {
                        case 0:
                            if (attr_mirror ^ instance_mirror)
                                { orient = 'H'; hAlign = 'R'; vAlign = 'B'; }
                            else
                                { orient = 'H'; hAlign = 'L'; vAlign = 'B'; }
                            break;
                        case 90:
                            if (attr_mirror ^ instance_mirror)
                                { orient = 'V'; hAlign = 'L'; vAlign = 'T'; }
                            else
                                { orient = 'V'; hAlign = 'L'; vAlign = 'B'; }//**
                            break;
                        case 180:
                            if (attr_mirror ^ instance_mirror)
                                { orient = 'H'; hAlign = 'L'; vAlign = 'T'; }
                            else
                                { orient = 'H'; hAlign = 'R'; vAlign = 'T'; }
                            break;
                        case 270:
                            if (attr_mirror ^ instance_mirror)
                                { orient = 'V'; hAlign = 'R'; vAlign = 'B'; }
                            else
                                { orient = 'V'; hAlign = 'R'; vAlign = 'T'; } //**
                            break;
                    }
                    break;
                case 180: // angle
                    switch (attr_angle)
                    {
                        case 0:
                            if (attr_mirror ^ instance_mirror)
                            { orient = 'H'; hAlign = 'L'; vAlign = 'T'; }
                            else
                            { orient = 'H'; hAlign = 'R'; vAlign = 'T'; }
                            break;
                        case 90:
                            if (attr_mirror ^ instance_mirror)
                            { orient = 'V'; hAlign = 'R'; vAlign = 'B'; }
                            else
                            { orient = 'V'; hAlign = 'R'; vAlign = 'T'; } //
                            break;
                        case 180:
                            if (attr_mirror ^ instance_mirror)
                            { orient = 'H'; hAlign = 'R'; vAlign = 'B'; }
                            else
                            { orient = 'H'; hAlign = 'L'; vAlign = 'B'; }
                            break;
                        case 270:
                            if (attr_mirror ^ instance_mirror)
                            { orient = 'V'; hAlign = 'L'; vAlign = 'T'; }
                            else
                            { orient = 'V'; hAlign = 'L'; vAlign = 'B'; }//
                            break;
                    }
                    break;
                case 90: // angle
                    switch (attr_angle)
                    {
                        case 0:
                            if (attr_mirror ^ instance_mirror)
                                { orient = 'V'; hAlign = 'L'; vAlign = 'T'; }
                            else
                                { orient = 'V'; hAlign = 'R'; vAlign = 'T'; } //vh
                            break;
                        case 90:
                            if (instance_mirror)
                            {
                                if (attr_mirror)
                                    { orient = 'H'; hAlign = 'L'; vAlign = 'B'; }
                                else
                                   { orient = 'H'; hAlign = 'L'; vAlign = 'T'; }
                            }
                            else
                            {
                                if (attr_mirror)
                                    { orient = 'H'; hAlign = 'L'; vAlign = 'T'; } // v
                                else
                                    { orient = 'H'; hAlign = 'L'; vAlign = 'B'; }
                            }
                            break;
                        case 180:
                            if (instance_mirror)
                            {
                                if (attr_mirror)
                                    { orient = 'V'; hAlign = 'L'; vAlign = 'B'; } //
                                else
                                    { orient = 'V'; hAlign = 'R'; vAlign = 'B'; }
                            }
                            else
                            {
                                if (attr_mirror)
                                    { orient = 'V'; hAlign = 'R'; vAlign = 'B'; } //h   
                                else
                                    { orient = 'V'; hAlign = 'L'; vAlign = 'B'; } //**
                            }
                            break;
                        case 270:
                            if (attr_mirror ^ instance_mirror)
                                { orient = 'H'; hAlign = 'R'; vAlign = 'B'; }
                            else
                                { orient = 'H'; hAlign = 'R'; vAlign = 'T'; }
                            break;
                    }
                    break;
                case 270: // angle
                    switch (attr_angle)
                    {
                        case 0:
                            if (instance_mirror)
                            {
                                if (attr_mirror)
                                    { orient = 'V'; hAlign = 'L'; vAlign = 'B'; }
                                 else
                                    { orient = 'V'; hAlign = 'R'; vAlign = 'B'; }
                            }
                            else
                            {
                                if (attr_mirror)
                                   { orient = 'V'; hAlign = 'R'; vAlign = 'T'; }
                                else
                                    { orient = 'V'; hAlign = 'L'; vAlign = 'B'; } //
                            }
                            break;
                        case 180:
                            if (attr_mirror ^ instance_mirror)
                                { orient = 'V'; hAlign = 'L'; vAlign = 'T'; }
                            else
                                { orient = 'V'; hAlign = 'R'; vAlign = 'T'; } //
                            break;
                        case 90:
                            // !!
                            if (instance_mirror)
                                { orient = 'H'; hAlign = 'R'; vAlign = 'B'; }
                            else
                                { orient = 'H'; hAlign = 'R'; vAlign = 'T'; }
                            break;

                        case 270:
                            if (attr_mirror ^ instance_mirror)
                                { orient = 'H'; hAlign = 'L'; vAlign = 'T'; }
                            else
                                { orient = 'H'; hAlign = 'L'; vAlign = 'B'; }
                            break;
                    }
                    break;
            } // switch

            field.Orientation = orient.ToString();
            field.HorizJustify = hAlign.ToString();
            field.VertJustify = vAlign.ToString();
            field.Pos = new PointF(x, y);
             
        }

        private void ConvertSheet(k.Schema.SchematicLegacy k_schematic, int EagleSheetNumber, string DestName, bool IsMain, int SheetNumber, int NumSheets)
        {
            List<PinConnection> connections = new List<PinConnection>();
            List<LineSegment> BusLines = new List<LineSegment>();

            Trace(string.Format("Processing schematic sheet: {0}", EagleSheetNumber + 1));

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
                    xGetAngleFlip(text.Rot, out mirror),
                    false, false);

                switch (GetAngle(text.Rot))
                {
                    case 0: break;
                    case 90:
                        if (mirror)
                        {
                            k_text.Pos.X += (int)(k_text.TextSize * 1.5f);
                            k_text.Pos.Y -= (int)(k_text.name.Length * k_text.TextSize * 1.5f);
                        }
                        break;
                    case 180:
                        k_text.Pos.Y += (int)(k_text.TextSize * 1.5f);
                        break;
                    case 270:
                        if (mirror)
                        {
                            //k_text.Pos.X += (int)(k_text.TextSize * 1.5f);
                            k_text.Pos.Y += (int)(k_text.name.Length * k_text.TextSize * 1.7f);
                        }
                        else
                            k_text.Pos.X += (int)(k_text.TextSize * 1.5f);
                        break;
                }

                k_sheet.Items.Add(k_text);
            }

            // lines
            foreach (Wire wire in source_sheet.Plain.Wire)
            {
                k.Schema.sch_wire k_line = sch_wire.CreateLine(StrToPointInchFlip(wire.X1, wire.Y1),
                    StrToPointInchFlip(wire.X2, wire.Y2));

                k_sheet.Items.Add(k_line);
            }

            #region === (Instance) components ====
            foreach (Instance instance in source_sheet.Instances.Instance)
            {
                // find part -> 
                Part part = FindPart(instance.Part);

                if (part == null)
                {
                    Trace("error: Part not found: " + instance.Part);
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
                        Trace(String.Format("note: {0} is renamed {1}", instance.Part, k_comp.Reference));
                }
                else
                {
                    k_comp.Reference = k_symbol.Reference;
                    k_comp.fReference.Hidden = !k_symbol.fReference.Text.Visible;
                    k_comp.fReference.Size = (int)k_symbol.fReference.Text.FontSize;
                }

                // set a default pos/
                k_comp.fReference.Pos = new PointF(k_comp.Position.X + k_symbol.fReference.Text.Pos.X, k_comp.Position.Y + k_symbol.fReference.Text.Pos.Y);
                k_comp.fReference.HorizJustify = "L";
                k_comp.fReference.VertJustify = "B";

                // Set Value field
                if (!string.IsNullOrEmpty(part.Value))
                    k_comp.Value = part.Value;
                else
                    k_comp.Value = k_symbol.fValue.Text.Value;

                k_comp.fValue.Pos = new PointF(k_comp.Position.X + k_symbol.fValue.Text.Pos.X, k_comp.Position.Y + k_symbol.fValue.Text.Pos.Y);
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

                //
                if (k_symbol.NumUnits > 1)
                {
                    int unit = GetUnitNumber(part, instance.Gate);
                    k_comp.N = unit;
                }

                // ---------------------------------
                // Set position, orientation
                bool instance_mirror;
                int instance_angle = GetAngle2(instance.Rot, out instance_mirror);

                if (!string.IsNullOrEmpty(instance.Rot))
                {
                    if (instance_mirror)
                        k_comp.Rotation = (instance_angle + 180) % 360;
                    else
                        k_comp.Rotation = instance_angle;
                    k_comp.Mirror = instance_mirror;
                }

                foreach (EagleImport.Attribute attrib in instance.Attribute)
                {
                    bool attr_mirror;
                    int attr_angle = GetAngle2(attrib.Rot, out attr_mirror);

                    //int angle = GetAngle(attrib.Rot);
                    //angle %= 360;
                    //string orientation = (angle == 0) || (angle == 180) ? "H" : "V";

                    k.Symbol.SymbolField sym_field = null;
                    LegacyField field = null;
                    switch (attrib.Name)
                    {
                        case "NAME":
                            sym_field = k_symbol.fReference;
                            field = k_comp.fReference;
                            //field.Pos = new PointF(k_comp.Position.X + k_symbol.fReference.Text.Pos.X, k_comp.Position.Y + k_symbol.fReference.Text.Pos.Y);
                            break;
                        case "VALUE":
                            sym_field = k_symbol.fValue;
                            field = k_comp.fValue;
                            //field.Pos = new PointF(k_comp.Position.X + k_symbol.fValue.Text.Pos.X, k_comp.Position.Y + k_symbol.fValue.Text.Pos.Y);
                            break;

                            //Part?
                            // voltage, current
                    }

                    if (field != null)
                    {
                        field.Size = (int)StrToInch(attrib.Size);

                        SetFieldAttributes(field, StrToPointInchFlip(attrib.X, attrib.Y),
                            //sym_field.Text.xAngle, sym_field.Text.xMirror, k_comp.Position, attr_angle, attr_mirror);
                            instance_angle, k_comp.Mirror, k_comp.Position, attr_angle, attr_mirror);

                        //PointF p = StrToPointInchFlip (attrib.X, attrib.Y);

                        //field.Pos = StrToPointInchFlip(attrib.X, attrib.Y);

                        //debug
                        // field pos rotated about comp pos
                        PointF p = PointFExt.RotatePoint(field.Pos, k_comp.Position, k_comp.Rotation);
                        k.Schema.sch_wire k_line;

                        //// field pos  +
                        ////PointF p = field.Pos;
                        //k_line = sch_wire.CreateLine(new PointF(p.X - 25, p.Y), new PointF(p.X + 25, p.Y));
                        //k_sheet.Items.Add(k_line);
                        //k_line = sch_wire.CreateLine(new PointF(p.X, p.Y - 25), new PointF(p.X, p.Y + 25));
                        //k_sheet.Items.Add(k_line);

                        // actual coord of attribute  |__
                        //p = StrToPointInchFlip(attrib.X, attrib.Y);
                        //k_line = sch_wire.CreateLine(new PointF(p.X-50, p.Y), new PointF(p.X + 50, p.Y));
                        //k_sheet.Items.Add(k_line);
                        //k_line = sch_wire.CreateLine(new PointF(p.X, p.Y-50), new PointF(p.X, p.Y + 50));
                        //k_sheet.Items.Add(k_line);
                    }
                }

                //
                AllComponents.Add(k_comp);

                k_sheet.Components.Add(k_comp);
            }
            #endregion

            #region ==== Busses ====
            foreach (Bus bus in source_sheet.Busses.Bus)
            {
                foreach (Segment segment in bus.Segment)
                {
                    foreach (Wire wire in segment.Wire)
                    {
                        k.Schema.sch_wire k_bus = sch_wire.CreateBus(StrToPointInchFlip(wire.X1, wire.Y1),
                            StrToPointInchFlip(wire.X2, wire.Y2));

                        k_sheet.Items.Add(k_bus);

                        BusLines.Add(new LineSegment(Vector2Ext.ToVector2(k_bus.start), Vector2Ext.ToVector2(k_bus.end)));
                    }
                }
            }
            #endregion


            #region ==== (Net) look for wires, junctions, labels ====
            foreach (Net net in source_sheet.Nets.Net)
            {
                foreach (Segment segment in net.Segment)
                {
                    List<Vector2> snap_points = new List<Vector2>();
                    List<LineSegment> snap_lines = new List<LineSegment>();

                    foreach (Wire wire in segment.Wire)
                    {
                        PointF start = StrToPointInchFlip(wire.X1, wire.Y1);
                        PointF end = StrToPointInchFlip(wire.X2, wire.Y2);

                        Vector2 p1 = Vector2Ext.ToVector2(start);
                        Vector2 p2 = Vector2Ext.ToVector2(end);

                        bool is_bus_entry = false;

                        foreach (LineSegment line in BusLines)
                            if (line.Contains(p1) || line.Contains(p2))
                                is_bus_entry = true;

                        if (is_bus_entry)
                        {
                            sch_wire k_wire = sch_wire.CreateWireToBusEntry(start, end);
                            k_sheet.Items.Add(k_wire);
                        }
                        else
                        {
                            sch_wire k_wire = sch_wire.CreateWire(start, end);
                            k_sheet.Items.Add(k_wire);
                            //snap_points.Add(Vector2Ext.ToVector2(k_wire.start));
                            //snap_points.Add(Vector2Ext.ToVector2(k_wire.end));
                            snap_lines.Add(new LineSegment(Vector2Ext.ToVector2(k_wire.start), Vector2Ext.ToVector2(k_wire.end)));
                        }
                    }

                    foreach (Junction junction in segment.Junction)
                    {
                        sch_Junction k_junction = new sch_Junction(StrToPointInchFlip(junction.X, junction.Y));
                        k_sheet.Items.Add(k_junction);

                        snap_points.Add(Vector2Ext.ToVector2(k_junction.Pos));
                    }

                    //todo: add gate positions to snap_points

                    foreach (Pinref pinref in segment.Pinref)
                    {
                        Part part = FindPart(pinref.Part);
                        k.Symbol.Symbol k_symbol = FindSymbol(part.Deviceset);

                        // get comp ...

                        PinConnection connect = connections.Find(x => x.Part == part && x.GateId == pinref.Gate && x.PinName == pinref.Pin);
                        if (connect == null)
                        {
                            connect = new PinConnection(net.Name, part, pinref.Gate, pinref.Pin);
                            connections.Add(connect);
                        }
                    }

                    foreach (Label label in segment.Label)
                    {
                        bool mirror;
                        sch_text k_text = sch_text.CreateLocalLabel(net.Name,
                            StrToPointInchFlip(label.X, label.Y),
                            StrToInch(label.Size),
                            GetAngle2(label.Rot, out mirror), false, false);

                        // find nearest point
                        //k_text.Pos = FindSnapPoint(snap_points, k_text.Pos);

                        k_text.Pos = FindNearestPoint(snap_points, snap_lines, Vector2Ext.ToVector2(StrToPointInchFlip(label.X, label.Y)));

                        k_sheet.Items.Add(k_text);
                    }
                }
            }
            #endregion

            #region add no-connects
            if (option_add_no_connects)
            foreach (Instance instance in source_sheet.Instances.Instance)
            {
                // find part -> 
                Part part = FindPart(instance.Part);
                if (part == null)
                {
                    //Trace("Part not found: " + instance.Part);
                    continue;
                }

                if (part.Library == "frames")
                    continue;

                k.Symbol.Symbol k_symbol = FindSymbol(part.Deviceset);

                //
                List<PinConnection> pins = connections.FindAll(x => x.Part.Name == instance.Part && x.GateId == instance.Gate);
                foreach (PinConnection p in pins)
                {
                //    Trace(string.Format("Part {0,-10} Gate {1,-10} Pin {2,-10} Net {3,-10}", p.Part.Name, p.GateId, p.PinName, p.NetLabel));
                }

                //
                List<PinConnection> NoConnects = new List<PinConnection>();

                Symbol symbol = FindSymbol(part, instance.Gate);

                foreach (Pin pin in symbol.Pin)
                {
                    PinConnection conn = connections.Find(x => x.Part == part && x.GateId == instance.Gate && x.PinName == pin.Name);
                    if (conn == null)
                    {
                        //Trace(string.Format("Part {0,-10} Gate {1,-10} Pin {2,-10}", part.Name, instance.Gate, pin.Name));
                        NoConnects.Add(new PinConnection(null, part, instance.Gate, pin.Name));

                        // todo: add no-connects
                        PointF instance_pos = StrToPointInchFlip(instance.X, instance.Y);
                        PointF pin_pos = StrToPointInch(pin.X, pin.Y);

                        bool instance_mirror;
                        pin_pos = PointFExt.RotatePoint(pin_pos, GetAngle2(instance.Rot, out instance_mirror));
                        if (instance_mirror)
                            pin_pos = new PointF(-pin_pos.X, pin_pos.Y);

                        PointF pos = new PointF (instance_pos.X + pin_pos.X, instance_pos.Y - pin_pos.Y);

                        sch_NoConnect k_noconn = new sch_NoConnect (pos);
                        k_sheet.Items.Add(k_noconn);

                    }
                }
            }
           
            #endregion

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

        public bool CheckValid(string Filename)
        {
            string baseName = Path.GetFileName(Filename);
            bool result = false;
            try
            {
                StreamReader file = new StreamReader(Filename);
                try
                {
                    string line = file.ReadLine();
                    if (line != null)
                    {
                        if (line.StartsWith("<?xml"))
                        {
                            line = file.ReadLine();
                            if (line.Contains("eagle.dtd"))
                                result = true;
                        }
                        if (!result)
                            Trace(string.Format("error: {0} is not an EAGLE XML file, try saving using EAGLE version 6 or later", baseName));
                    }
                    else
                        Trace(string.Format ("error reading {0}: empty file?", baseName));
                }
                catch (Exception ex)
                {
                    Trace (string.Format("error reading {0}: {1}", baseName, ex.Message));
                }
                finally
                {
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                Trace(string.Format("error opening {0}: {1}", baseName, ex.Message));
            }
            return result;
        }


        public bool ImportFromEagle(string SourceFilename, string DestFolder)
        {
            bool result = false;
            LibNames = new List<string>();
            AllDevices = new List<Device>();
            AllSymbols = new List<k.Symbol.Symbol>();
            AllFootprints = new List<k.ModuleDef.Module>();
            AllComponents = new List<ComponentBase>();
            footprintTable = new k.Project.FootprintTable();
            PartMap = new RenameMap();
            FootprintNameMap = new RenameMap();
            designRules = new DesignRules();

            //
            ProjectName = Path.GetFileNameWithoutExtension(SourceFilename);

            reportFile = new StreamWriter(Path.Combine(DestFolder, "Conversion report.txt"));

            Trace(string.Format("Conversion report on {0}", StringUtils.IsoFormatDateTime (DateTime.Now)));
            Trace("");
            Trace("Parameters:");
            Trace(string.Format("Input project  {0}", SourceFilename));
            Trace(string.Format("Output project {0}", DestFolder));

            Trace("");
            Trace("Log:");

            //
            Trace(string.Format("Reading schematic file {0}", SourceFilename));
            schematic = EagleSchematic.LoadFromXmlFile(SourceFilename);

            OutputFolder = DestFolder;

            if (schematic != null)
            {
                DrawingOffset = new PointF(10.16f, 12.7f);

                ConvertComponentLibraries();

                //
                foreach (Part part in schematic.Drawing.Schematic.Parts.Part)
                {
                    k.Symbol.Symbol k_symbol = FindSymbol(part.Deviceset);

                    if ((k_symbol != null) && !k_symbol.PowerSymbol)
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
                    CreateMainSheet(k_schematic, schematic.Drawing.Schematic.Sheets.Sheet.Count + 1);

                    for (int sheet_number = 0; sheet_number < schematic.Drawing.Schematic.Sheets.Sheet.Count; sheet_number++)
                    {
                        ConvertSheet(k_schematic, sheet_number, "sheet" + (sheet_number + 1).ToString(), false, sheet_number + 2, schematic.Drawing.Schematic.Sheets.Sheet.Count + 1);
                    }
                }

                string filename = Path.Combine(OutputFolder, ProjectName + ".sch");
                Trace(string.Format("Writing schematic {0}", filename));
                k_schematic.SaveToFile(filename);


                ConvertBoardFile(SourceFilename);

                //
                WriteProjectFile();

                Trace("");
                Trace("Done");
                result = true;
            }
            else
            {
                result = false;

                Trace(string.Format("error opening {0}", SourceFilename));
                Trace("");
                Trace("Terminated due to error");
            }

            //
            reportFile.Close();

            return result;
        }
    }





}
