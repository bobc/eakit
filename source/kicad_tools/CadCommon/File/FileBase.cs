using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;

namespace CadCommon
{
    public class FileBase
    {
        [XmlIgnore]
        public string FileName ="";

        [XmlIgnore]
        public UnitsSpecification Units;

        [XmlIgnore]
        public string LastError = "";

        public virtual string GetCsv()
        {
            return FileName + "," + "none" + "," + Units.Units + "," + Units.Scale.ToString("g6");
        }

        public virtual bool LoadFromFile(string FileName)
        {
            return false;
        }
    }


}
