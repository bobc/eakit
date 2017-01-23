using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SExpressions;

namespace Kicad_utils.Schema
{
    public class ComponentNetlist : ComponentBase
    {
        //
        public List<Field> Fields; // user fields

        //public PartSpecifier LibSource;

        public SheetPath SheetPath;

        public ComponentNetlist()
        { }

        public override List<Field> GetFields()
        {
            return Fields;
        }

        public static ComponentNetlist Parse(SExpression root_node)
        {
            ComponentNetlist result = new ComponentNetlist();

            foreach (SNodeBase item in root_node.Items)
            {
                if (item is SExpression)
                {
                    SExpression node = item as SExpression;
                    switch (node.Name)
                    {
                        case "ref":
                            result.Reference = node.GetValue();
                            break;
                        case "value":
                            result.Value = node.GetValue();
                            break;
                        case "footprint":
                            result.Footprint = node.GetValue();
                            break;

                        case "tstamp":
                            result.Timestamp = node.GetValue();
                            break;
                        case "libsource":
                            result.Symbol = PartSpecifier.Parse(node);
                            break;

                        case "sheetpath":
                            break;

                        case "fields":
                            {
                                result.Fields = new List<Field>();
                                foreach (SExpression sub in node.Items)
                                {
                                    Field field = new Field();

                                    SExpression name = (sub.Items[0] as SExpression);
                                    field.Name = (name.Items[0] as SNodeAtom).Value;
                                    field.Value = (sub.Items[1] as SNodeAtom).Value;

                                    result.Fields.Add(field);
                                }
                            }
                            break;
                    }
                }
                else if (item is SNodeList)
                {
                    SNodeList node = item as SNodeList;

                    result.Fields = new List<Field>();
                    foreach (SExpression sub in node.Items)
                    {
                        Field field = new Field();

                        SExpression name = (sub.Items[0] as SExpression);
                        field.Name = (name.Items[0] as SNodeAtom).Value;
                        field.Value = (sub.Items[1] as SNodeAtom).Value;

                        result.Fields.Add(field);
                    }
                }
            }

            return result;
        }
    }
}
