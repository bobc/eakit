using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EagleImport
{
    public class DesignRules
    {
        // *** NB : all sizes in mm ***

        int psElongationLong;   ///< percent over 100%.  0-> not elongated, 100->twice as wide as is tall
                                ///< Goes into making a scaling factor for "long" pads.

        int psElongationOffset; ///< the offset of the hole within the "long" pad.

        float rvPadTop;           ///< top pad size as percent of drill size
        // double   rvPadBottom;        ///< bottom pad size as percent of drill size

        float rlMinPadTop;        ///< minimum copper annulus on through hole pads
        float rlMaxPadTop;        ///< maximum copper annulus on through hole pads

        float rvViaOuter;         ///< copper annulus is this percent of via hole
        float rlMinViaOuter;      ///< minimum copper annulus on via
        float rlMaxViaOuter;      ///< maximum copper annulus on via
        float mdWireWire;         ///< wire to wire spacing I presume.


        public DesignRules()
        {
            psElongationLong = 100;
            psElongationOffset = 0;
            rvPadTop = 0.25f;
            // rvPadBottom = 0.25f;
            rlMinPadTop = 0.254f;  // = 10mil
            rlMaxPadTop = 0.508f;  // = 20mil
            rvViaOuter = 0.25f;
            rlMinViaOuter = 0.254f;  // = 10mil
            rlMaxViaOuter = 0.508f;  // = 20mil
            mdWireWire = 0;
        }

        public float CalcPadSize (float drill)
        {
            return 2 * (drill * rvPadTop) + drill;
        }

    }
}
