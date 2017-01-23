using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kicad_utils.Project
{
    public class Section
    {
        public string Name;
        public List<Item> Items;

        public Section()
        {
            Items = new List<Item>();
        }

        public Section(string name)
        {
            this.Name = name;
            Items = new List<Item>();
        }

        public void AddItem (string key, string value)
        {
            Items.Add(new Item(key, value));
        }

        public void AddItem(string key, int value)
        {
            Items.Add(new Item(key, value.ToString()));
        }

        public void AddItem(string key, float value)
        {
            Items.Add(new Item(key, value.ToString("f6")));
        }
    }
}
