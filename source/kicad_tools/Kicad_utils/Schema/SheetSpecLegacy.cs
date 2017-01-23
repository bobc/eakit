using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Kicad_utils.Schema
{
    public class SheetSpecLegacy
    {
        //$Sheet
        //S 7350 2900 1050 600 
        //U 504995A0
        //F0 "USB" 60
        //F1 "usb.sch" 60
        //$EndSheet

        public PointF Position;
        public PointF Size;
        public string Timestamp;

        public LegacyField Name;
        public LegacyField Filename;



        public void Write (List<string> data)
        {
            data.Add("$Sheet");

            data.Add(string.Format("S {0} {1} {2} {3}", (int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y));
            data.Add(string.Format("U {0}", Timestamp));
            data.Add(string.Format("F0 \"{0}\" {1}", Name.Value, Name.Size));
            data.Add(string.Format("F1 \"{0}\" {1}", Filename.Value, Filename.Size));

            data.Add("$EndSheet");
        }
    }

}
