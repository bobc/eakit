using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;

using SExpressions;

using Kicad_utils;
using Kicad_utils.Pcb;
using Kicad_utils.ModuleDef;

using RMC;

namespace EagleImporter
{
    public partial class Form1 : Form
    {
        string Company = "RMC";
        const string AppTitle = "Eagle_Importer";
        const string AppCaption = "KiCad to EAGLE Importer";
        string AppDataFolder;
        AppSettings AppSettings;
        string version = "0.1";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            AppDataFolder = Path.Combine(AppDataFolder, Company);
            AppDataFolder = Path.Combine(AppDataFolder, AppTitle);
            string Filename = Path.Combine(AppDataFolder, AppTitle + ".config.xml");
            LoadAppSettings(Filename);

            textBoxSource.Text = AppSettings.SourceFilename;
            textBoxDest.Text = AppSettings.DestFolder;

            this.Text = AppCaption + " v" + version;
            comboBoxKicadVersion.SelectedIndex = 0;
        }


        public void LoadAppSettings(string filename)
        {
            AppSettingsBase.Filename = filename;
            AppSettings = (AppSettings)AppSettings.LoadFromXmlFile(filename);
            if (AppSettings != null)
            {
                AppSettings.MainForm = this;
                AppSettings.OnLoad();
            }
            else
            {
                AppSettings = new AppSettings(this);
            }
        }

        private void SaveAppSettings()
        {
            AppSettings.OnSaving();
            AppSettings.SaveToXmlFile(AppSettingsBase.Filename);
        }


        private void buttonChooseSource_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Eagle Schematic|*.sch";
            openFileDialog1.Filter += "|Eagle PCB|*.brd";
            openFileDialog1.Filter += "|All Files|*.sch;*.brd";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.FileName = textBoxSource.Text;

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxSource.Text = openFileDialog1.FileName;

                AppSettings.SourceFilename = textBoxSource.Text;
                SaveAppSettings();
            }
        }

        private void buttonChooseDest_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = textBoxDest.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxDest.Text = folderBrowserDialog1.SelectedPath;

                AppSettings.DestFolder = textBoxDest.Text;
                SaveAppSettings();
            }
        }

        void Trace (string message)
        {
            textBoxTrace.AppendText(message + Environment.NewLine);
        }


        //
        bool MakeBackupFile (string Filename, string BackupExt)
        {
            string ext = Path.GetExtension(Filename);
            if (ext.StartsWith("."))
                ext = ext.Remove(0, 1);
            ext = BackupExt + ext;

            string backupFilename = Path.ChangeExtension(Filename, ext);

            try
            {
                File.Copy(Filename, backupFilename, true);
                return true;
            }
            catch
            {
                return false;
            }
        }


        bool FindSegment (kicad_pcb pcb, PointF pos)
        {
            return false;
        }

        void ProcessPcb_ViaToModule(string Filename)
        {
            //bool ref_pos_offset = false;

            kicad_pcb pcb = new kicad_pcb();

            pcb.LoadFromFile(Filename);

            SNodeBase pcb_node = pcb.RootNode;
            SExpression pcb_items = (pcb_node as SExpression);

            int via_number = 1;

            for (int index = 0; index < pcb_items.Items.Count; index++)
            {
                SExpression node = pcb_items.Items[index] as SExpression;

                if (node.Name == "via")
                {
                    Via via = Via.Parse(node);

                    if ( (via.net == 0)) // && !FindSegment (pcb, via.at)
                    {
                        Module module = new Module();
                        module.Name = "main:via";
                        module.layer = "Top";
                        module.tedit = via.tstamp;
                        module.tstamp = via.tstamp;
                        module.At = via.at;
                        module.attr = "virtual";
                        module.Reference = new fp_text("reference", "V" + via_number, new PointF(0, 1.5f), "F.SilkS", new SizeF(1, 1), 0.15f, false);
                        module.Value = new fp_text("value", "via", new PointF(0, -1), "F.Fab", new SizeF(1, 1), 0.15f, false);

                        pad pad = new pad("1", "thru_hole", "circle", new PointF(0, 0), new SizeF(0.7f, 07f), 0.3f, "*.Cu");
                        pad.net = new Net(1, "GND");
                        pad.zone_connect = 2;
                        module.Pads = new List<pad>();
                        module.Pads.Add(pad);

                        pcb_items.Items[index] = module.GetSExpression(true);

                        via_number++;
                    }
                }
                else if (node.Name == "module")
                {
                    Module module = Module.Parse(node);

                    if (module.Name == "main:via")
                    {
                        module.Pads[0].zone_connect = 2;
                        pcb_items.Items[index] = module.GetSExpression(true);
                    }
                }
            }

            MakeBackupFile(Filename, "$");

            pcb.RootNode.WriteToFile(Filename);
        }

        private void btnImportEagle_Click(object sender, EventArgs e)
        {
            //
           // GenTestData.GenerateData();

            //
            AppSettings.SourceFilename = textBoxSource.Text;
            AppSettings.DestFolder = textBoxDest.Text;
            SaveAppSettings();

            if (Directory.Exists(AppSettings.DestFolder))
            {
                bool is_empty = false;

                string[] Files = Directory.GetFiles(AppSettings.DestFolder);

                if (Files.Length==0)
                {
                    string[] folders = Directory.GetDirectories(AppSettings.DestFolder);
                    if (folders.Length == 0)
                        is_empty = true;
                }

                if (!is_empty)
                {
                    DialogResult res = MessageBox.Show("Warning - Destination folder is not empty, files will be overwritten!" + Environment.NewLine + Environment.NewLine +
                        "Continue ?",
                        "Overwrite check",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation
                        );

                    if (res != DialogResult.Yes)
                        return;
                }
            }
            else
            {
                AppSettingsBase.CreateDirectory(AppSettings.DestFolder);
            }

            EagleUtils utils = new EagleUtils();
            if (utils.CheckValid(textBoxSource.Text))
            { 
                utils.OnTrace += Trace;
                utils.ImportFromEagle(textBoxSource.Text, textBoxDest.Text);
            }
        }

    }
}
