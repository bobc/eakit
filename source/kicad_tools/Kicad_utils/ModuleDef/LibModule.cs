using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace Kicad_utils.ModuleDef
{
    public class LibModule
    {
        //public string Filename;
        public string Name;

        // legacy
        public string units = "mm";

        // index

        public List<Module> Modules;

        public LibModule ()
        {
            Modules = new List<Module>();
        }

        /// <summary>
        /// Write in legacy format
        /// </summary>
        /// <param name="Filename"></param>
        /// <returns></returns>
        public bool WriteToLegacyFile(string Filename)
        {
            bool result = false;
            List<string> data = new List<string>();

            DateTime now = DateTime.Now;
            data.Add("PCBNEW-LibModule-V1  " + now.ToShortDateString() + " " + now.ToLongTimeString());
            data.Add("# encoding utf-8");
            data.Add("Units "+units);

            data.Add("$INDEX");
            foreach (Module module in Modules)
            {
                data.Add(module.Name);
            }
            data.Add("$ENDINDEX");

            foreach (Module module in Modules)
            {
                List<string> module_text;
                module.ConvertToLegacyModule (out module_text);

                data.AddRange(module_text);
            }
    
            data.Add ("$EndLibrary");

            File.WriteAllLines(Filename, data);

            return result;
        }


        /// <summary>
        /// Write in S-Expression format.
        /// </summary>
        /// <param name="LibraryPath"></param>
        /// <returns></returns>
        public bool WriteLibrary(string LibraryPath)
        {
            bool result = true;

            // path/<Name>.pretty/modules*.kicad_mod

            string path = Path.Combine(LibraryPath, Name + ".pretty");
            //path = Path.ChangeExtension(path, ".pretty");

            Directory.CreateDirectory(path);

            foreach (Module module in Modules)
            {
                string filename = Path.Combine(path, module.Name);

                module.SaveToFile(filename);
            }

            return result;
        }



    }
}
