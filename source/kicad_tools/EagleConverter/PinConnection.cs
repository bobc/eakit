using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using EagleImport;

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


        //public int SheetNumber; // eagle sheet number
        //public string PadName;

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
