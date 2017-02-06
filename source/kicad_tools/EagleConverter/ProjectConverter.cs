using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using EagleImport;
using k = Kicad_utils;
using RMC;

namespace EagleConverter
{
    public class ProjectConverter
    {
        public delegate void TraceHandler(string s);

        public event TraceHandler OnTrace;

        public bool ExtractLibraries = true;

        //
        StreamWriter reportFile = null;

        public k.Project.KicadProject k_project;


        public void Trace(string s)
        {
            if (OnTrace != null)
                OnTrace(s);

            if (reportFile != null)
            {
                reportFile.WriteLine(s);
            }
        }

        /*
        Predefined EAGLE Layers
        =======================
        Layout
        ------
        1 Top 				Tracks, top side
        2 Route2            Inner layer
        3 Route3            Inner layer
        4 Route4            Inner layer
        5 Route5            Inner layer
        6 Route6            Inner layer
        7 Route7            Inner layer
        8 Route8            Inner layer
        9 Route9            Inner layer
        10 Route10          Inner layer
        11 Route11          Inner layer
        12 Route12          Inner layer
        13 Route13          Inner layer
        14 Route14          Inner layer
        15 Route15          Inner layer
        16 Bottom           Tracks, bottom side
        17 Pads             Pads (through-hole)
        18 Vias             Vias (through-hole)
        19 Unrouted         Airwires (rubberbands)
        20 Dimension        Board outlines (circles for holes)
        21 tPlace           Silk screen, top side
        22 bPlace           Silk screen, bottom side
        23 tOrigins         Origins, top side
        24 bOrigins         Origins, bottom side
        25 tNames           Service print, top side
        26 bNames           Service print, bottom side
        27 tValues          Component VALUE, top side
        28 bValues          Component VALUE, bottom side
        29 tStop            Solder stop mask, top side
        30 bStop            Solder stop mask, bottom side
        31 tCream           Solder cream, top side
        32 bCream           Solder cream, bottom side
        33 tFinish          Finish, top side
        34 bFinish          Finish, bottom side
        35 tGlue            Glue mask, top side
        36 bGlue            Glue mask, bottom side
        37 tTest            Test and adjustment inf., top side
        38 bTest            Test and adjustment inf. bottom side
        39 tKeepout         Nogo areas for components, top side
        40 bKeepout         Nogo areas for components, bottom side
        41 tRestrict        Nogo areas for tracks, top side
        42 bRestrict        Nogo areas for tracks, bottom side
        43 vRestrict        Nogo areas for via-holes
        44 Drills           Conducting through-holes
        45 Holes            Non-conducting holes
        46 Milling          Milling
        47 Measures         Measures
        48 Document         General documentation
        49 Reference        Reference marks
        51 tDocu            Part documentation, top side
        52 bDocu            Part documentation, bottom side

        Schematic
        ---------
        91 Nets             Nets
        92 Busses           Buses
        93 Pins             Connection points for component symbols
                            with additional information
        94 Symbols          Shapes of component symbols
        95 Names            Names of component symbols
        96 Values           Values/component types
        97 Info             General information
        98 Guide            Guide lines



        */

        //
        public k.LayerDescriptor ConvertLayer(List<Layer> LayerList, string number, string message = "")
        {
            k.LayerDescriptor result = new Kicad_utils.LayerDescriptor();

            //todo: for copper layers use number?
            //todo: use layer names from Eagle? (cu layers only)

            Layer layer = LayerList.Find(x => x.Number == number);

            if (layer == null)
            {
                switch (number)
                {
                    case "160": result.Name = "Eco1.User"; break;
                    case "161": result.Name = "Eco2.User"; break;
                }

                Trace(string.Format("warning: layer not found: {0}", number));
                result.Name = "Cmts.User";
            }
            else
            {
                switch (layer.Name)
                {
                    // 1
                    case "Top":
                    case "tCopper":
                        result.Name = "F.Cu";              // or Top
                        break;
                    // 16
                    case "Bottom":
                    case "bCopper":
                        result.Name = "B.Cu";           // or Bottom
                        break;

                    // 20
                    case "Dimension": result.Name = "Edge.Cuts"; break;  // or edge?

                    // 21
                    case "tPlace": result.Name = "F.SilkS"; break;
                    // 22
                    case "bPlace": result.Name = "B.SilkS"; break;

                    // 25
                    case "tNames": result.Name = "F.SilkS"; break; // or Fab?
                    // 26
                    case "bNames": result.Name = "B.SilkS"; break;

                    // 27
                    case "tValues": result.Name = "F.SilkS"; break;
                    // 28
                    case "bValues": result.Name = "B.SilkS"; break;

                    // 29
                    case "tStop": result.Name = "F.Mask"; break;
                    // 30
                    case "bStop": result.Name = "B.Mask"; break;

                    // 31
                    case "tCream": result.Name = "F.Paste"; break;
                    // 32
                    case "bCream": result.Name = "B.Paste"; break;

                    // 33
                    case "tFinish": result.Name = "F.Mask"; break;
                    // 34
                    case "bFinish": result.Name = "B.Mask"; break;

                    // 35
                    case "tGlue": result.Name = "F.Adhes"; break;
                    // 36
                    case "bGlue": result.Name = "B.Adhes"; break;

                    // 39
                    case "tKeepout": result.Name = "F.CrtYd"; break;
                    // 40
                    case "bKeepout": result.Name = "B.CrtYd"; break;

                    // -> clearance?
                    // 41
                    case "tRestrict": result.Name = "Dwgs.User"; break;
                    // 42
                    case "bRestrict": result.Name = "Dwgs.User"; break;
                    // 43
                    case "vRestrict": result.Name = "Dwgs.User"; break;

                    // 46
                    case "Milling": result.Name = "Dwgs.User"; break; // edge?

                    // 48 - Document

                    // 49 - Reference
                    case "ReferenceLC": result.Name = "Cmts.User"; break;
                    // 50 - ?
                    case "ReferenceLS": result.Name = "Cmts.User"; break;

                    // 51
                    case "tDocu": result.Name = "F.Fab"; break;
                    // 52
                    case "bDocu": result.Name = "B.Fab"; break;

                    default:
                        Trace(string.Format("warning: layer not found: {0} {1} {2}", message, number, layer.Name));
                        result.Name = "Cmts.User";
                        break;
                }
            }

            result.Number = k.LayerList.StandardLayers.GetLayerNumber(result.Name);
            return result;
        }



        //
        private void CreateProject()
        {
            k_project = new k.Project.KicadProject();

            k.Project.Section k_section = new k.Project.Section();
            k_project.Sections.Add(k_section);
            k_section.AddItem("update", k.Project.KicadProject.FormatDateTime(DateTime.Now));
            k_section.AddItem("version", 1);
            k_section.AddItem("last_client", "kicad");

            k_section = new k.Project.Section("general");
            k_project.Sections.Add(k_section);
            k_section.AddItem("version", 1);
            k_section.AddItem("RootSch", "");
            k_section.AddItem("BoardNm", "");

            k_section = new k.Project.Section("pcbnew");
            k_project.Sections.Add(k_section);
            k_section.AddItem("version", 1);
            //PageLayoutDescrFile=C:/git_bobc/WebRadio/hardware/test_main - Copy/custom.kicad_wks
            k_section.AddItem("LastNetListRead", "");
            //k_section.AddItem("UseCmpFile", 1);
            k_section.AddItem("PadDrill", 0.6f);
            k_section.AddItem("PadDrillOvalY", 0.6f);
            k_section.AddItem("PadSizeH", 1.5f);
            k_section.AddItem("PadSizeV", 1.5f);
            k_section.AddItem("PcbTextSizeV", 1.5f);
            k_section.AddItem("PcbTextSizeH", 1.5f);
            k_section.AddItem("PcbTextThickness", 0.3f);
            k_section.AddItem("ModuleTextSizeV", 1.0f);
            k_section.AddItem("ModuleTextSizeH", 1.0f);
            k_section.AddItem("ModuleTextSizeThickness", 0.15f);
            k_section.AddItem("SolderMaskClearance", 0.2f);
            k_section.AddItem("SolderMaskMinWidth", 0.0f);
            k_section.AddItem("DrawSegmentWidth", 0.2f);
            k_section.AddItem("BoardOutlineThickness", 0.1f);
            k_section.AddItem("ModuleOutlineThickness", 0.15f);

            k_section = new k.Project.Section("cvpcb");
            k_project.Sections.Add(k_section);
            k_section.AddItem("version", 1);
            k_section.AddItem("NetIExt", "net");

            k_section = new k.Project.Section("eeschema");
            k_project.Sections.Add(k_section);
            k_section.AddItem("version", 1);
            k_section.AddItem("LibDir", "");
        }

        public void SetLibNames (List<string> LibNames)
        {
            k.Project.Section k_section;

            k_section = new k.Project.Section("eeschema/libraries");
            k_project.Sections.Add(k_section);
            int index = 1;
            foreach (string lib in LibNames)
            {
                k_section.AddItem("LibName" + index, lib);
                index++;
            }

            // todo ?
            /*
            [schematic_editor]
            version=1
            PageLayoutDescrFile=custom.kicad_wks
            PlotDirectoryName=
            SubpartIdSeparator=0
            SubpartFirstId=65
            NetFmtName=
            SpiceForceRefPrefix=0
            SpiceUseNetNumbers=0
            LabSize=60
            */
        }


        void WriteProjectFile (string OutputFolder, string ProjectName)
        { 
            string filename = Path.Combine(OutputFolder, ProjectName);
            Trace(string.Format("Writing project file {0}", filename + ".pro"));

            k_project.SaveToFile(filename);
        }





        //

        public bool ConvertProject (string SourceFilename, string DestFolder)
        {
            bool result = false;
            bool IsProject = true;

            string ProjectName;
            string SourceFolder;
            string testname;
            string extension;
            //
            SourceFolder = Path.GetDirectoryName(SourceFilename);
            ProjectName = Path.GetFileNameWithoutExtension(SourceFilename);

            reportFile = new StreamWriter(Path.Combine(DestFolder, ProjectName + "-Conversion report.txt"));

            Trace(string.Format("Conversion report on {0}", StringUtils.IsoFormatDateTime(DateTime.Now)));
            Trace("");
            Trace("Parameters:");
            Trace(string.Format("Input project  {0}", SourceFilename));
            Trace(string.Format("Output project {0}", DestFolder));

            Trace("");
            Trace("Log:");

            //
            List<string> Files = new List<string>();

            Files.Add(SourceFilename);

            extension = Path.GetExtension(SourceFilename).ToLowerInvariant();
            if ((extension == ".sch") || (extension == ".brd"))
                IsProject = true;
            else
                IsProject = false;

            if (Path.GetExtension(SourceFilename).ToLowerInvariant() == ".sch")
            {
                testname = Path.Combine(SourceFolder, ProjectName + ".brd");
                if (File.Exists (testname))
                    Files.Add(testname);
            }

            SchematicConverter schema = new SchematicConverter(this);
            BoardConverter board = new BoardConverter(this);
            LibraryConverter library = new LibraryConverter(this);

            if (IsProject)
                CreateProject();
            

            foreach (string file in Files)
            {
                extension = Path.GetExtension(file).ToLowerInvariant();

                if (extension == ".sch")
                    result = schema.ConvertSchematic(file, DestFolder, ProjectName, ExtractLibraries);
                else if (extension == ".brd")
                    result = board.ConvertBoardFile(file, DestFolder, ProjectName);
                else if (extension == ".lbr")
                    result = library.ConvertLibraryFile(file, DestFolder);
            }

            if (IsProject)
                WriteProjectFile(DestFolder, ProjectName);

            Trace("");
            if (result)
            { 
                Trace("Done");
            }
            else
            {
                Trace("Terminated due to error");
            }

            //
            reportFile.Close();

            return result;
        }
    }

}
