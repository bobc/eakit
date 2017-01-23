using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kicad_utils.Schema
{
    public class ComponentLink
    {
        //TimeStamp = /53E95BC2/53EB298E;
        //Reference = C6;
        //ValeurCmp = 100nF;
        //IdModule  = SM0603;

        public string TimeStamp;
        public string Reference;
        public string ValeurCmp;
        public string Footprint; // IdModule
    }
}
