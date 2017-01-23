using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RMC
{
    public delegate void StatusEventHandler(object sender, EventArgs e);
    public delegate void LoadEventHandler(object sender, EventArgs e);
    public delegate void SaveEventHandler(object sender, EventArgs e);

    public class VirtualDocument
    {
        // Properties
        public bool Modified 
        {
            get {return mModified;}
            set 
            {
                if (value != mModified)
                {
                    mModified = value;
                    if (OnStatusChange != null)
                        OnStatusChange(this, new EventArgs ());
                }
            }
        }

        public string FileName;
        public string AppTitle;
        public string mExtension;
        public string mFilter;
        public string mInitialDirectory;

        // events
        public event StatusEventHandler OnStatusChange;
        public event LoadEventHandler OnLoad;
        public event SaveEventHandler OnSave;

        bool mModified;
        SaveFileDialog saveFileDialog;
        OpenFileDialog openFileDialog;


        public VirtualDocument()
        {
            FileName = "";
            mModified = false;
            AppTitle = "Application";
            mExtension = "*";
            mFilter = "Any file (*.*)|*.*";

            openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open file";
            openFileDialog.CheckFileExists = true;
            openFileDialog.InitialDirectory = mInitialDirectory;
            openFileDialog.DefaultExt = mExtension;
            openFileDialog.Filter = mFilter;

            saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save file";
            saveFileDialog.DefaultExt = mExtension;
            saveFileDialog.Filter = mFilter;

        }

        public bool ChangesSavedDialog()
        {
            if (Modified)
            {
                string DisplayName = Path.GetFileNameWithoutExtension(FileName);

                if (DisplayName == "")
                    DisplayName = "Untitled";

                DialogResult result = MessageBox.Show("Save changes to " + DisplayName + " ?",
                                              AppTitle, MessageBoxButtons.YesNoCancel,
                                              MessageBoxIcon.Question,
                                              MessageBoxDefaultButton.Button1);

                if (result == DialogResult.Yes)
                    return SaveDialog(false);
                else if (result == DialogResult.No)
                    return true;
                else // cancel
                    return false;
            }
            else
                return true;

        }

        public bool NewFileDialog(string fileName)
        {
            if (ChangesSavedDialog())
            {
                FileName = fileName;

                Modified = false;
                return true;
            }
            else
                return false;
        }

        public bool OpenFileDialog()
        {
            if (!Modified || ChangesSavedDialog())
            {
                openFileDialog.DefaultExt = mExtension;
                openFileDialog.Filter = mFilter;
                openFileDialog.InitialDirectory = mInitialDirectory;
                openFileDialog.FileName = FileName;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    FileName = openFileDialog.FileName;
                    mModified = false;

                    if (OnLoad != null)
                        OnLoad(this, new EventArgs());

                    if (OnStatusChange != null)
                        OnStatusChange(this, new EventArgs());

                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public bool SaveFileDialog()
        {
            return SaveDialog(false);
        }

        public bool SaveFileAsDialog()
        {
            return SaveDialog(true);
        }


        public bool SaveDialog(bool SaveAs)
        {
            if (SaveAs || (FileName == ""))
            {
                saveFileDialog.DefaultExt = mExtension;
                saveFileDialog.Filter = mFilter;
                saveFileDialog.FileName = FileName;
                if (FileName == "")
                    saveFileDialog.FileName = "Untitled";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    FileName = saveFileDialog.FileName;
                    if (OnSave != null)
                        OnSave (this, new EventArgs());

                    mModified = false;
                    if (OnStatusChange != null)
                        OnStatusChange(this, new EventArgs());

                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (OnSave != null)
                    OnSave(this, new EventArgs());

                Modified = false;
                return true;
            }
        }
    }
}
