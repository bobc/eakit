using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using RMC;

namespace Kicad_utils.Project
{
    public class KicadProject
    {
        public List<Section> Sections;

        public KicadProject()
        {
            Sections = new List<Section>();
        }


        private static string FormatDecimal(int Value, int Width)
        {
            return string.Format("{0}", Value).PadLeft(Width, '0');
        }

        public static string FormatDateTime(DateTime time)
        {
            return string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}",
                time.Year, time.Month, time.Day,
                time.Hour, time.Minute, time.Second);
        }

        public bool LoadFromFile(string Filename)
        {
            bool result = true;

            string[] lines;
            int index;

            try
            {
                lines = File.ReadAllLines(Filename);
            }
            catch
            {
                return false;
            }

            index = 0;

            Section section = null;
            Section global_section = null;

            Item current_key = null;

            Sections = new List<Section>();

            while (index < lines.Length)
            {
                if (lines[index].StartsWith("["))
                {
                    // new section
                    string section_name;
                    section_name = lines[index];
                    section_name = section_name.Substring(1, section_name.Length - 2);
                    section = new Section();
                    section.Name = section_name;

                    Sections.Add(section);
                    current_key = null;
                }
                else
                {
                    //new Item or continuation
                    string key = StringUtils.Before(lines[index], "=");
                    string value = StringUtils.After(lines[index], "=");

                    //
                    if (section == null)
                    {
                        if (global_section == null)
                        {
                            global_section = new Section();
                            Sections.Add(global_section);
                        }
                        section = global_section;
                    }
                    //

                    string baseName;
                    int num;
                    if (ParseKey(key, out baseName, out num))
                    {
                        if (current_key == null)
                        {
                            current_key = new Item();
                            section.Items.Add(current_key);
                        }
                        current_key.KeyName = baseName;
                        current_key.Values.Add(value);
                    }
                    else
                    {
                        section.Items.Add(new Item(key, value));
                        current_key = null;
                    }
                }
                index++;
            }

            return result;
        }

        private bool ParseKey(string key, out string baseName, out int num)
        {
            string t = "";
            while (Char.IsDigit(key[key.Length - 1]))
            {
                t = key[key.Length - 1] + t;
                key = key.Substring(0, key.Length - 1);
            }

            if (t != "")
            {
                baseName = key;
                num = int.Parse(t);
                return true;
            }
            else
            {
                baseName = null;
                num = 0;
                return false;
            }
        }

        public void Dump()
        {
            foreach (Section section in Sections)
            {
                if (section.Name == null)
                    Console.WriteLine("-- global --");
                else
                    Console.WriteLine("-- {0} --", section.Name);

                foreach (Item item in section.Items)
                {
                    if (item.Values.Count == 1)
                        Console.WriteLine("{0} = {1}", item.KeyName, item.Values[0]);
                    else
                        Console.WriteLine("{0} = {1}", item.KeyName, string.Join(",", item.Values));

                }
            }
        }

        public bool SaveToFile(string Filename)
        {
            List<string> lines = new List<string>();

            try {
                Filename = Path.ChangeExtension(Filename, ".pro");

                foreach (Section section in Sections)
                {
                    if (section.Name != null)
                        lines.Add(string.Format("[{0}]", section.Name));

                    foreach (Item item in section.Items)
                    {
                        if (item.Values.Count == 0)
                            lines.Add(string.Format("{0}=", item.KeyName));
                        else
                            lines.Add(string.Format("{0}={1}", item.KeyName, item.Values[0]));
                    }
                }
                File.WriteAllLines(Filename, lines.ToArray());
                return true;
            }
            catch
            {
                return false;
            }
        }

    }





}