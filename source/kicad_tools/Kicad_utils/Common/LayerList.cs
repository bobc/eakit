using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SExpressions;

namespace Kicad_utils
{
    public class LayerList
    {
        public static int NumCopperLayers = 32;

        public List<LayerDescriptor> Layers;

        public LayerList ReferenceLayers;

        public LayerList()
        {
            Layers = new List<LayerDescriptor>();

            ReferenceLayers = new LayerList (GetStandardLayers());
        }

        public LayerList(List<LayerDescriptor> referenceLayers)
        {
            Layers = referenceLayers;
            ReferenceLayers = null;
        }


        public static LayerList StandardLayers = new LayerList(GetStandardLayers());

        // e.g "F.Cu F.Paste F.Mask"
        // or 
        public void ParseLayers(string layers)
        {
           Layers.Clear();

            if (layers.IndexOf("*.Cu") != -1)
                foreach (LayerDescriptor ld in ReferenceLayers.Layers)
                    if (ld.Number < NumCopperLayers)
                        Layers.Add(ld);

            foreach (LayerDescriptor desc in ReferenceLayers.Layers)
            {
                if (layers.IndexOf(desc.Name) != -1)
                    Layers.Add(desc);
            }
        }

        public void AddLayer (string s)
        {
            if (s == "*.Cu")
            {
                foreach (LayerDescriptor ld in ReferenceLayers.Layers)
                    if (ld.Number < NumCopperLayers)
                        Layers.Add(ld);
            }
            else
            {
                LayerDescriptor ld = ReferenceLayers.Layers.Find(x => x.Name == s);
                if (ld != null)
                    Layers.Add(ld);
            }
        }

        public void RemoveLayer(string layer)
        {
            int j = 0;
            while (j < Layers.Count)
            {
                if (string.Compare(Layers[j].Name, layer, true) == 0)
                    Layers.RemoveAt(j);
                else
                    j++;
            }
        }

        /// <summary>
        /// Get the layer number from the name
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public int GetLayerNumber(string layer)
        {
            foreach (LayerDescriptor desc in Layers)
            {
                if (string.Compare(desc.Name, layer, true) == 0)
                    return desc.Number;
            }
            return 0;
        }

        // get layer name from number
        public string GetLayerName(int layer)
        {
            if ((layer >= 0) && (layer < Layers.Count))
                return Layers[layer].Name;
            else
                return "layer_" + layer;
        }

        public UInt64 GetLayerBitmask()
        {
            UInt64 layer_code = 0;

            foreach (LayerDescriptor desc in Layers)
            {
                layer_code |= (UInt64)1 << desc.Number;
            }

            return layer_code;
        }

        public void FlipLayers()
        {
            List<LayerDescriptor> new_list = new List<LayerDescriptor>();

            foreach (LayerDescriptor desc in Layers)
            {
                new_list.Add(desc.FlipLayer(ReferenceLayers));
            }

            Layers = new_list;
        }

        public bool Contains(string layer)
        {
            foreach (LayerDescriptor desc in Layers)
            {
                if (string.Compare(desc.Name, layer, true)==0)
                {
                    return true;
                }
            }
            return false;
        }

        // e.g (layers F.Cu F.Paste F.Mask)
        public void ParseLayers(SNodeBase node)
        {
            Layers.Clear();

            if ((node is SExpression) && ((node as SExpression).Name == "layers"))
            {
                SExpression expr = node as SExpression;
                foreach (SNodeAtom atom in expr.Items)
                {
                    AddLayer (atom.Value);
                }
            }
        }

        public override string ToString()
        {
            const uint AllLayersMask = 0xffffffff;

            string result = "";
            bool add_copper = true;

            UInt32 mask = 0;

            foreach (LayerDescriptor desc in Layers)
            {
                if (desc.Number < NumCopperLayers)
                    mask = mask | (1u << desc.Number);
                else
                    break;
            }

            if ((mask & AllLayersMask) == AllLayersMask)
            {
                add_copper = false;
                result = "*.Cu";
            }

            foreach (LayerDescriptor desc in Layers)
            {
                if (add_copper || desc.Number >= NumCopperLayers)
                {
                    if (result.Length != 0)
                        result += " ";
                    result += desc.Name;
                }
            }

            return result;
        }

        // add, remove
        // flip

        public static List<LayerDescriptor> GetStandardLayers()
        {
            List<LayerDescriptor> result = new List<LayerDescriptor>();

            result.AddRange(new List<LayerDescriptor>()
                {
                    new LayerDescriptor (0, Layer.F_Cu),
                    new LayerDescriptor (1, Layer.Inner1_Cu),
                    new LayerDescriptor (2, Layer.Inner2_Cu),
                    new LayerDescriptor (3, Layer.Inner3_Cu),
                    new LayerDescriptor (4, Layer.Inner4_Cu),
                    new LayerDescriptor (5, Layer.Inner5_Cu),
                    new LayerDescriptor (6, Layer.Inner6_Cu),
                    new LayerDescriptor (7, Layer.Inner7_Cu),
                    new LayerDescriptor (8, Layer.Inner8_Cu),
                    new LayerDescriptor (9, Layer.Inner9_Cu),
                    new LayerDescriptor (10, Layer.Inner10_Cu),
                    new LayerDescriptor (11, Layer.Inner11_Cu),
                    new LayerDescriptor (12, Layer.Inner12_Cu),
                    new LayerDescriptor (13, Layer.Inner13_Cu),
                    new LayerDescriptor (14, Layer.Inner14_Cu),

                    new LayerDescriptor (15, Layer.Inner15_Cu),
                    new LayerDescriptor (16, Layer.Inner16_Cu),
                    new LayerDescriptor (17, Layer.Inner17_Cu),
                    new LayerDescriptor (18, Layer.Inner18_Cu),
                    new LayerDescriptor (19, Layer.Inner19_Cu),
                    new LayerDescriptor (20, Layer.Inner20_Cu),
                    new LayerDescriptor (21, Layer.Inner21_Cu),
                    new LayerDescriptor (22, Layer.Inner22_Cu),
                    new LayerDescriptor (23, Layer.Inner23_Cu),
                    new LayerDescriptor (24, Layer.Inner24_Cu),
                    new LayerDescriptor (25, Layer.Inner25_Cu),
                    new LayerDescriptor (26, Layer.Inner26_Cu),
                    new LayerDescriptor (27, Layer.Inner27_Cu),
                    new LayerDescriptor (28, Layer.Inner28_Cu),
                    new LayerDescriptor (29, Layer.Inner29_Cu),
                    new LayerDescriptor (30, Layer.Inner30_Cu),

                    new LayerDescriptor (31, Layer.B_Cu),

                    new LayerDescriptor (32, Layer.B_Adhes),
                    new LayerDescriptor (33, Layer.F_Adhes),

                    new LayerDescriptor (34, Layer.B_Paste),
                    new LayerDescriptor (35, Layer.F_Paste),

                    new LayerDescriptor (36, Layer.B_SilkS),
                    new LayerDescriptor (37, Layer.F_SilkS),

                    new LayerDescriptor (38, Layer.B_Mask),
                    new LayerDescriptor (39, Layer.F_Mask),

                    new LayerDescriptor (40, Layer.Drawings),
                    new LayerDescriptor (41, Layer.Comments),
                    new LayerDescriptor (42, Layer.Eco1_User),
                    new LayerDescriptor (43, Layer.Eco2_User),
                    new LayerDescriptor (44, Layer.EdgeCuts),
                    // 45?
                    new LayerDescriptor (46, Layer.B_Courtyard),
                    new LayerDescriptor (47, Layer.F_Courtyard),

                    new LayerDescriptor (48, Layer.B_Fab),
                    new LayerDescriptor (49, Layer.F_Fab),
                });
            return result;
        }
    }
}
