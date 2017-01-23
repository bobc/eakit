using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SExpressions;

namespace Kicad_utils.Pcb
{
    public class NetClass
    {
        public string Name;
        public string Description;

        public float clearance;
        public float trace_width;
        public float via_dia;
        public float via_drill;
        public float uvia_dia;
        public float uvia_drill;

        public List<string> net_names; // members by name

        public NetClass()
        {
        }

        public SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "net_class";
            result.Items = new List<SNodeBase>();
            result.Items.Add(new SNodeAtom(Name));
            result.Items.Add(new SNodeAtom(Description));
            result.Items.Add(new SExpression("clearance", clearance));
            result.Items.Add(new SExpression("trace_width", trace_width));
            result.Items.Add(new SExpression("via_dia", via_dia));
            result.Items.Add(new SExpression("via_drill", via_drill));
            result.Items.Add(new SExpression("uvia_dia", uvia_dia));
            result.Items.Add(new SExpression("uvia_drill", uvia_drill));

            foreach (string s in net_names)
            {
                result.Items.Add(new SExpression("add_net", s));
            }

            return result;
        }

        public static List<SExpression> GetSExpressionList(List<NetClass> netclasses)
        {
            List<SExpression> result = new List<SExpression>();

            foreach (NetClass netClass in netclasses)
            {
                result.Add(netClass.GetSExpression());
            }

            return result;
        }

        public static NetClass DefaultNetclass()
        {
            NetClass result = new NetClass();

            result.Name = "Default";
            result.Description = "This is the default net class.";
            result.clearance = 0.254f;
            result.trace_width = 0.254f;
            result.via_dia = 0.889f;
            result.via_drill = 0.635f;
            result.uvia_dia = 0.508f;
            result.uvia_drill = 0.127f;

            result.net_names = new List<string>();
            result.net_names.Add("");

            return result;
        }
    }
}
