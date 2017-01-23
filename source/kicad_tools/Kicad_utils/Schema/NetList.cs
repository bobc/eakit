using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using SExpressions;

namespace Kicad_utils.Schema
{
    public class NetList
    {
        public SExpression RootNode;

        public string Version;
        public NetListDesign Design;

        public List<ComponentBase> Components; // ComponentNetlist

        public List<LibrarySpec> Libraries;
        // libparts
        // nets

        public NetList() { }

        public bool LoadFromFile (String Filename)
        {
            bool result = false;

            RootNode = new SExpression();
            RootNode.LoadFromFile(Filename);

            if (RootNode.Name == "export")
            {
                foreach (SNodeBase node in RootNode.Items)
                {
                    if (node is SExpression)
                    {
                        SExpression sexp = node as SExpression;
                        switch (sexp.Name)
                        {
                            case "version":
                                Version = sexp.GetValue();
                                break;
                            case "design":
                                break;
                            case "components":
                                {
                                    foreach (SExpression sub_node in sexp.Items)
                                    {
                                        if (sub_node.Name == "comp")
                                        {
                                            ComponentNetlist comp = ComponentNetlist.Parse(sub_node);
                                            if (Components == null)
                                                Components = new List<ComponentBase>();
                                            Components.Add(comp);
                                        }
                                    }
                                }
                                break;
                            case "libparts":
                                break;
                            case "libraries":
                                {
                                    foreach (SExpression sub_node in sexp.Items)
                                    {
                                        if (sub_node.Name == "library")
                                        {
                                            LibrarySpec lib = LibrarySpec.Parse(sub_node);
                                            if (Libraries == null)
                                                Libraries = new List<LibrarySpec>();
                                            Libraries.Add(lib);
                                        }
                                    }
                                }
                                break;
                            case "nets":
                                break;
                        }
                    }
                }
            }

            return result;
        }
    }





}
