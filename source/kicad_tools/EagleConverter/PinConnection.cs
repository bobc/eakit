using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using EagleImport;

using k = Kicad_utils;

namespace EagleConverter
{
    public class PinConnection
    {

        public string NetLabel;
        public Part Part;
        public string GateId;   // unit
        public string PinName;

        public PointF position; // kicad units
        public string Layer;
        public string PartName;

        //
        public k.Schema.sch_text Label;
        public k.Schema.SheetLegacy Sheet;


        public PinConnection()
        {
        }

        public PinConnection(string netLabel, Part part, string gate, string pin)
        {
            this.NetLabel = netLabel;
            this.Part = part;
            this.GateId = gate;
            this.PinName = pin;
        }

        public PinConnection(string netLabel, PointF pos, string layer, string part, string pin)
        {
            this.NetLabel = netLabel;
            this.position = pos;
            this.Layer = layer;

            this.PartName = part;
            this.PinName = pin;
        }

        public PinConnection(k.Schema.sch_text label, k.Schema.SheetLegacy sheet)
        {
            this.Label  = label;
            this.Sheet = sheet;
        }

    }

    // all sheets?
    public class PinConnections
    {
        public List<PinConnection> Connections;

        public PinConnections()
        {
            Connections = new List<PinConnection>();
        }
    }


    public class LabelRef
    {
        public string Name;

        public List<int> SheetNumbers; // eagle sheet number

        public LabelRef()
        {
            SheetNumbers = new List<int>();
        }
        public LabelRef(string name)
        {
            SheetNumbers = new List<int>();
            this.Name = name;
        }
    }

}
