using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using EagleConverter;
using RMC;

namespace eakit
{
    class Program
    {
        const string Company = "RMC";
        const string AppTitle = "Eagle_Importer";
        const string AppCaption = "KiCad to EAGLE Importer";
        const string version = "0.1";

        static AppSettings AppSettings;
        static string AppDataFolder;

        static void Main(string[] args)
        {

            AppSettings = new AppSettings();

            //todo: load/save settings

            ImportEagle(args[0], args[1], false);
        }

        private static void Trace(string message)
        {
            Console.WriteLine(message);
        }

        private static bool ImportEagle(string SourceFilename, string DestFolder, bool overwrite)
        {
            AppSettings.SourceFilename = SourceFilename;
            AppSettings.DestFolder = DestFolder;


            if (Directory.Exists(AppSettings.DestFolder))
            {
                bool is_empty = false;

                string[] Files = Directory.GetFiles(AppSettings.DestFolder);

                if (Files.Length == 0)
                {
                    string[] folders = Directory.GetDirectories(AppSettings.DestFolder);
                    if (folders.Length == 0)
                        is_empty = true;
                }

                if (!is_empty)
                {
                    Trace("error: Destination folder is not empty, specify -overwrite option");
                    return false;
                }
            }
            else
            {
                AppSettingsBase.CreateDirectory(AppSettings.DestFolder);
            }

            EagleUtils utils = new EagleUtils();
            if (utils.CheckValid(SourceFilename))
            {
                utils.OnTrace += Trace;
                utils.ImportFromEagle(SourceFilename, DestFolder);
            }

            return true;
        }

    }
}
