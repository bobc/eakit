using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Drawing;
using System.Globalization;

using CParser;
using CadCommon;


namespace SExpressions
{
    [Flags]
    public enum Types
    {
        None = 0x0,
        List = 0x1,
        Atom = 0x2,
        Symbol = 0x4,
        String = 0x8,
        Number = 0x10,
    }

    public interface ISExpression
    {
        string ToString();

        Types Type { get; }
    }


    public class SExpression : SNodeBase
    {
        public string Name;

        public List<SNodeBase> Items;

        public SExpression()
        {
        }

        public SExpression(string name, int value)
        {
            this.Name = name;
            this.Items = new List<SNodeBase>() { new SNodeAtom(value) };
        }

        public SExpression(string name, uint value)
        {
            this.Name = name;
            this.Items = new List<SNodeBase>() { new SNodeAtom(value) };
        }

        public SExpression(string name, float value)
        {
            this.Name = name;
            this.Items = new List<SNodeBase>() { new SNodeAtom(value) };
        }

        public SExpression(string name, string value)
        {
            this.Name = name;
            this.Items = new List<SNodeBase>() { new SNodeAtom(value) };
        }

        public SExpression(string name, bool value)
        {
            this.Name = name;
            this.Items = new List<SNodeBase>() { new SNodeAtom(value ? "true" : "false") };
        }

        public SExpression(string name, PointF value)
        {
            this.Name = name;
            this.Items = new List<SNodeBase>();
            this.Items.Add(new SNodeAtom(value.X));
            this.Items.Add(new SNodeAtom(value.Y));
        }

        public SExpression(string name, SizeF value)
        {
            this.Name = name;
            this.Items = new List<SNodeBase>();
            this.Items.Add(new SNodeAtom(value.Width));
            this.Items.Add(new SNodeAtom(value.Height));
        }

        public SExpression(string name, RectangleF value)
        {
            this.Name = name;
            this.Items = new List<SNodeBase>();
            this.Items.Add(new SNodeAtom(value.X));
            this.Items.Add(new SNodeAtom(value.Y));
            this.Items.Add(new SNodeAtom(value.Width));
            this.Items.Add(new SNodeAtom(value.Width));
        }

        public SExpression(string name, List<SNodeBase> nodes)
        {
            this.Name = name;
            this.Items = nodes;
        }

        public string GetValue ()
        {
            if (Items.Count > 0 )
            {
                if (Items[0] is SNodeAtom)
                    return (Items[0] as SNodeAtom).Value;
                else
                    return "";
            }
            else
                return "";
        }



        public override string ToString()
        {
            string result  = "(" + Name;

            if (Items.Count != 0)
            {
                if (Items[0] is SNodeAtom)
                    result += " " + (Items[0] as SNodeAtom).Value;
                else
                    result += " ...";
            }
            result += ")";
            return result;
        }

        public bool LoadFromFile(string Filename)
        {
            bool result = false;
            string[] lines;

            lines = File.ReadAllLines(Filename);

            CLex lexer = new CLex();
            lexer.IgnoreWhitespace = true;
            lexer.IgnoreEol = true;
            lexer.Initialise(Filename);

            SNodeBase node = null;

            node = ParseNode(lexer);

            //
            // Print(node);

            //
            if (node != null)
            {
                //if (node is SNodeList)
                //{
                //    SNodeList list = node as SNodeList;

                //    if (list.Items.Count > 1)
                //    {
                //        if (list.Items[0] is SNodeAtom)
                //        {
                //            SNodeAtom value = list.Items[0] as SNodeAtom;
                //            this.Name = value.Value;
                //        }

                //        this.Items = new List<SNodeBase>();
                //        for (int index = 1; index < list.Items.Count; index++)
                //        {
                //            this.Items.Add(list.Items[index]);
                //        }
                //    }
                //}

            }

            if (node is SExpression)
            {
                SExpression sexpr = node as SExpression;

                this.Name = sexpr.Name;
                this.Items = sexpr.Items;
            }

            //
            return result;
        }

        private SNodeBase ParseNode(CLex lexer)
        {
            if ((lexer.CurToken.Type == TokenType.Symbol) &&
                (lexer.CurToken.Value == "("))
            {
                //return ParseList(lexer);
                SNodeBase node = ParseList(lexer);

                if (node is SNodeList)
                {
                    SNodeList list = node as SNodeList;
                    //
                    if (list.Items.Count > 0)
                    {
                        SExpression sexpr = new SExpression();

                        if (list.Items[0] is SNodeAtom)
                        {
                            SNodeAtom value = list.Items[0] as SNodeAtom;
                            sexpr.Name = value.Value;
                        }

                        sexpr.Items = new List<SNodeBase>();
                        for (int index = 1; index < list.Items.Count; index++)
                        {
                            sexpr.Items.Add(list.Items[index]);
                        }

                        //
                        return sexpr;
                    }
                    else
                        return node;
                }
                else
                    return node;
            }
            else
            {
                // string
                SNodeAtom node;

                if (lexer.CurToken.Type == TokenType.StringLiteral)
                {
                    string value = lexer.CurToken.Value;

                    if (value.StartsWith("\"") && value.EndsWith("\""))
                    {
                        value = value.Substring(1, value.Length - 2);
                        // descape " and \
                    }

                    node = new SNodeAtom(value);
                }
                else
                    node = new SNodeAtom(lexer.CurToken.Value);
                lexer.GetToken();
                return node;
            }
        }

        private SNodeBase ParseList(CLex lexer)
        {
            SNodeList node = new SNodeList();
            node.Items = new List<SNodeBase>();
            lexer.GetToken();

            while ((lexer.CurToken.Type != TokenType.EOF) &&
                    !((lexer.CurToken.Type == TokenType.Symbol) &&
                        (lexer.CurToken.Value == ")")
                      )
                  )
            {
                //
                node.Items.Add(ParseNode(lexer));
            }

            // == )
            lexer.GetToken();

            return node;
        }

        //
        public void WriteToFile(string filename)
        {
            List<string> lines = new List<string>();
            Output(lines, 0);
            File.WriteAllLines(filename, lines);
        }

        public void Print(SNodeBase x)
        {
            Print_sub(x, 0);
        }

        private void Print_sub(SNodeBase x, int depth)
        {
            Console.Write(new String(' ', depth));

            //if ((x.Type & Types.Atom) == Types.Atom)
            if (x is SNodeAtom)
            {
                //if ((x.Type & Types.String) == Types.String)
                if (x is SNodeAtom)
                {
                    Console.WriteLine(String.Format("{0}", x.ToString()));
                }
                else if ((x.Type & Types.Symbol) == Types.Symbol)
                {
                    Console.WriteLine(String.Format("symbol: {0}", x.ToString()));
                }
                else if ((x.Type & Types.Number) == Types.Number)
                {
                    Console.WriteLine(String.Format("number: {0}", x.ToString()));
                }
            }
            //else if ((x.Type & Types.List) == Types.List)
            else if (x is SNodeList)
            {
                Console.WriteLine("(");
                SNodeList list = x as SNodeList;
                foreach (SNodeBase y in list.Items)
                {
                    Print_sub(y, depth + 1);
                }
                Console.WriteLine(new String(' ', depth) + ")");
            }
        }


        public override void Output(List<string> lines, int depth)
        {
            string line = new String(' ', depth) + "(" + Name;

            // sub
            if (Items != null)
            {
                foreach (SNodeBase item in Items)
                {
                    if (item is SExpression)
                    {
                        if (line != "")
                            lines.Add(line);
                        line = "";
                        item.Output(lines, depth + 1);
                    }
                    else if (item is SNodeList)
                    {
                        if (line != "")
                            lines.Add(line);
                        line = "";
                        item.Output(lines, depth + 1);
                    }
                    else
                    {
                        line = line + " " + item.ToString();
                    }
                }

                //if (!string.IsNullOrEmpty(line))
                //{
                //    line += ")";
                //    //lines.Add(line);
                //}
            }

            if (line == "") line = new String(' ', depth);
            line += ")";

            lines.Add(line);
        }


        //

        public static string GetString (SNodeBase baseNode)
        {
            if (baseNode is SExpression)
            {
                SExpression expr = baseNode as SExpression;
                if ((expr.Items != null) && (expr.Items.Count > 0) && (expr.Items[0] is SNodeAtom))
                {
                    return (expr.Items[0] as SNodeAtom).Value;
                }
                else
                    return "";
            }
            else
                return "";  // error
        }

        // eg "at 0 0"
        public PointF GetPointF()
        {
            if ((this.Items != null) && (this.Items.Count >= 2) && (this.Items[0] is SNodeAtom))
            {
                PointF result = new PointF();
                float val =  0;
                float.TryParse((this.Items[0] as SNodeAtom).Value, out val);
                result.X = val;

                val = 0;
                float.TryParse((this.Items[1] as SNodeAtom).Value, out val);
                result.Y = val;

                return result;
            }
            else
                return new PointF(0, 0);  // error
        }

        public Point3DF GetPoint3DF()
        {
            if ((this.Items != null) && (this.Items.Count >= 3) && (this.Items[0] is SNodeAtom))
            {
                Point3DF result = new Point3DF();
                float val = 0;
                float.TryParse((this.Items[0] as SNodeAtom).Value, out val);
                result.X = val;

                val = 0;
                float.TryParse((this.Items[1] as SNodeAtom).Value, out val);
                result.Y = val;

                val = 0;
                float.TryParse((this.Items[2] as SNodeAtom).Value, out val);
                result.Z = val;

                return result;
            }
            else
                return new Point3DF(0, 0, 0);  // error
        }

        public SizeF GetSizeF()
        {
            PointF p = GetPointF();
            return new SizeF(p.X, p.Y);
        }

        public RectangleF GetRectF()
        {
            float x, y, width, height;

            try {
                float.TryParse((this.Items[0] as SNodeAtom).Value, out x);
                float.TryParse((this.Items[1] as SNodeAtom).Value, out y);
                float.TryParse((this.Items[2] as SNodeAtom).Value, out width);
                float.TryParse((this.Items[3] as SNodeAtom).Value, out height);

                return new RectangleF(x, y, width, height);
            }
            catch (Exception)
            {
                return RectangleF.Empty;
            }
        }

        public float GetFloat()
        {
            return GetFloat(0);
        }

        public float GetFloat(int index)
        {
            if ((this.Items != null) && (index < this.Items.Count) && (this.Items[index] is SNodeAtom))
            {
                float result = 0;
                float.TryParse((this.Items[index] as SNodeAtom).Value, out result);
                
                return result;
            }
            else
                return 0;  // error
        }

        public int GetInt()
        {
            if ((this.Items != null) && (this.Items.Count >= 1) && (this.Items[0] is SNodeAtom))
            {
                int result = 0;
                int.TryParse((this.Items[0] as SNodeAtom).Value, out result);

                return result;
            }
            else
                return 0;  // error
        }

        public uint GetUint()
        {
            if ((this.Items != null) && (this.Items.Count >= 1) && (this.Items[0] is SNodeAtom))
            {
                uint result = 0;
                uint.TryParse((this.Items[0] as SNodeAtom).Value, out result);

                return result;
            }
            else
                return 0;  // error
        }

        public uint GetUintHex()
        {
            if ((this.Items != null) && (this.Items.Count >= 1) && (this.Items[0] is SNodeAtom))
            {
                uint result = 0;

                try {
                    result = Convert.ToUInt32((this.Items[0] as SNodeAtom).Value, 16);
                }
                catch
                {
                    result = 0;
                }
                //uint.TryParse((this.Items[0] as SNodeAtom).Value, out result);

                return result;
            }
            else
                return 0;  // error
        }

        public string  GetString()
        {
            return GetString(0);
        }

        public string GetString(int index)
        {
            if ((this.Items != null) && (index < this.Items.Count) && (this.Items[index] is SNodeAtom))
            {
                return (this.Items[index] as SNodeAtom).Value;
            }
            else
                return "";
        }


    }


    //public class SNodeNumber : SNodeBase
    //{
    //    public string sNumber;
    //    public int iNumber;

    //    public SNodeNumber(int number)
    //    {
    //        sNumber = number.ToString();
    //        iNumber = number;
    //    }

    //    public override void Output(List<string> lines, int depth)
    //    {
    //        lines.Add(sNumber);
    //    }

    //    public override string ToString()
    //    {
    //        return sNumber;
    //    }

    //    public Types Type { get { return Types.Atom | Types.Number; } }

    //}

    //public class SNodeString : SNodeBase
    //{
    //    public string Value;

    //    public SNodeString(string value)
    //    {
    //        Value = value;
    //    }

    //    public override void Output(List<string> lines, int depth)
    //    {
    //        lines.Add(Value);
    //    }

    //    public override string ToString()
    //    {
    //        return Value;
    //    }

    //    public Types Type { get { return Types.Atom | Types.String; } }
    //}


    //

}
