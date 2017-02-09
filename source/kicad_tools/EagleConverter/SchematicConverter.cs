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
    public class SchematicConverter
    {

        const bool sch_debug = false;

        /// <summary>
        /// Place no-connect flags on pins that are not in any net.
        /// Reduces "pin not connected" errors in DRC.
        /// </summary>
        public bool option_add_no_connects = true;

        //
        ProjectConverter Parent;

        //string ProjectName;
        string OutputFolder;

        EagleSchematic schematic;
        DesignRules designRules;
        LibraryConverter libraryConverter;

        //
        RenameMap PartMap = new RenameMap();           // All parts from <parts> element, with converted name

        //
        k.Project.FootprintTable footprintTable = new k.Project.FootprintTable();
        List<ComponentBase> AllComponents = new List<ComponentBase>(); // all component instances in all schematic sheets
        List<PinConnection> AllLabels = new List<PinConnection>(); // all label instances in all schematic sheets



        // A4, landscape
        //        float PageWidth = 271.78f;   // = 10.7 inches
        //        float PageHeight = 185.42f;  // = 7.3 inches

        SizeF PageSize;
        string PageStr;

        PointF DrawingOffset = new PointF(10.16f, 12.7f);
        //PointF DrawingOffset = new PointF(0, 0);


        public SchematicConverter(ProjectConverter parent)
        {
            Parent = parent;
        }

        void Trace (string s)
        {
            if (!sch_debug && (s.StartsWith("debug")))
                return;

            Parent.Trace(s);
        }



        // For schematic page
        private PointF StrToPointInchFlip(string x, string y)
        {
            float height = (int)(PageSize.Height * Common.mm_to_mil / 25) * 25;

            float y_offset = (PageSize.Height - DrawingOffset.Y * Common.mm_to_mil);
            y_offset = (int)(y_offset / 25) * 25;

            PointF result = new PointF(Common.StrToInch(x) + DrawingOffset.X * Common.mm_to_mil,
                Common.StrToInch(y) + DrawingOffset.Y * Common.mm_to_mil
                );

            //result.Y = PageSize.Height * mm_to_mil - result.Y;
            result.Y = height - result.Y;

            return result;
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
                    PageSize = new SizeF((float)8.5 * Common.inch_to_mm, (float)(11 * Common.inch_to_mm));
                    break;

                case "LETTER_L":
                    PageStr = "USLetter";
                    PageSize = new SizeF(11 * Common.inch_to_mm, (float)(8.5 * Common.inch_to_mm));
                    break;

                case "DINA-DOC": break; // n/a
                case "DOCFIELD": break; // n/a

                case "FRAME_A_L":
                    PageStr = "A";
                    PageSize = new SizeF(11 * Common.inch_to_mm, (float)(8.5 * Common.inch_to_mm));
                    break;
                case "FRAME_B_L":
                    PageStr = "B";
                    PageSize = new SizeF(17 * Common.inch_to_mm, (float)(11 * Common.inch_to_mm));
                    break;
                case "FRAME_C_L":
                    PageStr = "C";
                    PageSize = new SizeF(22 * Common.inch_to_mm, (float)(17 * Common.inch_to_mm));
                    break;
                case "FRAME_D_L":
                    PageStr = "D";
                    PageSize = new SizeF(34 * Common.inch_to_mm, (float)(22 * Common.inch_to_mm));
                    break;
                case "FRAME_E_L":
                    PageStr = "E";
                    PageSize = new SizeF(44 * Common.inch_to_mm, (float)(34 * Common.inch_to_mm));
                    break;
            }
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
            k.Symbol.Symbol result = libraryConverter.AllSymbols.Find(x => x.Name == Name);
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

        private void ConvertComponentLibraries(bool ExtractLibraries)
        {
            footprintTable = new k.Project.FootprintTable();

            foreach (Library lib in schematic.Drawing.Schematic.Libraries.Library)
            {
                if (lib.Name == "frames")
                    continue;

                libraryConverter.ConvertLibrary(lib.Name, lib, schematic.Drawing.Layers.Layer, OutputFolder, ExtractLibraries);

                //todo: check if any footprints were written
                footprintTable.Entries.Add(new Kicad_utils.Project.LibEntry(lib.Name, "KiCad", @"$(KIPRJMOD)\\" + lib.Name + ".pretty", "", ""));

            } // foreach library

            footprintTable.SaveToFile(Path.Combine(OutputFolder, "fp-lib-table")); // todo boardConv
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


        // this method was derived from:
        // https://github.com/lachlanA/eagle-to-kicad/blob/master/eagle6xx-sch-to-kicad-sch.ulp#L1181
        private void SetFieldAttributes(LegacyField field,
            PointF text_pos, 
            float instance_angle, bool instance_mirror, 
            PointF comp_pos, int attr_angle, bool attr_mirror)
        {
            int i_angle = (int)instance_angle;
            int x = (int)text_pos.X;
            int y = (int)text_pos.Y;

            // calculate text position from origin of component
            int diffX = (int)comp_pos.X - x;
            int diffY = (int)comp_pos.Y - y;

            if (!instance_mirror)
            {
                diffY = -diffY;
                instance_angle = instance_angle + 180;
            }
            // rotate offset position about origin by rotation degrees (clockwise)
            int px, py;
            px = (int)(Math.Cos(MathUtil.DegToRad(instance_angle)) * diffX + Math.Sin(MathUtil.DegToRad(instance_angle)) * diffY);
            py = (int)(-Math.Sin(MathUtil.DegToRad(instance_angle)) * diffX + Math.Cos(MathUtil.DegToRad(instance_angle)) * diffY);

            x = (int)(comp_pos.X + px);
            y = (int)(comp_pos.Y + py);

            field.Pos = new PointF(x, y);

            // now figure orientation, alignment
            char orient = 'H';
            char hAlign = 'L';
            char vAlign = 'B';

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
            
             
        }

        private void ConvertSheet(k.Schema.SchematicLegacy k_schematic, int EagleSheetNumber, string DestName, bool IsMain, int SheetNumber, int NumSheets)
        {
            List<PinConnection> connections = new List<PinConnection>();
            List<LineSegment> BusLines = new List<LineSegment>();

            Trace(string.Format("Processing schematic sheet: {0}", EagleSheetNumber + 1));

            Sheet source_sheet = schematic.Drawing.Schematic.Sheets.Sheet[EagleSheetNumber];
            SheetLegacy k_sheet = new SheetLegacy();
            k_sheet.Filename = DestName;
            k_sheet.LibNames = libraryConverter.LibNames;
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
                    k_sheet.PageSize = new SizeF(PageSize.Width * Common.mm_to_mil, PageSize.Height * Common.mm_to_mil);
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
                    Common.StrToInch(text.Size),
                    Common.xGetAngleFlip(text.Rot, out mirror),
                    false, false);

                switch (Common.GetAngle(text.Rot))
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
                k_comp.fReference.Pos = new PointF(k_comp.Position.X + k_symbol.fReference.Text.Pos.At.X, k_comp.Position.Y + k_symbol.fReference.Text.Pos.At.Y);
                k_comp.fReference.HorizJustify = "L";
                k_comp.fReference.VertJustify = "B";

                // Set Value field
                if (!string.IsNullOrEmpty(part.Value))
                    k_comp.Value = part.Value;
                else
                    k_comp.Value = k_symbol.fValue.Text.Value;

                k_comp.fValue.Pos = new PointF(k_comp.Position.X + k_symbol.fValue.Text.Pos.At.X, k_comp.Position.Y + k_symbol.fValue.Text.Pos.At.Y);
                k_comp.fValue.HorizJustify = "L";
                k_comp.fValue.VertJustify = "B";

                // Set Footprint field
                Device device = libraryConverter.AllDevices.Find(x => x.Name == part.Deviceset + part.Device);
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
                ExtRotation instanceRot = ExtRotation.Parse(instance.Rot);
                bool instance_mirror = instanceRot.Mirror;
                int instance_angle = (int)instanceRot.Rotation;

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
                    ExtRotation attrRot = ExtRotation.Parse(attrib.Rot);
                    bool attr_mirror = attrRot.Mirror;
                    int attr_angle = (int)attrRot.Rotation;

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
                        field.Size = (int)Common.StrToInch(attrib.Size);

                        SetFieldAttributes(field, StrToPointInchFlip(attrib.X, attrib.Y),
                            //sym_field.Text.xAngle, sym_field.Text.xMirror, k_comp.Position, attr_angle, attr_mirror);
                            instance_angle, k_comp.Mirror, k_comp.Position, attr_angle, attr_mirror);

                        //PointF p = Common.StrToPointInchFlip (attrib.X, attrib.Y);

                        //field.Pos = Common.StrToPointInchFlip(attrib.X, attrib.Y);

                        //debug
                        // field pos rotated about comp pos
                        PointF p = PointFExt.Rotate(field.Pos, k_comp.Position, k_comp.Rotation);
                        k.Schema.sch_wire k_line;

                        //// field pos  +
                        ////PointF p = field.Pos;
                        //k_line = sch_wire.CreateLine(new PointF(p.X - 25, p.Y), new PointF(p.X + 25, p.Y));
                        //k_sheet.Items.Add(k_line);
                        //k_line = sch_wire.CreateLine(new PointF(p.X, p.Y - 25), new PointF(p.X, p.Y + 25));
                        //k_sheet.Items.Add(k_line);

                        // actual coord of attribute  |__
                        //p = Common.StrToPointInchFlip(attrib.X, attrib.Y);
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
                        ExtRotation rot = ExtRotation.Parse(label.Rot);

                        int angle = (int)rot.Rotation;
                        if (rot.Mirror)
                            if ((angle % 180) == 0)
                                angle = 180 - angle;
                        //angle %= 360;
                        sch_text k_text = sch_text.CreateLocalLabel(net.Name,
                            StrToPointInchFlip(label.X, label.Y),
                            Common.StrToInch(label.Size),
                            angle);

                        // find nearest point
                        //k_text.Pos = FindSnapPoint(snap_points, k_text.Pos);
                        k_text.Pos = FindNearestPoint(snap_points, snap_lines, Vector2Ext.ToVector2(StrToPointInchFlip(label.X, label.Y)));

                        k_sheet.Items.Add(k_text);

                        AllLabels.Add (new PinConnection (k_text, k_sheet));
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
                        PointF pin_pos = Common.StrToPointInch(pin.X, pin.Y);

                        ExtRotation rot = ExtRotation.Parse(instance.Rot);
                        
                        pin_pos = PointFExt.Rotate(pin_pos, (int)rot.Rotation);
                        if (rot.Mirror)
                            pin_pos = new PointF(-pin_pos.X, pin_pos.Y);

                        PointF pos = new PointF (instance_pos.X + pin_pos.X, instance_pos.Y - pin_pos.Y);

                        sch_NoConnect k_noconn = new sch_NoConnect (pos);
                        k_sheet.Items.Add(k_noconn);

                    }
                }
            }
           
#endregion

        }

        private void CreateMainSheet(SchematicLegacy k_schematic, int NumSheets, string ProjectName)
        {
            List<string> Timestamps = new List<string>() ;

            SheetLegacy k_sheet = new SheetLegacy();
            k_sheet.Filename = ProjectName;
            k_sheet.LibNames = libraryConverter.LibNames;
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
                if (cur_pos.X + 1600 > k_sheet.PageSize.Width * Common.mm_to_mil)
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


        public bool ConvertSchematic (string SourceFilename, string DestFolder, string ProjectName, bool ExtractLibraries)
        {
            bool result = false;

            PartMap = new RenameMap();
            designRules = new DesignRules();
            AllLabels = new List<PinConnection>();
            AllComponents = new List<ComponentBase>();

            //
            footprintTable = new k.Project.FootprintTable();

            //
            Trace(string.Format("Reading schematic file {0}", SourceFilename));
            schematic = EagleSchematic.LoadFromXmlFile(SourceFilename);

            OutputFolder = DestFolder;

            if (schematic != null)
            {
                DrawingOffset = new PointF(10.16f, 12.7f);

                libraryConverter = new LibraryConverter(Parent);

                // if (Extract... ?
                ConvertComponentLibraries(ExtractLibraries);

                Parent.SetLibNames(libraryConverter.LibNames);

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
                    CreateMainSheet(k_schematic, schematic.Drawing.Schematic.Sheets.Sheet.Count + 1, ProjectName);

                    for (int sheet_number = 0; sheet_number < schematic.Drawing.Schematic.Sheets.Sheet.Count; sheet_number++)
                    {
                        ConvertSheet(k_schematic, sheet_number, "sheet" + (sheet_number + 1).ToString(), false, sheet_number + 2, schematic.Drawing.Schematic.Sheets.Sheet.Count + 1);
                    }
                }

                // Global schematic fixups

                List<string> labels = new List<string>();

                foreach (PinConnection conn in AllLabels)
                {
                    if (labels.IndexOf(conn.Label.Value) == -1)
                        labels.Add(conn.Label.Value);
                }

                foreach (string name in labels)
                {
                    List<PinConnection> instances = AllLabels.FindAll(x => x.Label.Value == name);

                    bool is_global = false;
                    foreach (PinConnection item in instances)
                    {
                        if (item.Sheet != instances[0].Sheet)
                        {
                            is_global = true;
                            break;
                        }
                    }

                    if (is_global)
                    {
                        Trace(String.Format("note: converting {0} to global label", name));

                        foreach (PinConnection item in instances)
                        {
                            item.Label.Type = "GLabel";
                            item.Label.Shape = "3State";
                            item.Label.TextSize = (int)(item.Label.TextSize * 0.75f);

                            if ((item.Label.Orientation % 2) == 0)
                                item.Label.Orientation = 2 - item.Label.Orientation;
                        }
                    }
                }

                //
                string filename = Path.Combine(OutputFolder, ProjectName + ".sch");
                Trace(string.Format("Writing schematic {0}", filename));
                k_schematic.SaveToFile(filename);

                result = true;
            }
            else
            {
                result = false;

                Trace(string.Format("error opening {0}", SourceFilename));
            }

            //

            return result;
        }
    }





}
