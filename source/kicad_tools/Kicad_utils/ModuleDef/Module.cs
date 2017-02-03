using System;
using System.Collections.Generic;

using System.Drawing;
using System.IO;
using OpenTK;
using SExpressions;

using Cad2D;

namespace Kicad_utils.ModuleDef
{
    public class Module
    {
        //
        public string Name;
        public bool locked;         // pcb: default: false
        public bool placed;         // pcb: default: false
        public string layer;        // should be Front or Back, or actual name in PCB
        public uint tedit;          // used in pcb file
        public uint tstamp;         // used in pcb file
        public Position position;   // aka "at":  0,0 in module, used in pcb file
        public string attr;         // smd | virtual | "" = thru-hole
        public string description;  // descr <string>
        public string tags;         // list?
        public string path;         // pcb

        public int autoplace_cost90;
        public int autoplace_cost180;
        public float solder_mask_margin;
        public float solder_paste_margin;
        public int solder_paste_ratio;
        public float clearance;
        public int zone_connect;
        public float thermal_width;
        public float thermal_gap;

        public fp_text Reference;
        public fp_text Value;

        public List<fp_text> UserText;
        public List<fp_shape> Borders;  // lines,circles,arcs etc
        public List<pad> Pads;

        public List<model> CadModels;

        public PointF At
        {
            get
            {
                if (position == null)
                    return new PointF(0, 0);
                else
                    return position.At;
            }
            set
            {
                if (position == null)
                    position = new Position(value.X, value.Y);
                else
                    position.At = new PointF(value.X, value.Y);
            }
        }
        // pcb

        public Module ()
        {
            UserText = new List<fp_text>();
            Borders = new List<fp_shape>();
            Pads = new List<pad>();
            CadModels = new List<model>();

            Reference = new fp_text("reference", "Ref", new PointF(0, 0), "F.SilkS", new SizeF(1.5f, 1.5f), 0.15f, false);
            Value = new fp_text("value", "Val", new PointF(0, 0), "F.Fab", new SizeF(1.5f, 1.5f), 0.15f, false);
        }

        public Module Clone(bool is_pcb = false)
        {
            Module result;
            SNodeBase sexpr;

            sexpr = GetSExpression(is_pcb);
            result = Parse(sexpr);
            return result;
        }

        public void SaveToFile(string filename)
        {
            filename = Path.ChangeExtension(filename, "kicad_mod");

            SExpression RootNode = GetSExpression(false);

            RootNode.WriteToFile(filename);
        }

        public void error(string message)
        {
            Console.WriteLine("Parse error: ", message);
        }


        // this performs the KiCad "flip" operation
        public void FlipX (PointF pos)
        {
            //swap front to back, and flip coords about X axis

            //position.Rotation = 360 - position.Rotation;

            //TODO: this needs list of pcb layers
            layer = Layer.FlipLayer(layer);

            foreach (pad pad in Pads)
            {
                pad.FlipX(pos);
            }
            
            Reference.FlipX(pos);
            Value.FlipX(pos);

            if (UserText != null)
                foreach (fp_text text in UserText)
                    text.FlipX(pos);

            if (Borders != null)
                foreach (fp_shape shape in Borders) // todo...
                    shape.FlipX(pos);

            

            // public List<model> CadModels; //?

        }
    
        public void RotateBy (float angle)
        {
            position.Rotation = MathUtil.NormalizeAngle (position.Rotation + angle);

            Reference.RotateBy(angle);
            Value.RotateBy(angle);

            if (UserText != null)
                foreach (fp_text text in UserText)
                    text.RotateBy(angle);

        }

        void addToExtent (ref PointF min, ref PointF max, PointF p)
        {
            if (p.X < min.X)
                min.X = p.X;
            if (p.Y < min.Y)
                min.Y = p.Y;

            if (p.X > max.X)
                max.X = p.X;
            if (p.Y > max.Y)
                max.Y = p.Y;

        }

        public RectangleF GetExtent (String layer)
        {
            PointF min = new PointF(0, 0);
            PointF max = new PointF(0, 0);

            if (Borders != null)
            foreach (fp_shape shape in Borders)
                if (shape.layer.Contains(layer))
                {
                    if (shape is fp_line)
                    {
                        fp_line line = shape as fp_line;
                        addToExtent(ref min, ref max, line.start);
                        addToExtent(ref min, ref max, line.end);
                    }
                }

            if (Pads != null)
            foreach (pad a_pad in Pads)
                if (a_pad.layers.Contains(layer))
                {
                    PointF pos = new PointF(a_pad.position.At.X - a_pad.size.Width / 2,
                        a_pad.position.At.Y - a_pad.size.Height / 2);

                    addToExtent(ref min, ref max, pos);

                    pos = new PointF(a_pad.position.At.X + a_pad.size.Width / 2,
                        a_pad.position.At.Y + a_pad.size.Height / 2);

                    addToExtent(ref min, ref max, pos);
                }

            if ((max.X - min.X==0) && (max.Y - min.Y==0))
                return RectangleF.Empty;
            else
                return new RectangleF(min.X, min.Y, max.X - min.X, max.Y - min.Y);
        }

        public RectangleF GetModuleExtent()
        {
            RectangleF courtyard = GetExtent(Layer.Courtyard);

            if (courtyard == RectangleF.Empty)
            {
                RectangleF extentSilk = GetExtent(Layer.SilkScreen);
                RectangleF extentPads = GetExtent(layer);

                courtyard = RectangleF.Union(extentSilk, extentPads);

                courtyard.Inflate(0.25f, 0.25f);

                SnapToGrid(ref courtyard, new PointF(0.05f, 0.05f));    
            }

            return courtyard;
        }

        float rsnap(double x, double grid)
        {
            double result;
            result = Math.Floor((x + grid / 2) / grid) * grid;
            return (float)result;
        }


        public void SnapToGrid (ref RectangleF rect, PointF GridSize)
        {
            PointF lt = rect.Location;
            PointF rb = new PointF(rect.Right, rect.Bottom);

            lt.X = rsnap(lt.X, GridSize.X); // round down?
            lt.Y = rsnap(lt.Y, GridSize.Y);

            rb.X = rsnap(rb.X, GridSize.X); // round up?
            rb.Y = rsnap(rb.Y, GridSize.Y);

            rect = RectangleF.FromLTRB(lt.X, lt.Y, rb.X, rb.Y);
        }

        public void AddBorderBox (string Layer, RectangleF box, float Width)
        {
            Cad2D.Polygon poly = new Cad2D.Polygon();

            poly.Vertices.Add(new Vector2(box.Left, box.Top));
            poly.Vertices.Add(new Vector2(box.Right, box.Top));
            poly.Vertices.Add(new Vector2(box.Right, box.Bottom));
            poly.Vertices.Add(new Vector2(box.Left, box.Bottom));

            //poly.Translate(new Vector2(-x / 2, -y / 2));

            AddBorderPolygon(poly, Layer, Width);
        }

        public void AddBorderPolygon(Cad2D.Polygon Polygon, string Layer, float Width)
        {
            //PointF last_pos;

            for (int j = 0; j < Polygon.Vertices.Count; j++) //? -1
            {
                PointF p0 = new PointF(Polygon.Vertices[j].X, Polygon.Vertices[j].Y);
                int jp1 = (j + 1) % Polygon.Vertices.Count;
                PointF p1 = new PointF(Polygon.Vertices[jp1].X, Polygon.Vertices[jp1].Y);

                Borders.Add(new fp_line(p0, p1, Layer, Width));
            }
        }


        public static Module Parse(SNodeBase Node)
        {
            Module result = null;
            SExpression RootNode = Node as SExpression;

            if ((RootNode != null) && (RootNode.Name == "module"))
            {
                int index = 0;

                result = new Module();
                result.Name = (RootNode.Items[index++] as SNodeAtom).Value;

                while (index < RootNode.Items.Count)
                {
                    SExpression sexpr = RootNode.Items[index] as SExpression;

                    switch (sexpr.Name)
                    {
                        case "layer":
                            result.layer = Layer.ParseLayer(RootNode.Items[index]);
                            break;
                        case "tedit":
                            result.tedit = sexpr.GetUintHex();
                            break;
                        case "tstamp":
                            result.tstamp = sexpr.GetUintHex();
                            break;
                        case "descr":
                            result.description = sexpr.GetString();
                            break;

                        case "tags":
                            result.tags = sexpr.GetString();
                            break;

                        case "path":
                            result.path = sexpr.GetString();
                            break;

                        case "at":
                            result.position = Position.Parse (sexpr);
                            break;

                        case "attr":
                            result.attr = sexpr.GetString();
                            break;

                        case "fp_text":
                            switch ((sexpr.Items[0] as SNodeAtom).Value)
                            {
                                case "reference":
                                    result.Reference = fp_text.Parse(RootNode.Items[index]);
                                    break;
                                case "value":
                                    result.Value = fp_text.Parse(RootNode.Items[index]);
                                    break;

                                default:
                                    //todo usertext
                                    if (result.UserText == null)
                                        result.UserText = new List<fp_text>();
                                    result.UserText.Add(fp_text.Parse(RootNode.Items[index]));
                                    break;
                            }
                            break;

                        case "fp_line":
                        case "fp_arc":
                        case "fp_circle":
                        case "fp_polygon":
                        case "fp_poly":
                            {
                                fp_shape shape = null;
                                switch (sexpr.Name)
                                {
                                    case "fp_line":
                                        shape = fp_line.Parse(RootNode.Items[index]);
                                        break;
                                    case "fp_arc":
                                        shape = fp_arc.Parse(sexpr);
                                        break;
                                    case "fp_circle":
                                        shape = fp_circle.Parse(sexpr);
                                        break;
                                    case "fp_polygon":
                                    case "fp_poly":
                                        shape = fp_polygon.Parse(sexpr);
                                        break;
                                }

                                if (shape != null)
                                {
                                    if (result.Borders == null)
                                        result.Borders = new List<fp_shape>();
                                    result.Borders.Add(shape);
                                }
                            }
                            break;

                        case "pad":
                            pad pad = pad.Parse(RootNode.Items[index]);
                            if (pad != null)
                            {
                                if (result.Pads == null)
                                    result.Pads = new List<pad>();
                                result.Pads.Add(pad);
                            }
                            break;

                        case "model":
                            if (result.CadModels == null)
                                result.CadModels = new List<model>();
                            result.CadModels.Add(model.Parse(sexpr));
                            break;
                    }
                    index++;
                }
            }

            return result;
        }

        public void AddCadModel (model cadModel)
        {
            if (CadModels == null)
                CadModels = new List<model>();

            CadModels.Add(cadModel);
        }

        public static Module LoadFromFile(string filename)
        {
            //TODO
            SExpression RootNode = new SExpression();
            
            RootNode.LoadFromFile(filename);

            if ((RootNode != null) && (RootNode.Name == "module"))
            {
                return Parse(RootNode);                    
            }
            else
            {
                // error("Expected 'module'");

                return null;
            }
        }

        //
        public void ConvertToLegacyModule(out List<string> text)
        {
            text = new List<string>();

            text.Add("$MODULE " + Name);
            text.Add(string.Format("Po 0 0 0 {0} {1} 00000000 ~~", Layer.GetLayerNumber_Legacy(layer), Utils.GetTimeStamp(DateTime.Now)));
            text.Add(string.Format("Li {0}", Name));
            // Cd
            // kw
            text.Add(string.Format("Sc 0"));
            // 
            text.Add(string.Format("AR"));
            text.Add(string.Format("Op 0 0 0"));
            text.Add(string.Format("T0 {0}", Reference.ToString() ));
            text.Add(string.Format("T1 {0}", Value.ToString()));
            // T2 user text
            if (UserText != null)
                foreach (fp_text t in UserText)
                    text.Add(string.Format("T2 {0}", t.ToString()));

            // borders
            foreach (fp_shape shape in Borders)
                text.Add(shape.ToString());

            // pads
            foreach (pad a_pad in Pads)
                a_pad.WriteText(text);

            //TODO: more than one model?
            if ( (CadModels != null) && (!string.IsNullOrEmpty(CadModels[0].path) ) )
            {
                text.Add("$SHAPE3D");
                text.Add(string.Format("Na \"{0}\"", CadModels[0].path));
                //TODO?
                text.Add(string.Format("Sc 1 1 1"));    
                text.Add(string.Format("Of 0 0 0"));
                text.Add(string.Format("Ro 0 0 0"));
                text.Add(string.Format("$EndSHAPE3D"));
            }

            text.Add("$ENDMODULE " + Name);
        }

        public bool CheckBorders()
        {
            bool result = true;

            for (int index = 0; index < Borders.Count; index++)
            {
                fp_line line0 = Borders[index] as fp_line;
                fp_line line1 = Borders[(index + 1) % Borders.Count] as fp_line;

                line0.end.X = (float)Math.Round(line0.end.X, 1);
                line0.end.Y = (float)Math.Round(line0.end.Y, 1);

                line1.start.X = line0.end.X;
                line1.start.Y = line0.end.Y;

                if ((line0.end.X == line1.start.X) &&
                    (line0.end.Y == line1.start.Y)
                    )
                {
                    // ok
                }
                else
                {
                    result = false;
                    Console.WriteLine(string.Format("mismatch {0},{1}  {2},{3}", line0.end.X, line0.end.Y, line1.start.X, line1.start.Y));
                }
            }
            return result;
        }


        //
        public SExpression GetSExpression(bool is_pcb)
        {
            SExpression result = new SExpression();

            result.Name = "module";
            result.Items = new List<SNodeBase>();
            result.Items.Add(new SNodeAtom(Name));


            //result.Items.Add(new SNodeAtom(description));
            //tags

            result.Items.Add(new SExpression("layer", layer));

            if (is_pcb)
            {
                result.Items.Add(new SExpression("tedit", tedit.ToString("X8")));
                result.Items.Add(new SExpression("tstamp", tstamp.ToString("X8")));
            }

            result.Items.Add(new SExpression("at", new List<SNodeBase>{
                new SNodeAtom(position.At.X), 
                new SNodeAtom(position.At.Y), 
                new SNodeAtom(position.Rotation)
            }));

            if (description != null)
                result.Items.Add(new SExpression("descr", description));

            if (tags != null)
                result.Items.Add(new SExpression("tags", tags));

            if (!string.IsNullOrEmpty(path))
                result.Items.Add(new SExpression("path", path));

            if (attr != null)
                result.Items.Add(new SExpression("attr", attr));

            if (Reference != null)
                result.Items.Add(Reference.GetSExpression());

            if (Value != null)
                result.Items.Add(Value.GetSExpression());

            if (UserText != null)
                foreach (fp_text text in UserText)
                    result.Items.Add(text.GetSExpression());

            if (Borders != null)
                foreach (fp_shape shape in Borders)
                    result.Items.Add(shape.GetSExpression());

            if (Pads != null)
                foreach (pad _pad in Pads)
                    result.Items.Add(_pad.GetSExpression());

            if (CadModels != null)
                foreach (model mod in CadModels)
                    result.Items.Add(mod.GetSExpression());

            return result;
        }


        // 
        public static List<SExpression> GetSExpressionList(List<Module> modules)
        {
            List<SExpression> result = new List<SExpression>();

            foreach (Module module in modules)
            {
                result.Add(module.GetSExpression(true));
            }
            return result;
        }
    }
}
