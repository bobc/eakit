using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SExpressions;

namespace Kicad_utils.Project
{
    public class FootprintTable
    {
        public SExpression RootNode;

        public List<LibEntry> Entries;


        public FootprintTable()
        {
            Entries = new List<LibEntry>();
        }


        public bool LoadFromFile(String Filename)
        {
            bool result = false;

            RootNode = new SExpression();
            RootNode.LoadFromFile(Filename);

            if (RootNode.Name == "fp_lib_table")
            {
                foreach (SNodeBase node in RootNode.Items)
                {
                    SExpression s_expr = node as SExpression;

                    if (s_expr.Name == "lib")
                    {
                        LibEntry lib = LibEntry.Parse(s_expr);
                        if (Entries == null)
                            Entries = new List<LibEntry>();
                        Entries.Add(lib);
                    }
                }
            }

            return result;
        }

        public bool SaveToFile (string Filename)
        {
            RootNode = new SExpression();

            RootNode.Name = "fp_lib_table";
            RootNode.Items = new List<SNodeBase>();

            foreach (LibEntry lib in Entries)
                RootNode.Items.Add(lib.GetSexpression());

            //
            RootNode.WriteToFile(Filename);
            return true;
        }
    }


}
