using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using RMC;

namespace EagleImporter
{

    public class AppSettings
    {
        // GUI elements
        public Point MainPos;
        public Size MainSize;


        // Data

        public string SourceFilename;

        public string DestFolder;

        [XmlIgnore]
        public Form1 MainForm;

        public AppSettings()
        {
            
        }

        public AppSettings(Form1 MainForm)
        {
            this.MainForm = MainForm;
            OnSaving();
        }

        public void OnLoad()
        {
            MainForm.Location = MainPos;
            MainForm.Width = MainSize.Width;
            MainForm.Height = MainSize.Height;
        }

        public void OnSaving()
        {
            MainPos = MainForm.Location;
            MainSize = new Size(MainForm.Width, MainForm.Height);
        }

        public static AppSettings LoadFromXmlFile(string FileName)
        {
            AppSettings result = null;
            XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));

            if (!File.Exists(FileName))
                return result;

            FileStream fs = new FileStream(FileName, FileMode.Open);

            try
            {
                result = (AppSettings)serializer.Deserialize(fs);
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

        public bool SaveToXmlFile(string FileName)
        {
            bool result = false;
            XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
            TextWriter Writer = null;

            AppSettingsBase.CreateDirectory(FileName);
            try
            {
                Writer = new StreamWriter(FileName, false, Encoding.UTF8);
                serializer.Serialize(Writer, this);
                result = true;
            }
            finally
            {
                if (Writer != null)
                {
                    Writer.Close();
                }
            }
            return result;
        }
    }



}
