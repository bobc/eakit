using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using SExpressions;

namespace Kicad_utils.Pcb
{
    public class PcbPlotParams
    {
        public uint layerselection; // bitmask, decimal? 3178497)
        public bool usegerberextensions;
        public bool excludeedgelayer;
        public float linewidth;
        public bool plotframeref;
        public bool viasonmask;
        public int mode;
        public bool useauxorigin;
        public int hpglpennumber;
        public int hpglpenspeed;
        public int hpglpendiameter;
        public int hpglpenoverlay;
        public bool psnegative;
        public bool psa4output;
        public bool plotreference;
        public bool plotvalue;
        public bool plotothertext;
        public bool plotinvisibletext;
        public bool padsonsilk;
        public bool subtractmaskfromsilk;
        public int outputformat;
        public bool mirror;
        public int drillshape;
        public int scaleselection;
        public string outputdirectory;

        public PcbPlotParams()
        {
            layerselection = 0x10F08001;
            usegerberextensions = true;
            excludeedgelayer = false;
            linewidth = 0.100000f;
            plotframeref = false;
            viasonmask = false;
            mode = 1;
            useauxorigin = true;
            hpglpennumber = 1;
            hpglpenspeed = 20;
            hpglpendiameter = 15;
            hpglpenoverlay = 2;
            psnegative = false;
            psa4output = false;
            plotreference = true;
            plotvalue = true;
            plotothertext = true;
            plotinvisibletext = false;
            padsonsilk = false;
            subtractmaskfromsilk = false;
            outputformat = 1;
            mirror = false;
            drillshape = 1;
            scaleselection = 1;
            outputdirectory = "";
        }

        public SExpression GetSExpression()
        {
            SExpression result = new SExpression();

            result.Name = "pcbplotparams";
            result.Items = new List<SNodeBase>();

            result.Items.Add(new SExpression("layerselection", layerselection));
            result.Items.Add(new SExpression("usegerberextensions", usegerberextensions));
            result.Items.Add(new SExpression("excludeedgelayer", excludeedgelayer));
            result.Items.Add(new SExpression("linewidth", linewidth));
            result.Items.Add(new SExpression("plotframeref", plotframeref));
            result.Items.Add(new SExpression("viasonmask", viasonmask));
            result.Items.Add(new SExpression("mode", mode));
            result.Items.Add(new SExpression("useauxorigin", useauxorigin));
            result.Items.Add(new SExpression("hpglpennumber", hpglpennumber));
            result.Items.Add(new SExpression("hpglpenspeed", hpglpenspeed));
            result.Items.Add(new SExpression("hpglpendiameter", hpglpendiameter));
            result.Items.Add(new SExpression("hpglpenoverlay", hpglpenoverlay));
            result.Items.Add(new SExpression("psnegative", psnegative));
            result.Items.Add(new SExpression("psa4output", psa4output));
            result.Items.Add(new SExpression("plotreference", plotreference));
            result.Items.Add(new SExpression("plotvalue", plotvalue));
            result.Items.Add(new SExpression("plotothertext", plotothertext));
            result.Items.Add(new SExpression("plotinvisibletext", plotinvisibletext));
            result.Items.Add(new SExpression("padsonsilk", padsonsilk));
            result.Items.Add(new SExpression("subtractmaskfromsilk", subtractmaskfromsilk));
            result.Items.Add(new SExpression("outputformat", outputformat));
            result.Items.Add(new SExpression("mirror", mirror));
            result.Items.Add(new SExpression("drillshape", drillshape));
            result.Items.Add(new SExpression("scaleselection", scaleselection));
            result.Items.Add(new SExpression("outputdirectory", outputdirectory));

            return result;
        }
    }
}
