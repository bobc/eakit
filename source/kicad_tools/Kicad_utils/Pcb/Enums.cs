using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kicad_utils.Pcb
{


    // zone

    public enum ZoneCornerSmoothing { none, chamfer, fillet };

    // default is "" which means thermal_relief
    public enum ZonePadConnection
    {
        thermal_relief,
        thru_hole_only, // THT_thermal
        yes, // = solid aka full
        no // = none
    };

    public enum ZoneFillMode { polygon, segment };

    public enum ZoneOutlineStyle  {none, edge, full };


}
