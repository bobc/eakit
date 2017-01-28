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
            //TODO; string containing '\n'

            lines.Add(EscapedValue());
        }

        // for debug display
        public override string ToString()
        {
            return EscapedValue();
        }

        // get escaped and quoted representation
        private string EscapedValue()
        {
            if (String.IsNullOrEmpty(Value))
                return "\"\"";

            if (Value.IndexOfAny(new char[] { '"' }) >= 0)
            {
                // replace " with \"
                Value = Value.Replace("\"", "\\\"");
            }

            if (NeedsQuoting(Value))
                return "\"" + Value + "\"";
            else
                return Value;
        }

        private bool NeedsQuoting(string Value)
        {
            return (Value.IndexOfAny(new char[] { '#', ' ', '\t', '(', ')', '%', '{', '}' }) >= 0)
                || (Value.IndexOf("-") > 0)
                ;
        }

        public Types Type { get { return Types.Atom | Types.String; } }
    }
}
