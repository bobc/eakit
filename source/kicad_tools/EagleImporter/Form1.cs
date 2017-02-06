using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;

using EagleConverter;
using RMC;

namespace EagleImporter
{
    public partial class Form1 : Form
    {
        string Company = "RMC";
        const string AppTitle = "Eagle_Importer";
        const string AppCaption = "Eagle to KiCad Converter";
        string AppDataFolder;
        AppSettings AppSettings;
        string version = "0.1.0.4";

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

            comboBoxKicadVersion.Items.Add("4.0.0");
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
            openFileDialog1.Filter += "|Eagle Library|*.lbr";
            openFileDialog1.Filter += "|All Files|*.sch;*.brd;*.lbr";
            openFileDialog1.FilterIndex = 4;
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

            ProjectConverter converter = new ProjectConverter();
            //if (converter.CheckValid(textBoxSource.Text))
            {
                converter.OnTrace += Trace;
                converter.ConvertProject (textBoxSource.Text, textBoxDest.Text);
            }
        }

    }
}
