using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SExpressions;

namespace Kicad_utils.Pcb
{
    public class Net
    {
        public int Number;
        public string Name;

        public Net()
        {
        }

        public Net(int number, string name)
        {
            this.Number = number;
            this.Name = name;
        }

        public static List<SExpression> GetSExpressionList(List<Net> nets)
        {
            List<SExpression> result = new List<SExpression>();

            foreach (Net net in nets)
            {
                result.Add(
                    new SExpression("net",
                        new List<SNodeBase>() {
                            new SNodeAtom(net.Number), 
                            new SNodeAtom(net.Name) 
                        }));
            }

            return result;
        }

        public SExpression GetSExpression()
        {
            SExpression result;

            result = new SExpression("net",
                        new List<SNodeBase>() {
                            new SNodeAtom(Number),
                            new SNodeAtom(Name)
                        });

            return result;
        }

        public static Net Parse (SNodeBase node)
        {
            Net result;

            if ((node is SExpression) && ((node as SExpression).Name == "net"))
            {
                result = new Net();
                SExpression expr = node as SExpression;

                int index = 0;

                result.Number = (expr.Items[index++] as SNodeAtom).AsInt;
                result.Name = (expr.Items[index++] as SNodeAtom).Value;

                return result;
            }
            else
                return null;  // error
        }

    }
}
