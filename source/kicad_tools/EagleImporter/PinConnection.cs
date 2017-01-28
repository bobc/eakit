using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

using EagleImport;

namespace EagleImporter
{
    public class PinConnection
    {
        public int SheetNumber; // eagle sheet number
        public PointF position; // kicad units

        public Part Part;
        public string GateId;   // unit
        public string PinName;

        public string NetLabel;

        public string PadName;

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
