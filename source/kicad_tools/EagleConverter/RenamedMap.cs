using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EagleConverter
{
    public class RenameMap
    {
        //public List<PrefixItem> PrefixItems;
        public List<RenamedItem> Renames;

        public RenameMap()
        {
            Renames = new List<RenamedItem>();
          //  PrefixItems = new List<PrefixItem>();
        }

        public void Add(string orig, string cleanName)
        {
            if (Renames.Find(x => x.Original == orig) == null)
                Renames.Add(new RenamedItem(orig, cleanName));
        }

        public void Add(string orig)
        {
            if (Renames.Find(x => x.Original == orig) == null)
                Renames.Add(new RenamedItem(orig, null));
        }

        public string GetNewName(string orig)
        {
            RenamedItem item = Renames.Find(x => x.Original == orig);

            if (item != null)
                return item.NewName;
            else
                return orig;
        }

        public string GetPrefix(string name)
        {
            while (char.IsDigit(name[name.Length - 1]))
            {
                name = name.Substring(0, name.Length - 1);
            }
            return name;
        }

        public int GetNumber(string name)
        {
            int result = 0;
            string t = "";
            int index = name.Length - 1;

            while ((index < name.Length) && char.IsDigit(name[index]))
            {
                t = name[index] + t;
                index--;
            }

            int.TryParse(t, out result);

            return result;
        }

        public void Annotate()
        {
            foreach (RenamedItem item in Renames)
            {
                string base_name = GetPrefix(item.Original);
                int num = GetNumber(item.Original);

                if (num != 0)
                {
                    item.NewName = item.Original;
                }
                else
                {
                    num = 1;
                    string new_name = base_name + num;
                    RenamedItem test = Renames.Find(x => x.Original == new_name);
                    while (test != null)
                    {
                        num++; new_name = base_name + num;
                        test = Renames.Find(x => x.Original == new_name);
                    }

                    item.NewName = new_name;
                }
            }
        }

    }
}
