using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SExpressions;

namespace Kicad_utils.Project
{
    public class LibEntry
    {
        public string Name;
        /// <summary>
        /// Github, KiCad, Legacy, Eagle, Geda-PCB
        /// </summary>
        public string Type;
        public string Uri;
        public string Options;
        public string Description;

        public LibEntry() { }

        public LibEntry(string Name, string Type, string Uri, string Options, string Description)
        {
            this.Name = Name;
            this.Type = Type;
            this.Uri = Uri;
            this.Options = Options;
            this.Description = Description;
        }

        public static LibEntry Parse(SExpression root_node)
        {
            LibEntry result = null;

            if (root_node.Name == "lib")
            {
                result = new LibEntry();
                int index = 0;

                //
                while (index < root_node.Items.Count)
                {
                    SNodeBase node = root_node.Items[index];
                    SExpression sub = node as SExpression;

                    switch (sub.Name)
                    {
                        case "name":
                            result.Name = sub.GetString();
                            break;
                        case "type":
                            result.Type = sub.GetString();
                            break;
                        case "uri":
                            result.Uri = sub.GetString();
                            break;
                        case "options":
                            result.Options= sub.GetString();
                            break;
                        case "descr":
                            result.Description= sub.GetString();
                            break;
                    }
                    index++;
                }

                return result;
            }
            else
                return null;  // error
        }

        public SExpression GetSexpression ()
        {
            SExpression result = new SExpression();

            result.Name = "lib";
            result.Items = new List<SNodeBase>();
            result.Items.Add(new SExpression("name", Name));
            result.Items.Add(new SExpression("type", Type));
            result.Items.Add(new SExpression("uri", Uri));
            result.Items.Add(new SExpression("options", Options));
            result.Items.Add(new SExpression("descr", Description));

            return result;

        }

    }
}
