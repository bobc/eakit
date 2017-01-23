using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Globalization;

namespace SExpressions
{
    public class SNodeAtom : SNodeBase
    {
        public string Value;

        public float AsFloat {
            get {return float.Parse (Value);}
        }

        public int AsInt
        {
            get {
                int int_val;
                if (int.TryParse(Value, out int_val))
                    return int_val;
                else
                    return 0;
            }
        }

        public SNodeAtom(string value)
        {
            Value = value;
        }

        public SNodeAtom(int value)
        {
            Value = value.ToString();
        }

        public SNodeAtom(uint value)
        {
            Value = value.ToString();
        }

        public SNodeAtom(float value)
        {
            Value = value.ToString("f6", CultureInfo.InvariantCulture);
        }

        public override void Output(List<string> lines, int depth)
        {
            //TODO; string containing " or \n

            if (String.IsNullOrEmpty(Value))
                lines.Add("\"\"");
            else if (NeedsQuoting(Value))
                lines.Add("\"" + Value + "\"");
            else
                lines.Add(Value);
        }

        // quoting?
        public override string ToString()
        {
            return EscapedValue();
        }

        public string EscapedValue()
        {
            if (String.IsNullOrEmpty(Value))
                return "\"\"";
            else if (NeedsQuoting(Value))
                return "\"" + Value + "\"";
            else
                return Value;
        }

        public static bool NeedsQuoting(string Value)
        {
            return (Value.IndexOfAny(new char[] { '#', ' ', '\t', '(', ')', '%', '{', '}', }) >= 0)
                || (Value.IndexOf("-") > 0)
                ;
        }

        public Types Type { get { return Types.Atom | Types.String; } }
    }
}
