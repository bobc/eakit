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

        public LayerDescriptor FlipLayer(LayerList referenceLayers)
        {
            LayerDescriptor result = this.Clone();

            if (result.Number < LayerList.NumCopperLayers)
            {
                result.Number = LayerList.NumCopperLayers -1 - result.Number;
                result.Name = referenceLayers.GetLayerName(result.Number);
            }
            else
            {
                if (result.Name.StartsWith ("F."))
                {
                    result.Name = Layer.MakeLayerName("B", result.Name);
                    result.Number = referenceLayers.GetLayerNumber(result.Name);
                }
                else if (result.Name.StartsWith("B."))
                {
                    result.Name = Layer.MakeLayerName("F", result.Name);
                    result.Number = referenceLayers.GetLayerNumber(result.Name);
                }
            }

            return result;
        }
    }
}
