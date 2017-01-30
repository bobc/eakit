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

        public string Type; // signal, user

        public bool Visible = true;

        public LayerDescriptor()
        {
        }

        public LayerDescriptor(int number, string name)
        {
            Number = number;
            Name = name;
            Type = null;
        }

        public LayerDescriptor(int number, string name, string type)
        {
            Number = number;
            Name = name;
            Type = type;
        }

        public LayerDescriptor Clone ()
        {
            LayerDescriptor result = new LayerDescriptor();
            result.Number = this.Number;
            result.Name = this.Name;
            result.Type = this.Type;
            result.Visible = this.Visible;
            return result;
        }

        public LayerDescriptor FlipLayer()
        {
            LayerDescriptor result = this.Clone();

            if (result.Number < Layer.NumCopperLayers)
            {
                result.Number = Layer.NumCopperLayers -1 - result.Number;
                result.Name = Layer.GetLayerName(result.Number);
            }
            else
            {
                if (result.Name.StartsWith ("F."))
                {
                    result.Name = Layer.MakeLayerName("B", result.Name);
                    result.Number = Layer.GetLayerNumber(result.Name);
                }
                else if (result.Name.StartsWith("B."))
                {
                    result.Name = Layer.MakeLayerName("F", result.Name);
                    result.Number = Layer.GetLayerNumber(result.Name);
                }
            }

            return result;
        }
    }
}
