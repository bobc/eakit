using System;

using System.Xml.Serialization;
using System.IO;

namespace EagleImport
{
    [XmlRoot(ElementName = "eagle")]
    public class EagleSchematic
    {
        [XmlElement(ElementName = "drawing")]
        public Drawing Drawing { get; set; }

        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }

        public static EagleSchematic LoadFromXmlFile(string FileName)
        {
            EagleSchematic result = null;
            XmlSerializer serializer = new XmlSerializer(typeof(EagleSchematic));
            if (!File.Exists(FileName))
                return result;
            FileStream fs = new FileStream(FileName, FileMode.Open);
            try
            {
                result = (EagleSchematic)serializer.Deserialize(fs);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
            return result;
        }

    }
}
