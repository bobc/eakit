using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kicad_utils
{
    public class LayerDescriptor
    {
        public int Number;
        public string Name;

        public LayerDescriptor()
        {
        }

        public LayerDescriptor(int number, string name)
        {
            Number = number;
            Name = name;
        }
    }
}
