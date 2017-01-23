using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace RMC
{
    public class AppSettingsBase
    {
        [XmlIgnore]
        public static string Filename;

        //public delegate void OnLoadEventHandler (object sender, EventArgs ev);
        //public event OnLoadEventHandler OnLoad;


        public AppSettingsBase()
        {
        }


        public static void CreateDirectory(string Filename)
        {
            string path = Path.GetDirectoryName(Filename);

            if ((path != "") && ! Directory.Exists(path))
                Directory.CreateDirectory(path);
        }


        public bool SaveToXmlFile(string FileName)
        {
            bool result = false;
            XmlSerializer serializer = new XmlSerializer(typeof(AppSettingsBase));
            TextWriter Writer = null;

            CreateDirectory(FileName);
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

        public static object LoadFromXmlFile (string Filename, Type t)
        {
            XmlSerializer sr = new XmlSerializer(t);

            if (!File.Exists(Filename))
                return null;

            FileStream fs = new FileStream(Filename, FileMode.Open);

            try
            {
                return sr.Deserialize(fs);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        public static AppSettingsBase LoadFromXmlFile(string FileName)
        {
            AppSettingsBase result = new AppSettingsBase() ;
            XmlSerializer serializer = new XmlSerializer(typeof(AppSettingsBase));

            if (!File.Exists(FileName))
                return result;

            FileStream fs = new FileStream(FileName, FileMode.Open);

            try
            {
                result = (AppSettingsBase)serializer.Deserialize(fs);
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

        private void SaveAppSettings()
        {
            //string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/" + Company;
            //string Filename = Path.Combine(AppDataFolder, AppTitle + ".config.xml");

            OnSaving();

            //AppSettings.Position = this.Location;
            //AppSettings.Size = new Size(this.Width, this.Height);

            SaveToXmlFile(Filename);
        }

        public static AppSettingsBase Load(string filename)
        {
            AppSettingsBase settings = new AppSettingsBase();

            Filename = filename;

            settings = LoadFromXmlFile(Filename);

            settings.OnLoad();

            return settings;
        }

        public virtual void OnLoad ()
        {
        }

        public virtual void OnSaving()
        {
        }
    }
}
