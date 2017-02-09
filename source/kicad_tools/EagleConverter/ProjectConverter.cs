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

        90 Modules          ?

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

        List<string> KnownErrors = new List<string>();

        //
        public k.LayerDescriptor ConvertLayer(List<Layer> LayerList, string number, string message = "")
        {
            k.LayerDescriptor result = new Kicad_utils.LayerDescriptor();

            //todo: for copper layers use number?
            //todo: use layer names from Eagle? (cu layers only)

            Layer layer = LayerList.Find(x => x.Number == number);

            if (layer == null)
            {

                Trace(string.Format("warning: layer not found: {0}", number));
                return null;
            }
            else
            {
                switch (number)
                {
                    case "1": result.Name = "F.Cu"; break;

                    case "2": result.Name = "Inner1.Cu"; break;
                    case "3": result.Name = "Inner2.Cu"; break;
                    case "4": result.Name = "Inner3.Cu"; break;
                    case "5": result.Name = "Inner4.Cu"; break;
                    case "6": result.Name = "Inner5.Cu"; break;
                    case "7": result.Name = "Inner6.Cu"; break;
                    case "8": result.Name = "Inner7.Cu"; break;
                    case "9": result.Name = "Inner8.Cu"; break;
                    case "10": result.Name = "Inner9.Cu"; break;
                    case "11": result.Name = "Inner10.Cu"; break;
                    case "12": result.Name = "Inner11.Cu"; break;
                    case "13": result.Name = "Inner12.Cu"; break;
                    case "14": result.Name = "Inner13.Cu"; break;
                    case "15": result.Name = "Inner14.Cu"; break;

                    case "16": result.Name = "B.Cu"; break;

                    // Dimension
                    case "20": result.Name = "Edge.Cuts"; break;

                    // tPlace
                    case "21": result.Name = "F.SilkS"; break;
                    // bPlace
                    case "22": result.Name = "B.SilkS"; break;

                    //  tNames
                    case "25": result.Name = "F.SilkS"; break; // or Fab?
                    //  bNames
                    case "26": result.Name = "B.SilkS"; break;

                    //  tValues
                    case "27": result.Name = "F.SilkS"; break;
                    //  bValues
                    case "28": result.Name = "B.SilkS"; break;

                    //  tStop
                    case "29": result.Name = "F.Mask"; break;
                    //  bStop
                    case "30": result.Name = "B.Mask"; break;

                    //  tCream
                    case "31": result.Name = "F.Paste"; break;
                    //  bCream
                    case "32": result.Name = "B.Paste"; break;

                    //  tFinish
                    case "33": result.Name = "F.Mask"; break;
                    //  bFinish
                    case "34": result.Name = "B.Mask"; break;

                    //  tGlue
                    case "35": result.Name = "F.Adhes"; break;
                    //  bGlue
                    case "36": result.Name = "B.Adhes"; break;

                    //  tKeepout
                    case "39": result.Name = "F.CrtYd"; break;
                    //  bKeepout
                    case "40": result.Name = "B.CrtYd"; break;

                    // tRestrict
                    case "41": result.Name = "Dwgs.User"; break;
                    // bRestrict
                    case "42": result.Name = "Dwgs.User"; break;
                    // vRestrict
                    case "43": result.Name = "Dwgs.User"; break;

                    // Milling
                    case "46": result.Name = "Dwgs.User"; break; // edge?

                    // Document
                    case "48": result.Name = "F.Fab"; break;

                    // Reference
                    case "49": result.Name = "F.Fab"; break;

                    //  tDocu
                    case "51": result.Name = "F.Fab"; break;
                    //  bDocu
                    case "52": result.Name = "B.Fab"; break;

                    default:

                        if (KnownErrors.Find(x => x == number+message) == null)
                        {
                            Trace(string.Format("warning: unsupported layer: {0} {1} in {2}", number, layer.Name, message));
                            KnownErrors.Add(number+message);
                        }
                        return null;
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
