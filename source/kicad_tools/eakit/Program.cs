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
        const string AppCaption = "EAGLE to KiCad Converter";
        const string version = "0.1";

        static AppSettings AppSettings;
        static string AppDataFolder;

        static string SourceFilename;
        static string DestFolder;
        static bool overwrite;

        static bool debug = false;
        static bool Quiet = false;


        static string banner = "eakit v" + version + " (c) Copyright Bob Cousins 2017";

        static void Main(string[] args)
        {
            AppSettings = new AppSettings();

            //todo: load/save settings?

            int argno = 0;

            while (argno < args.Length)
            {
                if (args[argno].StartsWith("-") || args[argno].StartsWith("/"))
                {
                    string arg = args[argno].ToUpperInvariant().Substring(1);

                    // get letters
                    string token = "";
                    while ((arg.Length != 0) && Char.IsLetter(arg[0]))
                    {
                        token += arg[0];
                        arg = arg.Substring(1);
                    }
                    // + / -
                    bool value = true;
                    if ((arg.Length != 0) && (arg[0] == '-'))
                        value = false;

                    switch (token)
                    {
                        case "OVERWRITE": overwrite = true; break;

                        default:
                            Console.WriteLine(banner);
                            Console.WriteLine("bad option {0}", args[argno]);
                            Usage();
                            return;
                    }
                }
                else if (SourceFilename == null)
                    SourceFilename = args[argno];
                else if (DestFolder == null)
                {
                    DestFolder = args[argno];
                }
                else
                {
                    Console.WriteLine(banner);
                    Console.WriteLine("bad parameter {0}", args[argno]);
                    Usage();
                    return;
                }

                argno++;
            }


            if ( (SourceFilename==null) || (DestFolder== null))
            {
                Console.WriteLine(banner);
                Console.WriteLine("missing parameter");
                Usage();
                return;
            }

            if (!Quiet)
                Console.WriteLine(banner);

            ConvertToKicad();
        }

        static void Usage()
        {
            Console.WriteLine("");

            Console.WriteLine("usage: eakit [options] <source_file> <output folder>");
            Console.WriteLine("   source file must be Eagle(tm) schematic file");
            Console.WriteLine("options:");
            Console.WriteLine("   /overwrite     write to non-empty output folder");

        }

        private static void Trace(string message)
        {
            Console.WriteLine(message);
        }

        private static bool ConvertToKicad()
        {
            AppSettings.SourceFilename = SourceFilename;
            AppSettings.DestFolder = DestFolder;

            if (Directory.Exists(AppSettings.DestFolder))
            {
                if (!overwrite)
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
                        Usage();
                        return false;
                    }
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
