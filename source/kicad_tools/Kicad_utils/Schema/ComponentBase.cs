using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Text.RegularExpressions;

namespace Kicad_utils.Schema
{
    public class ComponentBase : sch_item_base
    {
        //common
        public string Timestamp;


        // schematic symbol library and name
        // "libsource" in netlist
        public PartSpecifier Symbol;

        // designator e.g. "R12"
        public virtual string Reference
        {
            get { return mReference; }
            set { mReference = value; }
        }

        public virtual string Value
        {
            get { return mValue; }
            set { mValue = value; }
        }

        public virtual string Footprint
        {
            get { return mFootprint; }
            set { mFootprint = value; }
        }


        string mReference;
        string mValue;
        string mFootprint;

        // Fields?
        // Manf, 
        // MFPN, Mpn

        // Supplier
        // DKPN, SPN    SKU

        public virtual List<Field> GetFields ()
        {
            return null;
        }

        public string GetFieldValue(string Name)
        {
            List<Field> fields = GetFields();

            if (fields != null)
                foreach (Field field in fields)
                    if (string.Compare(field.Name, Name, true) == 0)
                        return field.Value;
            return null;
        }

        public virtual void AddOrSetField(string Name, string Value, TextFormat format)
        { }
    }

    public class ComponentComparer : IComparer<ComponentBase>
    {
        public int CompareReference(ComponentBase x, ComponentBase y)
        {
            int result;

            string s1 = Regex.Match(x.Reference, @"\D+").Value;
            string sn1 = Regex.Match(x.Reference, @"\d+").Value;
            int n1;
            if (!int.TryParse(sn1, out n1))
                n1 = 0;

            string s2 = Regex.Match(y.Reference, @"\D+").Value;
            string sn2 = Regex.Match(y.Reference, @"\d+").Value;
            int n2;
            if (!int.TryParse(sn2, out n2))
                n1 = 0;

            result = string.Compare(s1, s2, true);

            if (result == 0)
                return n1 - n2;
            else
                return result;
        }

        public int Compare(ComponentBase x, ComponentBase y)
        {
            return CompareReference(x, y);
        }
    }

}
