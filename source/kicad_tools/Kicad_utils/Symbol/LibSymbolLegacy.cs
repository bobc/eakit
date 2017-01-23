using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using RMC;
using Lexing;

namespace Kicad_utils.Symbol
{
    public class LibSymbolLegacy
    {
        public string Filename;
        public string Name;

        // index?
        public List<string> Header;
        public string Version;
        public string Encoding;
    
        public List<Symbol> Symbols;

        public LibSymbolLegacy()
        {
            Version = "Version 2.3";
            Encoding = "utf-8";
            Symbols = new List<Symbol>();
            Header = new List<string>();
        }

        public bool LoadFromFile (string Filename)
        {
            bool result = false;
            string[] lines;
            int index;

            try
            {
                lines = File.ReadAllLines(Filename);
                index = 0;
            }
            catch
            {
                return false;
            }

            Version = StringUtils.After(lines[index], " ");
            index++;

            Symbols = new List<Symbol>();

            while (index < lines.Length)
            {
                if (lines[index].StartsWith("#"))
                {
                    if (lines[index].StartsWith("#encoding"))
                    {
                        Encoding = StringUtils.After(lines[index], " ");
                    }

                    index++;
                }
                else if (lines[index].StartsWith("DEF"))
                {
                    List<string> symbol_def = new List<string>();

                    while ((index < lines.Length) && (lines[index] != "ENDDEF"))
                    {
                        symbol_def.Add(lines[index]);
                        index++;
                    }

                    // expecting ENDDEF
                    if ((index < lines.Length) && (lines[index] == "ENDDEF"))
                    {
                        symbol_def.Add(lines[index]);
                        index++;
                    }

                    Symbol sym = Symbol.Parse(symbol_def);

                    if (sym != null)   
                        Symbols.Add(sym);
                } // DEF
            }

            LoadDocFile(Path.ChangeExtension(Filename, ".dcm"));
            return result;
        }

        private void LoadDocFile (string Filename)
        {
            string[] lines;
            int index;

            try
            {
                lines = File.ReadAllLines(Filename);
                index = 0;
            }
            catch
            {
                return;
            }

            // skip header line
            if ((lines.Length == 0) || !lines[0].StartsWith("EESchema-DOCLIB"))
                return;
            index++;

            while (index < lines.Length)
            {
                if (lines[index].StartsWith("#"))
                {
                    index++;
                }
                else if (lines[index].StartsWith("$CMP"))
                {
                    List<Token> tokens = Utils.Tokenise(lines[index]);

                    Symbol sym = Symbols.Find(x => x.Name == tokens[1].Value);

                    while ((index < lines.Length) && (lines[index] != "$ENDCMP"))
                    {
                        if (sym != null)
                        {
                            tokens = Utils.Tokenise(lines[index]);
                            switch (tokens[0].Value)
                            {
                                case "D": sym.Description = StringUtils.After (lines[index], " "); break;
                                case "K": sym.Keywords = StringUtils.After(lines[index], " "); break;
                                case "F": sym.DataSheetFile = StringUtils.After(lines[index], " "); break;
                            }
                        }

                        index++;
                    }

                    // expecting $ENDCMP
                    if ((index < lines.Length) && (lines[index] == "$ENDCMP"))
                    {
                        index++;
                    }

                } // $ENDCMP
            }

        }

        public bool WriteToFile(string Filename)
        {
            bool result = false;
            List<string> data = new List<string>();

            DateTime now = DateTime.Now;
            //data.Add("EESchema-LIBRARY Version 2.3  Date: " + now.ToShortDateString() + " " + now.ToLongTimeString());
            data.Add("EESchema-LIBRARY " + Version); // Version 2.3
            data.Add("# encoding " + Encoding);
            
            foreach (Symbol symbol in Symbols)
            {
                List<string> symbol_text;
                symbol.ConvertToLegacySymbol(out symbol_text);

                data.AddRange(symbol_text);
            }

            data.Add("#");
            data.Add("#End Library");

            File.WriteAllLines(Filename, data);

            //
            WriteToDocFile (Path.ChangeExtension(Filename, ".dcm"));
            return result;
        }

        public bool WriteToDocFile(string Filename)
        {
            bool result = false;
            List<string> data = new List<string>();

            DateTime now = DateTime.Now;
            //data.Add("EESchema-LIBRARY Version 2.3  Date: " + now.ToShortDateString() + " " + now.ToLongTimeString());
            data.Add("EESchema-DOCLIB " + Version); // Version 2.3

            foreach (Symbol symbol in Symbols)
            {
                List<string> symbol_text = new List<string>(); ;
                //symbol.ConvertToLegacySymbol(out symbol_text);
                symbol_text.Add(string.Format("#"));
                symbol_text.Add(string.Format("$CMP {0}", symbol.Name));
                if (!string.IsNullOrEmpty(symbol.Description)) symbol_text.Add(string.Format("D {0}", symbol.Description));
                if (!string.IsNullOrEmpty(symbol.Keywords)) symbol_text.Add(string.Format("K {0}", symbol.Keywords));
                if (!string.IsNullOrEmpty(symbol.DataSheetFile)) symbol_text.Add(string.Format("F {0}", symbol.DataSheetFile));
                symbol_text.Add(string.Format("$ENDCMP"));

                data.AddRange(symbol_text);
            }

            data.Add("#");
            data.Add("#End Library");

            File.WriteAllLines(Filename, data);

            //
            return result;
        }

    }
}
