using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Drawing;


namespace EagleImporter
{
    public class GenTestData
    {
        const float to_mm = 24.4f;

        public static double DegToRad(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static PointF RotatePoint(PointF point, double angleDegree)
        {
            return RotatePoint(point, new PointF(0, 0), angleDegree);
        }

        public static PointF RotatePoint(PointF point, PointF pivot, double angleDegree)
        {
            double angle = DegToRad(angleDegree);
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            double dx = point.X - pivot.X;
            double dy = point.Y - pivot.Y;
            double x = cos * dx - sin * dy + pivot.X;
            double y = sin * dx + cos * dy + pivot.Y;

            PointF rotated = new PointF((float)x, (float)y);
            return rotated;
        }

        public static void MergeOutput (string TemplateFilename, string OutputFilename, List<List<string>> lists)
        {
            string[] input;
            List<string> output = new List<string>();

            input = File.ReadAllLines(TemplateFilename);

            foreach (string s in input)
            {
                if (s.StartsWith("$"))
                {
                    int index;
                    if (int.TryParse(s.Substring(1), out index))
                        if (index < lists.Count)
                            output.AddRange(lists[index]);
                }
                else
                    output.Add(s);
            }
            File.WriteAllLines(OutputFilename, output.ToArray());
        }

        public static void GenerateData()
        {
            List<string> part_lines = new List<string>();
            List<string> plain_items = new List<string>();
            List<string> instance_lines = new List<string>();

            PointF orig = new PointF(1.5f * 25.4f, 10.5f * 25.4f);
            PointF spacing = new PointF(1f * 25.4f, 1f * 25.4f);

            PointF offset = new PointF(38.1f - 36.6f, 266.7f - 256.2f);

            //orig.X += offset.X;
            //orig.Y += offset.Y;


            int part_index = 1;
            
            PointF cur_pos = new PointF(orig.X, orig.Y);

            for (int outer = 0; outer < 9; outer++)
            {
                bool smash = false;
                if (outer > 0)
                    smash = true;

                int attr_angle = 0;
                bool attr_mirror = false;

                if (outer > 0)
                {
                    attr_angle = (outer - 1) % 4 * 90;
                    attr_mirror = (outer - 1) > 3;
                }

                /*
                    <attribute name=\"NAME\" x=\"36.195\" y=\"86.1314\" size=\"1.6764\" layer=\"95\" rot=\"R180\"/>
                    <attribute name="VALUE" x="36.195" y="83.947" size="1.6764" layer="96" rot="R180"/>
                */

                string attr_rotation = string.Format("R{0}", attr_angle);
                if (attr_mirror)
                    attr_rotation = "M" + attr_rotation;

                for (int inner = 0; inner < 8; inner++)
                {
                    int angle = (inner % 4) * 90;
                    bool mirror = inner > 3;

                    //<part name=\"IR_R1\" library=\"rcl\" deviceset=\"R-EU_\" device=\"R0603\" value=\"100\"/>

                    string s_rotation = string.Format("R{0}", angle);

                    if (mirror)
                        s_rotation = "M" + s_rotation;
                    string s_part = string.Format("IR_R{0}", part_index);

                    string s_value = s_rotation + "-" + attr_rotation;

                    string line = string.Format("<part name=\"{0}\" library=\"rcl\" deviceset=\"R-EU_\" device=\"R0603\" value=\"{1}\"/>", s_part, s_value);
                    part_lines.Add(line);

                    line = string.Format("<instance part=\"{0}\" gate=\"G$1\" x=\"{1:f4}\" y=\"{2:f4}\" smashed=\"{3}\" rot=\"{4}\">",
                        s_part, cur_pos.X, cur_pos.Y, smash ? "yes" : "no", s_rotation);
                    instance_lines.Add(line);

                    if (smash)
                    {
                        PointF pos = new PointF(cur_pos.X - 0.15f * to_mm, cur_pos.Y + 0.05f * to_mm);
                        pos = RotatePoint(pos, cur_pos, angle);
                        line = string.Format("<attribute name=\"NAME\" x=\"{0:f4}\" y=\"{1:f4}\" size=\"1.6764\" layer=\"95\" rot=\"{2}\"/>",
                            pos.X, pos.Y, attr_rotation);
                        instance_lines.Add(line);

                        pos = new PointF(cur_pos.X - 0.1f * to_mm, cur_pos.Y - 0.15f * to_mm);
                        pos = RotatePoint(pos, cur_pos, angle);
                        line = string.Format("<attribute name=\"VALUE\" x=\"{0:f4}\" y=\"{1:f4}\" size=\"1.6764\" layer=\"96\" rot=\"{2}\"/>",
                            pos.X, pos.Y, attr_rotation);
                        instance_lines.Add(line);
                    }
                    instance_lines.Add("</instance>");

                    part_index++;
                    cur_pos.X += spacing.X;
                }

                cur_pos.Y -= spacing.Y;
                cur_pos.X = orig.X;
            }


            MergeOutput (@"c:\users\bob\documents\eagle\test\test_template.sch",
                @"c:\users\bob\documents\eagle\test\test.sch", 
                new List<List<string>>() { part_lines, plain_items, instance_lines });
        }
    }
}
