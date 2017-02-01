using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RMC;
using SExpressions;

namespace Kicad_utils
{

    public static class Layer
    {
        // (0 B.Cu signal)
        // (15 F.Cu signal)     now 31

        // (16 B.Adhes user)    now 32 ...
        // (17 F.Adhes user)
        // (18 B.Paste user)
        // (19 F.Paste user)
        // (20 B.SilkS user)
        // (21 F.SilkS user)
        // (22 B.Mask user)
        // (23 F.Mask user)
        // (24 Dwgs.User user)
        // (25 Cmts.User user)
        // (26 Eco1.User user)
        // (27 Eco2.User user)
        // (28 Edge.Cuts user)

        // (46 B.CrtYd user)
        // (47 F.CrtYd user)
        // (48 B.Fab user)
        // (49 F.Fab user)


        // standard english names
        // note: copper layers can be user defined

        // note : layer number is used in bit masks for 
        // visible_elements? and pcbplotparams.layerselection
        //

        public const int NumCopperLayers = 32;

        public const int nFront_Cu = 0;
        public const int nBack_Cu = 31;


        public const string F_Cu = "F.Cu";  // layer 0 (was 15?)

        public const string Inner1_Cu = "Inner1.Cu";
        public const string Inner2_Cu = "Inner2.Cu";
        public const string Inner3_Cu = "Inner3.Cu";
        public const string Inner4_Cu = "Inner4.Cu";
        public const string Inner5_Cu = "Inner5.Cu";
        public const string Inner6_Cu = "Inner6.Cu";
        public const string Inner7_Cu = "Inner7.Cu";
        public const string Inner8_Cu = "Inner8.Cu";
        public const string Inner9_Cu = "Inner9.Cu";
        public const string Inner10_Cu = "Inner10.Cu";
        public const string Inner11_Cu = "Inner11.Cu";
        public const string Inner12_Cu = "Inner12.Cu";
        public const string Inner13_Cu = "Inner13.Cu";
        public const string Inner14_Cu = "Inner14.Cu";

        public const string Inner15_Cu = "Inner15.Cu";
        public const string Inner16_Cu = "Inner16.Cu";
        public const string Inner17_Cu = "Inner17.Cu";
        public const string Inner18_Cu = "Inner18.Cu";
        public const string Inner19_Cu = "Inner19.Cu";
        public const string Inner20_Cu = "Inner20.Cu";
        public const string Inner21_Cu = "Inner21.Cu";
        public const string Inner22_Cu = "Inner22.Cu";
        public const string Inner23_Cu = "Inner23.Cu";
        public const string Inner24_Cu = "Inner24.Cu";
        public const string Inner25_Cu = "Inner25.Cu";
        public const string Inner26_Cu = "Inner26.Cu";
        public const string Inner27_Cu = "Inner27.Cu";
        public const string Inner28_Cu = "Inner28.Cu";
        public const string Inner29_Cu = "Inner29.Cu";
        public const string Inner30_Cu = "Inner30.Cu";

        public const string B_Cu = "B.Cu";  // layer 31 (was 0?)

        public const uint AllLayersMask = 0xffffffff;

        // used in footprint def?
        public const string Front = "Front";
        public const string Back = "Back";

        public const string B_Adhes = "B.Adhes";
        public const string F_Adhes = "F.Adhes";

        public const string B_Paste = "B.Paste";
        public const string F_Paste = "F.Paste";

        public const string SilkScreen = "SilkS";
        public const string B_SilkS = "B.SilkS";
        public const string F_SilkS = "F.SilkS";

        public const string Mask = "Mask";
        public const string B_Mask = "B.Mask";
        public const string F_Mask = "F.Mask";

        public const string Courtyard = "CrtYd";
        public const string B_Courtyard = "B.CrtYd";
        public const string F_Courtyard = "F.CrtYd";

        public const string B_Fab = "B.Fab";
        public const string F_Fab = "F.Fab";

        public const string Drawings = "Dwgs.User";
        public const string Comments = "Cmts.User";

        public const string Eco1_User = "Eco1.User";
        public const string Eco2_User = "Eco2.User";
        public const string EdgeCuts = "Edge.Cuts";


        public static bool IsFront(string layer)
        {
            if (layer.StartsWith("F."))
                return true;
            else
                return false;
        }

        public static bool IsCopperLayer(int layer)
        {
            return (layer >= 0) && (layer <= 31);
        }

        private static void ParseLayerName(string layer, ref string prefix, ref string suffix)
        {
            if (layer.Contains("."))
            {
                //return StringUtils.Before(layer, ".") + "." + new_layer;
                prefix = StringUtils.Before(layer, ".");
                suffix = StringUtils.After(layer, ".");
            }
            else
            {
                if (layer == "Front")
                    prefix = "F";
                else if (layer == "Back")
                    prefix = "B";
                else
                    suffix = layer;
            }
        }

        // 

        /// <summary>
        /// e.g. ChangeLayer ("F.SilkS", "Fab") ==> F.Fab 
        /// e.g. ChangeLayer ("B.Fab", "Front") ==> F.Fab
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="new_layer"></param>
        /// <returns></returns>
        public static string ChangeLayer(string layer, string new_layer)
        {
            string old_prefix = "";
            string old_suffix = "";
            string new_prefix = "";
            string new_suffix = "";

            ParseLayerName(layer, ref old_prefix, ref old_suffix);
            ParseLayerName(new_layer, ref new_prefix, ref new_suffix);

            if (string.IsNullOrEmpty(new_prefix))
                new_prefix = old_prefix;

            if (string.IsNullOrEmpty(new_suffix))
                new_suffix = old_suffix;

            //
            return new_prefix + "." + new_suffix;
        }

        // e.g. ChangeLayer ("Front", "B.Fab") ==> F.Fab
        public static string MakeLayerName(string CuLayer, string layer)
        {
            if (CuLayer.StartsWith("F"))
                CuLayer = "F";
            else if (CuLayer.StartsWith("B"))
                CuLayer = "B";

            if (layer.Contains("."))
                layer = StringUtils.After(layer, ".");

            return CuLayer + "." + layer;
        }

        // get layer name from number
        private static string GetLayerName_Legacy(int layer)
        {
            if ((layer >= 0) && (layer < Legacy_Layers.Count))
                return Legacy_Layers[layer].Name;
            else
                return "Layer_" + layer;
        }


        public static int GetLayerNumber_Legacy(string layer)
        {
            foreach (LayerDescriptor desc in Legacy_Layers)
            {
                if (string.Compare(desc.Name, layer, true) == 0)
                    return desc.Number;
            }
            return 0;
        }

        /// <summary>
        /// Get the layer number from the name
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static int GetLayerNumber(string layer)
        {
            foreach (LayerDescriptor desc in New_Layers)
            {
                if (string.Compare(desc.Name, layer, true) == 0)
                    return desc.Number;
            }
            return 0;
        }

        // get layer name from number
        public static string GetLayerName(int layer)
        {
            if ((layer >= 0) && (layer < New_Layers.Count))
                return New_Layers[layer].Name;
            else
                return "Layer_" + layer;
        }


        public static List<SNodeBase> GetLayerList(string layers)
        {
            List<SNodeBase> result = new List<SNodeBase>();

            string[] temp = StringUtils.SplitDsvText(layers, "|");

            if (temp != null)
            {
                foreach (string t in temp)
                {
                    result.Add(new SNodeAtom(t));
                }
            }

            return result;
        }


        // for legacy modules
        public static string GetLayerBitmaskString_Legacy(string layers)
        {
            return GetLayerBitmask_Legacy(layers).ToString("X8");
        }

        // get bitmask from list "..|..."

        // NB : for legacy .mod (and pcb?) shouud be 32 bit
        // for new .pretty (.kicad_pcb?) should be 64 bit
        private static UInt32 GetLayerBitmask_Legacy(string layers)
        {
            UInt32 layer_code = 0;

            if (layers.IndexOf("*.Cu") != -1)
                layer_code = 0x0000ffff;

            foreach (LayerDescriptor desc in Layer.Legacy_Layers)
            {
                if (layers.IndexOf(desc.Name) != -1)
                    layer_code |= (UInt32)1 << Layer.GetLayerNumber_Legacy(desc.Name);
            }

            return layer_code;
        }

        private static UInt64 GetLayerBitmask(string layers)
        {
            UInt64 layer_code = 0;

            if (layers.IndexOf("*.Cu") != -1)
                layer_code = 0xffffffff;

            foreach (LayerDescriptor desc in Layer.New_Layers)
            {
                if (layers.IndexOf(desc.Name) != -1)
                    layer_code |= (UInt64)1 << Layer.GetLayerNumber(desc.Name);
            }

            return layer_code;
        }

        private static List<LayerDescriptor> Legacy_Layers = new List<LayerDescriptor>()
            {
                new LayerDescriptor (0, B_Cu),
                new LayerDescriptor (1, Inner1_Cu),
                new LayerDescriptor (2, Inner2_Cu),
                new LayerDescriptor (3, Inner3_Cu),
                new LayerDescriptor (4, Inner4_Cu),
                new LayerDescriptor (5, Inner5_Cu),
                new LayerDescriptor (6, Inner6_Cu),
                new LayerDescriptor (7, Inner7_Cu),
                new LayerDescriptor (8, Inner8_Cu),
                new LayerDescriptor (9, Inner9_Cu),
                new LayerDescriptor (10, Inner10_Cu),
                new LayerDescriptor (11, Inner11_Cu),
                new LayerDescriptor (12, Inner12_Cu),
                new LayerDescriptor (13, Inner13_Cu),
                new LayerDescriptor (14, Inner14_Cu),
                new LayerDescriptor (15, F_Cu),

                new LayerDescriptor (16, B_Adhes),
                new LayerDescriptor (17, F_Adhes),
                new LayerDescriptor (18, B_Paste),
                new LayerDescriptor (19, F_Paste),
                new LayerDescriptor (20, B_SilkS),
                new LayerDescriptor (21, F_SilkS),
                new LayerDescriptor (22, B_Mask),
                new LayerDescriptor (23, F_Mask),

                new LayerDescriptor (24, Drawings),
                new LayerDescriptor (25, Comments),
                new LayerDescriptor (26, Eco1_User),
                new LayerDescriptor (27, Eco2_User),
                new LayerDescriptor (28, EdgeCuts)

            };

        private static List<LayerDescriptor> New_Layers = new List<LayerDescriptor>()
            {
                new LayerDescriptor (0, F_Cu),
                new LayerDescriptor (1, Inner1_Cu),
                new LayerDescriptor (2, Inner2_Cu),
                new LayerDescriptor (3, Inner3_Cu),
                new LayerDescriptor (4, Inner4_Cu),
                new LayerDescriptor (5, Inner5_Cu),
                new LayerDescriptor (6, Inner6_Cu),
                new LayerDescriptor (7, Inner7_Cu),
                new LayerDescriptor (8, Inner8_Cu),
                new LayerDescriptor (9, Inner9_Cu),
                new LayerDescriptor (10, Inner10_Cu),
                new LayerDescriptor (11, Inner11_Cu),
                new LayerDescriptor (12, Inner12_Cu),
                new LayerDescriptor (13, Inner13_Cu),
                new LayerDescriptor (14, Inner14_Cu),

                new LayerDescriptor (15, Inner15_Cu),
                new LayerDescriptor (16, Inner16_Cu),
                new LayerDescriptor (17, Inner17_Cu),
                new LayerDescriptor (18, Inner18_Cu),
                new LayerDescriptor (19, Inner19_Cu),
                new LayerDescriptor (20, Inner20_Cu),
                new LayerDescriptor (21, Inner21_Cu),
                new LayerDescriptor (22, Inner22_Cu),
                new LayerDescriptor (23, Inner23_Cu),
                new LayerDescriptor (24, Inner24_Cu),
                new LayerDescriptor (25, Inner25_Cu),
                new LayerDescriptor (26, Inner26_Cu),
                new LayerDescriptor (27, Inner27_Cu),
                new LayerDescriptor (28, Inner28_Cu),
                new LayerDescriptor (29, Inner29_Cu),
                new LayerDescriptor (30, Inner30_Cu),

                new LayerDescriptor (31, B_Cu),

                new LayerDescriptor (32, B_Adhes),
                new LayerDescriptor (33, F_Adhes),

                new LayerDescriptor (34, B_Paste),
                new LayerDescriptor (35, F_Paste),

                new LayerDescriptor (36, B_SilkS),
                new LayerDescriptor (37, F_SilkS),

                new LayerDescriptor (38, B_Mask),
                new LayerDescriptor (39, F_Mask),

                new LayerDescriptor (40, Drawings),
                new LayerDescriptor (41, Comments),
                new LayerDescriptor (42, Eco1_User),
                new LayerDescriptor (43, Eco2_User),
                new LayerDescriptor (44, EdgeCuts),
                // 45?
                new LayerDescriptor (46, B_Courtyard),
                new LayerDescriptor (47, F_Courtyard),

                new LayerDescriptor (48, B_Fab),
                new LayerDescriptor (49, F_Fab),

            };

        public static string FlipLayer (string layer)
        {
            //TODO: this needs list of pcb layers
            if (layer.StartsWith("F."))
            {
                layer = Layer.MakeLayerName("B", layer);
            }
            else if (layer.StartsWith("B."))
            {
                layer = Layer.MakeLayerName("F", layer);
            }    
            return layer;
        }

        // parsing
        // parse single layer
        // e.g (layer F.SilkS)
        public static string ParseLayer(SNodeBase node)
        {
            if ((node is SExpression) && ((node as SExpression).Name == "layer"))
            {
                SExpression expr = node as SExpression;
                string result = "";
                foreach (SNodeAtom atom in expr.Items)
                {
                    if (result != "")
                        result += "|";

                    result += atom.Value;
                }
                return result;
            }
            else
                return "";  // error
        }

        // returns PSV "...|..."
        // e.g (layers F.Cu F.Paste F.Mask)
        public static string ParseLayers(SNodeBase node)
        {
            if ((node is SExpression) && ((node as SExpression).Name == "layers"))
            {
                SExpression expr = node as SExpression;
                string result = "";
                foreach (SNodeAtom atom in expr.Items)
                {
                    if (result != "")
                        result += "|";

                    result += atom.Value;
                }
                return result;
            }
            else
                return "";  // error
        }


        // e.g (layers F.Cu F.Paste F.Mask)
        // or 
        public static List<LayerDescriptor> ParseLayers(string layers_psv)
        {
            List<LayerDescriptor> result = new List<LayerDescriptor>();

            if (layers_psv.IndexOf("*.Cu") != -1)
                foreach (LayerDescriptor ld in New_Layers)
                    if (ld.Number < NumCopperLayers)
                        result.Add(ld);

            foreach (LayerDescriptor desc in Layer.New_Layers)
            {
                if (layers_psv.IndexOf(desc.Name) != -1)
                    result.Add(desc);
            }
            return result;
        }

        public static List<LayerDescriptor> FlipLayers(List<LayerDescriptor> layer_list)
        {
            List<LayerDescriptor> result = new List<LayerDescriptor>();
            foreach (LayerDescriptor desc in layer_list)
            {
                result.Add(desc.FlipLayer());
            }
            return result;
        }

        // to PSV
        public static string ToString (List<LayerDescriptor> layer_list)
        {
            string result = "";
            bool add_copper = true;

            UInt32 mask = 0;

            foreach (LayerDescriptor desc in layer_list)
            {
                if (desc.Number < NumCopperLayers)
                    mask = mask | (1u << desc.Number);
                else
                    break;
            }

            if ( (mask & AllLayersMask) == AllLayersMask)
            {
                add_copper = false;
                result = "*.Cu";
            }

            foreach (LayerDescriptor desc in layer_list)
            {
                if (add_copper || desc.Number >= NumCopperLayers)
                {
                    if (result.Length != 0)
                        result += "|";
                    result += desc.Name;
                }
            }

            return result;
        }

    }


}
