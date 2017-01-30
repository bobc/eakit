using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;

namespace Kicad_utils.Pcb
{
    public class PcbLayer
    {
        public int Number;
        public string LayerName;
        public string Type; // signal, user
        public bool Visible
        {
            get { return desc.Visible; }
            set { desc.Visible = value; }
        }

        private LayerDescriptor desc = new LayerDescriptor();


        public PcbLayer(int number, string name, string type)
        {
            this.Number = number;
            this.LayerName = name;
            this.Type = type;
        }


        public static SExpression GetSExpression(List<PcbLayer> layers)
        {
            SExpression result = new SExpression();

            result.Name = "layers";
            result.Items = new List<SNodeBase>();

            foreach (PcbLayer layer in layers)
            {
                result.Items.Add(
                    new SExpression(layer.Number.ToString(),
                        new List<SNodeBase>() {
                            new SNodeAtom(layer.LayerName), 
                            new SNodeAtom(layer.Type) 
                        }));
            }

            return result;
        }


        public static List<PcbLayer> DefaultLayers(bool Legacy = false)
        {
            List<PcbLayer> result = new List<PcbLayer>();

            //todo: inner layers

            if (Legacy)
            {
                // = v2 / 3 ??
                // note : 0 and 31 swapped
                result.Add(new PcbLayer(0, Layer.B_Cu, "signal"));

                result.Add(new PcbLayer(15, Layer.F_Cu, "signal"));

                result.Add(new PcbLayer(16, Layer.B_Adhes, "user"));
                result.Add(new PcbLayer(17, Layer.F_Adhes, "user"));
                result.Add(new PcbLayer(18, Layer.B_Paste, "user"));
                result.Add(new PcbLayer(19, Layer.F_Paste, "user"));
                result.Add(new PcbLayer(20, Layer.B_SilkS, "user"));
                result.Add(new PcbLayer(21, Layer.F_SilkS, "user"));
                result.Add(new PcbLayer(22, Layer.B_Mask, "user"));
                result.Add(new PcbLayer(23, Layer.F_Mask, "user"));

                result.Add(new PcbLayer(24, Layer.Drawings, "user"));
                result.Add(new PcbLayer(25, Layer.Comments, "user"));
                result.Add(new PcbLayer(26, Layer.Eco1_User, "user"));
                result.Add(new PcbLayer(27, Layer.Eco2_User, "user"));
                result.Add(new PcbLayer(28, Layer.EdgeCuts, "user"));
            }
            else
            {
                result.Add(new PcbLayer(0, Layer.F_Cu, "signal"));
                result.Add(new PcbLayer(31, Layer.B_Cu, "signal"));

                result.Add(new PcbLayer(32, Layer.B_Adhes, "user"));
                result.Add(new PcbLayer(33, Layer.F_Adhes, "user"));
                result.Add(new PcbLayer(34, Layer.B_Paste, "user"));
                result.Add(new PcbLayer(35, Layer.F_Paste, "user"));
                result.Add(new PcbLayer(36, Layer.B_SilkS, "user"));
                result.Add(new PcbLayer(37, Layer.F_SilkS, "user"));
                result.Add(new PcbLayer(38, Layer.B_Mask, "user"));
                result.Add(new PcbLayer(39, Layer.F_Mask, "user"));

                result.Add(new PcbLayer(40, Layer.Drawings, "user"));
                result.Add(new PcbLayer(41, Layer.Comments, "user"));
                result.Add(new PcbLayer(42, Layer.Eco1_User, "user"));
                result.Add(new PcbLayer(43, Layer.Eco2_User, "user"));
                result.Add(new PcbLayer(44, Layer.EdgeCuts, "user"));

                result.Add(new PcbLayer(46, Layer.B_Courtyard, "user"));
                result.Add(new PcbLayer(47, Layer.F_Courtyard, "user"));
                result.Add(new PcbLayer(48, Layer.B_Fab, "user"));
                result.Add(new PcbLayer(49, Layer.F_Fab, "user"));

            }

            return result;
        }

    }
}
