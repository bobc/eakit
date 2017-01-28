using System;
using System.Collections.Generic;

using System.Xml.Serialization;
using System.IO;

namespace EagleImport
{
    [XmlRoot(ElementName = "eagle")]
    public class EagleFile
    {
        // TODO: maybe not quite right
        [XmlElement(ElementName = "compatibility")]
        public List<Compatibility> Compatibility { get; set; }

        [XmlElement(ElementName = "drawing")]
        public Drawing Drawing { get; set; }

        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }

        public static EagleFile LoadFromXmlFile(string FileName)
        {
            EagleFile result = null;
            XmlSerializer serializer = new XmlSerializer(typeof(EagleFile));
            if (!File.Exists(FileName))
                return result;
            FileStream fs = new FileStream(FileName, FileMode.Open);
            try
            {
                result = (EagleFile)serializer.Deserialize(fs);
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
