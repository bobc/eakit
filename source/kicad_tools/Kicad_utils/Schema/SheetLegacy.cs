using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Drawing;

using RMC;
using Lexing;

namespace Kicad_utils.Schema
{
    public class SheetLegacy
    {
        public bool IsSubSheet;
        public string Filename;

        //
        public string Version;

        public List<string> LibNames;

        // eelayer..

        // description
        public string PaperName;
        public SizeF PageSize;
        public bool Portrait;

        public string Encoding;
        public int SheetNumber;
        public int SheetCount;
        public string Title;
        public string Date;
        public string Rev;
        public string Company;
        public List<string> Comment;

        //
        public List<SheetSpecLegacy> SubSheets;

        //public List<LegacyComponent> Components;
        public List<ComponentBase> Components;

        public List<sch_item_base> Items;

        const float mm_to_mil = 1000.0f / 25.4f;

        // Sheet
        //$Sheet
        //S 7350 2900 1050 600 
        //U 504995A0
        //F0 "USB" 60
        //F1 "usb.sch" 60
        //$EndSheet

        // Wire
        // Wire Wire Line
        //     4300 3900 4300 3550

        // text
        // Text Notes 900  1800 0    100  ~ 0
        // TODO\n\nDIP 32?\n- VREF pin\n- move VRAW

        // Text GLabel 3600 3900 3    40   3State ~ 0
        // D7


        //
        public SheetLegacy()
        {
            IsSubSheet = false;
            Filename = "";
            Version = "2";
            LibNames = new List<string>();

            // description
            PaperName = "A4";
            PageSize = new SizeF(297*mm_to_mil, 210 * mm_to_mil);
            Portrait = false;

            Encoding = "utf-8";
            SheetNumber = 1;
            SheetCount = 1;
            Title = "";
            Date = "";
            Rev = "";
            Company = "";
            Comment = new List<string>();

            SubSheets = null;
            Components = new List<ComponentBase>();
            Items = new List<sch_item_base>();
        }


        public bool LoadFromFile(string filename, string path)
        {
            string[] lines;
            int index;
            List<Token> tokens;

            try
            {
                lines = File.ReadAllLines(filename);
            }
            catch
            {
                return false;
            }

            this.Filename = Path.GetFileName(filename);

            Items = new List<sch_item_base>();

            index = 0;

            Version = lines[index++];

            LibNames = new List<string>();
            while ((index < lines.Length) && lines[index].StartsWith("LIBS:"))
            {
                LibNames.Add(StringUtils.After(lines[index], ":"));
                index++;
            }

            while ((index < lines.Length) && lines[index].StartsWith("EELAYER"))
            {
                // skip
                index++;
            }

            // $Descr
            index++;

            Encoding = StringUtils.After(lines[index++], " ");

            tokens = Utils.Tokenise(lines[index++]);
            SheetNumber = tokens[1].IntValue;
            SheetCount = tokens[2].IntValue;

            Title = StringUtils.After(lines[index++], " ");
            Date = StringUtils.After(lines[index++], " ");
            Rev = StringUtils.After(lines[index++], " ");
            Company = StringUtils.After(lines[index++], " ");

            Comment = new List<string>();
            while ((index < lines.Length) && lines[index].StartsWith("Comment"))
            {
                Comment.Add(StringUtils.After(lines[index], " "));
                index++;
            }

            // $EndDescr
            index++;

            while ((index < lines.Length) && !lines[index].StartsWith("$EndSCHEMATC"))
            {
                string type = StringUtils.Before(lines[index], " ");

                string[] fields = lines[index].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                switch (type)
                {
                    case "$Comp":
                        {
                            LegacyComponent comp = new LegacyComponent();
                            index++;

                            // L CONN_18 P4
                            comp.Symbol = new PartSpecifier(StringUtils.GetDsvField(lines[index], " ", 1));
                            comp.Reference = StringUtils.GetDsvField(lines[index], " ", 2);
                            index++;

                            while ((index < lines.Length) && !lines[index].StartsWith("$EndComp"))
                            {
                                string token = StringUtils.Before(lines[index], " ");

                                tokens = Utils.Tokenise(lines[index]);

                                switch (token)
                                {
                                    case "F":
                                        {
                                            LegacyField f = new LegacyField();
                                            //TODO: need to handle quoted strings

                                            f.Number = tokens[1].IntValue;
                                            f.Value = tokens[2].Value;
                                            f.Orientation = tokens[3].Value;
                                            f.Pos = new System.Drawing.PointF(tokens[4].IntValue, tokens[5].IntValue);
                                            f.Size = tokens[6].IntValue;

                                            if (tokens[7].Value == "0001")
                                                f.Hidden = true;
                                            else
                                                f.Hidden = false;

                                            f.HorizJustify = tokens[8].Value;

                                            f.VertJustify = tokens[9].Value[0].ToString();
                                            f.Italic = tokens[9].Value[1].ToString();
                                            f.Bold = tokens[9].Value[2].ToString();

                                            if (tokens.Count > 10)
                                                f.UserName = StringUtils.TrimQuotes(tokens[10].Value);

                                            switch (f.Number)
                                            {
                                                case 0:
                                                    comp.fReference = f;
                                                    //? comp.Reference = f.Value;
                                                    break;
                                                case 1:
                                                    comp.fValue = f;
                                                    comp.Value = f.Value;
                                                    break;
                                                case 2:
                                                    comp.fPcbFootprint = f;
                                                    comp.Footprint = f.Value;
                                                    break;
                                                case 3:
                                                    comp.fUserDocLink = f;
                                                    break;
                                                default:
                                                    if (comp.UserFields == null)
                                                        comp.UserFields = new List<LegacyField>();
                                                    comp.UserFields.Add(f);
                                                    break;
                                            }
                                        }
                                        break;

                                    case "U":
                                        comp.N = tokens[1].IntValue;
                                        comp.mm = tokens[2].IntValue;
                                        comp.Timestamp = tokens[3].Value;
                                        break;

                                    case "P":
                                        comp.Position = new PointF((float)tokens[1].AsFloat(), (float)tokens[2].AsFloat());
                                        break;

                                    case "AR":
                                        AlternateRef aref = new AlternateRef();
                                        string value = StringUtils.After(tokens[1].Value, "=");
                                        aref.Path = StringUtils.TrimQuotes(value);
                                        value = StringUtils.After(tokens[2].Value, "=");
                                        aref.Ref = StringUtils.TrimQuotes(value);
                                        value = StringUtils.After(tokens[3].Value, "=");
                                        aref.Part = StringUtils.TrimQuotes(value);

                                        if (aref.Ref != comp.Reference)
                                        {
                                            if (comp.AltRefs == null)
                                                comp.AltRefs = new List<AlternateRef>();
                                            comp.AltRefs.Add(aref);
                                        }
                                        break;

                                    default:
                                        // skip line
                                        index++;

                                        // look for orientation
                                        string t = lines[index];
                                        t = t.Remove(0, 1);

                                        tokens = Utils.Tokenise(t);
                                        comp.SetOrientation (tokens[0].IntValue, tokens[1].IntValue, tokens[2].IntValue, tokens[3].IntValue);
                                        break;
                                }
                                //skip
                                index++;
                            }
                            index++;

                            if (Components == null)
                                Components = new List<ComponentBase>();

                            if (comp.AltRefs == null)
                                Components.Add(comp);
                            else
                            {
                                AlternateRef aref = comp.AltRefs.Find(x => x.Path == path);
                                if (aref != null)
                                {
                                    //todo: ???
                                }

                                Components.Add(comp);
                            }

                            Items.Add(comp);
                        }
                        break;

                    case "$Sheet":
                        {
                            SheetSpecLegacy sheet = new SheetSpecLegacy();
                            while ((index < lines.Length) && !lines[index].StartsWith("$EndSheet"))
                            {
                                tokens = Utils.Tokenise(lines[index]);
                                string token = StringUtils.Before(lines[index], " ");
                                switch (token)
                                {
                                    case "F0":
                                        sheet.Name = new LegacyField (StringUtils.TrimQuotes(tokens[1].Value), tokens[2].GetValueAsInt());
                                        break;
                                    case "F1":
                                        sheet.Filename = new LegacyField(StringUtils.TrimQuotes(tokens[1].Value), tokens[2].GetValueAsInt());
                                        break;
                                    case "S":
                                        sheet.Position = new PointF((float)tokens[1].AsFloat(), (float)tokens[2].AsFloat());
                                        sheet.Size = new PointF((float)tokens[3].AsFloat(), (float)tokens[4].AsFloat());
                                        break;
                                    case "U":
                                        sheet.Timestamp = tokens[1].Value;
                                        break;
                                    default:
                                        break;
                                }
                                index++;
                            }
                            if (SubSheets == null)
                                SubSheets = new List<SheetSpecLegacy>();
                            SubSheets.Add(sheet);
                            index++;
                        }
                        break;

                    case "Wire":
                    case "Entry":
                        {
                            sch_wire wire = new sch_wire();
                            wire.name = type;
                            wire.type = fields[1];
                            wire.type2 = fields[2];
                            index++;

                            fields = lines[index].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            wire.start = new Point(int.Parse(fields[0]), int.Parse(fields[1]));
                            wire.end = new Point(int.Parse(fields[2]), int.Parse(fields[3]));
                            index++;

                            Items.Add(wire);
                        }
                        break;

                    case "Text":
                        {
                            sch_text text = new sch_text();
                            text.name = type;
                            text.Type = fields[1];
                            text.Pos = new Point(int.Parse(fields[2]), int.Parse(fields[3]));
                            text.Orientation = int.Parse(fields[4]);
                            text.TextSize = int.Parse(fields[5]);
                            text.Shape = fields[6];
                            text.Param = int.Parse(fields[7]);
                            index++;

                            text.Value = lines[index];
                            index++;

                            Items.Add(text);
                        }
                        break;

                    case "NoConn":
                        sch_NoConnect noconn = new sch_NoConnect();
                        noconn.name = type;
                        noconn.dummy = fields[1];
                        noconn.pos = new Point(int.Parse(fields[2]), int.Parse(fields[3]));

                        Items.Add(noconn);
                        index++;
                        break;

                    case "Connection":
                        sch_Junction junction = new sch_Junction();
                        junction.name = type;
                        junction.dummy = fields[1];
                        junction.Pos = new Point(int.Parse(fields[2]), int.Parse(fields[3]));

                        Items.Add(junction);
                        index++;
                        break;

                    default://oops
                        Console.WriteLine("unrecognised: " + type);
                        index++;
                        break;
                }

                //index++;
            }

            return true;
        }


        public string QuoteStr (string s)
        {
            if (string.IsNullOrEmpty(s))
                return "\"\"";
            else
                return s;
        }

        public bool SaveToFile(string filename)
        {
            List<string> lines = new List<string>();

            lines.Add("EESchema Schematic File Version 2");

            foreach (string s in LibNames)
                lines.Add(string.Format("LIBS:{0}", s));

            lines.Add("EELAYER 25 0");
            lines.Add("EELAYER END");

            //lines.Add("$Descr A4 11693 8268");
            lines.Add(string.Format("$Descr {0} {1} {2}{3}", PaperName, 
                (int)Math.Round(PageSize.Width, MidpointRounding.AwayFromZero), 
                (int)Math.Round(PageSize.Height, MidpointRounding.AwayFromZero),
                Portrait ? " portrait" : ""
                ));
            lines.Add(string.Format("encoding {0}", Encoding));
            lines.Add(string.Format("Sheet {0} {1}", SheetNumber, SheetCount));
            lines.Add(string.Format("Title {0}", QuoteStr(Title)));
            lines.Add(string.Format("Date {0}", QuoteStr(Date)));
            lines.Add(string.Format("Rev {0}", QuoteStr(Rev)));
            lines.Add(string.Format("Comp {0}", QuoteStr(Company)));

            int j = 1;
            for (int index=0; index < Math.Max(4, Comment.Count); index++)
            {
                if (index < Comment.Count)
                    lines.Add(string.Format("Comment{0} {1}", j++, Comment[index]));
                else
                    lines.Add(string.Format("Comment{0} {1}", j++, QuoteStr(null)));
            }
            lines.Add("$EndDescr");

            if (SubSheets != null)
                foreach (SheetSpecLegacy spec in SubSheets)
                    spec.Write(lines);

            foreach (LegacyComponent comp in Components)
                comp.Write(lines);

            foreach (sch_item_base item in Items)
                item.Write(lines);

            lines.Add("$EndSCHEMATC");
            //
            File.WriteAllLines(filename, lines.ToArray());

            return true;
        }
    }


}
