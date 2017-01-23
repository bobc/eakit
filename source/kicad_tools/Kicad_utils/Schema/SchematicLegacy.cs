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
    public class SchematicLegacy
    {
        public string Filename;

        public SheetLegacy MainSheet;
        public List<SheetLegacy> Sheets;
        public List<ComponentLink> CompLinks;
        public List<ComponentBase> AllComponents;

        //
        public SchematicLegacy()
        {
            Sheets = new List<SheetLegacy>();
        }


        public bool LoadFromFile(string filename)
        {
            Sheets = new List<SheetLegacy>();
            AllComponents = new List<ComponentBase>();

            LoadSheet(filename);

            if (Sheets.Count > 0)
                MainSheet = Sheets[0];

            //
            filename = Path.ChangeExtension(filename, ".cmp");
            if (File.Exists(filename))
                LoadCmpFile(filename);

            return true;
        }

        void LoadSheet (string filename)
        {
            string path = Path.GetDirectoryName(filename);
            SheetLegacy sheet = new SheetLegacy();

            Console.WriteLine("Loading {0}", filename);
            sheet.LoadFromFile(filename, null);
            Sheets.Add(sheet);

            AllComponents.AddRange(sheet.Components);   //*** AR

            // get subsheets
            if (sheet.SubSheets != null)
            {
                foreach (SheetSpecLegacy spec in sheet.SubSheets)
                {
                    filename = Path.Combine(path, spec.Filename.Value);

                    if (Sheets.Find(x => x.Filename == spec.Filename.Value) == null)
                        LoadSheet(filename);
                }
            }

        }


        public void LoadCmpFile(string filename)
        {
            string[] lines;
            int index;

            try
            {
                lines = File.ReadAllLines(filename);
                index = 0;
            }
            catch
            {
                return;
            }

            // skip header line
            if ((lines.Length == 0) || !lines[0].StartsWith("Cmp-Mod V01"))
                return;
            index++;

            CompLinks = new List<ComponentLink>();

            while (index < lines.Length)
            {
                if (lines[index].StartsWith("#"))
                {
                    index++;
                }
                else if (lines[index].StartsWith("BeginCmp"))
                {
                    List<Token> tokens = Utils.Tokenise(lines[index]);

                    ComponentLink compLink = new ComponentLink();

                    while ((index < lines.Length) && (lines[index] != "EndCmp"))
                    {
                        tokens = Utils.Tokenise(lines[index]);
                        switch (tokens[0].Value)
                        {
                            case "TimeStamp": compLink.TimeStamp = StringUtils.Before(tokens[2].Value, ";"); break;
                            case "Reference": compLink.Reference = StringUtils.Before(tokens[2].Value, ";"); break;
                            case "ValeurCmp": compLink.ValeurCmp = StringUtils.Before(tokens[2].Value, ";"); break;
                            case "IdModule":
                                compLink.Footprint = StringUtils.Before(tokens[2].Value, ";");
                                break;
                        }
                        index++;
                    }

                    // expecting 
                    if ((index < lines.Length) && (lines[index] == "EndCmp"))
                    {
                        index++;
                    }

                    //if (Comp)

                    CompLinks.Add(compLink);
                } // $ENDCMP
                else
                {
                    index++;
                }
            }
        }

        public bool SaveToFile(string filename)
        {
            string path = Path.GetDirectoryName(filename);

            foreach (SheetLegacy sheet in Sheets)
            {
                filename = Path.Combine(path, sheet.Filename);
                filename = Path.ChangeExtension(filename, ".sch");

                sheet.SaveToFile(filename);
            }

            return true;
        }
    }


}
