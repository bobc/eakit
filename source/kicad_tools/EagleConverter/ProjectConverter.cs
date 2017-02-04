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

        //
        public k.LayerDescriptor ConvertLayer(List<Layer> LayerList, string number)
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
                    case "tNames": result.Name = "F.SilkS"; break;
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

                    // 49
                    case "ReferenceLC": result.Name = "Cmts.User"; break;
                    // 50
                    case "ReferenceLS": result.Name = "Cmts.User"; break;

                    // 51
                    case "tDocu": result.Name = "F.Fab"; break;
                    // 52
                    case "bDocu": result.Name = "B.Fab"; break;

                    default:
                        Trace(string.Format("warning: layer not found: {0} {1}", number, layer.Name));
                        result.Name = "Cmts.User";
                        break;
                }
            }

            result.Number = k.Layer.GetLayerNumber(result.Name);
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
            Trace(string.Format("Writing project file {0}", Path.ChangeExtension(filename, ".pro")));

            k_project.SaveToFile(filename);
        }





        //

        public bool ConvertProject (string SourceFilename, string DestFolder)
        {
            bool result = false;

            string ProjectName;
            string SourceFolder;
            bool bWriteProjectFile = true;

            //
            SourceFolder = Path.GetDirectoryName(SourceFilename);
            ProjectName = Path.GetFileNameWithoutExtension(SourceFilename);


            reportFile = new StreamWriter(Path.Combine(DestFolder, "Conversion report.txt"));

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

            if (Path.GetExtension(SourceFilename).ToLowerInvariant() == ".sch")
                Files.Add(Path.Combine(SourceFolder, Path.ChangeExtension(ProjectName, ".brd")));


            //Files.Add(Path.Combine(SourceFolder, Path.ChangeExtension(ProjectName, ".sch")));
            //Files.Add(Path.Combine(SourceFolder, Path.ChangeExtension(ProjectName, ".brd")));
            // libraries...

            SchematicConverter schema = new SchematicConverter(this);
            BoardConverter board = new BoardConverter(this);
            LibraryConverter library = new LibraryConverter(this);

            CreateProject();
            

            foreach (string file in Files)
            {
                string extension = Path.GetExtension(file).ToLowerInvariant();

                if (extension == ".sch")
                    result = schema.ConvertSchematic(file, DestFolder, ProjectName, ExtractLibraries);
                else if (extension == ".brd")
                    result = board.ConvertBoardFile(file, DestFolder, ProjectName);
                else if (extension == ".lbr")
                    result = library.ConvertLibraryFile(file, DestFolder);
            }

            if (bWriteProjectFile)
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
