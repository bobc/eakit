using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using Lexing;
using RMC;
using System.Drawing.Drawing2D;
using System.Collections;

using Kicad_utils.Drawing;

namespace Kicad_utils.Symbol
{
    public class Symbol : IComparable<Symbol>
    {
        public string Name;
        public string Reference;   // RefDes

        public bool Visible;
        public int Offset;
        public bool ShowPinNumber;
        public bool ShowPinName;
        public int NumUnits = 1;
        public bool Locked = true;
        public bool PowerSymbol = false;

        // units_locked : (Used only if unit_count > 1)
        // = L (units are not identical and cannot be swapped) or 
        // = F (units are identical and therefore can be swapped) 

        public SymbolField fReference = new SymbolField();
        public SymbolField fValue = new SymbolField();        // default value set to Name in library
        public SymbolField fPcbFootprint = new SymbolField(); // placeholder for Footprint, can default
        public SymbolField fUserDocLink = new SymbolField();  // Deprecated?

        // todo: user fields
        public List<SymbolField> UserFields;

        // alias
        public string Alias;

        // footprint list
        public List<string> Footprints;

        public List<sym_drawing_base> Drawings;

        // from dcm file
        public string Description;
        public string Keywords;
        public string DataSheetFile;

        public Symbol()
        {
            Drawings = new List<sym_drawing_base>();
            UserFields = new List<SymbolField>();
        }

        public Symbol(string name, bool visible, string prefix, int offset, bool showPinNumber,
            bool showPinName, int numUnits, bool locked, bool powerSymbol)
        {
            this.Name = name;
            this.Visible = visible;
            this.Reference = prefix;
            this.Offset = offset;
            this.ShowPinNumber = showPinNumber;
            this.ShowPinName = showPinName;
            this.NumUnits = numUnits;
            this.Locked = locked;
            this.PowerSymbol = powerSymbol;
        }


        public Symbol Clone ()
        {
            Symbol result;
            List<string> data = new List<string>();

            ConvertToLegacySymbol(out data);
            result = Symbol.Parse(data);
            result.Description = this.Description;
            result.Keywords = this.Keywords;
            result.DataSheetFile = this.DataSheetFile;

            return result;
        }

        // DEF CONN_15_RMC P 0 40 Y N 1 F N

        public void ConvertToLegacySymbol(out List<string> text)
        {
            text = new List<string>();

            text.Add(string.Format("#"));
            text.Add(string.Format("# {0}", Name));
            text.Add(string.Format("#"));

            text.Add(string.Format("DEF {0} {1} {2} {3} {4} {5} {6} {7} {8}",
                Visible ? Name : "~" + Name,
                Reference,
                0,
                Offset,
                ShowPinNumber ? "Y" : "N",
                ShowPinName ? "Y" : "N",
                NumUnits,
                Locked ? "L" : "F",
                PowerSymbol ? "P" : "N"));

            text.Add(string.Format("F0 {0}", fReference.ToString()));
            text.Add(string.Format("F1 {0}", fValue.ToString()));

            if (!string.IsNullOrEmpty(fPcbFootprint.Text.Value) && (fPcbFootprint.Text.Value != "~"))
                text.Add(string.Format("F2 {0}", fPcbFootprint.ToString()));

            if (!string.IsNullOrEmpty(fUserDocLink.Text.Value) && (fUserDocLink.Text.Value !="~"))
                text.Add(string.Format("F3 {0}", fUserDocLink.ToString()));

            //
            if (UserFields != null)
            {
                foreach (SymbolField field in UserFields)
                {
                    text.Add(string.Format("F{0} {1}", field.Index, field.ToString()));
                }
            }

            //
            if (!string.IsNullOrEmpty(Alias))
                text.Add(string.Format("ALIAS {0}", Alias));

            if (Footprints != null)
            {
                text.Add("$FPLIST");
                foreach (string s in Footprints)
                    text.Add(s);
                text.Add("$ENDFPLIST");
            }

            text.Add("DRAW");

            if (Drawings != null)
                foreach (sym_drawing_base drawing in Drawings)
                    text.Add(drawing.ToString());

            text.Add("ENDDRAW");
            text.Add("ENDDEF");
        }


        public static Symbol Parse (List<string> lines)
        {
            int index = 0;
            Symbol sym = new Symbol();
            List<Token> tokens;

            // DEF AT89C2051-P IC 0 40 Y Y 1 F N

            while ((index < lines.Count) && (lines[index].StartsWith("#")))
                index++;

            tokens = Utils.Tokenise(lines[index]);

            sym.Name = tokens[1].Value;
            sym.Visible = true;
            if (sym.Name.StartsWith("~"))
            {
                sym.Visible = false;
                sym.Name = sym.Name.Substring(1);
            }
            sym.Reference = tokens[2].Value;
            // param 3 is always 0
            sym.Offset = tokens[4].IntValue;
            sym.ShowPinNumber = tokens[5].Value == "Y";
            sym.ShowPinName = tokens[6].Value == "Y";
            sym.NumUnits = tokens[7].IntValue;
            sym.Locked = tokens[8].Value == "L";
            sym.PowerSymbol = tokens[9].Value == "P";
            index++;

            //
            while ((index < lines.Count) && (lines[index] != "DRAW"))
            {
                tokens = Utils.Tokenise(lines[index]);

                if (tokens[0].Value.StartsWith("F"))
                {
                    SymbolField text = SymbolField.Parse(tokens);
                    switch (tokens[0].Value)
                    {
                        case "F0": sym.fReference = text; break;
                        case "F1": sym.fValue = text; break;
                        case "F2": sym.fPcbFootprint = text; break;
                        case "F3": sym.fUserDocLink = text; break;
                        default:
                            {
                                int field_index;
                                if (int.TryParse(tokens[0].Value.Substring(1), out field_index))
                                {
                                    text.Index = field_index;
                                    sym.UserFields.Add(text);
                                }
                            }
                            break;
                    }
                    index++;
                }
                else if (tokens[0].Value == "ALIAS")
                {
                    sym.Alias = StringUtils.After(lines[index], " ");
                    index++;
                }
                else if (tokens[0].Value == "$FPLIST")
                {
                    index++;
                    sym.Footprints = new List<string>();
                    while ((index < lines.Count) && (lines[index] != "$ENDFPLIST"))
                    {
                        sym.Footprints.Add(lines[index].Trim());
                        index++;
                    }
                    if ((index < lines.Count) && (lines[index] == "$ENDFPLIST"))
                    {
                        index++;
                    }
                }
                else
                {
                    // unknown?
                    index++;
                }
            }

            if ((index < lines.Count) && (lines[index] == "DRAW"))
            {
                index++;
                while ((index < lines.Count) && (lines[index] != "ENDDRAW"))
                {
                    tokens = Utils.Tokenise(lines[index]);

                    if ((tokens[0].Value == "X") && (tokens.Count != 12))
                    {
                        if ((tokens.Count == 13) && (tokens[11].Type == TokenType.Name))
                        {
                            sym_drawing_base drawing = sym_drawing_base.Parse(tokens);
                            if (drawing != null)
                                sym.Drawings.Add(drawing);
                        }
                        else
                            Console.WriteLine("{0} invalid pin def {1}:{2}", sym.Name, index, lines[index]);
                    }
                    else
                    {
                        sym_drawing_base drawing = sym_drawing_base.Parse(tokens);
                        if (drawing != null)
                            sym.Drawings.Add(drawing);
                    }

                    // todo:
                    index++;
                }
                if ((index < lines.Count) && (lines[index] == "ENDDRAW"))
                {
                    index++;
                }
            } // DRAW

            // expecting ENDDEF
            if ((index < lines.Count) && (lines[index] == "ENDDEF"))
            {
                index++;
            }

            return sym;
        }

        //
        void UpdateExtent (ref Extent extent, PointF pos)
        {
            if (pos.X < extent.Min.X)
                extent.Min.X = pos.X;
            if (pos.Y < extent.Min.Y)
                extent.Min.Y = pos.Y;

            if (pos.X > extent.Max.X)
                extent.Max.X = pos.X;
            if (pos.Y > extent.Max.Y)
                extent.Max.Y = pos.Y;
        }

        const float gridsize = 50f;

        float RoundToGrid (float size, float gridsize)
        {
            // rounds away from zero
            if (size < 0)
                return -gridsize * ((int)(Math.Abs(size) / gridsize) + 1);
            else
                return gridsize * ((int)(size / gridsize) + 1);
        }

        float RoundToGrid(float size)
        {
            return RoundToGrid(size, gridsize);
        }

        RectangleF GetPinRect(sym_pin pin)
        {
            return GetPinRect(pin, Offset);
        }

        RectangleF GetPinRect (sym_pin pin, float offset)
        {
            float size = pin.Name.Length * pin.SizeName;

            if (pin.Name == "~")
                size = 0;
            if (size != 0)
                size += offset;
            size += pin.Length;

            //
            //  o---| name
            //
            switch (pin.Orientation)
            {
                case "D":
                    return new RectangleF(pin.Pos.X - pin.SizeName / 2f, pin.Pos.Y - size, pin.SizeName, size);

                case "U":
                    return new RectangleF(pin.Pos.X - pin.SizeName / 2f, pin.Pos.Y, pin.SizeName, size);

                case "R":
                    return new RectangleF(pin.Pos.X, pin.Pos.Y - pin.SizeName / 2f, size, pin.SizeName);

                case "L":
                    return new RectangleF(pin.Pos.X - size, pin.Pos.Y - pin.SizeName / 2f, size, pin.SizeName);

            }
            return RectangleF.Empty;
        }

        public sym_pin FindPin (int unit, string name)
        {
            if (Drawings != null)
            foreach (sym_drawing_base sym in Drawings)
            {
                if (sym is sym_pin)
                {
                    sym_pin pin = sym as sym_pin;

                    if ( (pin.Unit == unit) && (pin.Name == name) )
                        return pin;
                }
            }
            return null;
        }

        public void CheckSymbol(bool Update = false)
        {
            // find max lengths
            const int m_up = 0;
            const int m_down = 1;
            const int m_left = 2;
            const int m_right = 3;

            float pin_len = 0;

            float[] max = new float[4];

            Extent outer_extent = new Extent();
            Extent inner_extent = new Extent();
            Extent text_extent = new Extent();

            Extent new_outer_extent = new Extent();
            Extent lower_pins = new Extent();

            //PointF min_pos = new PointF(0,0);
            //PointF max_pos = new PointF(0, 0);
            List<RectangleF> bounds = new List<RectangleF>();
            bool[] overlaps = new bool[4] { false, false, false, false };
            List<sym_pin> pins = new List<sym_pin>();

            // check only visible items?

            foreach (sym_drawing_base drawing in Drawings)
            {
                if (drawing is sym_pin)
                {
                    sym_pin pin = drawing as sym_pin;
                    pins.Add(pin);

                    float size = pin.Name.Length * pin.SizeName;
                    size += pin.Length;

                    pin_len = pin.Length;

                    //size = 50.0f * ((int)(size / 50.0f) + 1);

                    // U D L R
                    int index = 0;
                    switch (pin.Orientation)
                    {
                        case "D": index = m_up; break;
                        case "U": index = m_down; break;
                        case "R": index = m_left; break;
                        case "L": index = m_right; break;
                    }

                    bounds.Add(GetPinRect(pin));

                    UpdateExtent(ref outer_extent, pin.Pos);

                    //size = RoundToGrid(size);
                    if (size > max[index])
                        max[index] = size;
                }
            }

            // check for overlaps
            bool overlap = false;
            for (int i = 0; i < bounds.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (i != j)
                    {
                        RectangleF r = RectangleF.Intersect(bounds[i], bounds[j]);
                        if (!r.IsEmpty)
                        {
                            overlap = true;

                            Console.WriteLine("  pin {0}:{1} overlaps {2}:{3}", pins[i].PinNumber, pins[i].Name, pins[j].PinNumber, pins[j].Name);
                                 
                            if ((pins[i].Orientation == "U") || (pins[j].Orientation == "U"))
                                overlaps[m_down] = true;
                            else if ((pins[i].Orientation == "D") || (pins[j].Orientation == "D"))
                                overlaps[m_up] = false;
                            else
                                overlaps[m_left] = true;
                        }
                    }
                }
            }

            if (overlap)
            {
                Console.WriteLine("Symbol {0} has overlapping text", Name);
            }

            if (overlap && Update)
            {
                Console.WriteLine("Formatting {0}", Name);
                     
                for (int i=0; i < 4; i++)
                    max[i] = RoundToGrid (max[i]);

                new_outer_extent = new Extent();

                float width = max[m_left] + max[m_right];
                width = RoundToGrid(width, gridsize * 2);

                max[m_left] = width/2;
                max[m_right] = width/2;

                // Pass 1: set max sizes of horizontal pins
                foreach (sym_drawing_base drawing in Drawings)
                {
                    if (drawing is sym_pin)
                    {
                        sym_pin pin = drawing as sym_pin;
                        switch (pin.Orientation)
                        {
                            case "R":
                                {
                                    if (pin.Pos.X > -max[m_left])
                                    {
                                        pin.Pos.X = -max[m_left];
                                    }
                                    UpdateExtent(ref text_extent, new PointF(pin.Pos.X + pin_len, pin.Pos.Y - pin.SizeName / 2f));
                                    UpdateExtent(ref text_extent, new PointF(pin.Pos.X + pin_len + RoundToGrid(pin.Name.Length * pin.SizeName), pin.Pos.Y + pin.SizeName / 2f));
                                }
                                break;
                            case "L":
                                {
                                    if (pin.Pos.X < max[m_right])
                                    {
                                        pin.Pos.X = max[m_right];
                                    }
                                    UpdateExtent(ref text_extent, new PointF(pin.Pos.X - pin_len, pin.Pos.Y - pin.SizeName / 2f));
                                    UpdateExtent(ref text_extent, new PointF(pin.Pos.X - pin_len - RoundToGrid(pin.Name.Length * pin.SizeName), pin.Pos.Y + pin.SizeName / 2f));
                                }
                                break;
                        }
                        UpdateExtent(ref new_outer_extent, pin.Pos);
                    }
                }


                // Pass 2: set vertical pins
                foreach (sym_drawing_base drawing in Drawings)
                {
                    if (drawing is sym_pin)
                    {
                        sym_pin pin = drawing as sym_pin;
                        switch (pin.Orientation)
                        {
                            case "U": // bottom pins
                                {
                                    float p = RoundToGrid(text_extent.Min.Y) - max[m_down] ;
                                    if (pin.Pos.Y > p)
                                    {
                                        pin.Pos.Y = p;
                                    }

                                    RectangleF r = GetPinRect(pin);

                                    UpdateExtent(ref lower_pins, r.Location);
                                    UpdateExtent(ref lower_pins, new PointF(r.Right, r.Bottom));

                                }
                                break;

                            case "D": // top pins
                                {
                                    float p = RoundToGrid(text_extent.Max.Y) + max[m_up] ;
                                    if (pin.Pos.Y < p)
                                    {
                                        pin.Pos.Y = p;
                                    }
                                }
                                break;
                        }
                        UpdateExtent(ref new_outer_extent, pin.Pos);

                    }
                }

                // Pass 3: update the bounding rectangle
                foreach (sym_drawing_base drawing in Drawings)
                {
                    if (drawing is sym_rectangle)
                    {
                        sym_rectangle rect = drawing as sym_rectangle;
                        if ( (rect.P1.X - pin_len == outer_extent.Min.X) && (rect.P2.Y - pin_len == outer_extent.Min.Y))
                        {
                            rect.P1.X = new_outer_extent.Min.X + pin_len;
                            rect.P1.Y = new_outer_extent.Max.Y - pin_len;
                            //
                            rect.P2.X = new_outer_extent.Max.X - pin_len; 
                            rect.P2.Y = new_outer_extent.Min.Y + pin_len;
                        }
                    }
                }

                // reposition texts

                // designator at top left
                fReference.Text.Pos.X = new_outer_extent.Min.X + pin_len;
                fReference.Text.Pos.Y = new_outer_extent.Max.Y - pin_len + gridsize;

                // symbol name /value at lower left
                fValue.Text.Pos.X = new_outer_extent.Max.X - pin_len - RoundToGrid (fValue.Text.Value.Length * fValue.Text.FontSize);
                if (fValue.Text.Pos.X < RoundToGrid(lower_pins.Max.X))
                    fValue.Text.Pos.X = RoundToGrid(lower_pins.Max.X) + gridsize;
                fValue.Text.Pos.Y = new_outer_extent.Min.Y + pin_len - gridsize*2f;
            }

            //
        }

        public float RadToDeg (double rad)
        {
            return (float)( rad * 180.0 / Math.PI);
        }

        public float DegToRad(double deg)
        {
            return (float)(deg * Math.PI / 180.0);
        }

        public float CalcAngle (PointF p1, PointF p2)
        {
            float ang= (float)RadToDeg(Math.Atan2(p2.Y - p1.Y, p2.X - p1.X));
            if (ang < 0)
                ang += 360;
            return ang;
        }

        public void Swap (ref float a, ref float b)
        {
            float t = a;
            a = b;
            b = t;
        }

        // drawing functions
        public void Render(Canvas canvas)
        {
            canvas.Ydir = -1; // +ve Y is up the page
            canvas.Initialise();

            //

            Pen pen = new Pen(canvas.color_symbol_drawing, 2);
            Brush brush = new SolidBrush(canvas.color_symbol_drawing);
            Brush brush_text = new SolidBrush(canvas.color_symbol_text);

            //
            pen.Width = 2;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;

            //
            DrawText(canvas, fValue.Text, brush_text);
            DrawText(canvas, fReference.Text, brush_text);

            SolidBrush fill_brush = new SolidBrush(canvas.color_symbol_drawing);

            // draw background fills
            foreach (sym_drawing_base node in this.Drawings)
            {
                if (node.Fill == FillTypes.PenColor)
                    fill_brush.Color = canvas.color_symbol_drawing;
                else
                    fill_brush.Color = canvas.color_symbol_background;

                if (node is sym_rectangle)
                {
                    sym_rectangle gr_rect = node as sym_rectangle;
                    Point sp1 = canvas.ToScreen(gr_rect.P1);
                    Point sp2 = canvas.ToScreen(gr_rect.P2);

                    if (gr_rect.Fill != FillTypes.None)
                        canvas.g.FillRectangle(fill_brush, new Rectangle(Math.Min(sp1.X, sp2.X), Math.Min(sp1.Y, sp2.Y),
                            Math.Abs(sp2.X - sp1.X), Math.Abs(sp2.Y - sp1.Y)));
                }
                else if (node is sym_polygon)
                {
                    sym_polygon gr_poly = node as sym_polygon;

                    if (node.Fill != FillTypes.None)
                    {
                        Point[] points = new Point[gr_poly.NumVertex];

                        for (int i = 0; i < gr_poly.NumVertex; i++)
                        {
                            points[i] = canvas.ToScreen(gr_poly.Vertex[i]);
                        }
                        canvas.g.FillPolygon(fill_brush, points);
                    }
                }
                else if (node is sym_arc)
                {
                }
                else if (node is sym_circle)
                {
                }
            }

            Console.WriteLine("sym {0}", Name);

            // draw foreground items
            foreach (sym_drawing_base node in this.Drawings)
            {
                if (node.PenSize == 0)
                    pen.Width = canvas.ToScreen(5f);
                else
                    pen.Width = canvas.ToScreen(node.PenSize);

                if (node.Fill == FillTypes.PenColor)
                    fill_brush.Color = canvas.color_symbol_drawing;
                else
                    fill_brush.Color = canvas.color_symbol_background;

                if (node is sym_rectangle)
                {
                    sym_rectangle gr_rect = node as sym_rectangle;
                    Point sp1 = canvas.ToScreen(gr_rect.P1);
                    Point sp2 = canvas.ToScreen(gr_rect.P2);

                    if ( (sp2.X - sp1.X == 0) || (sp2.Y - sp1.Y == 0))
                        canvas.g.DrawLine(pen, sp1, sp2);
                    else
                        canvas.g.DrawRectangle (pen, new Rectangle(Math.Min(sp1.X,sp2.X), Math.Min(sp1.Y,sp2.Y), 
                            Math.Abs(sp2.X-sp1.X), Math.Abs(sp2.Y-sp1.Y) ));
                }
                else if (node is sym_arc)
                {
                    sym_arc arc = node as sym_arc;

                    Point sp1 = canvas.ToScreen(arc.Position);
                    int radius = canvas.ToScreen(arc.Radius);

#if Method1
                    float start, end;
                    start = NormalizeAnglePos (arc.ArcStart);
                    end = NormalizeAnglePos(arc.ArcEnd);

                    float angle = end - start;
                    float a_start = CalcAngle(arc.Position, arc.Start);
                    float a_end = CalcAngle(arc.Position, arc.End);

                    Console.WriteLine("arc      {0:f1} {1:f1} {2:f1}", arc.ArcStart, arc.ArcEnd, arc.ArcEnd-arc.ArcStart);
                    Console.WriteLine("arc norm {0:f1} {1:f1} {2:f1}", start, end, angle);
                    Console.WriteLine("arc calc {0:f1} {1:f1} ", a_start, a_end);

                    if (Math.Abs(angle) < 180)
                    {
                        if (angle != 0)
                            g.DrawArc(pen, new Rectangle(sp1.X - radius, sp1.Y - radius, radius * 2, radius * 2), -a_end, angle);
                    }
                    else
                    {
                        if (angle > 180)
                            angle -= 180;
                        g.DrawArc(pen, new Rectangle(sp1.X - radius, sp1.Y - radius, radius * 2, radius * 2), -a_start, angle);
                    }
#endif
                    //
#if Method2
                    float start, end;
                    start = NormalizeAnglePos(arc.ArcStart);
                    end = NormalizeAnglePos(arc.ArcEnd);

                    bool swap = MapAngles(ref start, ref end);

                    PointF pos1 = arc.Start;
                    PointF pos2 = arc.End;

                    if (swap)
                    {
                        pos1 = arc.End;
                        pos2 = arc.Start;
                    }

                    //float angle = NormalizeAnglePos(end - start);
                    float angle = end - start;

                    //start = CalcAngle(arc.Position, pos1);

                    Console.WriteLine("arc      {0:f1} {1:f1} {2:f1}", arc.ArcStart, arc.ArcEnd, arc.ArcEnd-arc.ArcStart);
                    Console.WriteLine("arc norm {0:f1} {1:f1} {2:f1} {3}", start, end, angle, swap);
                    //Console.WriteLine("arc calc {0:f1} {1:f1} ", a_start, a_end);

                    if (angle != 0)
                        g.DrawArc(pen, new Rectangle(sp1.X - radius, sp1.Y - radius, radius * 2, radius * 2), -end, angle);
#endif

//#if Method3

                    float a_start = CalcAngle(arc.Position, arc.Start);
                    float a_end = CalcAngle(arc.Position, arc.End);

                    float start, end;
                    start = NormalizeAnglePos(a_start);
                    end = NormalizeAnglePos(a_end);

                    float angle = end - start;
                    if (angle < 0)
                        angle += 360;

                    //
                    float test_angle = NormalizeAnglePos(arc.ArcEnd) - NormalizeAnglePos(arc.ArcStart);
                    test_angle = NormalizeAnglePos(test_angle);

                    if (angle != 0)
                    {
                        if ( (test_angle < 180) )
                            canvas.g.DrawArc(pen, new Rectangle(sp1.X - radius, sp1.Y - radius, radius * 2, radius * 2), -end, angle);
                        else
                        {
                            canvas.g.DrawArc(pen, new Rectangle(sp1.X - radius, sp1.Y - radius, radius * 2, radius * 2), -start, 360.0f-angle);
                        }
                    }

//#endif
                    //

                    if (false)
                    {
                        Pen tpen = new Pen(Color.Green);
                        tpen.Width = 3;
                        canvas.g.DrawLine(tpen, sp1, canvas.ToScreen(arc.Start));
                        tpen.Color = Color.Blue;
                        canvas.g.DrawLine(tpen, sp1, canvas.ToScreen(arc.End));
                    }
                }
                else if (node is sym_circle)
                {
                    sym_circle circle = node as sym_circle;

                    Point sp1 = canvas.ToScreen(circle.Position);
                    int radius = canvas.ToScreen(circle.Radius);

                    canvas.g.DrawEllipse (pen, new Rectangle(sp1.X - radius, sp1.Y - radius, radius * 2, radius * 2));

                }
                else if (node is sym_bezier)
                {

                }
                else if (node is sym_polygon)
                {
                    sym_polygon gr_poly = node as sym_polygon;

                    if (node.Fill == FillTypes.PenColor)
                    {
                        Point[] points = new Point[gr_poly.NumVertex];
                        for (int i = 0; i < gr_poly.NumVertex; i++)
                        {
                            points[i] = canvas.ToScreen(gr_poly.Vertex[i]);
                        }
                        canvas.g.FillPolygon(fill_brush, points);
                    }

                    for (int i = 0; i < gr_poly.NumVertex - 1; i++)
                    {
                        Point sp1 = canvas.ToScreen(gr_poly.Vertex[i]);
                        Point sp2 = canvas.ToScreen(gr_poly.Vertex[i + 1]);
                        canvas.g.DrawLine(pen, sp1, sp2);
                    }

                    if ( (node.Fill != FillTypes.None) && (gr_poly.NumVertex > 2))
                    {
                        Point sp1 = canvas.ToScreen(gr_poly.Vertex[0]);
                        Point sp2 = canvas.ToScreen(gr_poly.Vertex[gr_poly.NumVertex-1]);
                        canvas.g.DrawLine(pen, sp1, sp2);
                    }
                }
                else if (node is sym_pin)
                {
                    sym_pin pin = node as sym_pin;

                    if (pin.Visible)
                    {
                        Point sp1 = canvas.ToScreen(pin.Pos);
                        int radius = canvas.ToScreen(10f);

                        PointF offset = new PointF(0, 0);
                        switch (pin.Orientation)
                        {
                            case "L": offset.X = -pin.Length; break;
                            case "R": offset.X = pin.Length; break;
                            case "U": offset.Y = pin.Length; break;
                            case "D": offset.Y = -pin.Length; break;
                        }

                        Point sp2 = canvas.ToScreen(new PointF(pin.Pos.X + offset.X, pin.Pos.Y + offset.Y));

                        if (pin.PenSize==0)
                            pen.Width = canvas.ToScreen(5f);
                        else
                            pen.Width = canvas.ToScreen(pin.PenSize);
                        canvas.g.DrawLine(pen, sp1, sp2);

                        pen.Width = 2;
                        canvas.g.DrawEllipse(pen, new Rectangle(sp1.X - radius, sp1.Y - radius, radius * 2, radius * 2));

                        //
                        int FontHeight = canvas.ToScreen(pin.SizeName);
                        Font font = new Font("Arial", FontHeight, FontStyle.Regular, GraphicsUnit.Pixel);

                        TextBase text = new TextBase();

                        // horiz
                        if ((pin.Orientation == "L") || (pin.Orientation == "R"))
                        {
                            if ((this.ShowPinName) && (pin.Name != "~"))
                            {
                                // inside?
                                if (this.Offset != 0)
                                {
                                    if (pin.Orientation == "R")
                                    {
                                        text.Pos = new PointF(pin.Pos.X + offset.X + this.Offset, pin.Pos.Y);
                                        text.HorizAlignment = SymbolField.HorizAlign_Left;
                                        text.VertAlignment = SymbolField.VertAlign_Center;
                                    }
                                    else
                                    {
                                        text.Pos = new PointF(pin.Pos.X + offset.X - this.Offset, pin.Pos.Y);
                                        text.HorizAlignment = SymbolField.HorizAlign_Right;
                                        text.VertAlignment = SymbolField.VertAlign_Center;
                                    }
                                }
                                else
                                {
                                    if (pin.Orientation == "R")
                                    {
                                        text.Pos = new PointF(pin.Pos.X + pin.Length / 2, pin.Pos.Y);
                                        text.HorizAlignment = SymbolField.HorizAlign_Center;
                                        text.VertAlignment = SymbolField.VertAlign_Bottom;
                                    }
                                    else
                                    {
                                        text.Pos = new PointF(pin.Pos.X - pin.Length / 2 , pin.Pos.Y);
                                        text.HorizAlignment = SymbolField.HorizAlign_Center;
                                        text.VertAlignment = SymbolField.VertAlign_Bottom;
                                    }
                                }


                                text.FontSize = pin.SizeName;
                                text.Value = pin.Name;
                                DrawText(canvas, text, brush_text);
                            }

                            if (this.ShowPinNumber)
                            {
                                if (pin.Orientation == "L")
                                    text.Pos = new PointF(pin.Pos.X - pin.Length/2, pin.Pos.Y);
                                else
                                    text.Pos = new PointF(pin.Pos.X + pin.Length/2, pin.Pos.Y);

                                text.HorizAlignment = SymbolField.HorizAlign_Center;
                                if (this.Offset == 0)
                                    text.VertAlignment = SymbolField.VertAlign_Top;
                                else
                                    text.VertAlignment = SymbolField.VertAlign_Bottom;
                                text.FontSize = pin.SizeName;
                                text.Value = pin.PinNumber;
                                DrawText(canvas, text, brush);
                            }
                        }
                        else // vertical
                        {
                            if ((this.ShowPinName) && (pin.Name != "~"))
                            {
                                if (pin.Orientation == "U")
                                {
                                    text.Pos = new PointF(pin.Pos.X, pin.Pos.Y + offset.Y + this.Offset);
                                    text.HorizAlignment = SymbolField.HorizAlign_Left;
                                }
                                else
                                {
                                    text.Pos = new PointF(pin.Pos.X , pin.Pos.Y + offset.Y - this.Offset);
                                    text.HorizAlignment = SymbolField.HorizAlign_Right;
                                }
                                text.VertAlignment = SymbolField.VertAlign_Center;
                                text.FontSize = pin.SizeName;
                                text.Value = pin.Name;
                                text.Angle = 90;
                                DrawText(canvas, text, brush_text);
                            }

                            if (this.ShowPinNumber)
                            {
                                if (pin.Orientation == "D")
                                    text.Pos = new PointF(pin.Pos.X, pin.Pos.Y - pin.Length / 2);
                                else
                                    text.Pos = new PointF(pin.Pos.X, pin.Pos.Y + pin.Length / 2);

                                text.HorizAlignment = SymbolField.HorizAlign_Center;
                                text.VertAlignment = SymbolField.VertAlign_Bottom;
                                text.FontSize = pin.SizeName;
                                text.Value = pin.PinNumber;
                                text.Angle = 90;
                                DrawText(canvas, text, brush);
                            }

                        }
                    }
                }
                else if (node is sym_text)
                {
                    sym_text text = node as sym_text;
                    DrawText (canvas, text.Text, brush);
                }
            }

            //return bm;
        }

        // normalize angle to 0 to 360
        private float NormalizeAnglePos (float angle)
        {
            while (angle < 0)
                angle += 360;
            while (angle > 360)
                angle -= 360;
            return angle;
        }

        private float KiROUND (float x)
        {
            return x;
        }

        // code translated from Kicad
        // not sure why it is so complicated but it doesn't seem to be needed anyway
        private bool MapAngles(ref float aAngle1, ref float aAngle2)
        {
            float Angle, Delta;
            double x, y, t;
            bool swap = false;

            float x1 = 1;
            float y1 = 0;
            float x2 = 0;
            float y2 = 1;

            Delta = aAngle2 - aAngle1;
            if( Delta >= 180 )
            {
                aAngle1 -= 0.1f;
                aAngle2 += 0.1f;
            }

            x = Math.Cos(DegToRad( aAngle1 ) );
            y = Math.Sin(DegToRad( aAngle1 ) );
            t = x* x1 + y* y1;
            y = x* x2 + y* y2;
            x = t;
            aAngle1 = KiROUND(RadToDeg(Math.Atan2(y, x)));

            x = Math.Cos(DegToRad( aAngle2 ) );
            y = Math.Sin(DegToRad( aAngle2 ) );
            t = x * x1 + y * y1;
            y = x * x2 + y * y2;
            x = t;
            aAngle2 = KiROUND(RadToDeg(Math.Atan2(y, x)));

            aAngle1 = NormalizeAnglePos(aAngle1);
            aAngle2 = NormalizeAnglePos(aAngle2);

            if ( aAngle2 < aAngle1 )
                aAngle2 += 360;

            if ( aAngle2 - aAngle1 > 180 ) // Need to swap the two angles
            {
                Angle = aAngle1;
                aAngle1 = aAngle2;
                aAngle2 = Angle;

                aAngle1 = NormalizeAnglePos(aAngle1);
                aAngle2 = NormalizeAnglePos(aAngle2);

                if (aAngle2 < aAngle1)
                    aAngle2 += 360;

                swap = true;
            }

            if( Delta >= 180 )
            {
                aAngle1 += 0.1f;
                aAngle2 -= 0.1f;
            }

            return swap;
        }


        private void DrawText (Canvas canvas, TextBase text, Brush brush)
        {
            bool debug = true;

            if (text.Visible)
            {
                Point sp1 = canvas.ToScreen(text.Pos);
                FontStyle style = FontStyle.Regular;

                int FontHeight = canvas.ToScreen(text.FontSize);
                if (text.Italic) style |= FontStyle.Italic;
                if (text.Bold) style |= FontStyle.Bold;

                FontFamily family = new FontFamily("Arial");
                float emHeight = FontHeight * family.GetEmHeight(style) / family.GetLineSpacing(style);
                Font font = new Font(family, emHeight, GraphicsUnit.Pixel);

                //

                Rectangle srect = new Rectangle();
                SizeF s = canvas.g.MeasureString(text.Value, font);

                srect.X = 0;
                srect.Y = 0;
                srect.Height = (int)s.Height + 5;
                srect.Width = (int)s.Width + 5;

                switch (text.HorizAlignment)
                {
                    case SymbolField.HorizAlign_Left: break;
                    case SymbolField.HorizAlign_Center:
                        srect.X = (int)(-s.Width / 2);
                        break;
                    case SymbolField.HorizAlign_Right:
                        srect.X = (int)(-s.Width); 
                        break;
                }

                switch (text.VertAlignment)
                {
                    case SymbolField.VertAlign_Top:
                        break;
                    case SymbolField.VertAlign_Center:
                        srect.Y = (int)(-s.Height / 2);
                        break;
                    case SymbolField.VertAlign_Bottom:
                        srect.Y = (int)(-s.Height);
                        break;
                }

                StringFormat strF = new StringFormat();
                strF.Alignment = StringAlignment.Center;
                strF.LineAlignment = StringAlignment.Center;

                //switch (text.HorizAlignment)
                //{
                //    case symbol_text.HorizAlign_Left:
                //        strF.Alignment = StringAlignment.Near;
                //        break;
                //    case symbol_text.HorizAlign_Center:
                //        strF.Alignment = StringAlignment.Center;
                //        break;
                //    case symbol_text.HorizAlign_Right:
                //        strF.Alignment = StringAlignment.Far;
                //        break;
                //}

                //switch (text.VertAlignment)
                //{
                //    case symbol_text.VertAlign_Top:
                //        strF.LineAlignment = StringAlignment.Far;
                //        break;
                //    case symbol_text.VertAlign_Center:
                //        strF.LineAlignment = StringAlignment.Center;
                //        break;
                //    case symbol_text.VertAlign_Bottom:
                //        strF.LineAlignment = StringAlignment.Near;
                //        break;
                //}


                if (debug)
                    DrawCross(canvas, sp1);

                canvas.g.TranslateTransform(sp1.X, sp1.Y);

                if (text.Angle == 90)
                    canvas.g.RotateTransform(-90);

                canvas.g.DrawString(text.Value, font, brush, srect, strF);

                if (debug)
                {
                    // draw bounding box (?)
                    Pen pen = new Pen(Color.Gray, 1);
                    pen.DashStyle = DashStyle.Dot;
                    canvas.g.DrawRectangle(pen, srect.Left, srect.Top, srect.Width, srect.Height);
                }

                canvas.g.ResetTransform();
            }
        }

        public int GetTextHeight (Canvas canvas, float TextSize)
        {
            int result = 1;
            int maxHeight = canvas.ToScreen(TextSize);

            Font font = new Font("Arial", result, FontStyle.Regular, GraphicsUnit.Pixel);

            while (font.FontFamily.GetCellAscent(FontStyle.Regular) + font.FontFamily.GetCellDescent(FontStyle.Regular) < maxHeight)
            {
                result++;
                font.Dispose();
                font = new Font("Arial", result, FontStyle.Regular, GraphicsUnit.Pixel);
            }
            font.Dispose();
            return result;
        }


        public void DrawCross (Canvas canvas, Point p)
        {
            canvas.g.DrawLine(Pens.Blue, new Point(p.X - 5, p.Y), new Point(p.X + 5, p.Y));
            canvas.g.DrawLine(Pens.Blue, new Point(p.X, p.Y-5), new Point(p.X, p.Y+5));
        }

        public int CompareTo(Symbol other)
        {
            return string.Compare(Name, other.Name, true);
        }
    }

    // N, F, f
    public enum FillTypes {
        None,   // N
        PenColor,   // F
        BackgroundColor //f
    };  

    //    
    // Create a sorter that implements the IComparer interface. 
    public class SymbolNameSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            Symbol tx = x as Symbol;
            Symbol ty = y as Symbol;

            int res = string.Compare(tx.Name, ty.Name, true);
            return res;
        }
    }

}
